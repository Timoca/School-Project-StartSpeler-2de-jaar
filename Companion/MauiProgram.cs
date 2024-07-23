using Companion.Services;
using Companion.ViewModels;
using Companion.Views;
using Microsoft.Extensions.Logging;

namespace Companion
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

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<EventViewModel>();
            builder.Services.AddSingleton<DrankViewModel>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LogoutViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();
            builder.Services.AddSingleton<WinkelmandViewModel>();

            builder.Services.AddSingleton<ApiService>();

            builder.Services.AddTransient<HomePagina>();
            builder.Services.AddTransient<EventPagina>();
            builder.Services.AddTransient<LoginPagina>();
            builder.Services.AddTransient<LogoutPagina>();
            builder.Services.AddTransient<DrankPagina>();
            builder.Services.AddTransient<RegistratiePagina>();
            builder.Services.AddTransient<WinkelmandPagina>();

            return builder.Build();
        }
    }
}