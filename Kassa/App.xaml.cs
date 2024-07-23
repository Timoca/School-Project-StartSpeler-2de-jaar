using Kassa.Models;
using Kassa.Services;
using Kassa.ViewModels;

namespace Kassa
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Registreer de ApiService
            var apiService = new ApiService();
            // creëer de  AppShellViewModel
            var appShellViewModel = new AppShellViewModel(apiService);
            // creëer de AppShell
            MainPage = new AppShell(appShellViewModel);
            // Registreer de navigation events
            appShellViewModel.RegisterNavigationEvents();
        }

        public class ProductDataTemplateSelector : DataTemplateSelector
        {
            public DataTemplate ValidTemplate { get; set; } = default!;
            public DataTemplate InvalidTemplate { get; set; } = default!;

            protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
            {
                return ((Product)item).Type == "Frisdrank" ? ValidTemplate : InvalidTemplate;
            }
        }
    }
}