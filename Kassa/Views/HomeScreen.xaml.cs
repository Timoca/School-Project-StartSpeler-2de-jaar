using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class HomeScreen : ContentPage
{
    public HomeScreen(HomeScreenViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        if (vm == null)
        {
            throw new ArgumentNullException(nameof(vm), "ViewModel must not be null.");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!(BindingContext is HomeScreenViewModel viewModel)) return;

        var isAuthenticated = await viewModel.CheckAuthentication();
        if (!isAuthenticated)
        {
            try
            {
                Debug.WriteLine("Navigating to LoginPage");
                await Task.Delay(10); // Wacht even totdat de Shell is geladen.
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                // Log de fout of toon een bericht
                Debug.WriteLine($"Navigatiefout: {ex.Message}");
            }
        }
    }
}