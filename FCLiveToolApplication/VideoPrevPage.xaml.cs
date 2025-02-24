using CommunityToolkit.Maui.Storage;
using System.Xml.Serialization;

namespace FCLiveToolApplication;

public partial class VideoPrevPage : ContentPage
{
    public VideoPrevPage()
    {
        InitializeComponent();
    }
    public static VideoPrevPage videoPrevPage;
    public List<string[]> M3U8PlayList = new List<string[]>();
    public List<LocalM3U8List> CurrentLocalM3U8List = new List<LocalM3U8List>();
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (videoPrevPage != null)
        {
            return;
        }
        videoPrevPage=this;

        NowPlayingTb.Text =Preferences.Get("DefaultPlayM3U8Name", "");
        VideoWindow.Source= Preferences.Get("DefaultPlayM3U8URL", "");
        //VideoWindow.ShouldAutoPlay=Preferences.Get("StartAutoPlayM3U8", true);
        if (Preferences.Get("StartAutoPlayM3U8", true))
        {
            VideoWindow.Play();
        }


        ReadAndLoadLocalM3U8();
        await Task.Delay(1000);
        LoadRecent();
#if ANDROID
        RecentPanel.WidthRequest=PageGrid.Width;
#endif

        //CurrentURL=(VideoWindow.Source as UriMediaSource).Uri.ToString();
    }
    private void PageGrid_SizeChanged(object sender, EventArgs e)
    {
#if WINDOWS

        VideoWindow.WidthRequest=PageGrid.Width;
        VideoWindow.HeightRequest=PageGrid.Height-50;

        RecentList.HeightRequest=PageGrid.Height-100;
        LocalM3U8Panel.HeightRequest=PageGrid.Height-50;
        //LocalM3U8List.HeightRequest=PageGrid.Height-(LocalM3U8SP.Height+100);

        //如果最近播放的列表是展开状态，则窗口大小改变时也要同时调整它，保证它不错位
        if (RecentPanel.Width>0)
        {
            ShowRecentAnimation(true);
        }
#endif
#if ANDROID
        //安卓横屏 
        if (DeviceDisplay.Current.MainDisplayInfo.Orientation==DisplayOrientation.Landscape)
        {
            CheckRecentBtn.IsVisible=true;

            PageGrid.SetRow(RecentPanel, 1);

            RecentPanel.IsVisible=false;
            RecentPanel.HeightRequest=PageGrid.Height-50;

            VideoWindow.HeightRequest=PageGrid.Height-50;

            LocalM3U8List.HeightRequest=VideoWindow.HeightRequest/3;
        }
        //安卓竖屏
        else
        {
            CheckRecentBtn.IsVisible=false;

            PageGrid.SetRow(RecentPanel, 2);

            RecentPanel.IsVisible=true;
            RecentPanel.ClearValue(HeightRequestProperty);
            RecentPanel.ClearValue(WidthRequestProperty);

            VideoWindow.HeightRequest=PageGrid.Width;

            LocalM3U8List.HeightRequest=VideoWindow.HeightRequest/3;
        }
#endif
    }
    public async void ReadAndLoadLocalM3U8()
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存和读取文件！如果不授权，后续涉及文件读写的操作将无法正常使用！", "确定");
            return;
        }

        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("LocalM3U8Log");
        if (dataPath != null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<LocalM3U8List>));
            try
            {
                if (File.Exists(dataPath+"/LocalM3U8.log"))
                {
                    var tlist = (List<LocalM3U8List>)xmlSerializer.Deserialize(new StringReader(File.ReadAllText(dataPath+"/LocalM3U8.log")));

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LocalM3U8List.ItemsSource = tlist;
                    });
                }
            }
            catch (Exception)
            {
                await DisplayAlert("提示信息", "读取本地M3U8播放列表时出错！", "确定");
            }

        }

    }

    private async void CheckRecentBtn_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        if (RecentPanel.Width==0)
        {
            ShowRecentAnimation(true);
        }
        else
        {
            ShowRecentAnimation(false);
        }
#endif
        //安卓横屏时才显示当前按钮并处理事件
#if ANDROID
        if (RecentPanel.IsVisible)
        {
            ShowRecentAnimation(false);
            await Task.Delay(1000);
            RecentPanel.IsVisible=false;
        }
        else
        {
            ShowRecentAnimation(true);
            RecentPanel.IsVisible=true;
        }
#endif
    }
    private async void RecentList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        List<string[]> tmlist = new List<string[]>();
        M3U8PlayList.ForEach(tmlist.Add);


        var selectitem = e.Item as RecentVList;

        string readresult = await new VideoManager().DownloadAndReadM3U8File(M3U8PlayList, new string[] { selectitem.SourceName, selectitem.SourceLink });
        if (readresult!="")
        {
            M3U8PlayList=tmlist;
            await DisplayAlert("提示信息", readresult, "确定");
            return;
        }


        M3U8PlayList.Insert(0, new string[] { "默认", selectitem.SourceLink });
        string[] MOptions = new string[M3U8PlayList.Count];
        MOptions[0]="默认\n";
        string WantPlayURL = selectitem.SourceLink;

        if (M3U8PlayList.Count > 2)
        {
            for (int i = 1; i<M3U8PlayList.Count; i++)
            {
                MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
            }

            string MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                M3U8PlayList=tmlist;
                return;
            }
            else if (!MSelectResult.Contains("默认"))
            {
                int tmindex = Convert.ToInt32(MSelectResult.Remove(0, 1).Split("】")[0]);
                WantPlayURL=M3U8PlayList[tmindex][1];
            }

        }


        VideoWindow.Source=WantPlayURL;
        VideoWindow.Play();
        NowPlayingTb.Text=selectitem.SourceName;
    }
    public async void LoadRecent()
    {
        RecentListRing.IsRunning=true;
        //未加载成功不覆盖数据，仍可操作原来的数据
        try
        {
            string videodata = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetRecent");
            videodata= videodata.Replace("/img/TVSICON.png", "fclive_tvicon.png");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RecentVList>));
            List<RecentVList> list = (List<RecentVList>)xmlSerializer.Deserialize(new StringReader(videodata));
            list.ForEach(p => p.PastTime=GetPastTime(p.AddDT));

            RecentList.ItemsSource =list;
        }
        catch (Exception ex)
        {
            await DisplayAlert("提示信息", "获取最近播放数据失败，请稍后重试！", "确定");
        }

        RecentListRing.IsRunning=false;
    }
    public string GetPastTime(DateTime dt)
    {
        if (dt.Year==0001||dt.Year==1970)
        {
            return "";
        }

        TimeSpan PastTime = DateTime.UtcNow.AddHours(8)-dt;
        if (PastTime<TimeSpan.FromMinutes(1))
        {
            return PastTime.Seconds+"秒钟前";
        }
        else if (PastTime<TimeSpan.FromHours(1))
        {
            return PastTime.Minutes+"分钟前";
        }
        else if (PastTime<TimeSpan.FromDays(1))
        {
            return PastTime.Hours+"小时前";
        }
        else if (PastTime<TimeSpan.FromDays(30))
        {
            return PastTime.Days+"天前";
        }
        else if (PastTime==TimeSpan.FromDays(30))
        {
            return "一个月前";
        }
        else
        {
            return "超过一个月";
        }

    }

    public void ShowRecentAnimation(bool NeedOpen)
    {
        //展开
        if (NeedOpen)
        {
            var animation = new Animation(v => RecentPanel.WidthRequest = v, 0, PageGrid.Width);
            animation.Commit(this, "ShowRec", 16, 500, Easing.BounceOut);
        }
        //收回
        else
        {
            var animation = new Animation(v => RecentPanel.WidthRequest = v, RecentPanel.Width, 0);
            animation.Commit(this, "HiddenRec", 16, 500, Easing.CubicInOut);
        }
    }

    private void RecentList_Refreshing(object sender, EventArgs e)
    {       
        //不使用ListView自己的加载圈
        RecentList.IsRefreshing=false;

        LoadRecent();
    }

    private void RLRefreshBtn_Clicked(object sender, EventArgs e)
    {
        LoadRecent();
    }

    private async void PlaylistBtn_Clicked(object sender, EventArgs e)
    {
        string MSelectResult;

        if (M3U8PlayList.Count<=2)
        {
            MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, new string[] { "默认\n" });
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                return;
            }
            else
            {
                VideoWindow.Stop();
                VideoWindow.Source=VideoWindow.Source;
            }
        }
        else
        {
            string[] MOptions = new string[M3U8PlayList.Count];
            MOptions[0]="默认\n";
            for (int i = 1; i<M3U8PlayList.Count; i++)
            {
                MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
            }

            MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                return;
            }
            else if (!MSelectResult.Contains("默认"))
            {
                int tmindex = Convert.ToInt32(MSelectResult.Remove(0, 1).Split("】")[0]);
                VideoWindow.Source=M3U8PlayList[tmindex][1];
            }
            else
            {
                VideoWindow.Stop();
                VideoWindow.Source=M3U8PlayList[0][1];
            }
        }

        VideoWindow.Play();
    }

    private void LocalM3U8Btn_Clicked(object sender, EventArgs e)
    {
        //收回
        if (LocalM3U8Panel.TranslationY==0)
        {
            var animation = new Animation {
                { 0, 1, new Animation(v => LocalM3U8Panel.TranslationY  = v, 0, -1000) },
                { 0, 1, new Animation(v => LocalM3U8Panel.Opacity  = v, 1, 0) }
            };
            animation.Commit(this, "MPanelAnimation", 16, 500, Easing.CubicInOut);

        }
        //展开
        else
        {
            var animation = new Animation {
                { 0, 1,  new Animation(v => LocalM3U8Panel.TranslationY  = v, -1000, 0)},
                { 0, 1, new Animation(v => LocalM3U8Panel.Opacity  = v, 0, 1) }
            };
            animation.Commit(this, "MPanelAnimation", 16, 500, Easing.CubicOut);

        }
    }

    private async void SelectLocalM3U8FolderBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        //每次点击按钮将当前列表数据放入变量里，在添加时进行对比
        if (LocalM3U8List.ItemsSource is not null)
        {
            CurrentLocalM3U8List=LocalM3U8List.ItemsSource.Cast<LocalM3U8List>().ToList();
        }
        //VideoWindow.Source=new Uri("C:\\Users\\Lee\\Desktop\\cgtn-f.m3u8");
        var folderPicker = await FolderPicker.PickAsync(FileSystem.AppDataDirectory, CancellationToken.None);

        if (folderPicker.IsSuccessful)
        {
            List<LocalM3U8List> mlist = new List<LocalM3U8List>();
            LoadM3U8FileFromSystem(folderPicker.Folder.Path, ref mlist);
            CurrentLocalM3U8List.AddRange(mlist);

            //重新添加序号
            int tmindex = 0;          
            CurrentLocalM3U8List.ForEach(p =>
            {
                p.ItemId="LMRB"+tmindex;
                tmindex++;
            });
            LocalM3U8List.ItemsSource=CurrentLocalM3U8List;
        }
        else
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }

    }
    private async void SelectLocalM3U8FileBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        //每次点击按钮将当前列表数据放入变量里，在添加时进行对比
        if (LocalM3U8List.ItemsSource is not null)
        {
            CurrentLocalM3U8List=LocalM3U8List.ItemsSource.Cast<LocalM3U8List>().ToList();
        }

        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
{
    { DevicePlatform.iOS, new[] { "com.apple.mpegurl", "public.m3u8-playlist" , "application/vnd.apple.mpegurl" } },
    { DevicePlatform.macOS, new[] { "public.m3u8", "application/vnd.apple.mpegurl" } },
    { DevicePlatform.Android, new[] {  "audio/x-mpegurl" } },
    { DevicePlatform.WinUI, new[] { ".m3u8" ,".m3u"} }
});
        var filePicker = await FilePicker.PickMultipleAsync(new PickOptions()
        {
            PickerTitle="选择M3U8文件",
            FileTypes=fileTypes
        });

        if (filePicker is not null&&filePicker.Count()>0)
        {
            filePicker.ToList().ForEach(p =>
            {
                if (CurrentLocalM3U8List.Where(p2 => p2.FullFilePath==p.FullPath).Count()<1)
                {
                    CurrentLocalM3U8List.Add(new LocalM3U8List() { FileName=p.FileName, FilePath=p.FullPath.Replace(p.FileName, ""), FullFilePath=p.FullPath });
                }
            });

            //重新添加序号
            int tmindex = 0;
            CurrentLocalM3U8List.ForEach(p =>
            {
                p.ItemId="LMRB"+tmindex;
                tmindex++;
            });
            LocalM3U8List.ItemsSource=CurrentLocalM3U8List;
        }
        else
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }

    }
    public void LoadM3U8FileFromSystem(string path, ref List<LocalM3U8List> list)
    {
        foreach (string item in Directory.EnumerateFileSystemEntries(path).ToList())
        {
            if (File.GetAttributes(item).HasFlag(FileAttributes.Directory))
            {
                LoadM3U8FileFromSystem(item, ref list);
            }
            else
            {
                if (item.ToLower().EndsWith(".m3u8"))
                {
                    string tname;
#if ANDROID
                    tname = item.Substring(item.LastIndexOf("/")+1);
#else
tname = item.Substring(item.LastIndexOf("\\")+1);
#endif
                    //string tfoldername = "."+item.Replace(initFoldername, "").Replace(tname, "");
                    string tfoldername = item.Replace(tname, "");

                    if (CurrentLocalM3U8List.Where(p => p.FullFilePath==item).Count()<1)
                    {
                        list.Add(new LocalM3U8List() { FileName=tname, FilePath=tfoldername, FullFilePath=item });
                    }

                }

            }

        }
    }
    private async void LocalM3U8List_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        LocalM3U8List list = e.Item as LocalM3U8List;

        //暂时编写一些冗余代码
        List<string[]> tmlist = new List<string[]>();
        M3U8PlayList.ForEach(tmlist.Add);

        string readresult = await new VideoManager().ReadLocalM3U8File(M3U8PlayList, new string[] { list.FileName, list.FullFilePath });
        if (readresult!="")
        {
            M3U8PlayList=tmlist;
            await DisplayAlert("提示信息", readresult, "确定");
            return;
        }


        M3U8PlayList.Insert(0, new string[] { "默认", list.FullFilePath });
        string[] MOptions = new string[M3U8PlayList.Count];
        MOptions[0]="默认\n";
        string WantPlayURL = list.FullFilePath;

        if (M3U8PlayList.Count > 2)
        {
            for (int i = 1; i<M3U8PlayList.Count; i++)
            {
                MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
            }

            string MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                M3U8PlayList=tmlist;
                return;
            }
            else if (!MSelectResult.Contains("默认"))
            {
                int tmindex = Convert.ToInt32(MSelectResult.Remove(0, 1).Split("】")[0]);
                WantPlayURL=M3U8PlayList[tmindex][1];
            }

            if (WantPlayURL=="")
            {
                await DisplayAlert("提示信息", "当前直播源的URL是相对地址，不是绝对地址，因为不知道主机地址，故无法播放该直播源。", "确定");
                return;
            }
        }


        VideoWindow.Source=new Uri(WantPlayURL);
        VideoWindow.Play();
        NowPlayingTb.Text="本地文件： "+list.FileName;


    }

    private void LocalM3U8List_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            if (LocalM3U8List.ItemsSource is not null&&LocalM3U8List.ItemsSource.Cast<LocalM3U8List>().Count()>0)
            {
                LocalM3U8IfmText.Text="";
            }
            else
            {
                LocalM3U8IfmText.Text="当前播放列表中没有直播源，快去添加吧~";
            }
        }
    }

    private async void LocalM3U8RemoveBtn_Clicked(object sender, EventArgs e)
    {
        Button LMRBtn = sender as Button;
        List<LocalM3U8List> tlist = LocalM3U8List.ItemsSource.Cast<LocalM3U8List>().ToList();

        var item = tlist.Where(p => p.ItemId==LMRBtn.CommandParameter.ToString()).FirstOrDefault();
        if (item is null)
        {
            await DisplayAlert("提示信息", "移除时发生异常", "确定");
            return;
        }
        tlist.Remove(item);

        LocalM3U8List.ItemsSource=tlist;
    }

    private async void SaveLocalM3U8Btn_Clicked(object sender, EventArgs e)
    {
        var tlist = LocalM3U8List.ItemsSource;
        if (tlist is null||tlist.Cast<LocalM3U8List>().Count()<1)
        {
            await DisplayAlert("提示信息", "当前列表中没有本地直播源文件，请先选择一些直播源！", "确定");
            return;
        }
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult!=0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要保存文件！", "确定");
            return;
        }

        string dataPath = new APPFileManager().GetOrCreateAPPDirectory("LocalM3U8Log");
        if (dataPath is null)
        {
            await DisplayAlert("提示信息", "保存文件失败！可能是没有权限或者当前平台暂不支持保存操作！", "确定");
            return;
        }

        using (StringWriter sw = new StringWriter())
        {
            new XmlSerializer(typeof(List<LocalM3U8List>)).Serialize(sw, tlist.Cast<LocalM3U8List>().ToList());
            File.WriteAllText(dataPath+"LocalM3U8.log", sw.ToString());
        }

        await DisplayAlert("提示信息", "保存播放列表成功啦！", "确定");
    }

    private void NowPlayingTb_PointerEntered(object sender, PointerEventArgs e)
    {
        NowPlayingTb.Background=Colors.LightYellow;      
    }

    private void NowPlayingTb_PointerExited(object sender, PointerEventArgs e)
    {
        NowPlayingTb.Background=Colors.Transparent;
    }

    private void PointerGestureRecognizer_PointerReleased(object sender, PointerEventArgs e)
    {
        ChangeCurrentPlayerSource();
    }
    //为了应对PointerGestureRecognizer在安卓上不起作用的方案，待微软后期修复
    private void NowPlayingTb_Tapped(object sender, TappedEventArgs e)
    {
#if ANDROID
        ChangeCurrentPlayerSource();
#endif
    }
    public async void ChangeCurrentPlayerSource()
    {
        string urlnewvalue = await DisplayPromptAsync("播放一个直播源", "请输入直播源URL：", "播放", "取消", "URL...", -1, Keyboard.Text, "");
        if (string.IsNullOrWhiteSpace(urlnewvalue))
        {
            if (urlnewvalue!=null)
                await DisplayAlert("提示信息", "请输入正确的内容！", "确定");
            return;
        }
        if (!urlnewvalue.Contains("://"))
        {
            await DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
            return;
        }


        List<string[]> tmlist = new List<string[]>();
        M3U8PlayList.ForEach(tmlist.Add);

        string readresult = await new VideoManager().DownloadAndReadM3U8File(M3U8PlayList, new string[] { "在线直播源", urlnewvalue });
        if (readresult!="")
        {
            M3U8PlayList=tmlist;
            await DisplayAlert("提示信息", readresult, "确定");
            return;
        }


        M3U8PlayList.Insert(0, new string[] { "默认", urlnewvalue });
        string[] MOptions = new string[M3U8PlayList.Count];
        MOptions[0]="默认\n";
        string WantPlayURL = urlnewvalue;

        if (M3U8PlayList.Count > 2)
        {
            for (int i = 1; i<M3U8PlayList.Count; i++)
            {
                MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
            }

            string MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
            if (MSelectResult == "取消"||MSelectResult is null)
            {
                M3U8PlayList=tmlist;
                return;
            }
            else if (!MSelectResult.Contains("默认"))
            {
                int tmindex = Convert.ToInt32(MSelectResult.Remove(0, 1).Split("】")[0]);
                WantPlayURL=M3U8PlayList[tmindex][1];
            }

        }


        VideoWindow.Source=WantPlayURL;
        VideoWindow.Play();
        NowPlayingTb.Text="在线直播源";
    }
}