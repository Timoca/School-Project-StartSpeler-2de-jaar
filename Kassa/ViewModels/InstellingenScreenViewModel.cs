using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kassa.ViewModels
{
    public partial class InstellingenScreenViewModel : BaseViewModel
    {
        private const string MasterAccountEmail = "admin@admin.be"; // Vervang dit door het emailadres van het master account

        [ObservableProperty]
        private List<Rechten> rechten = default!;

        [ObservableProperty]
        private List<Gebruiker> gebruiker = default!;

        [ObservableProperty]
        private string id = default!;

        [ObservableProperty]
        private string voornaam = default!;

        [ObservableProperty]
        private string achternaam = default!;

        [ObservableProperty]
        private string email = default!;

        [ObservableProperty]
        private string rolesAsString = default!;

        [ObservableProperty]
        private string rechtNaam = default!;

        [ObservableProperty]
        private ObservableCollection<Gebruiker> searchResults = new();

        [ObservableProperty]
        private string searchQuery = default!;

        [ObservableProperty]
        private string selectedUserId = default!;

        [ObservableProperty]
        private bool isRoleSelectionVisible = false;

        [ObservableProperty]
        private List<string> availableRoles = [];

        [ObservableProperty]
        private ObservableCollection<object> selectedRoles = new ObservableCollection<object>();

        [ObservableProperty]
        private string searchMessage = default!;

        private readonly ApiService _apiService;

        public InstellingenScreenViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            AvailableRoles = new List<string> { "Beheerder", "Kelner", "Event manager" };
        }

        [RelayCommand]
        private void ShowRoleSelection(string userId)
        {
            Debug.WriteLine($"ShowRoleSelection: {userId}");
            var selectedUser = SearchResults.FirstOrDefault(u => u.Id == userId);
            if (selectedUser != null)
            {
                SelectedUserId = selectedUser.Id;
                Voornaam = selectedUser.Voornaam;
                Achternaam = selectedUser.Achternaam;
                Email = selectedUser.Email;

                // Set the SelectedRoles to the user's current roles
                SelectedRoles.Clear();
                foreach (var role in selectedUser.CurrentRoles)
                {
                    SelectedRoles.Add(role);
                }
            }
            IsRoleSelectionVisible = true;
            Debug.WriteLine($"IsroleSelectionVisible: {IsRoleSelectionVisible}");
        }

        [RelayCommand]
        private void HideRoleSelection()
        {
            IsRoleSelectionVisible = false;
        }

        [RelayCommand]
        private async Task SaveRolesAsync()
        {
            Debug.WriteLine("SaveRolesAsync");
            Debug.WriteLine($"SelectedUserId: {SelectedUserId}");
            Debug.WriteLine($"SelectedRoles: {string.Join(", ", SelectedRoles.Cast<string>())}");
            if (string.IsNullOrEmpty(SelectedUserId) || SelectedRoles.Count == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Select a user and at least one role.", "OK");
                return;
            }

            var result = await _apiService.SetUserRolesAsync(SelectedUserId, SelectedRoles.Cast<string>().ToList());
            if (!result.IsSuccess)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.ErrorMessage, "OK");
            }
            else
            {
                // Call SearchUsersAsync to refresh the list
                await SearchUsersAsync();
            }

            HideRoleSelection();
        }

        [RelayCommand]
        private async Task SearchUsersAsync()
        {
            if (string.IsNullOrEmpty(SearchQuery))
            {
                return;
            }

            var results = await _apiService.SearchUsersAsync(SearchQuery);
            if (results != null && results.Any())
            {
                // Filter out the master account (om een nieuwe masteraccount in te stellen even deze lijn weg commenten en in settings rechten geven.)
                results = results.Where(u => u.Email != MasterAccountEmail).ToList();

                // Set the CurrentRoles for each user
                foreach (var gebruiker in results)
                {
                    var rolesResult = await _apiService.GetUserRolesAsync(gebruiker.Id);
                    if (rolesResult.IsSuccess)
                    {
                        Debug.WriteLine($"CurrentRoles: {string.Join(", ", rolesResult.Roles)}");
                        gebruiker.CurrentRoles = rolesResult.Roles;
                    }
                }
                SearchResults.Clear();
                foreach (var user in results)
                {
                    SearchResults.Add(user);
                }
                SearchMessage = string.Empty; // Clear the message if results are found
            }
            else
            {
                SearchResults.Clear();
                SearchMessage = "Geen resultaten gevonden.";
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync(string userId)
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert("Confirm", "Bent u zeker dat u deze gebruiker wil verwijderen?", "Ja", "Nee");
            if (!confirm)
            {
                return;
            }

            var result = await _apiService.DeleteUserAsync(userId);
            if (!result.IsSuccess)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", result.ErrorMessage, "OK");
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Success", "De gebruiker is verwijderd.", "OK");

                // Refresh the user list after deletion
                await SearchUsersAsync();
            }
        }
    }
}