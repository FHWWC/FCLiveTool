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

    private async void EmailBtn_Clicked(object sender, EventArgs e)
    {
        if(!await Launcher.TryOpenAsync("mailto:justineedyoumost@163.com"))
        {
            await DisplayAlert("提示信息", "无法打开邮件客户端，如有需要请手动输入邮箱：\n justineedyoumost@163.com", "确定");
        }
    }

    private void GroupsBtn_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("提示信息", "我们的群组：\n", "确定");
    }
}