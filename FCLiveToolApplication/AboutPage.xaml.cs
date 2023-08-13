namespace FCLiveToolApplication;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Navigation.PopAsync();
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        AppVersionText.Text=VersionTracking.CurrentVersion;
    }

    private void GitHubBtn_Clicked(object sender, EventArgs e)
    {
        Launcher.OpenAsync("https://github.com/FHWWC/FCLiveTool");
    }

    private void EmailBtn_Clicked(object sender, EventArgs e)
    {
        Launcher.OpenAsync("mailto:justineedyoumost@163.com");
    }

    private void GroupsBtn_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("提示信息", "我们的群组：\n", "确定");
    }
}