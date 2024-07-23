using Companion.ViewModels;
using System.Diagnostics;

namespace Companion.Views;

public partial class HomePagina : ContentPage
{
    public HomePagina(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnHamburgerMenuTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!(BindingContext is HomeViewModel viewModel)) return;

        var isAuthenticated = await viewModel.CheckAuthentication();
        if (!isAuthenticated)
        {
            try
            {
                await Task.Delay(10); // Wacht even totdat de Shell is geladen.
                await Shell.Current.GoToAsync(nameof(LoginPagina));
            }
            catch (Exception ex)
            {
                // Log de fout of toon een bericht
                Debug.WriteLine($"Navigatiefout: {ex.Message}");
            }
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        double desiredHeight = height / 2; // Verdeel het scherm in tweeën voor twee rijen
        myFrame.HeightRequest = desiredHeight; // Pas de berekende hoogte toe op het Frame
    }

    private void OnBestellenClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//DrankPagina");
    }

    private void OnEventsClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//EventPagina");
    }

    private async void OnFrameTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        // Animeren van de schaal naar beneden en terug
        await frame!.ScaleTo(0.98, 50); // Verklein de schaal
        await frame!.ScaleTo(1, 50); // Herstel de schaal
    }
}