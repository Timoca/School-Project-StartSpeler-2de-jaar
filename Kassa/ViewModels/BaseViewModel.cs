using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Services;
using Kassa.ViewModels;
using System.ComponentModel;
using System.Diagnostics;

namespace Kassa.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string title = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty]
        private bool isUserLoggedIn;

        [ObservableProperty]
        private bool isBeheerder;

        [ObservableProperty]
        private bool isEventManager;

        [ObservableProperty]
        private bool isKelner;

        public bool IsNotBusy => !IsBusy;

        public BaseViewModel(ApiService apiService)
        {
            _apiService = apiService;
            GlobalSettings.PropertyChanged += GlobalSettings_PropertyChanged;
            Initialize();
        }

        private async void Initialize()
        {
            await CheckUserRole();
        }

        protected async Task CheckUserRole()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            var result = await _apiService.GetUserRolesAsync(userId!);
            if (result.IsSuccess)
            {
                GlobalSettings.UserRoles = result.Roles;
                IsBeheerder = GlobalSettings.IsBeheerder;
                IsEventManager = GlobalSettings.IsEventManager;
                IsKelner = GlobalSettings.IsKelner;

                OnPropertyChanged(nameof(IsBeheerder));
                OnPropertyChanged(nameof(IsEventManager));
                OnPropertyChanged(nameof(IsKelner));

                Debug.WriteLine($"Gebruikersrollen: Beheerder = {IsBeheerder}, Event Manager = {IsEventManager}, Kelner = {IsKelner}");
            }
            else
            {
                Debug.WriteLine("Fout bij het ophalen van de gebruikersrollen");
            }
        }

        private void GlobalSettings_PropertyChanged(string propertyName)
        {
            if (propertyName == nameof(GlobalSettings.IsBeheerder))
            {
                IsBeheerder = GlobalSettings.IsBeheerder;
                OnPropertyChanged(nameof(IsBeheerder));
            }
            if (propertyName == nameof(GlobalSettings.IsEventManager))
            {
                IsEventManager = GlobalSettings.IsEventManager;
                OnPropertyChanged(nameof(IsEventManager));
            }
            if (propertyName == nameof(GlobalSettings.IsKelner))
            {
                IsKelner = GlobalSettings.IsKelner;
                OnPropertyChanged(nameof(IsKelner));
            }
        }

        [RelayCommand]
        public static async Task GoToProductScreenAsync()
        {
            await Shell.Current.GoToAsync("//ProductScreen");
        }

        [RelayCommand]
        public static async Task GoToMenuScreenAsync()
        {
            await Shell.Current.GoToAsync("//MenuScreen");
        }

        [RelayCommand]
        public static async Task GoToEventScreenAsync()
        {
            await Shell.Current.GoToAsync("//EventScreen");
        }

        [RelayCommand]
        public static async Task GoToAfrekenenScreenAsync()
        {
            await Shell.Current.GoToAsync("//AfrekenenScreen");
        }

        [RelayCommand]
        public static async Task GoToInstellingenScreenAsync()
        {
            await Shell.Current.GoToAsync("//InstellingenScreen");
        }

        [RelayCommand]
        public static async Task GoToOverzichtScreenAsync()
        {
            await Shell.Current.GoToAsync("//OverzichtScreen");
        }

        [RelayCommand]
        public static async Task GoToStockScreenAsync()
        {
            await Shell.Current.GoToAsync("//StockScreen");
        }

        [RelayCommand]
        public static async Task GoToLoginScreenAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public async Task<bool> CheckAuthentication()
        {
            return await _apiService.IsUserLoggedInAsync();
        }
    }
}