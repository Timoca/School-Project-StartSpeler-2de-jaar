using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using Kassa.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.ViewModels
{
    public partial class MenuScreenViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        public string? naam;

        [ObservableProperty]
        public float prijs;

        [ObservableProperty]
        public ObservableCollection<Product> frisdrank;

        [ObservableProperty]
        public ObservableCollection<Product> warmedrank;

        [ObservableProperty]
        public ObservableCollection<Product> snack;

        [ObservableProperty]
        public ObservableCollection<Product> alcoholischeDrank;

        [ObservableProperty]
        public ObservableCollection<Product> nonAlcoholischeDrank;

        [ObservableProperty]
        public Product selectedProduct;

        [ObservableProperty]
        public BesteldProduct besteldProduct;

        [ObservableProperty]
        public Bestelling huidigeBestelling;

        public async Task InitializeAsync()
        {
            await LoadProducten();
        }

        public MenuScreenViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Frisdrank = new ObservableCollection<Product>();
            Warmedrank = new ObservableCollection<Product>();
            Snack = new ObservableCollection<Product>();
            AlcoholischeDrank = new ObservableCollection<Product>();
            NonAlcoholischeDrank = new ObservableCollection<Product>();
            selectedProduct = new Product();
            BesteldProduct = new BesteldProduct();
            HuidigeBestelling = new Bestelling();
            LoadProducten();
        }

        [RelayCommand]
        public async Task LoadProducten()
        {
            try
            {
                var loadedProducten = await _apiService.GetAllProductenAsync();
                Debug.WriteLine($"Geladen producten: {JsonConvert.SerializeObject(loadedProducten)}"); // Loggen van de geladen evenementen

                if (loadedProducten != null && loadedProducten.Count > 0)
                {
                    Frisdrank.Clear();
                    Warmedrank.Clear();
                    Snack.Clear();
                    AlcoholischeDrank.Clear();
                    NonAlcoholischeDrank.Clear();
                    foreach (var product in loadedProducten)
                    {
                        if (product.StockAantal > 0)
                        {
                            if (product.Type == "Frisdrank")
                            {
                                Frisdrank.Add(product);
                            }
                            else if (product.Type == "Warme Drank")
                            {
                                Warmedrank.Add(product);
                            }
                            else if (product.Type == "Snack")
                            {
                                Snack.Add(product);
                            }
                            else if (product.Type == "Alcoholische Drank")
                            {
                                AlcoholischeDrank.Add(product);
                            }
                            else if (product.Type == "Non Alcoholische Drank")
                            {
                                NonAlcoholischeDrank.Add(product);
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Geen producten geladen of de lijst is leeg.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het laden van producten: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ExecuteAddToCart(Product product)
        {
            Debug.WriteLine("Knop werkt");
            List<int> aantalOpties = Enumerable.Range(1, 5).ToList();

            // Show dialog to input the number of items to order
            string input = await App.Current!.MainPage!.DisplayActionSheet($"Hoeveel stuks wilt u bestellen?", "Annuleren", null, aantalOpties.Select(a => a.ToString()).ToArray());

            // Check if the user selected a valid option
            if (input != null && int.TryParse(input, out int aantal))
            {
                var userId = await SecureStorage.GetAsync("user_id");
                var existingProduct = await _apiService.GetProductByIdAsync(product.Id);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Create a new BesteldProduct object
                    var besteldProduct = new BesteldProduct
                    {
                        //Product = existingProduct,
                        ProductId = existingProduct.Id,
                        Aantal = aantal,
                        Opmerking = "",// Set the Bestelling property of BesteldProduct
                        BestellingId = HuidigeBestelling.Id,
                        //Bestelling = HuidigeBestelling
                    };

                    // Add the besteld product to the current order
                    HuidigeBestelling.BesteldeProducten.Add(besteldProduct);

                    // Update the total amount of the current order
                    HuidigeBestelling.TotaalBedrag += product.Prijs * aantal;

                    // Debug output for verification
                    //Debug.WriteLine($"Toegevoegd product: {besteldProduct.Product.Naam}, Aantal: {besteldProduct.Aantal}");
                    Debug.WriteLine("Informatie over de huidige bestelling:");
                    Debug.WriteLine($"TotaalBedrag: {HuidigeBestelling.TotaalBedrag}");

                    // Assuming there is no need to update other order details at this point
                }
                else
                {
                    Debug.WriteLine("Gebruiker ID niet gevonden.");
                }
            }
            else
            {
                Debug.WriteLine("Geen geldig aantal geselecteerd.");
            }
        }

        [RelayCommand]
        public async Task Afrekenen()
        {
            if (HuidigeBestelling != null && HuidigeBestelling.BesteldeProducten.Any())
            {
                try
                {
                    // Retrieve the user details
                    var userId = await SecureStorage.GetAsync("user_id");
                    var email = await SecureStorage.GetAsync("user_email");

                    // Check if user information is available
                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(email))
                    {
                        // Retrieve details of the currently logged-in user
                        var ingelogdeGebruiker = await _apiService.GetUserDetailsAsync(email);

                        // Set the order time
                        HuidigeBestelling.BestelTijd = DateTime.Now;

                        // Prompt the user to enter the table number
                        var tafelnummerInput = await App.Current!.MainPage!.DisplayPromptAsync("Tafelnummer", "Voer het tafelnummer in:");

                        // Check if the user entered a table number
                        if (!string.IsNullOrEmpty(tafelnummerInput))
                        {
                            int tafelnummer;
                            // Check if the entered value is a valid integer
                            if (int.TryParse(tafelnummerInput, out tafelnummer))
                            {
                                // Update the current order with user details and table number
                                HuidigeBestelling.GebruikerId = userId;
                                HuidigeBestelling.TafelNummer = tafelnummer;

                                // Send the order to the database
                                await _apiService.CreateBestellingAsync(HuidigeBestelling);

                                Debug.WriteLine("Bestelling verzonden naar de database");

                                // Reset the current order after it's been sent
                                HuidigeBestelling = new Bestelling();

                                await Shell.Current.GoToAsync("//OverzichtScreen");
                            }
                            else
                            {
                                // Inform the user if the entered value is not a valid integer
                                await App.Current.MainPage.DisplayAlert("Ongeldig tafelnummer", "Voer een geldig tafelnummer in.", "OK");
                            }
                        }
                        else
                        {
                            // Inform the user if no table number was entered
                            await App.Current.MainPage.DisplayAlert("Geen tafelnummer ingevoerd", "Voer een tafelnummer in om door te gaan.", "OK");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Gebruikersinformatie niet gevonden.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fout bij het verzenden van de bestelling: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("Geen producten in de bestelling om af te rekenen");
            }
        }
    }
}