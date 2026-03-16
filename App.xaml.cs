using MauiApp1.Views.Auth;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());
    }
}