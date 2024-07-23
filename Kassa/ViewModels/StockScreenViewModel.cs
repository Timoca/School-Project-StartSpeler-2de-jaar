using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using Microsoft.Extensions.Logging;
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
    public partial class StockScreenViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

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
        public ObservableCollection<Product> producten;

        [ObservableProperty]
        public Product selectedProduct;

        public bool IsNotEventManager => !GlobalSettings.IsEventManager;

        public async Task InitializeAsync()
        {
            await LoadProducten();
        }

        public StockScreenViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Producten = new ObservableCollection<Product>();
            Frisdrank = new ObservableCollection<Product>();
            Warmedrank = new ObservableCollection<Product>();
            Snack = new ObservableCollection<Product>();
            AlcoholischeDrank = new ObservableCollection<Product>();
            NonAlcoholischeDrank = new ObservableCollection<Product>();
            selectedProduct = new Product();
            LoadProducten();

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
                        if (product.Type == "Frisdrank")
                        {
                            Frisdrank.Add(product);
                        }
                        if (product.Type == "Warme Drank")
                        {
                            Warmedrank.Add(product);
                        }
                        if (product.Type == "Snack")
                        {
                            Snack.Add(product);
                        }
                        if (product.Type == "Alcoholische Drank")
                        {
                            AlcoholischeDrank.Add(product);
                        }
                        if (product.Type == "Non Alcoholische Drank")
                        {
                            NonAlcoholischeDrank.Add(product);
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
        public async Task DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                bool answer = await Application.Current!.MainPage!.DisplayAlert("Bevestigen", $"Weet u zeker dat u {SelectedProduct.Naam} wilt verwijderen?", "Ja", "Nee");
                if (answer)
                {
                    var result = await _apiService.DeleteProductAsync(SelectedProduct.Id);
                    if (result)
                    {
                        await LoadProducten();
                        SelectedProduct = new Product();
                    }
                }
            }
        }

        [RelayCommand]
        public async Task CreateProduct()
        {
            try
            {
                // Toon een popup voor het invoeren van productgegevens
                string productName = await Application.Current!.MainPage!.DisplayPromptAsync("Nieuw Product", "Voer de naam van het product in", "OK", "Annuleren", placeholder: "Product naam");

                if (productName == null) // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;

                string productNotes = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Product", "Voer de notities van het product in", "OK", "Annuleren", placeholder: "Product notities");

                if (productNotes == null) // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;

                string productPriceString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Product", "Voer de prijs van het product in", "OK", "Annuleren", placeholder: "Product prijs");
                productPriceString = productPriceString.Replace(',', '.');

                if (productPriceString == null) // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;

                float productPrice;
                if (!float.TryParse(productPriceString, NumberStyles.Number, CultureInfo.InvariantCulture, out productPrice))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige prijs. Voer een geldige numerieke waarde in.", "OK");
                    return;
                }

                string productStockString = await Application.Current.MainPage.DisplayPromptAsync("Nieuw Product", "Voer de voorraad van het product in", "OK", "Annuleren", placeholder: "Product voorraad");

                if (productStockString == null) // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;

                int productStock;
                if (!int.TryParse(productStockString, out productStock))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige voorraad. Voer een geldige numerieke waarde in.", "OK");
                    return;
                }
                List<string> beschikbareProductTypes = new List<string> { "Frisdrank", "Warme Drank", "Snack, Alcoholische Drank, Non Alcoholische Drank" }; // Voeg de beschikbare producttypes toe
                string productType = await Application.Current.MainPage.DisplayActionSheet("Type van het product", "Annuleren", null, beschikbareProductTypes.ToArray());

                if (productType == "Annuleren") // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;
                //Controleer of de lijst van evenementen niet leeg is

                // Toevoegen van verdere inputvelden zoals startdatum, einddatum, kosten, etc.

                // Creëer een nieuw evenement object

                var newProduct = new Product
                {
                    Naam = productName,
                    Notities = productNotes,
                    Prijs = productPrice,
                    StockAantal = productStock,
                    Type = productType,
                };

                // Roep de methode aan om het nieuwe evenement aan te maken via de API-service
                bool success = await _apiService.CreateProductAsync(newProduct);

                if (success)
                {
                    // Evenement succesvol aangemaakt, laad de evenementen opnieuw in
                    await LoadProducten();
                }
                else
                {
                    // Kon het evenement niet aanmaken, behandel dit geval
                    Debug.WriteLine("Kon het product niet aanmaken.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het aanmaken van het product: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task UpdateProduct()
        {
            try
            {
                if (SelectedProduct == null)
                {
                    // Als er geen geselecteerd product is, stop de methode
                    return;
                }
                string productStockString = await Application.Current!.MainPage!.DisplayPromptAsync("Update Voorraad", "Voer het nieuwe stockaantal van het product in", "OK", "Annuleren", placeholder: "Nieuw stockaantal", initialValue: SelectedProduct.StockAantal.ToString());

                if (productStockString == null) // Als de gebruiker op Annuleren klikt, stoppen we de methode
                    return;

                int productStock;
                if (!int.TryParse(productStockString, out productStock))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Ongeldige voorraad. Voer een geldige numerieke waarde in.", "OK");
                    return;
                }
                SelectedProduct.StockAantal = productStock;
                // Creëer een nieuw evenement object

                // Roep de methode aan om het nieuwe evenement aan te maken via de API-service
                bool success = await _apiService.UpdateProductAsync(SelectedProduct.Id, SelectedProduct);

                if (success)
                {
                    // Evenement succesvol aangemaakt, laad de evenementen opnieuw in
                    await LoadProducten();
                    SelectedProduct = new Product();
                    await Application.Current.MainPage.DisplayAlert("Succes", "Het product is succesvol bijgewerkt.", "OK");
                }
                else
                {
                    // Kon het evenement niet aanmaken, behandel dit geval
                    await Application.Current.MainPage.DisplayAlert("Fout", "Het bijwerken van het product is mislukt.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Er is een fout opgetreden bij het aanmaken van het product: {ex.Message}");
            }
        }

        [RelayCommand]
        public static async Task GoToProductScreenAsync()
        {
            await Shell.Current.GoToAsync("//ProductScreen");
        }
    }
}