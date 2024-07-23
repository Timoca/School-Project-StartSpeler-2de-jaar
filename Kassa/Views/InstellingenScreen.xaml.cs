using Kassa.ViewModels;

namespace Kassa.Views;

public partial class InstellingenScreen : ContentPage
{
	public InstellingenScreen(InstellingenScreenViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}