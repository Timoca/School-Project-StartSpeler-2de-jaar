using Kassa.ViewModels;

namespace Kassa.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (!(BindingContext is LoginViewModel viewModel)) return;
		await viewModel.ResetLogginUI();
	}
}