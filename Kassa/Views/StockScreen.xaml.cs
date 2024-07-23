using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class StockScreen : ContentPage
{
	public StockScreen(StockScreenViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        Loaded += async (sender, args) => {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }
}