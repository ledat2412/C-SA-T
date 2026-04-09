using MauiApp1.Models;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views.Maps;

public partial class PoiMapPage
{
    private async Task LoadLanguagesAsync(bool forceReload = false)
    {
        if (!forceReload && _languages.Count > 0)
            return;

        _languages.Clear();
        _languages.Add(new NgonNgu
        {
            MaNgonNgu = "vi",
            TenNgonNgu = "Tiếng Việt"
        });
        _languages.Add(new NgonNgu
        {
            MaNgonNgu = "en",
            TenNgonNgu = "English"
        });

        if (_languages.All(x => !x.MaNgonNgu.Equals(_selectedLanguageCode, StringComparison.OrdinalIgnoreCase)))
        {
            _selectedLanguageCode = _languages[0].MaNgonNgu;
        }
    }

    private void RenderLanguageOptions()
    {
        _languageRow.Children.Clear();

        foreach (var language in _languages)
        {
            var code = language.MaNgonNgu;
            var selected = code.Equals(_selectedLanguageCode, StringComparison.OrdinalIgnoreCase);

            var chip = new Border
            {
                Stroke = selected ? Color.FromArgb("#FF6B00") : Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                BackgroundColor = selected ? Color.FromArgb("#FFF1E6") : Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                Padding = new Thickness(12, 8),
                Content = new Label
                {
                    Text = string.IsNullOrWhiteSpace(language.TenNgonNgu) ? code.ToUpperInvariant() : language.TenNgonNgu,
                    FontSize = 13,
                    FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = selected ? Color.FromArgb("#9A3412") : Color.FromArgb("#374151")
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) => await SelectLanguageAsync(code);
            chip.GestureRecognizers.Add(tap);

            _languageRow.Children.Add(chip);
        }
    }

    private async Task SelectLanguageAsync(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return;

        languageCode = NormalizeLanguageCode(languageCode);

        if (_selectedLanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            return;

        _selectedLanguageCode = languageCode;
        ResetAudioState();
        RenderLanguageOptions();
        await ApplySelectedLanguageToCurrentDetailAsync();
    }

    private async Task ApplySelectedLanguageToCurrentDetailAsync()
    {
        if (_currentDetailGianHang is null)
            return;

        var translated = await _gianHangService.GetByIdAsync(_currentDetailGianHang.IdGianHang, NormalizeLanguageCode(_selectedLanguageCode));
        if (translated is null)
        {
            SetDetailInfo(_currentDetailGianHang);
            return;
        }

        _detailTitle.Text = string.IsNullOrWhiteSpace(translated.Ten) ? "Tên gian hàng" : translated.Ten;
        _detailAddress.Text = string.IsNullOrWhiteSpace(translated.DiaChi) ? "Chưa có địa chỉ" : translated.DiaChi;
        _detailDescription.Text = string.IsNullOrWhiteSpace(translated.MoTa) ? "Chưa có mô tả." : translated.MoTa;

        _detailAudioLabel.Text = string.IsNullOrWhiteSpace(translated.AudioURL)
            ? "Audio: chưa có"
            : "Audio thuyết minh đã sẵn sàng";

        System.Diagnostics.Debug.WriteLine(
            $"[Lang] selected={_selectedLanguageCode}, translatedAudio={translated.AudioURL}");

        if (!string.IsNullOrWhiteSpace(translated.HinhAnhChinh) || !string.IsNullOrWhiteSpace(translated.HinhAnh))
        {
            _detailImage.Source = BuildImageSource(
                !string.IsNullOrWhiteSpace(translated.HinhAnhChinh)
                    ? translated.HinhAnhChinh
                    : translated.HinhAnh);
        }

        _currentDetailGianHang = translated;
    }

    private static string NormalizeLanguageCode(string? rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return "vi";

        var code = rawCode.Trim().ToLowerInvariant();
        if (code.StartsWith("en"))
            return "en";
        if (code.StartsWith("vi"))
            return "vi";

        return code;
    }

    private string? ResolveAudioPathForSelectedLanguage(string? rawAudioPath)
    {
        if (string.IsNullOrWhiteSpace(rawAudioPath))
            return rawAudioPath;

        var lang = NormalizeLanguageCode(_selectedLanguageCode);
        var audioPath = rawAudioPath.Trim();
        var targetSuffix = $"_{lang}";
        var alternateSuffix = lang == "en" ? "_vi" : "_en";

        var lastSlash = audioPath.LastIndexOf('/');
        var basePath = lastSlash >= 0 ? audioPath[..(lastSlash + 1)] : string.Empty;
        var fileName = lastSlash >= 0 ? audioPath[(lastSlash + 1)..] : audioPath;

        var dotIdx = fileName.LastIndexOf('.');
        var stem = dotIdx >= 0 ? fileName[..dotIdx] : fileName;
        var ext = dotIdx >= 0 ? fileName[dotIdx..] : string.Empty;

        if (stem.EndsWith(targetSuffix, StringComparison.OrdinalIgnoreCase))
            return audioPath;

        if (stem.EndsWith(alternateSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var replaced = stem[..^alternateSuffix.Length] + targetSuffix + ext;
            return basePath + replaced;
        }

        return audioPath;
    }

}

