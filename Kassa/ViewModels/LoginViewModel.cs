using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Services;
using System.ComponentModel;
using System.Diagnostics;

namespace Kassa.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly AppShellViewModel _appShellViewModel;

        [ObservableProperty]
        private string email = default!;

        [ObservableProperty]
        private string password = default!;

        [ObservableProperty]
        private bool displayButton;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = default!;

        public LoginViewModel(ApiService apiService, AppShellViewModel appShellViewModel) : base(apiService)
        {
            _apiService = apiService;
            _appShellViewModel = appShellViewModel;
            displayButton = true;
            IsBusy = false;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            DisplayButton = false;
            IsBusy = true;

            var token = await _apiService.LoginAsync(Email, Password);
            if (!string.IsNullOrEmpty(token))
            {
                // login was succesvol, sla het token op voor toekomstig gebruik
                await SecureStorage.Default.SetAsync("jwt_token", token);

                IsBusy = false;

                ClearErrorMessage();

                // Initialiseer AppShellViewModel en stel de MainPage opnieuw in
                await CheckUserRole();
                Application.Current!.MainPage = new AppShell(_appShellViewModel);

                // Controleer de zichtbaarheid van de logout-knop
                _appShellViewModel.CheckForLogoutButtonVisibility();

                //Stuur de gebruiker door naar het main menu
                await Shell.Current.GoToAsync("//HomeScreen");
            }
            else
            {
                DisplayButton = true;
                IsBusy = false;

                ErrorMessage = "Ongeldige gebruikersnaam of wachtwoord.";
            }
        }

        [RelayCommand]
        public async Task NavigateToRegister()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }

        public void ClearErrorMessage()
        {
            ErrorMessage = "";
        }

        public async Task ResetLogginUI()
        {
            bool isAuthenticated = await _apiService.IsUserLoggedInAsync();
            if (!isAuthenticated)
            {
                Email = string.Empty;
                Password = string.Empty;
                DisplayButton = true;
                ErrorMessage = string.Empty;
            }
        }
    }
}