using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Kassa.ViewModels
{
    public partial class OverzichtScreenViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        public ObservableCollection<Bestelling> bestelling = default!;

        [ObservableProperty]
        public string gebruikerId = default!;

        [ObservableProperty]
        public ObservableCollection<Gebruiker> gebruiker = default!;

        [ObservableProperty]
        public string naam = default!;

        [ObservableProperty]
        public int aantal;

        [ObservableProperty]
        public float totaalBedrag = default!;

        [ObservableProperty]
        public bool betaalStatus = default!;

        [ObservableProperty]
        private int tafelNummer = default!;

        [ObservableProperty]
        public string betaalStatusString = default!;

        [ObservableProperty]
        public List<BesteldProduct> besteldeProducten = default!;

        [ObservableProperty]
        public List<Product> producten = default!;

        [ObservableProperty]
        public Bestelling huidigeBestelling = default!;

        public bool IsNotEventManager => !GlobalSettings.IsEventManager;

        public async Task InitializeAsync()
        {
            await LoadBestellingen();
        }

        public OverzichtScreenViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            Bestelling = new ObservableCollection<Bestelling>();
            Gebruiker = new ObservableCollection<Gebruiker>();
            BesteldeProducten = new List<BesteldProduct>();
            Producten = new List<Product>();
            HuidigeBestelling = new Bestelling();

            GlobalSettings.PropertyChanged += OnGlobalSettingsChanged;
        }

        private void OnGlobalSettingsChanged(string propertyName)
        {
            if (propertyName == nameof(GlobalSettings.IsEventManager))
            {
                OnPropertyChanged(nameof(IsNotEventManager));
            }
        }

        // Update je LoadBestellingen methode om de lijst van BesteldeProducten correct te laden
        public async Task LoadBestellingen()
        {
            try
            {
                // Load all bestellingen, gebruikers, bestelde producten, and producten asynchronously
                var loadedBestellingen = await _apiService.GetAllBestellingenAsync();
                var loadedGebruikers = await _apiService.GetAllGebruikersAsync();
                var loadedBesteldeProducten = await _apiService.GetAllBesteldeProductenAsync();
                var loadedProducten = await _apiService.GetAllProductenAsync();

                // Initialize the collections
                Bestelling = new ObservableCollection<Bestelling>(loadedBestellingen);
                Gebruiker = new ObservableCollection<Gebruiker>(loadedGebruikers);
                Producten = new List<Product>(loadedProducten);

                foreach (var bestelling in Bestelling)
                {
                    // Find the user associated with the bestelling
                    var gebruiker = Gebruiker.FirstOrDefault(g => g.Id == bestelling.GebruikerId);

                    if (gebruiker != null)
                    {
                        bestelling.Gebruiker = gebruiker;
                        // Add the gebruiker to the Gebruiker collection
                        bestelling.VolledigeNaam = gebruiker.Voornaam + " " + gebruiker.Achternaam;
                        TafelNummer = bestelling.TafelNummer;
                        Gebruiker.Add(gebruiker);
                    }
                    else
                    {
                        Console.WriteLine("Gebruiker niet gevonden");
                        // Add an empty gebruiker if none is found
                        bestelling.Gebruiker = new Gebruiker();
                    }

                    // Initialize the BesteldeProducten collection for this bestelling
                    bestelling.BesteldeProducten = new List<BesteldProduct>(
                        loadedBesteldeProducten.Where(bp => bp.BestellingId == bestelling.Id)
                    );

                    foreach (var besteldProduct in bestelling.BesteldeProducten)
                    {
                        // Find the corresponding product
                        var product = Producten.FirstOrDefault(p => p.Id == besteldProduct.ProductId);
                        if (product != null)
                        {
                            // Update de Naam-eigenschap van het BesteldProduct met de productnaam
                            besteldProduct.Naam = product.Naam;
                        }
                    }
                }
                Bestelling = new ObservableCollection<Bestelling>(Bestelling);
                Debug.WriteLine($"Geladen Bestellingen: {JsonConvert.SerializeObject(Bestelling)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het laden van bestellingen: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SetBetaalStatus(Bestelling selectedBestelling)
        {
            try
            {
                // Controleer eerst of de huidige bestelling geldig is
                if (selectedBestelling == null)
                {
                    Debug.WriteLine("Geen bestelling geselecteerd.");
                    return;
                }

                // Bijwerk de betaalstatus van de huidige bestelling naar 1
                selectedBestelling.BetaalStatus = 1;

                // Verzend de gewijzigde bestelling naar de database via de ApiService
                var updatedBestelling = await _apiService.UpdateBestellingAsync(selectedBestelling.Id, selectedBestelling);

                // Controleer of de update succesvol was

                selectedBestelling = null!;
                await LoadBestellingen();
                Debug.WriteLine("Betaalstatus van de bestelling succesvol bijgewerkt.");
                // Voer verdere acties uit indien nodig
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden: {ex.Message}");
                // Voer eventuele foutafhandeling uit
            }
        }

        [RelayCommand]
        public async Task DeleteBestelling(Bestelling selectedBestelling)
        {
            try
            {
                // Controleer eerst of er een bestelling is geselecteerd om te verwijderen
                if (selectedBestelling == null)
                {
                    Debug.WriteLine("Geen bestelling geselecteerd om te verwijderen.");
                    return;
                }

                // Vraag de gebruiker om bevestiging voordat de bestelling wordt verwijderd
                var confirmDelete = await App.Current!.MainPage!.DisplayAlert("Bestelling verwijderen", "Weet u zeker dat u deze bestelling wilt verwijderen?", "Ja", "Nee");

                if (confirmDelete)
                {
                    // Verwijder de bestelling via de ApiService
                    var deleted = await _apiService.DeleteBestellingAsync(selectedBestelling.Id);

                    if (deleted)
                    {
                        Debug.WriteLine("Bestelling succesvol verwijderd.");
                        // Reset de huidige bestelling
                        selectedBestelling = null!;
                        await LoadBestellingen();
                    }
                    else
                    {
                        Debug.WriteLine("Er is een fout opgetreden bij het verwijderen van de bestelling.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden: {ex.Message}");
                // Voer eventuele foutafhandeling uit
            }
        }

        [RelayCommand]
        public async Task GoToAfrekenenAsync(Bestelling bestelling)
        {
            if (bestelling != null)
            {
                if (bestelling.BetaalStatus == 1)
                {
                    Debug.WriteLine("Deze bestelling is al betaald.");
                    return;
                }

                await Shell.Current.GoToAsync($"AfrekenenScreen?gebruikerId={bestelling.GebruikerId}");
            }
        }
    }
}