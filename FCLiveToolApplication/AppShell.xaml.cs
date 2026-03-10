#if WINDOWS
using Windows.UI.ViewManagement;
#endif
using Microsoft.Maui.Devices;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace FCLiveToolApplication;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }
    private void Shell_Loaded(object sender, EventArgs e)
    {
        // 仅在不存在时设置默认值（避免覆盖已保存的用户设置）
        if (!Preferences.ContainsKey("DefaultPlayM3U8Name"))
            Preferences.Set("DefaultPlayM3U8Name", GlobalParameter.DefaultPlayM3U8Name);
        if (!Preferences.ContainsKey("DefaultPlayM3U8URL"))
            Preferences.Set("DefaultPlayM3U8URL", GlobalParameter.DefaultPlayM3U8URL);
        if (!Preferences.ContainsKey("StartAutoPlayM3U8"))
            Preferences.Set("StartAutoPlayM3U8", GlobalParameter.StartAutoPlayM3U8);
        if (!Preferences.ContainsKey("AppThemeType"))
            Preferences.Set("AppThemeType", GlobalParameter.AppThemeType);
        if (!Preferences.ContainsKey("VideoCheckThreadNum"))
            Preferences.Set("VideoCheckThreadNum", GlobalParameter.VideoCheckThreadNum);
        if (!Preferences.ContainsKey("VideoCheckUA"))
            Preferences.Set("VideoCheckUA", GlobalParameter.VideoCheckUA);


        switch (Preferences.Get("AppThemeType", GlobalParameter.AppThemeType))
        {
            case 0:
#if WINDOWS

                var uiSettings = new UISettings();
                if (uiSettings.GetColorValue(UIColorType.Background) == Windows.UI.Color.FromArgb(255, 0, 0, 0))
                {
                    Application.Current.UserAppTheme = AppTheme.Dark;
                }
                else
                {
                    Application.Current.UserAppTheme = AppTheme.Light;
                }
#else
bool isDark = Application.Current.RequestedTheme == AppTheme.Dark
              || Application.Current.UserAppTheme == AppTheme.Dark;
if (isDark)
{
    Application.Current.UserAppTheme = AppTheme.Dark;
}
else
{
    Application.Current.UserAppTheme = AppTheme.Light;
}
#endif
                break;
            case 1:
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case 2:
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
        }

#if WINDOWS
        try
        {
            Window.Height=720;
            Window.Width=1200;
        }
        catch (Exception ex)
        {

        }
#endif
    }


    private async void AboutBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
        await Navigation.PushAsync(new AboutPage());

#if ANDROID
Shell.Current.FlyoutIsPresented = false;
#endif
    }

    private async void SettingBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
        await Navigation.PushAsync(new AppSettingPage());

#if ANDROID
Shell.Current.FlyoutIsPresented = false;
#endif
    }

    private void Shell_Navigating(object sender, ShellNavigatingEventArgs e)
    {
#if WINDOWS
        if (Preferences.Get("AppThemeType", GlobalParameter.AppThemeType)==0)
        {
            var uiSettings = new UISettings();
            if (uiSettings.GetColorValue(UIColorType.Background) == Windows.UI.Color.FromArgb(255, 0, 0, 0))
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }
#endif


        /*
         if (!e.Target.Location.ToString().Contains("VideoPrevPage"))
                {
                    if (VideoPrevPage.videoPrevPage!=null)
                    {
                        VideoPrevPage.videoPrevPage.VideoWindow.Stop();
                    }
                }
         */

    }
}
