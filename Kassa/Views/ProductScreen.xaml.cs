using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class ProductScreen : ContentPage
{
	public ProductScreen(ProductScreenViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        Loaded += async (sender, args) => {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }
}