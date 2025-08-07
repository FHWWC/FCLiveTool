namespace FCLiveToolApplication.Popup;

public partial class RegexSelectPopup : CommunityToolkit.Maui.Views.Popup
{
    /// <summary>
    /// 页面构造函数，用于自动选择方案
    /// </summary>
    /// <param name="popupType">0代表VideoListPage; 1代表VideoCheckPage; 2代表VideoSubPage;</param>
    /// <param name="setRegexIndex">指定一个方案</param>
    /// <param name="recommendRegex">根据直播源的原始数据，获得的推荐选择的方案</param>
    public RegexSelectPopup(int popupType, int setRegexIndex, string recommendRegex)
    {
        InitializeComponent();

        PopupType = popupType;
        SetRegexIndex= setRegexIndex;
        RecommendRegex = recommendRegex;

        InitRegexList();
    }

    /// <summary>
    /// 要操作的列表所在的页面
    /// </summary>
    public int PopupType;
    /// <summary>
    /// 指定一个方案
    /// </summary>
    public int SetRegexIndex;
    /// <summary>
    /// 推荐选择的方案
    /// </summary>
    public string RecommendRegex;
    public bool isOKBtnClicked;

    public void InitRegexList()
    {
        List<string> RegexOption = new List<string>() { "规则1", "规则2", "规则3", "规则4", "规则5" };
        RegexSelectBox.ItemsSource = RegexOption;

        RegexSelectBox.SelectedIndex=SetRegexIndex;
        RecommendRegexTb.Text=RecommendRegex;
    }
    private void SaveOptionBtn_Clicked(object sender, EventArgs e)
    {
        if (PopupType == 1)
        {
            VideoCheckPage.videoCheckPage.RegexSelectIndex=RegexSelectBox.SelectedIndex;
            VideoCheckPage.videoCheckPage.RegexOption1=RegexOptionCB.IsChecked;
        }
        else if (PopupType == 2)
        {
            VideoSubPage.videoSubPage.RegexSelectIndex=RegexSelectBox.SelectedIndex;
            VideoSubPage.videoSubPage.RegexOption1=RegexOptionCB.IsChecked;
        }

        isOKBtnClicked = true;
        this.CloseAsync();
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        //暂时不做询问提示框
        isOKBtnClicked = false;
        this.CloseAsync();
    }

    private void RegexSelectTipBtn_Clicked(object sender, EventArgs e)
    {
        VideoPrevPage.videoPrevPage.PopShowMsg("帮助信息", new MsgManager().GetRegexOptionTip(), "关闭");
    }
}