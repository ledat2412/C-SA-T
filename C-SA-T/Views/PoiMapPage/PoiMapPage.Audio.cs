namespace MauiApp1.Views.Maps;

public partial class PoiMapPage
{
    private async void OnPlayButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_currentDetailGianHang == null)
            {
                await DisplayAlertAsync("Thông báo", "Chưa có gian hàng để phát audio.", "OK");
                return;
            }

            var selectedAudioPath = ResolveAudioPathForSelectedLanguage(_currentDetailGianHang.AudioURL);
            var audioUrl = BuildFullUrl(selectedAudioPath);
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                await DisplayAlertAsync("Thông báo", "Gian hàng này chưa có audio.", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[Audio] selectedLang={_selectedLanguageCode}, source={_currentDetailGianHang.AudioURL}, resolved={selectedAudioPath}, fullUrl={audioUrl}");

            if (_player != null && string.Equals(_loadedAudioUrl, audioUrl, StringComparison.OrdinalIgnoreCase))
            {
                if (_player.IsPlaying)
                {
                    _player.Pause();
                    _playButton.Text = "▶ Phát nè";
                    StopProgressTimer();
                    return;
                }

                _player.Play();
                _playButton.Text = "⏸ Tạm dừng";
                StartProgressTimer();
                return;
            }

            if (_player != null && !string.Equals(_loadedAudioUrl, audioUrl, StringComparison.OrdinalIgnoreCase))
            {
                StopAndDisposeAudio();
            }

            var bytes = await GetAudioBytesAsync(audioUrl);
            if (bytes is null || bytes.Length == 0)
            {
                await DisplayAlertAsync("Lỗi audio", "Không tải được audio từ mạng hoặc cache offline.", "OK");
                return;
            }

            _audioStream?.Dispose();
            _audioStream = new MemoryStream(bytes);

            _player?.Dispose();
            _player = _audioManager.CreatePlayer(_audioStream);
            _loadedAudioUrl = audioUrl;

            _progressSlider.Minimum = 0;
            _progressSlider.Maximum = _player.Duration > 0 ? _player.Duration : 1;
            _progressSlider.Value = 0;

            _currentTimeLabel.Text = "00:00";
            _durationLabel.Text = FormatTime(_player.Duration);

            _player.Play();
            _playButton.Text = "⏸ Tạm dừng";
            StartProgressTimer();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi audio", ex.Message, "OK");
        }
    }

    private void OnProgressDragCompleted(object? sender, EventArgs e)
    {
        try
        {
            if (_player != null)
            {
                _player.Seek(_progressSlider.Value);
                _currentTimeLabel.Text = FormatTime(_progressSlider.Value);
            }
        }
        finally
        {
            _isDraggingSlider = false;
        }
    }

    private void StartProgressTimer()
    {
        StopProgressTimer();

        _progressTimer = new System.Timers.Timer(500);
        _progressTimer.Elapsed += (_, __) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_player == null)
                    return;

                if (!_isDraggingSlider)
                {
                    var max = _player.Duration > 0 ? _player.Duration : 1;
                    _progressSlider.Maximum = max;
                    _progressSlider.Value = Math.Min(_player.CurrentPosition, max);
                    _currentTimeLabel.Text = FormatTime(_player.CurrentPosition);
                    _durationLabel.Text = FormatTime(_player.Duration);
                }

                if (!_player.IsPlaying && _player.Duration > 0 && _player.CurrentPosition >= _player.Duration)
                {
                    _playButton.Text = "▶ Phát nè";
                    _progressSlider.Value = 0;
                    _currentTimeLabel.Text = "00:00";
                    StopProgressTimer();
                }
            });
        };
        _progressTimer.AutoReset = true;
        _progressTimer.Start();
    }

    private void StopProgressTimer()
    {
        if (_progressTimer != null)
        {
            _progressTimer.Stop();
            _progressTimer.Dispose();
            _progressTimer = null;
        }
    }

    private void ResetAudioState()
    {
        StopAndDisposeAudio();

        _playButton.Text = "▶ Phát nè";
        _progressSlider.Minimum = 0;
        _progressSlider.Maximum = 1;
        _progressSlider.Value = 0;
        _currentTimeLabel.Text = "00:00";
        _durationLabel.Text = "00:00";
        _isDraggingSlider = false;
        _loadedAudioUrl = null;
    }

    private void StopAndDisposeAudio()
    {
        try
        {
            StopProgressTimer();

            _player?.Stop();
            _player?.Dispose();
            _player = null;

            _audioStream?.Dispose();
            _audioStream = null;
            _loadedAudioUrl = null;

            _playButton.Text = "▶ Phát nè";
        }
        catch
        {
        }
    }

    private static string? BuildFullUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return global::MauiApp1.Utils.BackendUrlResolver.BuildUrl(path);
    }

    private static HttpClient CreateHttpClientForAudio()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => true;
#endif

        return new HttpClient(handler);
    }

    private static string FormatTime(double seconds)
    {
        if (seconds < 0)
            seconds = 0;

        var ts = TimeSpan.FromSeconds(seconds);
        return ts.Hours > 0
            ? ts.ToString(@"hh\:mm\:ss")
            : ts.ToString(@"mm\:ss");
    }

}
