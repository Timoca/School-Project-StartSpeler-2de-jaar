using CommunityToolkit.Mvvm.Input;
using Companion.ViewModels;

namespace Companion.Views;

public partial class LoginPagina : ContentPage
{
    public LoginPagina(LoginViewModel vm)
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
        if (!(BindingContext is LoginViewModel viewModel)) return;

        await viewModel.ResetLogginUI();
    }
}