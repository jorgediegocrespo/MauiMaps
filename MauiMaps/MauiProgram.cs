using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;

namespace MauiMaps;

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
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
            })
            .UseMauiMaps()
            .ConfigureMauiHandlers(handlers =>
        {
#if IOS 
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
            Microsoft.Maui.Maps.Handlers.MapHandler.Mapper.ModifyMapping(nameof(Microsoft.Maui.Maps.IMap.Pins), (handler, view, action) =>
                CustomMapHandler.CustomMapPins(handler, view));
#elif ANDROID
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
            Microsoft.Maui.Maps.Handlers.MapHandler.Mapper.ModifyMapping(nameof(Microsoft.Maui.Maps.IMap.Pins), (handler, view, action) =>
                CustomMapHandler.CustomMapPins(handler, view));

            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, CustomMapHandler>();
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}