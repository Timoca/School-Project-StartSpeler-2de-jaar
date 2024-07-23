using Companion.ViewModels;

namespace Companion.Views;

public partial class RegistratiePagina : ContentPage
{
	public RegistratiePagina(RegisterViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private void OnHamburgerMenuTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

}