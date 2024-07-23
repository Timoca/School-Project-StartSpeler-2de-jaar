using CommunityToolkit.Mvvm.ComponentModel;
using Kassa.ViewModels;
using Kassa.Views;

namespace Kassa
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            Routing.RegisterRoute(nameof(HomeScreen), typeof(HomeScreen));
            Routing.RegisterRoute(nameof(ProductScreen), typeof(ProductScreen));
            Routing.RegisterRoute(nameof(MenuScreen), typeof(MenuScreen));
            Routing.RegisterRoute(nameof(AfrekenenScreen), typeof(AfrekenenScreen));
            Routing.RegisterRoute(nameof(InstellingenScreen), typeof(InstellingenScreen));
            Routing.RegisterRoute(nameof(StockScreen), typeof(StockScreen));
            Routing.RegisterRoute(nameof(OverzichtScreen), typeof(OverzichtScreen));
            Routing.RegisterRoute(nameof(EventScreen), typeof(EventScreen));
        }
    }
}