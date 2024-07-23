using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Companion.Models;
using Companion.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Companion.ViewModels
{
    [QueryProperty(nameof(HuidigeBestelling), "HuidigeBestelling")]
    public partial class WinkelmandViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private Bestelling huidigeBestelling = default!;

        public ObservableCollection<BesteldProduct> BesteldeProducten { get; set; } = new ObservableCollection<BesteldProduct>();

        public WinkelmandViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            UpdateBesteldeProducten();
        }

        partial void OnHuidigeBestellingChanged(Bestelling value)
        {
            UpdateBesteldeProducten();
        }

        private void UpdateBesteldeProducten()
        {
            if (HuidigeBestelling != null)
            {
                BesteldeProducten = new ObservableCollection<BesteldProduct>(HuidigeBestelling.BesteldeProducten);
                OnPropertyChanged(nameof(BesteldeProducten));
            }
        }

        [RelayCommand]
        private void LeegWinkelmand()
        {
            if (HuidigeBestelling != null && HuidigeBestelling.BesteldeProducten != null)
            {
                Debug.WriteLine($"Aantal producten voor het legen: {HuidigeBestelling.BesteldeProducten.Count}");
                HuidigeBestelling.BesteldeProducten.Clear();
                HuidigeBestelling.TotaalBedrag = 0;
                HuidigeBestelling = new Bestelling();
                Debug.WriteLine("Winkelmand geleegd.");
                Debug.WriteLine($"Aantal producten na het legen: {HuidigeBestelling.BesteldeProducten.Count}");

                UpdateBesteldeProducten();
                OnPropertyChanged(nameof(HuidigeBestelling));
            }
        }

        [RelayCommand]
        private async Task BestellenAsync()
        {
            try
            {
                Debug.WriteLine("Bestelling plaatsen...");
                HuidigeBestelling.BestelTijd = DateTime.Now;
                var response = await _apiService.PlaatsBestellingAsync(HuidigeBestelling);
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Bestelling succesvol geplaatst.");
                    HuidigeBestelling = new Bestelling(); // Reset bestelling na succesvolle plaatsing
                    UpdateBesteldeProducten();
                    OnPropertyChanged(nameof(HuidigeBestelling));
                }
                else
                {
                    Debug.WriteLine("Fout bij het plaatsen van de bestelling.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij het plaatsen van de bestelling: {ex.Message}");
            }
        }
    }
}