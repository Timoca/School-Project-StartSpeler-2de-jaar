using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Services;
using Kassa.Views;
using System.Diagnostics;

namespace Kassa.ViewModels
{
    public partial class AppShellViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private bool removeLogoutButton;

        public bool IsNotEventManager => !GlobalSettings.IsEventManager;

        public AppShellViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            GlobalSettings.PropertyChanged += OnGlobalSettingsChanged;
        }

        private void OnGlobalSettingsChanged(string propertyName)
        {
            if (propertyName == nameof(GlobalSettings.IsEventManager))
            {
                OnPropertyChanged(nameof(IsNotEventManager));
            }
        }

        [RelayCommand]
        public async Task LogoutUser()
        {
            bool isLoggedOut = await _apiService.LogoutAsync();
            if (isLoggedOut)
            {
                try
                {
                    await CheckUserRole();
                    await Shell.Current.GoToAsync("//LoginPage");
                    CheckForLogoutButtonVisibility();
                }
                catch (Exception ex)
                {
                    // Log de fout of toon een bericht
                    Debug.WriteLine($"Navigatiefout: {ex.Message}");
                }
            }
        }

        // Deze methode wordt aangeroepen wanneer de navigatie is voltooid
        private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            UpdateLogoutButtonVisibility();
        }

        // Registreer de navigatie events
        public void RegisterNavigationEvents()
        {
            Shell.Current.Navigated += OnNavigated;
        }

        public void CheckForLogoutButtonVisibility()
        {
            UpdateLogoutButtonVisibility();
        }

        // Deze methode wordt aangeroepen wanneer de logout knop onzichtbaar moet worden
        [RelayCommand]
        private void UpdateLogoutButtonVisibility()
        {
            var currentPage = Shell.Current?.CurrentPage;
            RemoveLogoutButton = currentPage is LoginPage ||
                               currentPage is RegisterPage;

            // Als de huidige pagina een van de bovenstaande pagina's is, verberg de knop
            RemoveLogoutButton = !RemoveLogoutButton;

            // Roep OnPropertyChanged aan om de UI te laten weten dat de waarde is gewijzigd
            OnPropertyChanged(nameof(RemoveLogoutButton));
        }
    }
}