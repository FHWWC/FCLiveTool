using Microsoft.Maui.Platform;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Microsoft.Maui.Controls.Button.ButtonContentLayout;
using static Microsoft.Maui.Controls.Button;
using static System.Net.WebRequestMethods;
using System.IO;
using System.Reflection.PortableExecutable;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls.PlatformConfiguration;
using CommunityToolkit.Maui.Storage;
using System.Text;
using Encoding = System.Text.Encoding;
using System.Net;


#if WINDOWS
using Windows.Storage.Pickers;
using Windows.Media.Protection.PlayReady;
#endif

namespace FCLiveToolApplication;

public partial class VideoListPage : ContentPage
{
    public VideoListPage()
    {
        InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        LoadVideos();
        InitRegexList();
        DeviceDisplay.MainDisplayInfoChanged+=DeviceDisplay_MainDisplayInfoChanged;
    }
    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
#if ANDROID

        if (DeviceDisplay.Current.MainDisplayInfo.Orientation==DisplayOrientation.Landscape)
        {

        }
        else
        {

        }
#endif
    }
    /// <summary>
    /// 初始化规则列表
    /// </summary>
    public void InitRegexList()
    {
        RegexSelectBox.BindingContext = this;
        RegexOption = new List<string>() { "自动匹配", "规则1", "规则2", "规则3", "规则4", "规则5" };
        RegexSelectBox.ItemsSource = RegexOption;

        //RegexSelectBox.SelectedIndex = 0;
    }

    public int VLCurrentPageIndex = 1;
    public int VLMaxPageIndex;
    public const int VL_COUNT_PER_PAGE = 15;
    public string AllVideoData;
    public int[] UseGroup;
    public List<VideoList> CurrentVideosList;
    public string CurrentVURL = "";
    public string RecommendReg = "0";
    public List<string> RegexOption;
    public bool IgnoreSelectionEvents = false;

    private void Question_Clicked(object sender, EventArgs e)
    {
        ImageButton button = sender as ImageButton;

        switch (button.StyleId)
        {
            case "VideoListQue":
#if ANDROID
                DisplayAlert("帮助信息", "所有直播源均来自于网络，我们不对直播里的内容负责，如有侵权请联系我们删除"+
                "\n若要刷新列表，下拉该列表即可刷新，也可以点击本按钮左侧的刷新按钮刷新。", "关闭");
#else
                DisplayAlert("帮助信息", "所有直播源均来自于网络，我们不对直播里的内容负责，如有侵权请联系我们删除", "关闭");
#endif
                break;
            case "VideosQue":
#if ANDROID
                DisplayAlert("帮助信息", "由于某些M3U8内URL使用的是相对地址而不是绝对地址，故APP无法播放，并非是APP的bug，请知悉"+
                "\n若要刷新列表，下拉该列表即可刷新，也可以点击本按钮左侧的刷新按钮刷新。", "关闭");
#else
                DisplayAlert("帮助信息", "由于某些M3U8内URL使用的是相对地址而不是绝对地址，故APP无法播放，并非是APP的bug，请知悉", "关闭");
#endif

                break;
        }
    }

    private async void DownloadBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoDetailList.SelectedItem==null)
        {
            await DisplayAlert("提示信息", "请先在列表里选择一条直播源！", "确定");
            return;
        }
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存文件！", "确定");
            return;
        }


        int statusCode;
        VideoDetailList selectVDL = VideoDetailList.SelectedItem as VideoDetailList;

        using (HttpClient httpClient = new HttpClient())
        {

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            /*
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, selectVDL.SourceLink);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
           */

            HttpResponseMessage response=null;

            try
            {
                response = await httpClient.GetAsync(selectVDL.SourceLink);

                statusCode=(int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("提示信息", "下载文件失败，请稍后重试！", "确定");
                    return;
                }
            }
            catch(Exception)
            {
                await DisplayAlert("提示信息", "连接对方服务器失败，请检查网络！", "确定");
                return;
            }

            //var httpstream = await httpClient.GetStreamAsync(selectVDL.SourceLink)
            using (var httpstream = await response.Content.ReadAsStreamAsync())
            {
                var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, selectVDL.SourceName+".m3u8", httpstream, CancellationToken.None);

                if (fileSaver.IsSuccessful)
                {
                    await DisplayAlert("提示信息", "文件已成功下载至：\n"+fileSaver.FilePath, "确定");
                }
                else
                {
                    await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                }
            }

        }



        /*
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("")))
         */

    }
    /*
             var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
{
    { DevicePlatform.iOS, new[] { "com.apple.mpegurl", "public.m3u8-playlist" , "application/vnd.apple.mpegurl" } },
    { DevicePlatform.macOS, new[] { "public.m3u8", "application/vnd.apple.mpegurl" } },
    { DevicePlatform.Android, new[] { "application/x-mpegURL" , "application/vnd.apple.mpegurl" } },
    { DevicePlatform.WinUI, new[] { ".m3u8" ,".m3u"} }
});

        var filePicker = await FilePicker.PickAsync(new PickOptions()
        {
            PickerTitle="",
            FileTypes=fileTypes
        });

        if (filePicker is not null)
        {
            await DisplayAlert("", filePicker.FullPath, "确定");
        }

     */
    private async void EditBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoDetailList.SelectedItem==null)
        {
            await DisplayAlert("提示信息", "请先在列表里选择一条直播源！", "确定");
            return;
        }

        //string appDataDirectory = FileSystem.AppDataDirectory;
    }
    private async void DeleteBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoDetailList.SelectedItem==null)
        {
            await DisplayAlert("提示信息", "请先在列表里选择一条直播源！", "确定");
            return;
        }

    }

    /// <summary>
    /// 左侧M3U列表点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VideosList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoList videoList = e.Item as VideoList;
        LoadVideoDetail(videoList.SourceLink, videoList.RecommendReg);
    }
    /// <summary>
    /// 右侧M3U8列表点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VideoDetailList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (!VDLToogleBtn.IsToggled)
        {
            VideoDetailList detail = e.Item as VideoDetailList;

            VideoPrevPage.videoPrevPage.VideoWindow.Source=detail.SourceLink;
            VideoPrevPage.videoPrevPage.VideoWindow.Play();
            VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;
        }
    }
    /// <summary>
    /// 加载M3U数据
    /// </summary>
    public async void LoadVideos()
    {
        VideosListRing.IsRunning=true;
        //未加载成功不覆盖数据，仍可操作原来的数据
        try
        {
            //暂时不在API里获取分页数据
            string videodata = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetVList");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoList>));

            //暂时忽略小于15的情况
            CurrentVideosList = (List<VideoList>)xmlSerializer.Deserialize(new StringReader(videodata));
            VideosList.ItemsSource= CurrentVideosList.Take(15);

            //暂时忽略页数小于1的情况
            VLMaxPageIndex= (int)Math.Ceiling(CurrentVideosList.Count/15.0);
            SetPage(1);
            VLBackBtn.IsEnabled=false;
            VLNextBtn.IsEnabled=true;

        }
        catch (Exception)
        {
            await DisplayAlert("提示信息", "获取数据失败，请稍后重试！", "确定");
        }

        VideosListRing.IsRunning=false;
    }
    /// <summary>
    /// 加载M3U8数据
    /// </summary>
    public async void LoadVideoDetail(string url, string reg)
    {
        VideoDataListRing.IsRunning = true;
        try
        {
            AllVideoData = await new HttpClient().GetStringAsync("https://fclivetool.com/api/APPGetVD?url="+url);

            RecommendReg=reg;
            CurrentVURL =url;

            //更改索引会触发SelectedIndexChanged事件，所以在仅刷新列表的时候选择不执行事件内后续代码
            IgnoreSelectionEvents=true;
            RegexSelectBox.SelectedIndex = 0;
            IgnoreSelectionEvents=false;

            VideoDetailList.ItemsSource= DoRegex(AllVideoData, RecommendReg);

            VDLIfmText.Text="";
        }
        catch (Exception)
        {
            await DisplayAlert("提示信息", "获取数据失败，请稍后重试！", "确定");

            /*
                        var link = videoList.SourceLink;
                        if (link.Contains("githubusercontent.com")||link.Contains("github.com"))
                        {
                            await DisplayAlert("提示信息", "您所在的地区访问GitHub可能不顺畅", "确定");
                        }
                        else
                        {
                            await DisplayAlert("提示信息", "获取数据失败，请稍后重试！", "确定");
                        }
            
             */
        }

        VideoDataListRing.IsRunning = false;
    }
    /// <summary>
    /// 获取当前选择的规则索引
    /// </summary>
    /// <returns></returns>
    public string GetRegexOptionIndex()
    {
        bool isOnlyM3U8 = RegexOptionCB.IsChecked;
        switch (RegexSelectBox.SelectedIndex.ToString())
        {
            case "1":
                if (!isOnlyM3U8)
                {
                    return "1.2";
                }
                return "1";
            case "2":
                if (!isOnlyM3U8)
                {
                    return "2.2";
                }
                return "2";
            case "3":
                if (!isOnlyM3U8)
                {
                    return "3.2";
                }
                return "3";
            case "4":
                return "4";
            case "5":
                return "5";
            default:
                return "0";
        }

    }
    /// <summary>
    /// 使用正则表达式解析直播源数据，直播源数据是包含若干个M3U8直播源的字符串
    /// </summary>
    /// <param name="videodata">直播源数据</param>
    /// <param name="recreg">正则表达式</param>
    /// <returns>正则表达式匹配出的列表</returns>
    public List<VideoDetailList> DoRegex(string videodata, string recreg)
    {
        MatchCollection match = Regex.Matches(videodata, UseRegex(recreg));

        List<VideoDetailList> result = new List<VideoDetailList>();
        for (int i = 0; i<match.Count(); i++)
        {
            VideoDetailList videoDetail = new VideoDetailList()
            {
                //ID，台标，台名，直播源地址
                Id=i,
                LogoLink=match[i].Groups[UseGroup[0]].Value=="" ? "fclive_tvicon.png" : match[i].Groups[UseGroup[0]].Value,
                SourceName=match[i].Groups[UseGroup[1]].Value,
                SourceLink=match[i].Groups[UseGroup[2]].Value
            };
            //videoDetail.LogoLink=videoDetail.LogoLink=="" ? "fclive_tvicon.png" : videoDetail.LogoLink;

            result.Add(videoDetail);
        }

        return result;
    }
    /// <summary>
    /// 根据提供的索引来获取对应的正则表达式
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns>索引对应的正则表达式</returns>
    public string UseRegex(string index)
    {
        switch (index)
        {
            case "1":
                UseGroup =new int[] { 1, 2, 3 };
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,))";
            //1.2为1的不限制M3U8后缀的版本
            case "1.2":
                UseGroup =new int[] { 1, 2, 3 };
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n|,))";
            //只有2方案是先匹配台名后匹配台标
            case "2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,))";
            //2.2为2的不限制M3U8后缀的版本
            case "2.2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n|,))";
            case "3":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,)))";
            //3.2为3的不限制M3U8后缀的版本
            case "3.2":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n|,)))";
            case "4":
                UseGroup =new int[] { 3, 5, 8 };
                return @",?((tvg-logo=""([^""]*)"")(.*?)),(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n|,)))";
            case "5":
                UseGroup =new int[] { 2, 5, 7 };
                return @"(((http|https)://\S+)(,))?(.*?)(,)((http|https)://\S+(?=\n|,|\s{1}))";
            default:
                return "";
        }

    }
    /// <summary>
    /// 重置M3U列表的页码
    /// </summary>
    /// <param name="index"></param>
    public void SetPage(int index)
    {
        VLCurrentPageIndex=index;
        VLCurrentPage.Text=index+"/"+VLMaxPageIndex;
    }
    private void VLBackBtn_Clicked(object sender, EventArgs e)
    {
        if (VLCurrentPageIndex<=1)
        {
            return;
        }

        SetPage(VLCurrentPageIndex-1);

        int skipcount = (VLCurrentPageIndex-1)*VL_COUNT_PER_PAGE;
        VideosList.ItemsSource= CurrentVideosList.Skip(skipcount).Take(VL_COUNT_PER_PAGE);

        if (VLCurrentPageIndex<=1)
        {
            VLBackBtn.IsEnabled = false;
            VLNextBtn.IsEnabled = true;
        }
        else
        {
            VLBackBtn.IsEnabled = true;
            VLNextBtn.IsEnabled = true;
        }
    }

    private void VLNextBtn_Clicked(object sender, EventArgs e)
    {
        if (VLCurrentPageIndex>=VLMaxPageIndex)
        {
            return;
        }

        SetPage(VLCurrentPageIndex+1);

        int skipcount = (VLCurrentPageIndex-1)*VL_COUNT_PER_PAGE;
        if (VLCurrentPageIndex==VLMaxPageIndex)
        {
            VideosList.ItemsSource= CurrentVideosList.Skip(skipcount).Take(CurrentVideosList.Count-skipcount);
        }
        else
        {
            VideosList.ItemsSource= CurrentVideosList.Skip(skipcount).Take(VL_COUNT_PER_PAGE);
        }

        if (VLCurrentPageIndex>=VLMaxPageIndex)
        {
            VLBackBtn.IsEnabled = true;
            VLNextBtn.IsEnabled = false;
        }
        else
        {
            VLBackBtn.IsEnabled = true;
            VLNextBtn.IsEnabled = true;
        }
    }

    private async void VLJumpBtn_Clicked(object sender, EventArgs e)
    {

        int TargetPage = 1;
        if (!int.TryParse(VLPageTb.Text, out TargetPage))
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }
        if (TargetPage<1||TargetPage>VLMaxPageIndex)
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }


        SetPage(TargetPage);

        int skipcount = (VLCurrentPageIndex-1)*VL_COUNT_PER_PAGE;
        if (VLCurrentPageIndex==VLMaxPageIndex)
        {
            VideosList.ItemsSource= CurrentVideosList.Skip(skipcount).Take(CurrentVideosList.Count-skipcount);
        }
        else
        {
            VideosList.ItemsSource= CurrentVideosList.Skip(skipcount).Take(VL_COUNT_PER_PAGE);
        }

        if (VLCurrentPageIndex>=VLMaxPageIndex)
        {
            VLBackBtn.IsEnabled = true;
            VLNextBtn.IsEnabled = false;
        }
        else if (VLCurrentPageIndex<=1)
        {
            VLBackBtn.IsEnabled = false;
            VLNextBtn.IsEnabled = true;
        }
        else
        {
            VLBackBtn.IsEnabled = true;
            VLNextBtn.IsEnabled = true;
        }
    }

    private void VLRefreshBtn_Clicked(object sender, EventArgs e)
    {
        LoadVideos();
    }

    private void VideosList_Refreshing(object sender, EventArgs e)
    {
        LoadVideos();

        //不使用ListView自己的加载圈
        VideosList.IsRefreshing=false;
    }

    private void VLToolbarBtn_Clicked(object sender, EventArgs e)
    {
        //收回
        if (VideosListPanel.TranslationY==0)
        {
            var animation = new Animation(v => VideosListPanel.TranslationY  = v, 0, -1000);
            animation.Commit(this, "VLPAnimation", 16, 500, Easing.CubicInOut);

            VideosList.IsEnabled = true;
        }
        //展开
        else
        {
            var animation = new Animation(v => VideosListPanel.TranslationY  = v, -1000, 0);
            animation.Commit(this, "VLPAnimation", 16, 500, Easing.CubicOut);

            VideosList.IsEnabled = false;
        }

    }

    private void VDLRefreshBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            return;
        }

        LoadVideoDetail(CurrentVURL, RecommendReg);
    }

    private void VDLToolbarBtn_Clicked(object sender, EventArgs e)
    {

        //收回
        if (VideoDataListPanel.TranslationY==0)
        {
            var animation = new Animation(v => VideoDataListPanel.TranslationY  = v, 0, -1000);
            animation.Commit(this, "VLPAnimation", 16, 500, Easing.CubicInOut);

            VideoDetailList.IsEnabled=true;
        }
        //展开
        else
        {
            var animation = new Animation(v => VideoDataListPanel.TranslationY  = v, -1000, 0);
            animation.Commit(this, "VLPAnimation", 16, 500, Easing.CubicOut);

            VideoDetailList.IsEnabled=false;
        }
    }
    private void RegexSelectBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            DisplayAlert("提示信息", "当前列表为空！", "确定");
            return;
        }
        if(IgnoreSelectionEvents)
        {
            return;
        }

        VDLIfmText.Text="";
        string regexIndex = GetRegexOptionIndex();
        if (regexIndex!="0")
        {
            VideoDetailList.ItemsSource= DoRegex(AllVideoData, regexIndex);
        }
        else
        {
            VideoDetailList.ItemsSource= DoRegex(AllVideoData, RecommendReg);
        }


        if (VideoDetailList.ItemsSource.Cast<VideoDetailList>().Count()<1)
        {
            VDLIfmText.Text="这里空空如也，请更换一个解析方案吧~";
        }


    }

    private void RegexSelectIfmBtn_Clicked(object sender, EventArgs e)
    {
        //以文件读取复杂的文本
        /*
                 using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("FCLiveToolApplication.Resources.Raw.regex_option_help"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        DisplayAlert("帮助信息", reader.ReadToEnd(), "关闭");
                    }
                }
         */
        ;
        string showmsg = "由于不同的M3U文件里的数据格式各有不同，甚至一个文件内有多种不同的格式，因此本程序提供了多种解析方案，来尽可能的完整解析M3U文件里的数据。"+"\n"
        +"如果您发现许多直播源没有名称，无法播放以及URL有错误等情况，您可以尝试更换解析方案。"+"\n\n\n"
        +"以下是所有复选框的解释："+"\n\n"
        +"“仅匹配M3U8文件名”：勾选则表示，仅匹配直播源URL的文件名为“M3U8”后缀的文件，其他文件则不获取。"+"\n\n\n"
        +"以下是所有规则的解释："+"\n\n"
        +"规则1"+"\n"
        +"匹配：台标(tvg-logo)，台名(tvg-name)，URL"+"\n\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +"tvg-name=\"台名\"(可选，不建议为空)"+"\n"
        +"\\r或\\n(可选)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则2"+"\n"
        +"与第一条相反，匹配：台名(tvg-name)，台标(tvg-logo)，URL"+"\n\n"
        +"tvg-name=\"台名\""+"\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +"\\r或\\n(可选)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则3"+"\n"
        +"匹配：台标(tvg-logo)，台名(两逗号之间文本)，URL"+"\n\n"
        +"tvg-logo=\"台标\"(可选)"+"\n"
        +","+"\n"
        +"台名"+"\n"
        +",或\\n(可选)"+"\n"
        +"http://或https://(+任意字符).m3u8?参数名=值&参数名=值...(全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则4"+"\n"
        +"和第三项相同，区别在于#EXTINF字符和台标字符之间有多个逗号"+"\n\n"
        +","+"\n"
        +"tvg-logo=\"台标\""+"\n"
        +","+"\n"
        +"台名"+"\n"
        +",或\\n(可选)"+"\n"
        +"http://或https://(+任意字符，不限.m3u8后缀)?参数名=值&参数名=值...(全部可选)"+"\n\n"
        +"****************************************"+"\n\n"
        +"规则5"+"\n"
        +"简单粗暴，无附加格式，匹配：台标，台名，URL"+"\n\n"
        +"台标(可选)"+"\n"
        +","+"\n"
        +"台名"+"\n"
        +","+"\n"
        +"http://或https://(+任意字符，不限.m3u8后缀)?参数名=值&参数名=值...(全部可选)";

        DisplayAlert("帮助信息", showmsg, "关闭");

    }
}