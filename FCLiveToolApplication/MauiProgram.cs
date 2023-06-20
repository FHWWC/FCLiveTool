using Microsoft.Maui.LifecycleEvents;
using CommunityToolkit.Maui;

#if WINDOWS10_0_17763_0_OR_GREATER
using FCLiveToolApplication.Platforms.Windows;
using FCLiveToolApplication.WinUI;
#endif

namespace FCLiveToolApplication;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("segmdl2.ttf", "Segoe MDL2 Assets");
            });

        // **** THIS SECTION IS WHAT IS RELEVANT FOR YOU ************ //
        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS10_0_17763_0_OR_GREATER

            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    window.TryMicaOrAcrylic(); // requires 'using YOUR_APP.WinUI;'
                });
            });
#endif
        });
        // ************* END OF RELEVANT SECTION *********** //

        return builder.Build();
    }
}
