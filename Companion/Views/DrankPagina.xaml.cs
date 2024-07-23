using Companion.ViewModels;

namespace Companion.Views;

public partial class DrankPagina : ContentPage
{
    public DrankPagina()
    {
        InitializeComponent();
    }

	public DrankPagina(DrankViewModel vm)
	{
        BindingContext = vm;
		InitializeComponent();
	}


    private void OnHamburgerMenuTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }
}