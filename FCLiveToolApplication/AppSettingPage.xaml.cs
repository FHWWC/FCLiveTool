namespace FCLiveToolApplication;

public partial class AppSettingPage : ContentPage
{
	public AppSettingPage()
	{
		InitializeComponent();
	}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Navigation.PopAsync();
    }

    private async void ShowDPInput_Clicked(object sender, EventArgs e)
    {
        string urlnewvalue = await DisplayPromptAsync("设置默认值", "第1步 请输入新的直播源URL：", "更新", "取消", "URL...", -1, Keyboard.Text, "");
        if (string.IsNullOrWhiteSpace(urlnewvalue))
        {
            if(urlnewvalue!=null)
                await DisplayAlert("提示信息", "请输入正确的内容！", "确定");
            return;
        }
        if(!urlnewvalue.Contains("://"))
        {
            await DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
            return;
        }

        string namenewvalue = await DisplayPromptAsync("设置默认值", "第2步 请输入新的名称：", "更新", "取消", "名称...", -1, Keyboard.Text, "");
        if (string.IsNullOrWhiteSpace(namenewvalue))
        {
            if (namenewvalue!=null)
                await DisplayAlert("提示信息", "请输入正确的内容！", "确定");
            return;
        }

        Preferences.Set("DefaultPlayM3U8URL", urlnewvalue);
        DefaultPlayM3U8URLText.Text=urlnewvalue;
        Preferences.Set("DefaultPlayM3U8Name", namenewvalue);
        DefaultPlayM3U8NameText.Text=namenewvalue;

        await DisplayAlert("提示信息", "设置成功！再次打开APP后即可生效。", "确定");
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        DefaultPlayM3U8NameText.Text=Preferences.Get("DefaultPlayM3U8Name", "");
        DefaultPlayM3U8URLText.Text=Preferences.Get("DefaultPlayM3U8URL", "");
        StartAutoPlayToogleBtn.IsToggled= Preferences.Get("StartAutoPlayM3U8", true);
        DarkModeToogleBtn.IsToggled= Preferences.Get("AppDarkMode", false);
    }

    private void StartAutoPlayToogleBtn_Toggled(object sender, ToggledEventArgs e)
    {
        if(e.Value)
        {
            Preferences.Set("StartAutoPlayM3U8", true);
        }
        else
        {
            Preferences.Set("StartAutoPlayM3U8", false);
        }
    }

    private void DarkModeToogleBtn_Toggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            Preferences.Set("AppDarkMode", true);
            Application.Current.UserAppTheme = AppTheme.Dark;
        }
        else
        {
            Preferences.Set("AppDarkMode", false);
            Application.Current.UserAppTheme = AppTheme.Light;
        }
    }
}