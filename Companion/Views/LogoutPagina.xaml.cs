using Companion.ViewModels;

namespace Companion.Views;

public partial class LogoutPagina : ContentPage
{
    private readonly LogoutViewModel _viewModel;

    public LogoutPagina(LogoutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _viewModel = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Simuleer een kleine vertraging om de pagina niet direct te laten verdwijnen
        await Task.Delay(1500);  // 1.5 seconde vertraging

        await _viewModel.LogoutAsync();

        // Voer de uitlog logica uit
        await Shell.Current.GoToAsync(nameof(LoginPagina));
    }
}