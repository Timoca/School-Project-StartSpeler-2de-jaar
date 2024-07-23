using Companion.ViewModels;

namespace Companion.Views;

public partial class WinkelmandPagina : ContentPage
{
    public WinkelmandPagina(WinkelmandViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnHamburgerMenuTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }
}