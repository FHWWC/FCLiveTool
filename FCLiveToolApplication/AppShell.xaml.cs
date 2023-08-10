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
        Application.Current.UserAppTheme = AppTheme.Light;      
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
