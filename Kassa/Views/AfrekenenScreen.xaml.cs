using Kassa.ViewModels;

namespace Kassa.Views;

public partial class AfrekenenScreen : ContentPage
{
    public AfrekenenScreen(AfrekenenScreenViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AfrekenenScreenViewModel viewModel)
        {
            await viewModel.LoadBesteldeProductenAsync();
        }
    }
}