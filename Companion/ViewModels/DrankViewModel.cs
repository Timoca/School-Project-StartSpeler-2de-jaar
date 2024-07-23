using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Companion.Models;
using Companion.Services;
using Companion.Views;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Companion.ViewModels
{
    public partial class DrankViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        string naam = default!;

        [ObservableProperty]
        public string notities = default!;

        [ObservableProperty]
        public float prijs = default!;

        [ObservableProperty]
        public int stockAantal = default!;

        [ObservableProperty]
        public string type = default!;

        [ObservableProperty]
        private Bestelling huidigeBestelling = default!;

        public DrankViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            Dranken = new ObservableCollection<ProductGroup>();
            HuidigeBestelling = new Bestelling();
            LoadDrankenCommand = new AsyncRelayCommand(LoadDrankenAsync);
            BestelDrankCommand = new AsyncRelayCommand<Product>(BestelDrankAsync);
            BestellenCommand = new AsyncRelayCommand(BestellenAsync);
            NaarWinkelmand = new AsyncRelayCommand(GaNaarWinkelmandAsync);

            // Roep de async initialisatie methode aan vanuit de constructor
            Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            // Asynchronously set the GebruikerId
            HuidigeBestelling.GebruikerId = await SecureStorage.GetAsync("user_id");

            // Load the drinks when the ViewModel is created
            await LoadDrankenCommand.ExecuteAsync(null);
        }

        [ObservableProperty]
        private ObservableCollection<ProductGroup> dranken;

        public IAsyncRelayCommand LoadDrankenCommand { get; }
        public IAsyncRelayCommand<Product> BestelDrankCommand { get; }
        public IAsyncRelayCommand BestellenCommand { get; }
        public IAsyncRelayCommand NaarWinkelmand { get; }

        private async Task LoadDrankenAsync()
        {
            var producten = await _apiService.GetAllProductsAsync();
            Debug.WriteLine($"Geladen producten: {JsonConvert.SerializeObject(producten)}");
            if (producten != null)
            {
                var groupedProducts = producten
                    .GroupBy(p => p.Type)
                    .Select(g => new ProductGroup(g.Key, g.ToList()))
                    .ToList();

                Dranken.Clear();
                foreach (var group in groupedProducts)
                {
                    Dranken.Add(group);
                }
            }
        }

        private async Task BestelDrankAsync(Product product)
        {
            var bestaandProduct = HuidigeBestelling.BesteldeProducten.FirstOrDefault(p => p.ProductId == product.Id);
            if (bestaandProduct != null)
            {
                bestaandProduct.Aantal++;
            }
            else
            {
                HuidigeBestelling.BesteldeProducten.Add(new BesteldProduct
                {
                    ProductId = product.Id,
                    Product = product,
                    Aantal = 1,
                    Opmerking = ""
                });
            }

            HuidigeBestelling.TotaalBedrag += product.Prijs;
            Debug.WriteLine($"Drank toegevoegd: {product.Naam}, Totaal: {HuidigeBestelling.TotaalBedrag}");
        }

        private async Task GaNaarWinkelmandAsync()
        {
            if (HuidigeBestelling.BesteldeProducten.Count > 0)
            {
                await Shell.Current.GoToAsync("///WinkelmandPagina", new Dictionary<string, object>
        {
            { "HuidigeBestelling", HuidigeBestelling }
        });
            }
            else
            {
                Debug.WriteLine("Winkelmand is leeg.");
            }
        }

        private async Task BestellenAsync()
        {
            HuidigeBestelling.BestelTijd = DateTime.Now;
            var response = await _apiService.PlaatsBestellingAsync(HuidigeBestelling);
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Bestelling succesvol geplaatst.");
                HuidigeBestelling = new Bestelling(); // Reset bestelling na succesvolle plaatsing
            }
            else
            {
                Debug.WriteLine("Fout bij het plaatsen van de bestelling.");
            }
        }
    }

    public class ProductGroup : ObservableCollection<Product>
    {
        public string Type { get; }

        public ProductGroup(string type, List<Product> products) : base(products)
        {
            Type = type;
        }
    }
}