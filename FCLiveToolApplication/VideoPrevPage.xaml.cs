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
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        videoPrevPage=this;


        LoadRecent();
#if ANDROID
        RecentPanel.WidthRequest=PageGrid.Width;
#endif
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
    private void RecentList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var selectitem = e.Item as RecentVList;

        if (selectitem.SourceLink=="")
        {
            DisplayAlert("错误", "无法播放该直播源，请更换一个试试", "确定");
            return;
        }

        VideoWindow.Source=selectitem.SourceLink;
        NowPlayingTb.Text=selectitem.SourceName;
    }
    public async void LoadRecent()
    {
        string videodata = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetRecent");
        videodata= videodata.Replace("/img/TVSICON.png", "fclive_tvicon.png");

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RecentVList>));
        List<RecentVList> list = (List<RecentVList>)xmlSerializer.Deserialize(new StringReader(videodata));
        list.ForEach(p => p.PastTime=GetPastTime(p.AddDT));

        RecentList.ItemsSource =list;
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
}