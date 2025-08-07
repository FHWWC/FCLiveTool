using CommunityToolkit.Maui.Views;

namespace FCLiveToolApplication;

public partial class VideoCheckPagePopup : CommunityToolkit.Maui.Views.Popup
{
	public VideoCheckPagePopup()
	{
		InitializeComponent();
	}

    private void SaveOptionBtn_Clicked(object sender, EventArgs e)
    {
        #region 判断“同时检测的线程数”
        if (string.IsNullOrWhiteSpace(UseThreadNumTb.Text))
        {
            VideoCheckPage.videoCheckPage.PopShowMsg("你还没有输入“同时检测的线程数”！");
            return;
        }
        int threadNum;
        if (!int.TryParse(UseThreadNumTb.Text, out threadNum))
        {
            VideoCheckPage.videoCheckPage.PopShowMsg("“同时检测的线程数”必须是有效的数值！");
            return;
        }
        if (threadNum<1)
        {
            threadNum=GlobalParameter.VideoCheckThreadNum;
        }
        #endregion

        #region 判断“User-Agent”
        if (string.IsNullOrWhiteSpace(UseUATb.Text))
        {
            VideoCheckPage.videoCheckPage.PopShowMsg("你还没有输入“User-Agent”！");
            return;
        }
        #endregion

        Preferences.Set("VideoCheckThreadNum", threadNum);
        Preferences.Set("VideoCheckUA", UseUATb.Text);


        VideoCheckPage.videoCheckPage.PopShowMsg("已更新检测选项的参数。");
        CloseAsync();
    }

    private async void ResetOptionBtn_Clicked(object sender, EventArgs e)
    {
        if (await VideoCheckPage.videoCheckPage.PopShowMsgAndReturn("你要重置所有选项为默认值吗？"))
        {
            Preferences.Set("VideoCheckThreadNum", GlobalParameter.VideoCheckThreadNum);
            Preferences.Set("VideoCheckUA", GlobalParameter.VideoCheckUA);

            await CloseAsync();
        }
    }

    private void MainGrid_Loaded(object sender, EventArgs e)
    {
        UseThreadNumTb.Text= Preferences.Get("VideoCheckThreadNum", GlobalParameter.VideoCheckThreadNum).ToString();
        UseUATb.Text= Preferences.Get("VideoCheckUA", GlobalParameter.VideoCheckUA);
    }
}