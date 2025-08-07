using CommunityToolkit.Maui.Extensions;
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
    public List<string[]> M3U8PlayList = new List<string[]>();
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoSubPage != null)
        {
            return;
        }

        videoSubPage=this;

        new Thread(async()=>
        {
            ReadLocalSubList();
        }).Start();

    }
    public async void ReadLocalSubList()
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！", "确定");
                return;
            });
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
                await MainThread.InvokeOnMainThreadAsync(async() =>
                {
                    await DisplayAlert("提示信息", "读取本地数据时出错！", "确定");
                });

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
        string itemSubName = (button.BindingContext as VideoSubList).SubName;

        if (string.IsNullOrWhiteSpace(itemSubName))
        {
            await DisplayAlert("提示信息", "参数错误！请重试！", "确定");
            return;
        }
        if (!await DisplayAlert("提示信息","你要删除订阅 "+itemSubName+" 吗？", "确定", "取消"))
        {
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

        UpdateSubFunc(itemSubName);
    }
    public async void UpdateSubFunc(string itemSubName)
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
                                AutoSelectRecommendRegex();
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

                        await DisplayAlert("提示信息", "更新订阅成功！", "确定");

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
    public async Task<string> UpdateAllSubFunc(string itemSubName)
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
                                    return "请求失败！" + "HTTP错误代码：" + statusCode;
                                }

                                items.SubDetailStr = await response.Content.ReadAsStringAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            return "更新数据失败，请稍后重试！";
                        }

                        //items.SubVDL=CurrentVideoSubDetailList;

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                        return "";

                    }
                    else
                    {
                        return "未查找到当前订阅名称对应的本地数据，请重试！";
                    }
                }
                else
                {
                    return "源文件丢失！请重新创建！";
                }
            }
            catch (Exception)
            {
                return "更新数据时出错，请刷新重试！";
            }

        }
        else
        {
            return "源文件丢失！请重新创建！";
        }
    }

    public string DeleteSubFunc(string itemSubName)
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
                        tlist.Remove(items);
                        //批量操作不在这里刷新

                        using (StringWriter sw = new StringWriter())
                        {
                            xmlSerializer.Serialize(sw, tlist);
                            File.WriteAllText(dataPath+"\\VideoSubList.log", sw.ToString());
                        }

                        //return "删除订阅成功！";
                        return "";
                    }
                    else
                    {
                        return "未查找到当前订阅名称对应的本地数据，请重试！";
                    }
                }
                else
                {
                    return "源文件丢失！请重新创建！";
                }
            }
            catch (Exception)
            {
                return "操作数据时出错，请刷新重试！";
            }

        }
        else
        {
            return "源文件丢失！请重新创建！";
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
        VideoSubDetailListRing.IsRunning=true;

        await Task.Run(async() =>
        {
            for (int i = 0; i < tlist.Count; i++)
            {
                await UpdateAllSubFunc(tlist[i].SubName);
            }

        }).ContinueWith(async (p) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                //如果当前右侧列表里展示的数据是左侧选中的订阅，则执行更新右侧的UI。
                var selectItem = VideoSubList.SelectedItem;
                if (selectItem != null)
                {
                    AllVideoData = (selectItem as VideoSubList).SubDetailStr;
                    AutoSelectRecommendRegex();
                }

                VideoSubListUpdateAllBtn.IsEnabled=true;
                VideoSubDetailListRing.IsRunning=false;
                await DisplayAlert("提示信息", "已执行更新"+tlist.Count+"个订阅！", "确定");
            });
        });

    }

    private async void VideoSubDetailList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoDetailList detail = e.Item as VideoDetailList;

        string readresult = await new VideoManager().DownloadAndReadM3U8File(M3U8PlayList, new string[] { detail.SourceName, detail.SourceLink });
        if (readresult!="")
        {
            await DisplayAlert("提示信息", readresult, "确定");
            return;
        }


        M3U8PlayList.Insert(0, new string[] { "默认", detail.SourceLink });
        string[] MOptions = new string[M3U8PlayList.Count];
        MOptions[0]="默认\n";
        string WantPlayURL = detail.SourceLink;

        if (M3U8PlayList.Count > 2)
        {
            for (int i = 1; i<M3U8PlayList.Count; i++)
            {
                MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
            }

            string MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                return;
            }
            else if (!MSelectResult.Contains("默认"))
            {
                int tmindex = Convert.ToInt32(MSelectResult.Remove(0, 1).Split("】")[0]);
                WantPlayURL=M3U8PlayList[tmindex][1];
            }

        }


        new VideoManager().UpdatePrevPagePlaylist(M3U8PlayList);
        VideoPrevPage.videoPrevPage.VideoWindow.Source=WantPlayURL;
        VideoPrevPage.videoPrevPage.VideoWindow.Play();
        VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;


        var mainpage = (Shell)App.Current.Windows[0].Page;
        mainpage.CurrentItem = mainpage.Items.FirstOrDefault();
        await mainpage.Navigation.PopToRootAsync();
    }

    private async void VSLRemoveCheckedBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoSubList.ItemsSource is null)
        {
            await DisplayAlert("提示信息", "当前列表为空！", "确定");
            return;
        }

        var tlist = VideoSubList.ItemsSource.Cast<VideoSubList>().Where(p => p.IsSelected).ToList();
        if (tlist.Count < 1)
        {
            await DisplayAlert("提示信息", "当前订阅列表内没有被勾选的订阅！", "确定");
            return;
        }
        if (!await DisplayAlert("提示信息", "你要批量删除"+tlist.Count+"个订阅吗？", "确定", "取消"))
        {
            return;
        }

        await Task.Run(() =>
        {
            for (int i = 0; i<tlist.Count; i++)
            {
                DeleteSubFunc(tlist[i].SubName);
            }

        }).ContinueWith(async (p) =>
        {
            ReadLocalSubList();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("提示信息", "已执行移除"+tlist.Count+"条数据！", "确定");
            });
        });

        /*
                 var tlist=VideoSubList.ItemsSource.Cast<VideoSubList>().ToList();
                var tcount = tlist.Where(p => p.IsSelected).Count();
                if (tcount < 1)
                {
                    await DisplayAlert("提示信息", "当前订阅列表内没有被勾选的订阅！", "确定");
                    return;
                }

                int successRemoveCount = 0;
                await Task.Run(() =>
                {
                    for (int i = tlist.Count-1; i>=0; i--)
                    {
                        if (tlist[i].IsSelected)
                        {
                            if(DeleteSubFunc(tlist[i].SubName)=="")
                            {
                                successRemoveCount++;
                            }
                            tlist.RemoveAt(i);
                        }
                    }

                }).ContinueWith(async(p)=>
                {
                    RefreshVSL(tlist);
                    await DisplayAlert("提示信息", "已成功移除"+successRemoveCount+"条数据！", "确定");
                },TaskScheduler.FromCurrentSynchronizationContext());
         */
    }

    private async void VideoSubListSelectCB_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (VideoSubList.ItemsSource is null)
        {
            await DisplayAlert("提示信息", "当前列表为空！", "确定");
            return;
        }

        var tlist = VideoSubList.ItemsSource.Cast<VideoSubList>().ToList();   
        
        tlist.ForEach(p =>
        {
            p.IsSelected=e.Value;
        });

        VideoSubList.ItemsSource=tlist;
    }
}