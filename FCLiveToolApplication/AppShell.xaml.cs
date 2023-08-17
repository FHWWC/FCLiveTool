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
        Preferences.Set("DefaultPlayM3U8Name", Preferences.Get("DefaultPlayM3U8Name", "CGTN (法语)"));
        Preferences.Set("DefaultPlayM3U8URL", Preferences.Get("DefaultPlayM3U8URL", @"https://livefr.cgtn.com/1000f/prog_index.m3u8"));
        Preferences.Set("StartAutoPlayM3U8", Preferences.Get("StartAutoPlayM3U8", true));
        Preferences.Set("AppDarkMode", Preferences.Get("AppDarkMode", false));

        if(Preferences.Get("AppDarkMode", false))
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
