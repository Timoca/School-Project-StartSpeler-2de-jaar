using Kassa.Models;
using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class MenuScreen : ContentPage
{
	public MenuScreen(MenuScreenViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        Loaded += async (sender, args) => {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }
    
}