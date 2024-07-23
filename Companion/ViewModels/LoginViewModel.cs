using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Companion.Services;
using Companion.Views;

namespace Companion.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string email = default!;

        [ObservableProperty]
        private string password = default!;

        [ObservableProperty]
        private bool displayButton;

        [ObservableProperty]
        private bool displayError;

        public LoginViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            displayButton = true;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            DisplayButton = false;
            DisplayError = false;

            var token = await _apiService.LoginAsync(Email, Password);
            if (!string.IsNullOrEmpty(token))
            {
                // Sla het token op voor toekomstig gebruik
                await SecureStorage.SetAsync("jwt_token", token);

                //Stuur de gebruiker door naar het main menu
                await Shell.Current.GoToAsync("///HomePagina");
            }
            else
            {
                DisplayError = true;
                DisplayButton = true;
            }
        }

        [RelayCommand]
        public async Task NavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegistratiePagina));
        }

        public async Task ResetLogginUI()
        {
            bool isAuthenticated = await _apiService.IsUserLoggedInAsync();
            if (!isAuthenticated)
            {
                Email = string.Empty;
                Password = string.Empty;
                DisplayError = false;
            }
        }
    }
}