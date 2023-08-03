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

    private void AboutBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void SettingBtn_Clicked(object sender, EventArgs e)
    {

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
