using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.ViewModels
{
    public partial class EventScreenViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        private ObservableCollection<Evenement> events = new ObservableCollection<Evenement>();

        public ObservableCollection<Evenement> Events
        {
            get => events;
            set
            {
                SetProperty(ref events, value);
                Debug.WriteLine($"Events updated. Count: {events.Count}");
            }
        }

        [ObservableProperty]
        public Evenement selectedEvent;

        [ObservableProperty]
        public string naam = default!;

        [ObservableProperty]
        private DateTime datumStart;

        [ObservableProperty]
        private DateTime datumEinde;

        [ObservableProperty]
        public int aantalDeelnemers;

        [ObservableProperty]
        public float kosten;

        [ObservableProperty]
        public int maxDeelnemers;

        [ObservableProperty]
        public bool isUserRegistered;

        [ObservableProperty]
        public string beschrijving = default!;

        [ObservableProperty]
        public bool isLeaveButtonVisible;

        [ObservableProperty]
        public bool isJoinButtonVisible;

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
                    LoadEventsForSelectedCommunity();
                }
            }
        }

        public bool IsNotKelner => !GlobalSettings.IsKelner;

        public EventScreenViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Events = new ObservableCollection<Evenement>();
            Communities = new ObservableCollection<Community>();

            GlobalSettings.PropertyChanged += OnGlobalSettingsChanged;
        }

        private void OnGlobalSettingsChanged(string propertyName)
        {
            if (propertyName == nameof(GlobalSettings.IsKelner))
            {
                OnPropertyChanged(nameof(IsNotKelner));
            }
        }

        public async Task InitializeAsync()
        {
            await LoadCommunities();
            await LoadEvents();
        }

        private async void LoadEventsAsync()
        {
            await LoadEvents();
        }

        [RelayCommand]
        public void SelectCommunity()
        {
            LoadEventsAsync();
        }

        [RelayCommand]
        private async Task LoadCommunities()
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load communities: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadEvents()
        {
            await LoadEventsForSelectedCommunity();
        }

        private async Task LoadEventsForSelectedCommunity()
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

                if (loadedEvents.Count > 0)
                {
                    Events.Clear();
                    foreach (var eventItem in loadedEvents)
                    {
                        eventItem.IsUserRegistered = await _apiService.IsUserRegisteredForEventAsync(eventItem.Id);
                        Naam = eventItem.Naam;
                        Events.Add(eventItem);
                        Debug.WriteLine($"Event: {eventItem.Naam}, IsUserRegistered: {eventItem.IsUserRegistered}, aantal: {Events.Count}");
                    }
                }
                Debug.WriteLine($"Events loaded. {Events.Count}");
                foreach (var item in Events)
                {
                    Debug.WriteLine(item.Naam);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load events: {ex}");
            }
        }

        [RelayCommand]
        public async Task CreateCommunity()
        {
            List<string> existingCommunityNames = new List<string>();
            foreach (var item in Communities!)
            {
                var communityNaam = item.Naam;
                existingCommunityNames.Add(communityNaam.ToLower());
            }
            string communityName = await Application.Current!.MainPage!.DisplayPromptAsync("Nieuwe Community", "Voer de naam van de community in", "OK", "Annuleren", placeholder: "Community naam");

            if (communityName == null) return;
            if (string.IsNullOrWhiteSpace(communityName))
            {
                await Application.Current!.MainPage!.DisplayAlert("Ongeldige invoer", "De community naam moet ingevuld zijn.", "OK");
                return;
            }
            communityName = string.Join(" ", communityName.Split(' ').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
            if (existingCommunityNames.Contains(communityName.ToLower()))
            {
                await Application.Current!.MainPage!.DisplayAlert("Ongeldige invoer", "Deze community bestaat al.", "OK");
                return;
            }

            var newCommunity = new Community
            {
                Naam = communityName,
            };

            bool success = await _apiService.CreateCommunityAsync(newCommunity);

            if (success)
            {
                await LoadCommunities();
            }
            else
            {
                Debug.WriteLine("Kon de community niet aanmaken.");
            }
        }

        [RelayCommand]
        public async Task CreateEvent()
        {
            try
            {
                string eventName = await Application.Current!.MainPage!.DisplayPromptAsync("Nieuw Evenement", "Voer de naam van het evenement in", "OK", "Annuleren", placeholder: "Evenement naam");
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Naam van het evenement mag niet leeg zijn.", "OK");
                    return;
                }

                string eventDescription = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer de beschrijving van het evenement in", "OK", "Annuleren", placeholder: "Evenement Beschrijving");

                string eventPrijsString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer de nieuwe prijs van het evenement in", "OK", "Annuleren", placeholder: "Nieuwe prijs");

                if (eventPrijsString == null) return;

                if (!float.TryParse(eventPrijsString, out float eventPrijs) || eventPrijs < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige prijs. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }

                string startDatumString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer de nieuwe startdatum van het evenement in (dd/MM/yyyy)", "OK", "Annuleren", placeholder: "Nieuwe startdatum (DD/MM/JJJJ)");

                if (!DateTime.TryParseExact(startDatumString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDatum))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige datumindeling. Voer de datum in het formaat DD/MM/JJJJ in.", "OK");
                    return;
                }
                if (startDatum <= DateTime.Now.Date)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De startdatum moet in de toekomst liggen.", "OK");
                    return;
                }

                string eindDatumString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer de nieuwe einddatum van het evenement in (dd/MM/yyyy)", "OK", "Annuleren", placeholder: "Nieuwe einddatum (DD/MM/JJJJ)");

                if (!DateTime.TryParseExact(eindDatumString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eindDatum))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige datumindeling. Voer de datum in het formaat DD/MM/JJJJ in.", "OK");
                    return;
                }
                if (eindDatum <= DateTime.Now.Date)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De einddatum moet in de toekomst liggen.", "OK");
                    return;
                }

                if (eindDatum <= startDatum)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De einddatum moet na de startdatum liggen.", "OK");
                    return;
                }

                string maxDeelnemersString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer het maximaal aantal spelers in", "OK", "Annuleren", placeholder: "Max spelers");

                if (maxDeelnemersString == null) return;

                if (!int.TryParse(maxDeelnemersString, out int maxDeelnemers) || maxDeelnemers < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldig maximum aantal deelnemers. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }

                string aantalDeelnemersString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Evenement", "Voer het nieuwe aantal deelnemers in", "OK", "Annuleren", placeholder: "Nieuw aantal deelnemers");

                if (aantalDeelnemersString == null) return;

                if (!int.TryParse(aantalDeelnemersString, out int aantalDeelnemers) || aantalDeelnemers < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldig aantal deelnemers. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }
                var communities = await _apiService.GetAllCommunitiesAsync();

                // Stap 2: Converteer de lijst van communities naar een array van strings
                var communityNames = communities.Select(c => c.Naam).ToArray();

                // Stap 3: Gebruik DisplayActionSheet om de lijst van communities aan de gebruiker te tonen
                string selectedCommunityName = await Application.Current.MainPage.DisplayActionSheet("Selecteer een community", "Annuleren", null, communityNames);

                // Stap 4: Controleer of de gebruiker op 'Annuleren' heeft geklikt
                if (selectedCommunityName == "Annuleren" || selectedCommunityName == null)
                {
                    // De gebruiker heeft op 'Annuleren' geklikt of geen selectie gemaakt
                    return;
                }

                // Zoek de geselecteerde community op basis van de naam
                var selectedCommunity = communities.FirstOrDefault(c => c.Naam == selectedCommunityName);

                if (selectedCommunity != null)
                {
                    // Gebruik de geselecteerde community voor je verdere logica
                    // Bijvoorbeeld: sla de geselecteerde community op in een ViewModel-eigenschap
                    SelectedCommunity = selectedCommunity;
                }

                int nieuweEventId = Events.Any() ? Events.Max(e => e.Id) + 1 : 1;

                var newEvent = new Evenement
                {
                    Id = nieuweEventId,
                    Naam = eventName,
                    Beschrijving = eventDescription,
                    DatumStart = startDatum,
                    DatumEinde = eindDatum,
                    AantalDeelnemers = aantalDeelnemers,
                    MaxDeelnemers = maxDeelnemers,
                    CommunityId = SelectedCommunity.Id,
                    Kosten = eventPrijs,
                };

                bool success = await _apiService.CreateEventAsync(newEvent);

                if (success)
                {
                    await LoadEvents();
                    selectedCommunity = null;
                }
                else
                {
                    Debug.WriteLine("Kon het evenement niet aanmaken.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het aanmaken van het evenement: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteEvent()
        {
            if (SelectedEvent != null)
            {
                bool answer = await Application.Current!.MainPage!.DisplayAlert("Bevestigen", $"Weet u zeker dat u {SelectedEvent.Naam} wilt verwijderen?", "Ja", "Nee");
                if (answer)
                {
                    var result = await _apiService.DeleteEventAsync(SelectedEvent.Id);
                    if (result)
                    {
                        await LoadEvents();
                        SelectedEvent = new Evenement();
                    }
                }
            }
        }

        [RelayCommand]
        public async Task UpdateEvent()
        {
            try
            {
                if (SelectedEvent == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Geen geselecteerd evenement om bij te werken.", "OK");
                    return;
                }

                string eventName = await Application.Current!.MainPage!.DisplayPromptAsync("Update Evenement", "Voer de naam van het evenement in", "OK", "Annuleren", $"{SelectedEvent.Naam}");
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Naam van het evenement mag niet leeg zijn.", "OK");
                    return;
                }

                string eventDescription = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer de beschrijving van het evenement in", "OK", "Annuleren", $"{SelectedEvent.Beschrijving}");

                string eventPrijsString = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer de nieuwe prijs van het evenement in", "OK", "Annuleren", $"{SelectedEvent.Kosten}");

                if (eventPrijsString == null) return;

                if (!float.TryParse(eventPrijsString, out float eventPrijs) || eventPrijs < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige prijs. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }

                string startDatumString = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer de nieuwe startdatum van het evenement in (dd/MM/yyyy)", "OK", "Annuleren", $"{SelectedEvent.DatumStart}");

                if (!DateTime.TryParseExact(startDatumString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDatum))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige datumindeling. Voer de datum in het formaat DD/MM/JJJJ in.", "OK");
                    return;
                }
                if (startDatum <= DateTime.Now.Date)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De startdatum moet in de toekomst liggen.", "OK");
                    return;
                }

                string eindDatumString = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer de nieuwe einddatum van het evenement in (dd/MM/yyyy)", "OK", "Annuleren", $"{SelectedEvent.DatumEinde}");

                if (!DateTime.TryParseExact(eindDatumString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eindDatum))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige datumindeling. Voer de datum in het formaat DD/MM/JJJJ in.", "OK");
                    return;
                }
                if (eindDatum <= DateTime.Now.Date)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De einddatum moet in de toekomst liggen.", "OK");
                    return;
                }

                if (eindDatum <= startDatum)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "De einddatum moet na de startdatum liggen.", "OK");
                    return;
                }

                string maxDeelnemersString = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer het maximaal aantal spelers in", "OK", "Annuleren", $"{SelectedEvent.MaxDeelnemers}");

                if (maxDeelnemersString == null) return;

                if (!int.TryParse(maxDeelnemersString, out int maxDeelnemers) || maxDeelnemers < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldig maximum aantal deelnemers. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }

                string aantalDeelnemersString = await Application.Current.MainPage.DisplayPromptAsync("Update Evenement", "Voer het nieuwe aantal deelnemers in", "OK", "Annuleren", $"{SelectedEvent.AantalDeelnemers}");

                if (aantalDeelnemersString == null) return;

                if (!int.TryParse(aantalDeelnemersString, out int aantalDeelnemers) || aantalDeelnemers < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldig aantal deelnemers. Voer een geldige numerieke waarde in groter of gelijk aan 0.", "OK");
                    return;
                }
                var communities = await _apiService.GetAllCommunitiesAsync();

                // Stap 2: Converteer de lijst van communities naar een array van strings
                var communityNames = communities.Select(c => c.Naam).ToArray();

                // Stap 3: Gebruik DisplayActionSheet om de lijst van communities aan de gebruiker te tonen
                string selectedCommunityName = await Application.Current.MainPage.DisplayActionSheet("Selecteer een community", "Annuleren", null, communityNames);

                // Stap 4: Controleer of de gebruiker op 'Annuleren' heeft geklikt
                if (selectedCommunityName == "Annuleren" || selectedCommunityName == null)
                {
                    // De gebruiker heeft op 'Annuleren' geklikt of geen selectie gemaakt
                    return;
                }

                // Zoek de geselecteerde community op basis van de naam
                var selectedCommunity = communities.FirstOrDefault(c => c.Naam == selectedCommunityName);

                if (selectedCommunity != null)
                {
                    // Gebruik de geselecteerde community voor je verdere logica
                    // Bijvoorbeeld: sla de geselecteerde community op in een ViewModel-eigenschap
                    SelectedCommunity = selectedCommunity;
                }

                // Update de eigenschappen van het geselecteerde evenement
                SelectedEvent.Naam = eventName;
                SelectedEvent.Beschrijving = eventDescription;
                SelectedEvent.Kosten = eventPrijs;
                SelectedEvent.DatumStart = startDatum;
                SelectedEvent.DatumEinde = eindDatum;
                SelectedEvent.MaxDeelnemers = maxDeelnemers;
                SelectedEvent.AantalDeelnemers = aantalDeelnemers;
                SelectedEvent.CommunityId = SelectedCommunity.Id;

                // Roep de methode aan om het evenement bij te werken via de API-service
                bool success = await _apiService.UpdateEventAsync(SelectedEvent.Id, SelectedEvent);

                if (success)
                {
                    // Evenement succesvol bijgewerkt, laad de evenementen opnieuw in

                    await LoadEvents();
                    selectedCommunity = null;
                    SelectedEvent = new Evenement();
                    await Application.Current.MainPage.DisplayAlert("Succes", "Het evenement is succesvol bijgewerkt.", "OK");
                }
                else
                {
                    // Kon het evenement niet bijwerken, behandel dit geval
                    await Application.Current.MainPage.DisplayAlert("Fout", "Het bijwerken van het evenement is mislukt.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het bijwerken van het evenement: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task JoinEvent(Evenement eventItem)
        {
            if (eventItem == null)
            {
                Debug.WriteLine("Event item is null.");
                return;
            }

            var userId = await SecureStorage.GetAsync("user_id");
            Debug.WriteLine($"User ID: {userId}");
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("User ID is not available.");
                return;
            }

            try
            {
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
                    await LoadEvents();
                    Debug.WriteLine("User successfully registered for the event.");
                }
                else
                {
                    Debug.WriteLine($"Failed to register user for the event: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in JoinEvent: {ex.Message}");
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

        [RelayCommand]
        public async Task DeleteCommunity()
        {
            var communities = await _apiService.GetAllCommunitiesAsync();

            // Stap 2: Converteer de lijst van communities naar een array van strings
            var communityNames = communities.Select(c => c.Naam).ToArray();

            // Stap 3: Gebruik DisplayActionSheet om de lijst van communities aan de gebruiker te tonen
            string selectedCommunityName = await Application.Current.MainPage.DisplayActionSheet("Selecteer een community", "Annuleren", null, communityNames);

            // Stap 4: Controleer of de gebruiker op 'Annuleren' heeft geklikt
            if (selectedCommunityName == "Annuleren" || selectedCommunityName == null)
            {
                // De gebruiker heeft op 'Annuleren' geklikt of geen selectie gemaakt
                return;
            }

            // Zoek de geselecteerde community op basis van de naam
            var selectedCommunity = communities.FirstOrDefault(c => c.Naam == selectedCommunityName);

            if (selectedCommunity != null)
            {
                // Gebruik de geselecteerde community voor je verdere logica
                // Bijvoorbeeld: sla de geselecteerde community op in een ViewModel-eigenschap
                SelectedCommunity = selectedCommunity;
            }

            if (SelectedCommunity != null)
            {
                bool answer = await Application.Current!.MainPage!.DisplayAlert("Bevestigen", $"Weet u zeker dat u {SelectedCommunity.Naam} wilt verwijderen?", "Ja", "Nee");
                if (answer)
                {
                    var result = await _apiService.DeleteCommunityAsync(SelectedCommunity.Id);
                    if (result)
                    {
                        await LoadCommunities();
                        SelectedCommunity = new Community();
                    }
                }
            }
        }
    }
}