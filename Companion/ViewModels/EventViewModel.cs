using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Companion.Models;
using Companion.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Companion.ViewModels
{
    public partial class EventViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        public string naam = default!;

        [ObservableProperty]
        private DateTime datumStart;

        [ObservableProperty]
        private DateTime datumEinde;

        [ObservableProperty]
        public int aantalDeelnemers;

        [ObservableProperty]
        public int maxDeelnemers;

        [ObservableProperty]
        public bool isUserRegistered;

        [ObservableProperty]
        public string beschrijving = default!;

        [ObservableProperty]
        public bool isJoinButtonVisible;

        [ObservableProperty]
        public bool isLeaveButtonVisible;

        [ObservableProperty]
        private ObservableCollection<Evenement> events;

        [ObservableProperty]
        private ObservableCollection<Community>? communities;

        private Community? selectedCommunity;
        public Community? SelectedCommunity
        {
            get => selectedCommunity;
            set
            {
                if (SetProperty(ref selectedCommunity, value))
                {
                    // Asynchronously reload events when a new community is selected
                    LoadEventsForSelectedCommunity();
                }
            }
        }

        private async void LoadEventsForSelectedCommunity()
        {
            try
            {
                var loadedEvents = SelectedCommunity != null
                    ? await _apiService.GetEventsByCommunityAsync(SelectedCommunity.Id)
                    : await _apiService.GetAllEventsAsync();

                if (Application.Current.Dispatcher != null)
                {
                    await Application.Current.Dispatcher.DispatchAsync(async () => // Zorg ervoor dat dit asynchroon is
                    {
                        Events.Clear();
                        foreach (var eventItem in loadedEvents)
                        {
                            eventItem.IsUserRegistered = await _apiService.IsUserRegisteredForEventAsync(eventItem.Id); // Maak dit asynchroon
                            Events.Add(eventItem);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load events: {ex}");
            }
        }

        private async void LoadEventsAsync()
        {
            await LoadEvents();
        }

        public async Task InitializeAsync()
        {
            await LoadEvents();
            await LoadCommunities();
        }

        public EventViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            Events = new ObservableCollection<Evenement>();
            Communities = new ObservableCollection<Community>(); // Zorg ervoor dat dit vóór de methodes staat die het gebruiken
        }

        [RelayCommand]
        private async Task LoadCommunities()
        {
            var communityList = await _apiService.GetAllCommunitiesAsync();
            if (communityList != null && communityList.Count > 0)
            {
                Communities.Clear();
                foreach (var community in communityList)
                {
                    Communities.Add(community);
                }
            }
            else
            {
                Debug.WriteLine("No communities loaded or list is empty.");
            }
        }

        [RelayCommand]
        public async Task LoadEvents()
        {
            try
            {
                List<Evenement> loadedEvents;
                if (SelectedCommunity != null)
                {
                    loadedEvents = await _apiService.GetEventsByCommunityAsync(SelectedCommunity.Id);
                }
                else
                {
                    loadedEvents = await _apiService.GetAllEventsAsync();
                }

                /*Debug.WriteLine($"Geladen events: {JsonConvert.SerializeObject(loadedEvents)}");*/
                Debug.WriteLine(loadedEvents.Count);
                if (loadedEvents.Count > 0)
                {
                    Events.Clear();
                    foreach (var eventItem in loadedEvents)
                    {
                        eventItem.IsUserRegistered = await _apiService.IsUserRegisteredForEventAsync(eventItem.Id);
                        Events.Add(eventItem);
                    }
                }
                else
                {
                    Debug.WriteLine("Geen events geladen of de lijst is leeg.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het laden van evenementen: {ex.Message}");
            }
        }

        [RelayCommand]
        public void SelectCommunity()
        {
            LoadEventsAsync();
        }

        [RelayCommand]
        public async Task JoinEvent(Evenement eventItem)
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (eventItem == null || string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("Event item is null or User ID is not available.");
                return;
            }

            var model = new
            {
                GebruikerId = userId,
                Evenement = eventItem.Id,
                AantalPersonen = 1
            };
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiService.RegisterUserForEventAsync(content);

            if (response.IsSuccessStatusCode)
            {
                eventItem.AantalDeelnemers += 1;
                eventItem.IsUserRegistered = true; // Dit triggert ook IsJoinButtonVisible en IsLeaveButtonVisible updates
                Debug.WriteLine("User successfully registered for the event.");
            }
            else
            {
                Debug.WriteLine($"Failed to register user for the event: {response.StatusCode}");
            }
        }

        [RelayCommand]
        public async Task LeaveEvent(Evenement eventItem)
        {
            var userId = await SecureStorage.GetAsync("user_id");
            if (eventItem == null || string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("Event item is null or User ID is not available.");
                return;
            }

            var response = await _apiService.UnregisterUserFromEventAsync(userId, eventItem.Id);

            if (response.IsSuccessStatusCode)
            {
                eventItem.AantalDeelnemers -= 1;
                eventItem.IsUserRegistered = false; // Dit triggert ook IsJoinButtonVisible en IsLeaveButtonVisible updates
                Debug.WriteLine("User successfully unregistered from the event.");
            }
            else
            {
                Debug.WriteLine($"Failed to unregister user from the event: {response.StatusCode}");
            }
        }

        [RelayCommand]
        public async Task JoinOrLeaveEvent(Evenement eventItem)
        {
            if (eventItem.IsUserRegistered)
            {
                await LeaveEvent(eventItem);
            }
            else
            {
                await JoinEvent(eventItem);
            }
        }
    }
}