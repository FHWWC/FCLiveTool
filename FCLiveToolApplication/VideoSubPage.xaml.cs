using CommunityToolkit.Maui.Views;
using FCLiveToolApplication.Popup;
using System.Xml.Serialization;

namespace FCLiveToolApplication;

public partial class VideoSubPage : ContentPage
{
    public VideoSubPage()
    {
        InitializeComponent();
    }

    public static VideoSubPage videoSubPage;
    public List<VideoDetailList> CurrentVideoSubDetailList = new List<VideoDetailList>();
    public string CurrentVSLName;
    public string AllVideoData;
    public int VSDLCurrentPageIndex = 1;
    public int VSDLMaxPageIndex;
    public const int VSDL_COUNT_PER_PAGE = 100;
    public const double D_VSDL_COUNT_PER_PAGE = 100.0;
    public int RegexSelectIndex = 2;
    public string RecommendRegex = "3";
    public bool RegexOption1 = false;
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoSubPage != null)
        {
            return;
        }

        videoSubPage=this;
        ReadLocalSubList();
    }
    public async void ReadLocalSubList()
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！", "确定");
            return;
        }

        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var localStr = File.ReadAllText(dataPath+"\\VideoSubList.log");
                    if (!string.IsNullOrWhiteSpace(localStr))
                    {
                        var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(localStr));

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            VideoSubList.ItemsSource = tlist;
                        });
                    }
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "读取本地数据时出错！", "确定");
            }

        }

    }
    private void VideoSubListAddItemBtn_Clicked(object sender, EventArgs e)
    {
        VideoSubListPopup videoSubListPopup = new VideoSubListPopup(0);
        //videoSubListPopup.MainGrid.HeightRequest = Window.Height/1.5;
        videoSubListPopup.MainGrid.WidthRequest =Window.Width/1.5;
        this.ShowPopup(videoSubListPopup);
    }

    private void VSLEditBtn_Clicked(object sender, EventArgs e)
    {
        Button button=sender as Button;

        VideoSubListPopup videoSubListPopup = new VideoSubListPopup(1,button.CommandParameter.ToString());
        //videoSubListPopup.MainGrid.HeightRequest = Window.Height/1.5;
        videoSubListPopup.MainGrid.WidthRequest =Window.Width/1.5;
        this.ShowPopup(videoSubListPopup);
    }

    private async void VSLRemoveBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;

        if(!await DisplayAlert("提示信息","你要删除订阅 "+ button.CommandParameter.ToString()+" 吗？", "确定", "取消"))
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(button.CommandParameter.ToString()))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }


        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                    var items = tlist.FirstOrDefault(p => p.SubName==button.CommandParameter.ToString());
                    if (items != null)
                    {
                        tlist.Remove(items);
                        RefreshVSL(tlist);

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                        await DisplayAlert("提示信息", "删除订阅成功！", "确定");

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "操作数据时出错，请刷新重试！", "确定");
            }

        }
        else
        {
            await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
        }

    }

    private async void VSLEnabledUpdateToogle_Toggled(object sender, ToggledEventArgs e)
    {
        Switch button = sender as Switch;
        if (button.BindingContext is null)
        {
            return;
        }

        string itemSubName = (button.BindingContext as VideoSubList).SubName;
        if (string.IsNullOrWhiteSpace(itemSubName))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }


        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                    var items = tlist.FirstOrDefault(p => p.SubName==itemSubName);
                    if (items != null)
                    {
                        items.IsEnabledUpdate=button.IsToggled;
                        //不需要刷新，也避免了反复触发
                        //RefreshVSL(tlist);

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "操作数据时出错，请刷新重试！", "确定");
            }

        }
        else
        {
            await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
        }

    }

    public async void PopShowMsg(string msg)
    {
        await DisplayAlert("提示信息", msg, "确定");
    }
    public async Task<bool> PopShowMsgAndReturn(string msg)
    {
        return await DisplayAlert("提示信息", msg, "确定", "取消");
    }
    public void RefreshVSL(List<VideoSubList> tlist)
    {
        VideoSubList.ItemsSource = tlist.Take(tlist.Count);
    }
    private async void VSLUpdateBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string itemSubName = (button.BindingContext as VideoSubList).SubName;
        if (string.IsNullOrWhiteSpace(itemSubName))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }

        await UpdateSubFunc(itemSubName);

        await DisplayAlert("提示信息", "更新订阅成功！", "确定");
    }

    public async Task UpdateSubFunc(string itemSubName)
    {
        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
            try
            {
                if (File.Exists(dataPath+"\\VideoSubList.log"))
                {
                    var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                    var items = tlist.FirstOrDefault(p => p.SubName==itemSubName);
                    if (items != null)
                    {
                        VideoSubDetailListRing.IsRunning=true;

                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                if (!string.IsNullOrWhiteSpace(items.UserAgent))
                                {
                                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(items.UserAgent);
                                }
                                HttpResponseMessage response = await httpClient.GetAsync(items.SubURL);

                                int statusCode = (int)response.StatusCode;
                                if (!response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("提示信息", "请求失败！" + "HTTP错误代码：" + statusCode, "确定");
                                    VideoSubDetailListRing.IsRunning=false;
                                    return;
                                }

                                AllVideoData = await response.Content.ReadAsStringAsync();

                                //如果当前右侧列表里展示的数据是左侧选中的订阅，则执行更新右侧的UI。
                                var selectItem = VideoSubList.SelectedItem;
                                if(selectItem != null)
                                {
                                    var selectSubName = (selectItem as VideoSubList).SubName;
                                    if(selectSubName==itemSubName)
                                    {
                                        AutoSelectRecommendRegex();
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("提示信息", "更新数据失败，请稍后重试！", "确定");
                            VideoSubDetailListRing.IsRunning=false;
                            return;
                        }

                        VideoSubDetailListRing.IsRunning=false;
                        items.SubDetailStr = AllVideoData;
                        items.SubVDL=CurrentVideoSubDetailList;

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                        //await DisplayAlert("提示信息", "更新订阅成功！", "确定");

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "更新数据时出错，请刷新重试！", "确定");
            }

        }
        else
        {
            await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
        }
    }

    public async void AutoSelectRecommendRegex()
    {
        if (string.IsNullOrWhiteSpace(AllVideoData))
        {
            await DisplayAlert("提示信息", "参数错误！", "确定");
            return;
        }

        int tvglogoIndex = AllVideoData.IndexOf("tvg-logo=");
        int tvgnameIndex = AllVideoData.IndexOf("tvg-name=");
        //int OldSelectedIndex = RegexSelectBox.SelectedIndex;

        if (tvglogoIndex>-1&&tvgnameIndex>-1)
        {
            if (tvgnameIndex<tvglogoIndex)
            {
                RegexSelectIndex=1;
                //RegexSelectBox.SelectedIndex=1;
                //RecommendRegexTb.Text = "2";
                RecommendRegex="2";
            }
            else
            {
                RegexSelectIndex=0;
                //RegexSelectBox.SelectedIndex=0;
                //RecommendRegexTb.Text = "1";
                RecommendRegex="1";
            }
        }
        else if (tvglogoIndex>-1)
        {
            RegexSelectIndex=2;
            //RegexSelectBox.SelectedIndex=2;
            //RecommendRegexTb.Text = "3";
            RecommendRegex="3";
        }
        else if (tvglogoIndex<0&&tvgnameIndex<0&&!AllVideoData.Contains("#EXTINF:"))
        {
            RegexSelectIndex=4;
            //RegexSelectBox.SelectedIndex=4;
            //RecommendRegexTb.Text = "5";
            RecommendRegex="5";
        }
        else
        {
            RegexSelectIndex=3;
            //RegexSelectBox.SelectedIndex=3;
            //RecommendRegexTb.Text = "4";
            RecommendRegex="4";
        }


        LoadDataToCheckList(AllVideoData);
        //手动触发
        /*
                 if (OldSelectedIndex==RegexSelectBox.SelectedIndex)
                {
                    LoadDataToCheckList();
                }
         */

    }
    public async void LoadDataToCheckList(string allVideoData)
    {
        if (string.IsNullOrWhiteSpace(allVideoData))
        {
            await DisplayAlert("提示信息", "参数错误！", "确定");
            return;
        }

        //VideoSubDetailListRing.IsRunning=true;

        RegexManager regexManager = new RegexManager();
        CurrentVideoSubDetailList=regexManager.DoRegex(allVideoData, regexManager.GetRegexOptionIndex(RegexOption1, (RegexSelectIndex+1).ToString()));

        ProcessPageJump(CurrentVideoSubDetailList, 1);
    }

    private async void OpenRegexPageBtn_Clicked(object sender, EventArgs e)
    {
        RegexSelectPopup regexSelectPopup = new RegexSelectPopup(2, RegexSelectIndex, RecommendRegex);
        await this.ShowPopupAsync(regexSelectPopup);

        if (regexSelectPopup.isOKBtnClicked)
        {
            LoadDataToCheckList(AllVideoData);
        }
    }

    private void VideoSubDetailList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            VideoSubDetailList.SelectedItem=null;

            if (VideoSubDetailList.ItemsSource is not null&&VideoSubDetailList.ItemsSource.Cast<VideoDetailList>().Count()>0)
            {
                VSDLIfmText.IsVisible=false;
                VSDLPagePanel.IsVisible=true;
            }
            else
            {
                VSDLIfmText.IsVisible=true;
                VSDLPagePanel.IsVisible=false;
            }
        }
    }

    private void VSDLBackBtn_Clicked(object sender, EventArgs e)
    {
        if (VSDLCurrentPageIndex<=1)
        {
            return;
        }

        ProcessPageJump(CurrentVideoSubDetailList, VSDLCurrentPageIndex-1);
    }

    private async void VSDLJumpBtn_Clicked(object sender, EventArgs e)
    {
        int TargetPage = 1;
        if (!int.TryParse(VSDLPageTb.Text, out TargetPage))
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }
        if (TargetPage<1||TargetPage>VSDLMaxPageIndex)
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }

        ProcessPageJump(CurrentVideoSubDetailList, TargetPage);
    }

    private void VSDLNextBtn_Clicked(object sender, EventArgs e)
    {
        if (VSDLCurrentPageIndex>=VSDLMaxPageIndex)
        {
            return;
        }

        ProcessPageJump(CurrentVideoSubDetailList, VSDLCurrentPageIndex+1);
    }

    /// <summary>
    /// 跳转页面的操作
    /// </summary>
    /// <param name="videoCheckList">要操作的列表</param>
    /// <param name="TargetPage">目标页码</param>
    public void ProcessPageJump(List<VideoDetailList> videoCheckList, int TargetPage)
    {
        VSDLMaxPageIndex= (int)Math.Ceiling(videoCheckList.Count/D_VSDL_COUNT_PER_PAGE);

        VSDLCurrentPageIndex=TargetPage;
        VSDLCurrentPage.Text=TargetPage+"/"+VSDLMaxPageIndex;

        MakeVideosDataToPage(videoCheckList, (VSDLCurrentPageIndex-1)*VSDL_COUNT_PER_PAGE);
    }
    public void MakeVideosDataToPage(List<VideoDetailList> list, int skipcount)
    {
        if (list.Count()<1)
        {
            VideoSubDetailList.ItemsSource=new List<VideoDetailList>() { };
            return;
        }

        if (VSDLCurrentPageIndex==VSDLMaxPageIndex)
        {
            VideoSubDetailList.ItemsSource=list.Skip(skipcount).Take(list.Count-skipcount);
        }
        else
        {
            VideoSubDetailList.ItemsSource=list.Skip(skipcount).Take(VSDL_COUNT_PER_PAGE);
        }

        if (VSDLCurrentPageIndex>=VSDLMaxPageIndex)
        {
            VSDLBackBtn.IsEnabled = true;
            VSDLNextBtn.IsEnabled = false;
        }
        else if (VSDLCurrentPageIndex<=1)
        {
            VSDLBackBtn.IsEnabled = false;
            VSDLNextBtn.IsEnabled = true;
        }
        else
        {
            VSDLBackBtn.IsEnabled = true;
            VSDLNextBtn.IsEnabled = true;
        }

    }

    private async void VideoSubList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
       VideoSubList videoSubList=e.Item as VideoSubList;

        if (videoSubList!=null)
        {
            string dataPath = new APPFileManager().GetOrCreateAPPDirectory("AppData\\VideoSubList");
            if (dataPath != null)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoSubList>));
                try
                {
                    if (File.Exists(dataPath+"\\VideoSubList.log"))
                    {
                        var tlist = (List<VideoSubList>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"\\VideoSubList.log")));

                        var items = tlist.FirstOrDefault(p => p.SubName==videoSubList.SubName);
                        if (items != null)
                        {
                            if(string.IsNullOrWhiteSpace(items.SubDetailStr))
                            {
                                await DisplayAlert("提示信息", "检测到本地订阅数据为空，请尝试更新订阅获取最新数据！", "确定");
                                return;
                            }

                            AllVideoData = items.SubDetailStr;
                            AutoSelectRecommendRegex();

                        }
                        else
                        {
                            await DisplayAlert("提示信息", "未查找到当前订阅名称对应的本地数据，请重试！", "确定");
                        }

                    }
                    else
                    {
                        await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
                    }
                }
                catch (Exception)
                {
                    await DisplayAlert("提示信息", "读取本地数据时出错，请刷新重试，或更新订阅！", "确定");
                }

            }
            else
            {
                await DisplayAlert("提示信息", "源文件丢失！请重新创建！", "确定");
            }
        }
       
    }

    private async void VideoSubListUpdateAllBtn_Clicked(object sender, EventArgs e)
    {
        var videoSubList = VideoSubList.ItemsSource;
        if (videoSubList is null)
        {
            await DisplayAlert("提示信息", "当前列表为空！", "确定");
            return;
        }

        var tlist = videoSubList.Cast<VideoSubList>().Where(p => p.IsEnabledUpdate).ToList();
        if (tlist.Count < 1)
        {
            await DisplayAlert("提示信息", "当前订阅列表内没有启用更新的订阅！", "确定");
            return;
        }
        
        VideoSubListUpdateAllBtn.IsEnabled=false;

        for (int i = 0; i < tlist.Count; i++)
        {
            await UpdateSubFunc(tlist[i].SubName);
        }

        VideoSubListUpdateAllBtn.IsEnabled=true;
        await DisplayAlert("提示信息", "已更新全部"+tlist.Count+"个订阅！", "确定");

    }
}