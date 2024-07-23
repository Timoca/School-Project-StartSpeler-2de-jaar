using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Services;

namespace Kassa.ViewModels
{
	public partial class RegisterViewModel : BaseViewModel
	{
		private readonly ApiService _apiService;

		[ObservableProperty]
		private string voornaam = default!;

		[ObservableProperty]
		private string achternaam = default!;

		[ObservableProperty]
		private string email = default!;

		[ObservableProperty]
		private string password = default!;

		[ObservableProperty]
		private string confirmPassword = default!;

		[ObservableProperty]
		private string telefoonnummer = default!;

		[ObservableProperty]
		private string registrationMessage = default!;

		[ObservableProperty]
		private string errorMessage = default!;

		[ObservableProperty]
		private bool displayButton;

		[ObservableProperty]
		private bool isBusy;

		public RegisterViewModel(ApiService apiService) : base(apiService)
		{
			_apiService = apiService;
			displayButton = true;
			isBusy = false;
		}

		[RelayCommand]
		public async Task RegisterAsync()
		{
			DisplayButton = false;
			IsBusy = true;
			ErrorMessage = string.Empty;

			var (isSuccess, errorMessage) = await _apiService.RegisterUserAsync(Email, Password, ConfirmPassword, Voornaam, Achternaam, Telefoonnummer);

			if (isSuccess)
			{
				RegistrationMessage = "Registratie succesvol! U wordt omgeleid...";
				NavigateWithDelay();
			}
			else
			{
				var errorMessages = errorMessage?.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)
									 .Select(s => s.Trim() + ".")
									 .ToList();

				var formattedErrorMessage = string.Join("\n", errorMessages!);
				ErrorMessage = formattedErrorMessage;
				DisplayButton = true;
				IsBusy = false;
			}
		}

		private async void NavigateWithDelay()
		{
			DisplayButton = true;
			IsBusy = false;

			// Wacht bijvoorbeeld 2 seconden (2000 milliseconden)
			await Task.Delay(2000);

			// Navigeer naar een andere pagina
			await Shell.Current.GoToAsync("//LoginPage");
		}
	}
}