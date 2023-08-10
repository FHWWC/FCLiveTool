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
}