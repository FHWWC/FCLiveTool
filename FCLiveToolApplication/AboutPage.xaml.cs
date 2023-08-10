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


}