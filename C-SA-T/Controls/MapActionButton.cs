using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Controls;

public sealed class MapActionButton : ContentView
{
    private readonly Border _frame;
    private readonly View _icon;
    private readonly ActivityIndicator _indicator;
    private bool _isButtonEnabled = true;
    private bool _isBusy;

    public MapActionButton(View icon, double widthRequest = 52, double heightRequest = 52)
    {
        _icon = icon;
        _indicator = new ActivityIndicator
        {
            IsVisible = false,
            IsRunning = false,
            Color = Color.FromArgb("#EA580C"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 18,
            HeightRequest = 18
        };

        var content = new Grid
        {
            Children = { _icon, _indicator }
        };

        _frame = new Border
        {
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Color.FromArgb("#F3E8E2")),
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            WidthRequest = widthRequest,
            HeightRequest = heightRequest,
            Padding = 0,
            Content = content,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.10f,
                Radius = 12,
                Offset = new Point(0, 4)
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, __) =>
        {
            if (!_isButtonEnabled || _isBusy)
                return;

            Clicked?.Invoke(this, EventArgs.Empty);
        };
        _frame.GestureRecognizers.Add(tap);

        Content = _frame;
    }

    public event EventHandler? Clicked;

    public bool IsButtonEnabled
    {
        get => _isButtonEnabled;
        set
        {
            _isButtonEnabled = value;
            UpdateVisualState();
        }
    }

    public void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        _icon.IsVisible = !_isBusy;
        _indicator.IsVisible = _isBusy;
        _indicator.IsRunning = _isBusy;
        _frame.Opacity = _isButtonEnabled ? 1 : 0.58;
    }
}
