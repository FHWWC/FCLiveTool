using CommunityToolkit.Maui.Views;

namespace FCLiveToolApplication.Popup;

public partial class VideoDetailListPopup : CommunityToolkit.Maui.Views.Popup
{
	public VideoDetailListPopup()
	{
		InitializeComponent();
	}

	public bool[] VideoOptions;
    public bool isStartRun;

    private void MainGrid_Loaded(object sender, EventArgs e)
    {
        //1：台标；2：台名；3：URL
        VideoOptions=new bool[] { false, false, true };
    }

    private void SaveOptionBtn_Clicked(object sender, EventArgs e)
    {
        if (TVGLogoCB.IsChecked)
        {
            VideoOptions[0]=true;
        }
        else
        {
            VideoOptions[0]=false;
        }    
        
        if (TVGNameCB.IsChecked)
        {
            VideoOptions[1]=true;
        }
        else
        {
            VideoOptions[1]=false;
        }      
        
        if (URLCB.IsChecked)
        {
            VideoOptions[2]=true;
        }
        else
        {
            VideoOptions[2]=false;
        }


        isStartRun=true;
        this.Close();
    }

    private void CancelOptionBtn_Clicked(object sender, EventArgs e)
    {
        isStartRun = false;
        this.Close();
    }
}