using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using CommunityToolkit.Maui.Storage;
using Encoding = System.Text.Encoding;

#if ANDROID
using Android.Content;
using Java.Security;
using Android.Provider;
#endif

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
    public static VideoListPage videoListPage;
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoListPage!=null)
        {
            return;
        }
        videoListPage=this;

        //让主页的播放列表引用当前页的播放列表
        //（由于用户可能会在弹窗上点取消按钮，所以不使用这种引用方法）
        //VideoPrevPage.videoPrevPage.M3U8PlayList=M3U8PlayList;

        LoadVideos();
        InitRegexList();
        DeviceDisplay.MainDisplayInfoChanged+=DeviceDisplay_MainDisplayInfoChanged;
    }
    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
#if ANDROID

        //横屏
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation==DisplayOrientation.Landscape)
        {
            EditModeRightPanel.Margin=new Thickness(0, 0, 0, 0);
        }
        else
        {
            EditModeRightPanel.Margin=new Thickness(0, 60, 0, 0);
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
    public int VDLCurrentPageIndex = 1;
    public int VLMaxPageIndex;
    public int VDLMaxPageIndex;
    public const int VL_COUNT_PER_PAGE = 15;
    public const int VDL_COUNT_PER_PAGE = 100;
    public string AllVideoData;
    public int[] UseGroup;
    public List<VideoList> CurrentVideosList;
    public List<VideoDetailList> CurrentVideosDetailList;
    public string CurrentVURL = "";
    public string RecommendReg = "0";
    public List<string> RegexOption;
    public bool IgnoreSelectionEvents = false;
    public List<string[]> M3U8PlayList = new List<string[]>();
    public bool isFinishM3U8VCheck = true;
    public int M3U8VCheckFinishCount = 0;
    public bool ShowLoadOrRefreshDialog = false;
    CancellationTokenSource M3U8ValidCheckCTS;

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

            HttpResponseMessage response = null;

            try
            {
                response = await httpClient.GetAsync(selectVDL.SourceLink);

                statusCode=(int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("提示信息", "下载文件失败，请稍后重试！\n"+"HTTP错误代码："+statusCode, "确定");
                    return;
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "无法连接到对方服务器，请检查您的网络或者更换一个直播源！", "确定");
                return;
            }

            //var httpstream = await httpClient.GetStreamAsync(selectVDL.SourceLink)
            using (var httpstream = await response.Content.ReadAsStreamAsync())
            {
                try
                {
                    selectVDL.SourceName=selectVDL.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                    var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, selectVDL.SourceName+".m3u8", httpstream, CancellationToken.None);

                    if (fileSaver.IsSuccessful)
                    {
                        await DisplayAlert("提示信息", "文件已成功下载至：\n"+fileSaver.FilePath, "确定");
                    }
                    else
                    {
                        //暂时判断为用户在选择目录时点击了取消按钮
                        await DisplayAlert("提示信息", "您已取消了操作。", "确定");
                    }
                }
                catch (Exception)
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

        VideoDetailList selectVDL = VideoDetailList.SelectedItem as VideoDetailList;
        if (string.IsNullOrWhiteSpace(selectVDL.SourceName))
        {
            await DisplayAlert("提示信息", "无法编辑该直播源，这不是你的原因，因为在解析时未能解析到直播源名称。", "确定");
            return;
        }
        string vname = selectVDL.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");

        string newvalue = await DisplayPromptAsync("编辑直播源", "当前直播源名称："+vname+"\n"+"请输入新的直播源名称：", "更新", "取消", "Name", -1, null, vname);
        if (string.IsNullOrWhiteSpace(newvalue))
        {
            if (newvalue !=null)
                await DisplayAlert("提示信息", "名称不能为空或空白字符！", "确定");
            return;
        }

        /*
                 var tresult = GetOLDStr(AllVideoData, vname, selectVDL.SourceLink.Replace("\r", "").Replace("\n", "").Replace("\r\n", ""));
                if (tresult is null||tresult.Contains("#EXTINF"))
                {
                    await DisplayAlert("提示信息", "无法更新当前直播源，因为M3U文件内包含多个和当前直播源相同的名称+URL！", "确定");
                    return;
                }
         */

        string m3u8Str = GetFullM3U8Str(selectVDL);
        if (m3u8Str is "")
        {
            await DisplayAlert("提示信息", "无法更新当前直播源，在M3U文件内找不到当前直播源！", "确定");
            return;
        }

        CurrentVideosDetailList.Where(p => p==selectVDL).FirstOrDefault().SourceName=newvalue; ;

        string regexIndex = GetRegexOptionIndex();
        if (regexIndex=="0")
            regexIndex=RecommendReg;
        if (regexIndex.StartsWith("1")||regexIndex.StartsWith("2"))
        {
            vname="tvg-name=\""+vname+"\"";
            newvalue="tvg-name=\""+newvalue+"\"";
        }
        else
        {
            vname=","+vname;
            newvalue=","+newvalue;
        }

        if (m3u8Str is null)
        {
            AllVideoData=AllVideoData.Replace(selectVDL.FullM3U8Str, selectVDL.FullM3U8Str.Replace(vname, newvalue));
        }
        else
        {
            AllVideoData=AllVideoData.Replace(m3u8Str, m3u8Str.Replace(vname, newvalue));
        }

        //仅在调试 AllVideoData是否被正确修改 以及 AllVideoData能否正常被加载到列表 时使用
        /*
                 string regexIndex = GetRegexOptionIndex();
                string treg;
                if (regexIndex!="0")
                {
                    treg=regexIndex;
                }
                else
                {
                    treg=RecommendReg;
                }

                List<VideoDetailList> tlist = DoRegex(AllVideoData, treg);
         */

        VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
        SetVDLPage(1);

        MakeVideosDataToPage(CurrentVideosDetailList, 0);

        await DisplayAlert("提示信息", "更新成功！", "确定");
    }
    private async void DeleteBtn_Clicked(object sender, EventArgs e)
    {
        if (VideoDetailList.SelectedItem==null)
        {
            await DisplayAlert("提示信息", "请先在列表里选择一条直播源！", "确定");
            return;
        }

        VideoDetailList selectVDL = VideoDetailList.SelectedItem as VideoDetailList;
        if (string.IsNullOrWhiteSpace(selectVDL.SourceName))
        {
            await DisplayAlert("提示信息", "无法编辑该直播源，这不是你的原因，因为在解析时未能解析到直播源名称。", "确定");
            return;
        }
        string vname = selectVDL.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");

        bool readydel = await DisplayAlert("你确定要移除该直播源吗？", vname, "确定", "取消");
        if (readydel)
        {
            /*
              var tresult = GetOLDStr(AllVideoData, vname, selectVDL.SourceLink.Replace("\r", "").Replace("\n", "").Replace("\r\n", ""));
             if (tresult is null||tresult.Contains("#EXTINF"))
             {
                 await DisplayAlert("提示信息", "无法移除当前直播源，因为M3U文件内包含多个和当前直播源相同的名称+URL！", "确定");
                 return;
             }
             */

            string m3u8Str = GetFullM3U8Str(selectVDL);
            if (m3u8Str is "")
            {
                await DisplayAlert("提示信息", "无法移除当前直播源，在M3U文件内找不到当前直播源！", "确定");
                return;
            }
            if (m3u8Str is null)
            {
                AllVideoData=AllVideoData.Replace(selectVDL.FullM3U8Str, "");
            }
            else
            {
                AllVideoData=AllVideoData.Replace(m3u8Str, "");
            }
            CurrentVideosDetailList.Remove(selectVDL);

            //仅在调试 AllVideoData是否被正确修改 以及 AllVideoData能否正常被加载到列表 时使用
            /*
                     string regexIndex = GetRegexOptionIndex();
                    string treg;
                    if (regexIndex!="0")
                    {
                        treg=regexIndex;
                    }
                    else
                    {
                        treg=RecommendReg;
                    }

                    List<VideoDetailList> tlist = DoRegex(AllVideoData, treg);
             */

            VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
            SetVDLPage(1);

            MakeVideosDataToPage(CurrentVideosDetailList, 0);

            await DisplayAlert("提示信息", "移除成功！", "确定");

        }

    }

    public string GetFullM3U8Str(VideoDetailList vdlList)
    {
        //如果正则表达式更改，这里可能也需要更新
        string regexIndex = GetRegexOptionIndex();
        if (regexIndex=="0")
            regexIndex=RecommendReg;

        string tlogolink = vdlList.LogoLink;
        string tsourcelink = vdlList.SourceLink;
        //如果之前就未匹配到Logo，那么表达式就用原来的字符串。
        if (vdlList.LogoLink.Contains("fclive_tvicon.png"))
        {
            tlogolink="([^\"]*)";
            tsourcelink="(http|https)://\\S+\\.m3u8(\\?(.*?))?";
        }

        if (regexIndex.StartsWith("1")||regexIndex.StartsWith("2")||regexIndex=="5")
        {
            return null;
        }
        else if (regexIndex=="3")
        {
            string reg = "(.*?)((tvg-logo=\""+tlogolink+"\")(.*?))?,("+vdlList.SourceName+")(,)?(\n)?("+tsourcelink+"(?=\n))";
            return Regex.Match(AllVideoData, reg).Groups[0].Value;
        }
        else if (regexIndex=="3.2")
        {
            string reg = "(.*?)((tvg-logo=\""+tlogolink+"\")(.*?))?,("+vdlList.SourceName+")(,)?(\n)?((http|https)://\\S+(.*?)(?=\n))";
            return Regex.Match(AllVideoData, reg).Groups[0].Value;
        }
        else if (regexIndex=="4")
        {
            string reg = "(.*?),?((tvg-logo=\""+tlogolink+"\")(.*?)),("+vdlList.SourceName+")(,)?(\n)?((http|https)://\\S+(.*?)(?=\n))";
            return Regex.Match(AllVideoData, reg).Groups[0].Value;
        }

        return "";
    }

    /*
         public string GetOLDStr(string videodata, string name, string link)
        {
            string oldvalue;
            try
            {
                Match tVResult = Regex.Match(videodata, Regex.Escape(name)+@"(?s)([\s\S]*?)"+Regex.Escape(link));
                oldvalue = tVResult.Value;
                if (oldvalue=="")
                    return null;
                int tncount = oldvalue.Split(name).Length;
                int tlcount = oldvalue.Split(link).Length;

                if (tncount>2&&tlcount>2)
                {
                    return null;
                }
                if (tncount>2)
                {
                    string tvdata = oldvalue.Remove(oldvalue.IndexOf(name), name.Length);
                    return GetOLDStr(tvdata, name, link);
                }
                if (tlcount>2)
                {
                    string tvdata = oldvalue.Remove(oldvalue.LastIndexOf(link), link.Length);
                    return GetOLDStr(tvdata, name, link);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return oldvalue;
        }
     */

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
    private async void VideoDetailList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (!VDLToogleBtn.IsToggled)
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


            var mainpage = ((Shell)App.Current.MainPage);
            mainpage.CurrentItem = mainpage.Items.FirstOrDefault();
            await mainpage.Navigation.PopToRootAsync();


            /*
             
            int permResult = await new APPPermissions().CheckAndReqPermissions();
            if (permResult!=0)
            {
                await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存文件！", "确定");
                return;
            }

            //根据不同平台选择不同的缓存方式
            string cachePath;
#if WINDOWS
            cachePath = Path.Combine(FileSystem.AppDataDirectory+"/LiveStreamCache");
#elif ANDROID
            //var test= Directory.CreateDirectory(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.AbsolutePath)+"/LiveStreamCache");
            cachePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android", "data", Android.App.Application.Context.PackageName+"/LiveStreamCache");
#else
            //暂时不对苹果设备以及其他平台进行直播源缓存

            VideoPrevPage.videoPrevPage.VideoWindow.Source=detail.SourceLink;
            VideoPrevPage.videoPrevPage.VideoWindow.Play();
            VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;

            return;
#endif


            try
            {
                Directory.CreateDirectory(cachePath);
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                return;
            }


            int statusCode;
            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                HttpResponseMessage response = null;

                try
                {
                    response = await httpClient.GetAsync(detail.SourceLink);

                    statusCode=(int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("提示信息", "获取文件失败，请稍后重试！\n"+"HTTP错误代码："+statusCode, "确定");
                        return;
                    }
                }
                catch (Exception)
                {
                    await DisplayAlert("提示信息", "无法连接到对方服务器，请检查您的网络或者更换一个直播源！", "确定");
                    return;
                }


                detail.SourceName=detail.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                string FullM3U8Path = cachePath+"/"+detail.SourceName+".m3u8";
                try
                {
                    File.WriteAllText(FullM3U8Path, await response.Content.ReadAsStringAsync());

                    string[] result = File.ReadAllLines(FullM3U8Path);
                    for (int i = 0; i<result.Length; i++)
                    {
                        if (result[i].StartsWith("#")||String.IsNullOrEmpty(result[i])||String.IsNullOrWhiteSpace(result[i]))
                            continue;
                        else if (!result[i].Contains("://"))
                        {
                            result[i]=detail.SourceLink.Substring(0, detail.SourceLink.LastIndexOf("/")+1)+result[i];
                        }
                    }

                    File.WriteAllLines(FullM3U8Path, result);
                }
                catch (Exception)
                {
                    await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                    return;
                }


                VideoPrevPage.videoPrevPage.VideoWindow.Source=detail.SourceLink;
                VideoPrevPage.videoPrevPage.VideoWindow.Play();
                VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;

            }

             */


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
            CurrentVideosList=CurrentVideosList.Where(p => p.RecommendReg!="-1").ToList();
            VideosList.ItemsSource= CurrentVideosList.Take(VL_COUNT_PER_PAGE);

            //重置
            VideosList.SelectedItem=null;
            VideoDetailList.ItemsSource=null;
            CurrentVURL="";

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
        VideoDetailListRing.IsRunning = true;
        try
        {
            //重置直播源检测标记
            isFinishM3U8VCheck=true;
            if (M3U8ValidCheckCTS!=null)
            {
                M3U8ValidCheckCTS.Cancel();
            }

            AllVideoData = await new HttpClient().GetStringAsync("https://fclivetool.com/api/APPGetVD?url="+url);

            RecommendReg=reg;
            CurrentVURL =url;
            VDLIfmText.Text="";
            //更改索引会触发SelectedIndexChanged事件，所以在仅刷新列表的时候选择不执行事件内后续代码
            IgnoreSelectionEvents=true;
            RegexSelectBox.SelectedIndex = 0;
            IgnoreSelectionEvents=false;

            CurrentVideosDetailList = DoRegex(AllVideoData, RecommendReg);
            VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
            SetVDLPage(1);
            MakeVideosDataToPage(CurrentVideosDetailList, 0);
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

        VideoDetailListRing.IsRunning = false;
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
        for (int i = 0; i<match.Count; i++)
        {
            VideoDetailList videoDetail = new VideoDetailList()
            {
                //ID，台标，台名，直播源地址
                Id=i,
                LogoLink=match[i].Groups[UseGroup[0]].Value=="" ? "fclive_tvicon.png" : match[i].Groups[UseGroup[0]].Value,
                SourceName=match[i].Groups[UseGroup[1]].Value,
                SourceLink=match[i].Groups[UseGroup[2]].Value,
                FullM3U8Str=match[i].Groups[0].Value,
                isHTTPS=match[i].Groups[UseGroup[2]].Value.ToLower().StartsWith("https://") ? true : false,
                FileName=Regex.Match(match[i].Groups[UseGroup[2]].Value, @"\/([^\/]+\.m3u8)").Groups[1].Value
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
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n))";
            //1.2为1的不限制M3U8后缀的版本
            case "1.2":
                UseGroup =new int[] { 1, 2, 3 };
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n))";
            //只有2方案是先匹配台名后匹配台标
            case "2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n))";
            //2.2为2的不限制M3U8后缀的版本
            case "2.2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n))";
            case "3":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+\.m3u8(\?(.*?))?(?=\n)))";
            //3.2为3的不限制M3U8后缀的版本
            case "3.2":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n)))";
            case "4":
                UseGroup =new int[] { 3, 5, 8 };
                return @",?((tvg-logo=""([^""]*)"")(.*?)),(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n)))";
            case "5":
                UseGroup =new int[] { 2, 5, 7 };
                return @"(((http|https)://\S+)(,))?(.*?)(,)((http|https)://\S+(?=\n|\s{1}))";
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
    /// <summary>
    /// 重置M3U8列表的页码
    /// </summary>
    /// <param name="index"></param>
    public void SetVDLPage(int index)
    {
        VDLCurrentPageIndex=index;
        VDLCurrentPage.Text=index+"/"+VDLMaxPageIndex;
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

    private void VDLBackBtn_Clicked(object sender, EventArgs e)
    {
        if (VDLCurrentPageIndex<=1)
        {
            return;
        }

        SetVDLPage(VDLCurrentPageIndex-1);


        MakeVideosDataToPage(CurrentVideosDetailList, (VDLCurrentPageIndex - 1) * VDL_COUNT_PER_PAGE);

    }

    private async void VDLJumpBtn_Clicked(object sender, EventArgs e)
    {
        int TargetPage = 1;
        if (!int.TryParse(VDLPageTb.Text, out TargetPage))
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }
        if (TargetPage<1||TargetPage>VDLMaxPageIndex)
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }


        SetVDLPage(TargetPage);

        MakeVideosDataToPage(CurrentVideosDetailList, (VDLCurrentPageIndex - 1) * VDL_COUNT_PER_PAGE);
    }

    private void VDLNextBtn_Clicked(object sender, EventArgs e)
    {
        if (VDLCurrentPageIndex>=VDLMaxPageIndex)
        {
            return;
        }

        SetVDLPage(VDLCurrentPageIndex+1);

        MakeVideosDataToPage(CurrentVideosDetailList, (VDLCurrentPageIndex-1)*VDL_COUNT_PER_PAGE);
    }

    public void MakeVideosDataToPage(List<VideoDetailList> list, int skipcount)
    {
        if (list.Count()<1)
        {
            VDLIfmText.Text="这里空空如也，请更换一个解析方案吧~";

            VideoDetailList.ItemsSource=new List<VideoDetailList>() { };
            //如果启用直接赋值null，则需要在部分按钮点击事件里分别使用CurrentVURL，ItemSource，Count的判断。
            //VideoDetailList.ItemsSource=null;

            return;
        }

        if (VDLCurrentPageIndex==VDLMaxPageIndex)
        {
            VideoDetailList.ItemsSource=list.Skip(skipcount).Take(list.Count-skipcount);
        }
        else
        {
            VideoDetailList.ItemsSource=list.Skip(skipcount).Take(VDL_COUNT_PER_PAGE);
        }

        if (VDLCurrentPageIndex>=VDLMaxPageIndex)
        {
            VDLBackBtn.IsEnabled = true;
            VDLNextBtn.IsEnabled = false;
        }
        else if (VDLCurrentPageIndex<=1)
        {
            VDLBackBtn.IsEnabled = false;
            VDLNextBtn.IsEnabled = true;
        }
        else
        {
            VDLBackBtn.IsEnabled = true;
            VDLNextBtn.IsEnabled = true;
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

    private void VideoDetailList_Refreshing(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            return;
        }

        LoadVideoDetail(CurrentVURL, RecommendReg);

        //不使用ListView自己的加载圈
        VideoDetailList.IsRefreshing=false;
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
        if (IgnoreSelectionEvents)
        {
            return;
        }

        VDLIfmText.Text="";
        string treg;

        string regexIndex = GetRegexOptionIndex();
        if (regexIndex!="0")
        {
            treg=regexIndex;
        }
        else
        {
            treg=RecommendReg;
        }

        CurrentVideosDetailList = DoRegex(AllVideoData, treg);
        VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
        SetVDLPage(1);

        MakeVideosDataToPage(CurrentVideosDetailList, 0);

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

    private async void SaveM3UFileBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            await DisplayAlert("提示信息", "请先在左侧M3U列表里选择一条直播源！", "确定");
            return;
        }
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存文件！", "确定");
            return;
        }


        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(AllVideoData)))
        {
            try
            {
                /*
                                 string sname;
                                if (VideosList.SelectedItem is null)
                                {
                                    sname="FileName";
                                }
                                else
                                {
                                    VideoList selectVL = VideosList.SelectedItem as VideoList;
                                    sname=selectVL.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                                }
                 */
                VideoList selectVL = VideosList.SelectedItem as VideoList;
                selectVL.SourceName=selectVL.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, selectVL.SourceName+".m3u", ms, CancellationToken.None);

                if (fileSaver.IsSuccessful)
                {
                    await DisplayAlert("提示信息", "文件已成功保存至：\n"+fileSaver.FilePath, "确定");
                }
                else
                {
                    //暂时判断为用户在选择目录时点击了取消按钮
                    await DisplayAlert("提示信息", "您已取消了操作。", "确定");
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
            }
        }
    }

    private async void M3U8ValidBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            await DisplayAlert("提示信息", "请先在左侧M3U列表里选择一条直播源！", "确定");
            return;
        }

        var vdlcount = VideoDetailList.ItemsSource.Cast<VideoDetailList>().Count();
        if (vdlcount<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有直播源，请尝试更换一个解析方案！", "确定");
            return;
        }
        if (!isFinishM3U8VCheck)
        {
            await DisplayAlert("提示信息", "当前正在执行直播源检测！", "确定");
            return;
        }
        bool tSelect = await DisplayAlert("提示信息", "本次将要检测 "+CurrentVideosDetailList.Count+" 个直播信号，你确定要开始测试吗？\n全部测试完后才会自动更新结果。", "确定", "取消");
        if (!tSelect)
        {
            return;
        }

        M3U8ValidCheckCTS=new CancellationTokenSource();
        isFinishM3U8VCheck=false;
        M3U8ValidStopBtn.IsEnabled=true;
        M3U8VProgressText.Text="0 / "+CurrentVideosDetailList.Count;
        M3U8VCheckFinishCount = 0;
        RegexSelectBox.IsEnabled=false;
        ShowLoadOrRefreshDialog=false;


        M3U8ValidCheck(CurrentVideosDetailList);

        while (M3U8VCheckFinishCount<CurrentVideosDetailList.Count)
        {
            if (M3U8ValidCheckCTS.IsCancellationRequested)
            {
                break;
            }
            await Task.Delay(1000);
        }

        M3U8ValidStopBtn.IsEnabled=false;
        isFinishM3U8VCheck=true;
        RegexSelectBox.IsEnabled=true;
        //还需要重置分页信息，此处不需要指定页面
        VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
        SetVDLPage(1);

        if (!M3U8ValidCheckCTS.IsCancellationRequested)
        {
            MakeVideosDataToPage(CurrentVideosDetailList, 0);

            await DisplayAlert("提示信息", "已全部检测完成！", "确定");
        }
        else
        {
            if (ShowLoadOrRefreshDialog)
            {
                bool tresult = await DisplayAlert("提示信息", "是要查看部分检测完的结果还是要重新加载列表？", "查看结果", "重新加载");
                if (tresult)
                {
                    MakeVideosDataToPage(CurrentVideosDetailList, 0);
                }
                else
                {
                    GetCurrentIndexAndLoadData(0);
                }
            }
            else
            {
                GetCurrentIndexAndLoadData(0);
                await DisplayAlert("提示信息", "您已取消检测！", "确定");
            }

            M3U8VProgressText.Text="";

        }


    }
    public async void M3U8ValidCheck(List<VideoDetailList> videodetaillist)
    {
        object obj = new object();
        for (int i = 0; i<videodetaillist.Count; i++)
        {
            var vd = videodetaillist[i];
            Thread thread = new Thread(async () =>
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout=TimeSpan.FromMinutes(2);

                    int statusCode;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                    HttpResponseMessage response = null;

                    try
                    {
                        //取消操作
                        M3U8ValidCheckCTS.Token.ThrowIfCancellationRequested();

                        vd.HTTPStatusCode="Checking...";
                        vd.HTTPStatusTextBKG=Colors.Gray;

                        response = await httpClient.GetAsync(vd.SourceLink, M3U8ValidCheckCTS.Token);

                        statusCode=(int)response.StatusCode;
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                vd.HTTPStatusCode="OK";
                                vd.HTTPStatusTextBKG=Colors.Green;
                            }
                            else
                            {
                                vd.HTTPStatusCode=statusCode.ToString();
                                vd.HTTPStatusTextBKG=Colors.Orange;
                            }
                        });

                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            vd.HTTPStatusCode="ERROR";
                            vd.HTTPStatusTextBKG=Colors.Red;
                        });
                    }

                    if (!M3U8ValidCheckCTS.IsCancellationRequested)
                    {
                        lock(obj)
                        {
                            M3U8VCheckFinishCount++;

                            MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                M3U8VProgressText.Text=M3U8VCheckFinishCount+" / "+videodetaillist.Count;
                            });
                        }


                    }

                }
            });
            thread.Start();

            await Task.Delay(20);
        }
    }
    private async void M3U8ValidRemoveBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            await DisplayAlert("提示信息", "请先在左侧M3U列表里选择一条直播源！", "确定");
            return;
        }

        var vdlcount = VideoDetailList.ItemsSource.Cast<VideoDetailList>().Count();
        if (vdlcount<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有直播源，请尝试更换一个解析方案！", "确定");
            return;
        }
        if (!isFinishM3U8VCheck)
        {
            await DisplayAlert("提示信息", "当前正在执行直播源检测！", "确定");
            return;
        }
        var notokcount = CurrentVideosDetailList.Where(p => (p.HTTPStatusCode!="OK")&&(p.HTTPStatusCode!=null)).Count();
        if (notokcount<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有无效的直播信号，无需操作！", "确定");
            return;
        }
        bool tSelect = await DisplayAlert("提示信息", "本次将要移除 "+notokcount+" 个无效的直播信号，你确定移除吗？\n移除后可点击页面右上角的保存按钮。", "确定", "取消");
        if (!tSelect)
        {
            return;
        }

        int tNOKRemoveCount = 0;
        int tOKRemoveCount = 0;
        CurrentVideosDetailList.ForEach(p =>
        {
            if (p.HTTPStatusCode!="OK"&&p.HTTPStatusCode!=null)
            {
                string vname = p.SourceName.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");

                string m3u8Str = GetFullM3U8Str(p);
                if (m3u8Str is "")
                {
                    //如果仍然不能搜索到完整的字符串，那就不进行替换
                    tNOKRemoveCount++;
                }
                else if (m3u8Str is null)
                {
                    AllVideoData=AllVideoData.Replace(p.FullM3U8Str, "");
                    tOKRemoveCount++;
                }
                else
                {
                    AllVideoData=AllVideoData.Replace(m3u8Str, "");
                    tOKRemoveCount++;
                }

            }
        });
        CurrentVideosDetailList.RemoveAll(p => p.HTTPStatusCode!="OK"&&p.HTTPStatusCode!=null);

        VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
        SetVDLPage(1);
        MakeVideosDataToPage(CurrentVideosDetailList, 0);

        await DisplayAlert("提示信息", "已成功从列表里移除所有无效的直播信号！\n已从M3U文件缓存里移除无效的直播信号"+tOKRemoveCount+"条，未成功移除"+tNOKRemoveCount+"条。", "确定");

    }

    private async void M3U8ValidStopBtn_Clicked(object sender, EventArgs e)
    {
        if (M3U8ValidCheckCTS!=null)
        {
            bool CancelCheck = await DisplayAlert("提示信息", "您要停止检测吗？停止后暂时不支持恢复进度。", "确定", "取消");
            if (CancelCheck)
            {
                M3U8ValidCheckCTS.Cancel();
                //这句可以注释掉
                M3U8ValidStopBtn.IsEnabled=false;

                ShowLoadOrRefreshDialog = true;
            }

        }
    }
    public void GetCurrentIndexAndLoadData(int skipcount)
    {
        string regexIndex = GetRegexOptionIndex();
        if (regexIndex!="0")
        {
            MakeVideosDataToPage(DoRegex(AllVideoData, regexIndex), skipcount);
        }
        else
        {
            MakeVideosDataToPage(DoRegex(AllVideoData, RecommendReg), skipcount);
        }
    }

    private void VideoDetailList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            VideoDetailList.SelectedItem=null;

            if (VideoDetailList.ItemsSource is not null&&VideoDetailList.ItemsSource.Cast<VideoDetailList>().Count()>0)
            {
                VDLPagePanel.IsVisible=true;
            }
            else
            {
                VDLPagePanel.IsVisible=false;
            }
        }

    }

    private async void VDLSearchBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            await DisplayAlert("提示信息", "请先在左侧M3U列表里选择一条直播源，才能搜索里面的内容！", "确定");
            return;
        }
        if (!isFinishM3U8VCheck)
        {
            await DisplayAlert("提示信息", "当前正在执行直播源检测，请先停止检测再搜索！", "确定");
            return;
        }

        string searchText = VDLSearchTb.Text;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            await DisplayAlert("提示信息", "输入的内容无效！", "确定");
            return;
        }
        /*
                 if (searchText.Length < 3)
                {
                    await DisplayAlert("提示信息", "搜索内容长度不能小于3！", "确定");
                    return;
                }
         */

        VDLIfmText.Text="";
        string treg;
        string regexIndex = GetRegexOptionIndex();
        if (regexIndex!="0")
        {
            treg=regexIndex;
        }
        else
        {
            treg=RecommendReg;
        }

        List<VideoDetailList> tlist = DoRegex(AllVideoData, treg);
        if (tlist.Count<1)
        {
            tlist = DoRegex(AllVideoData, RecommendReg);
            DisplayAlert("提示信息", "当前解析方案未能解析出直播源，已改为推荐的方案去解析并执行搜索。", "确定");
        }
        //暂时不编写智能搜索
        CurrentVideosDetailList= tlist.Where(p => p.SourceName.Contains(searchText)).ToList();

        VDLMaxPageIndex= (int)Math.Ceiling(CurrentVideosDetailList.Count/100.0);
        SetVDLPage(1);

        MakeVideosDataToPage(CurrentVideosDetailList, 0);

    }

    private async void JumpEditM3UBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentVURL=="")
        {
            await DisplayAlert("提示信息", "请先在左侧M3U列表里选择一条直播源！", "确定");
            return;
        }


        List<VideoEditList> videoEditLists = await new VideoManager().ReadM3UString(AllVideoData);

        var mainpage = ((Shell)App.Current.MainPage);
        mainpage.CurrentItem = mainpage.Items.FirstOrDefault(p => p.Title=="直播源编辑");
        await mainpage.Navigation.PopToRootAsync();

        VideoEditPage.videoEditPage.VideoEditList.ItemsSource=videoEditLists;
    }
}