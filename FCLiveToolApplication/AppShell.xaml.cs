using CommunityToolkit.Maui.Views;

namespace FCLiveToolApplication;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }
    private void Shell_Loaded(object sender, EventArgs e)
    {
        Preferences.Set("DefaultPlayM3U8Name", Preferences.Get("DefaultPlayM3U8Name", GlobalParameter.DefaultPlayM3U8Name));
        Preferences.Set("DefaultPlayM3U8URL", Preferences.Get("DefaultPlayM3U8URL",GlobalParameter.DefaultPlayM3U8URL));
        Preferences.Set("StartAutoPlayM3U8", Preferences.Get("StartAutoPlayM3U8", GlobalParameter.StartAutoPlayM3U8));
        Preferences.Set("AppDarkMode", Preferences.Get("AppDarkMode", GlobalParameter.AppDarkMode));
        Preferences.Set("VideoCheckThreadNum", Preferences.Get("VideoCheckThreadNum",GlobalParameter.VideoCheckThreadNum));
        Preferences.Set("VideoCheckUA", Preferences.Get("VideoCheckUA",GlobalParameter.VideoCheckUA));
       
        if(Preferences.Get("AppDarkMode", GlobalParameter.AppDarkMode))
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }

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
