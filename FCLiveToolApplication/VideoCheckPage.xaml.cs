namespace FCLiveToolApplication;

public partial class VideoCheckPage : ContentPage
{
	public VideoCheckPage()
	{
		InitializeComponent();
	}
    List<VideoDetailList> CurrentCheckList=new List<VideoDetailList>();
    private void M3USourceRBtn_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton entry = sender as RadioButton;

        if (entry.StyleId == "M3USourceRBtn1")
        {
            LocalM3USelectPanel.IsVisible = true;
            M3USourcePanel.IsVisible = false;
        }
        else if (entry.StyleId == "M3USourceRBtn2")
        {
            LocalM3USelectPanel.IsVisible = false;
            M3USourcePanel.IsVisible = true;
        }
    }

    private void SelectLocalM3UFileBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void M3UAnalysisBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void StartCheckBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void VideoCheckList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

    }

    private void RegexSelectBox_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void RegexSelectTipBtn_Clicked(object sender, EventArgs e)
    {

    }
}