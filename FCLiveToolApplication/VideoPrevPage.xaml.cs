using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Linq.Expressions;
using System.Security;
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
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        videoPrevPage=this;


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

        RecentList.HeightRequest=PageGrid.Height-50;

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
        }
#endif
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
        catch (Exception)
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
        LoadRecent();

        //不使用ListView自己的加载圈
        RecentList.IsRefreshing=false;
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
}