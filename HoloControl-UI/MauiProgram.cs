using Microsoft.Extensions.Logging;
using Microsoft.Maui.Foldable;

namespace HoloControl
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFoldable()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("B612-Regular.ttf", "B612")
                         .AddFont("B612-Italic.ttf");
                    fonts.AddFont("B612Mono-Regular.ttf", "B612 Mono");
                })
                .ConfigureEssentials(essentials =>
                {
                    essentials.AddAppAction("mode_standard", "Standard Mode", icon: "steering_wheel")
                              .AddAppAction("mode_kiosk", "Kiosk Mode", icon: "storefront")
                              .OnAppAction(App.HandleAppActions);
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}