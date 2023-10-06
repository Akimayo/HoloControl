using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Foldable;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("B612-Regular.ttf", "B612")
                         .AddFont("B612-Italic.ttf");
                    fonts.AddFont("B612Mono-Regular.ttf", "B612 Mono");
                });
            builder.UseFoldable();

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}