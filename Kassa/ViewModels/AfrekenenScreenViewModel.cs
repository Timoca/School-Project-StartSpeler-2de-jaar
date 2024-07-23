using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kassa.Models;
using Kassa.Services;
using Kassa.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.ViewModels
{
    [QueryProperty(nameof(GebruikerId), "gebruikerId")]
    public partial class AfrekenenScreenViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        public ObservableCollection<BesteldProduct> besteldeProducten = default!;

        [ObservableProperty]
        private string gebruikerId = default!;

        [ObservableProperty]
        public string klantNaam = default!;

        [ObservableProperty]
        public float totaalPrijs = default!;

        public AfrekenenScreenViewModel(ApiService apiService)
        {
            _apiService = apiService;
            BesteldeProducten = new ObservableCollection<BesteldProduct>();
        }

        public async Task InitializeAsync(Gebruiker gebruiker)
        {
            GebruikerId = gebruiker.Id;
            await LoadBesteldeProductenAsync();
        }

        [RelayCommand]
        public async Task LoadBesteldeProductenAsync()
        {
            try
            {
                var gebruiker = await _apiService.GetGebruikerByIdAsync(GebruikerId);
                if (gebruiker != null)
                {
                    KlantNaam = gebruiker.Voornaam + " " + gebruiker.Achternaam;
                }

                var bestellingen = await _apiService.GetAllBestellingenAsync();
                var besteldeProducten = await _apiService.GetAllBesteldeProductenAsync();

                BesteldeProducten.Clear();
                TotaalPrijs = 0;

                foreach (var bestelling in bestellingen)
                {
                    if (bestelling.GebruikerId == GebruikerId && bestelling.BetaalStatus == 0)
                    {
                        foreach (var besteldProduct in besteldeProducten)
                        {
                            if (besteldProduct.BestellingId == bestelling.Id)
                            {
                                BesteldeProducten.Add(besteldProduct);
                                TotaalPrijs += besteldProduct.Product!.Prijs * besteldProduct.Aantal;
                            }
                        }
                    }
                }

                Debug.WriteLine($"Loaded {BesteldeProducten.Count} producten for gebruikerId {GebruikerId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading bestelde producten: {ex.Message}");
            }
        }

        [RelayCommand]
        public void AddProductToAfrekenen(Product product)
        {
            BesteldeProducten.Add(new BesteldProduct { Product = product, Aantal = 1 });
            TotaalPrijs = BesteldeProducten.Sum(p => p.Aantal * p.Product!.Prijs);
        }

        [RelayCommand]
        public void DeleteProductFromAfrekenen(BesteldProduct product)
        {
            BesteldeProducten.Remove(product);
            TotaalPrijs = BesteldeProducten.Sum(p => p.Aantal * p.Product!.Prijs);
        }

        [RelayCommand]
        public async Task BetaaldAsync()
        {
            try
            {
                // Markeer alle bestellingen van deze gebruiker als betaald
                var bestellingen = await _apiService.GetAllBestellingenAsync();
                var besteldeProducten = await _apiService.GetAllBesteldeProductenAsync();

                foreach (var bestelling in bestellingen)
                {
                    if (bestelling.GebruikerId == GebruikerId && bestelling.BetaalStatus == 0)
                    {
                        bestelling.BetaalStatus = 1;

                        // Voeg de bestelde producten toe aan de bestelling
                        bestelling.BesteldeProducten = besteldeProducten.Where(bp => bp.BestellingId == bestelling.Id).ToList();

                        var result = await _apiService.UpdateBestellingAsync(bestelling.Id, bestelling);

                        if (!result)
                        {
                            Debug.WriteLine($"Failed to update bestelling with ID: {bestelling.Id}");
                        }
                        else
                        {
                            Debug.WriteLine($"Successfully updated bestelling with ID: {bestelling.Id}");
                        }
                    }
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking as paid: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task TerugAsync()
        {
            await Shell.Current.GoToAsync("//OverzichtScreen");
        }
    }
}