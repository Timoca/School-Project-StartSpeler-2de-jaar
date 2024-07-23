using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class OverzichtScreen : ContentPage
{
	public OverzichtScreen(OverzichtScreenViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        Loaded += async (sender, args) => {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }
}