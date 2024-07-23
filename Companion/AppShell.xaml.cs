using Companion.Services;
using Companion.ViewModels;
using Companion.Views;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace Companion
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DrankPagina), typeof(DrankPagina));
            Routing.RegisterRoute(nameof(HomePagina), typeof(HomePagina));
            Routing.RegisterRoute(nameof(EventPagina), typeof(EventPagina));
            Routing.RegisterRoute(nameof(LoginPagina), typeof(LoginPagina));
            Routing.RegisterRoute(nameof(RegistratiePagina), typeof(RegistratiePagina));
            Routing.RegisterRoute(nameof(LogoutPagina), typeof(LogoutPagina));

            Navigated += HandleNavigated!;
        }

        private void HandleNavigated(object sender, ShellNavigatedEventArgs e)
        {
            try
            {
                var route = e.Current.Location.OriginalString;
                //Debug.WriteLine($"Navigated to {route}");

                // Controleer of de huidige route een login- of registratiepagina is
                var isLoginOrRegister = route.Contains(nameof(LoginPagina)) || route.Contains(nameof(RegistratiePagina));

                // Verberg de flyout als de gebruiker is ingelogd
                logoutShellContent.FlyoutItemIsVisible = !isLoginOrRegister;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout in HandleNavigated: {ex.Message}");
            }
        }
    }
}