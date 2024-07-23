using Kassa.Services;
using Kassa.ViewModels;
using Kassa.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Newtonsoft.Json;

namespace Kassa
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
           
                var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});
            
        

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                    var _appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
                    (_appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter).Maximize();
                });
            });
        });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

			builder.Services.AddSingleton<ApiService>();

			builder.Services.AddSingleton<LoginPage>();
			builder.Services.AddTransient<RegisterPage>();

			builder.Services.AddSingleton<HomeScreen>();
			builder.Services.AddSingleton<ProductScreen>();
			builder.Services.AddSingleton<InstellingenScreen>();
			builder.Services.AddSingleton<OverzichtScreen>();
			builder.Services.AddSingleton<StockScreen>();
			builder.Services.AddSingleton<EventScreen>();
			builder.Services.AddSingleton<AfrekenenScreen>();
			builder.Services.AddSingleton<MenuScreen>();

			builder.Services.AddSingleton<HomeScreenViewModel>();
			builder.Services.AddSingleton<ProductScreenViewModel>();
			builder.Services.AddSingleton<InstellingenScreenViewModel>();
			builder.Services.AddSingleton<OverzichtScreenViewModel>();
			builder.Services.AddSingleton<StockScreenViewModel>();
			builder.Services.AddSingleton<EventScreenViewModel>();
			builder.Services.AddSingleton<AfrekenenScreenViewModel>();
			builder.Services.AddSingleton<MenuScreenViewModel>();
			builder.Services.AddSingleton<LoginViewModel>();
			builder.Services.AddSingleton<RegisterViewModel>();
			builder.Services.AddSingleton<AppShellViewModel>();

			return builder.Build();
		}
	}
}