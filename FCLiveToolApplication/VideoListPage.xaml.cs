using Microsoft.Maui.Platform;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

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

    public int VLCurrentPageIndex = 1;
    public int VLMaxPageIndex;
    public const int VL_COUNT_PER_PAGE = 15;
    public string AllVideoData;
    public int[] UseGroup;
    public List<VideoList> CurrentVideosList;

    private void Question_Clicked(object sender, EventArgs e)
    {
        ImageButton button = sender as ImageButton;

        switch (button.StyleId)
        {
            case "VideoListQue":
                DisplayAlert("帮助信息", "所有直播源均来自于网络，我们不对直播里的内容负责，如有侵权请联系我们删除", "关闭");
                break;
            case "VideosQue":
                DisplayAlert("帮助信息", "由于某些M3U8内URL使用的是相对地址而不是绝对地址，故APP无法播放，并非是APP的bug，请知悉", "关闭");
                break;
        }
    }
    private void DownloadBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("DOWNB", "");
    }
    private void EditBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("EB", "");
    }
    private void DeleteBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("DELB", "");
    }

    private async void VideosList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoDataListRing.IsRunning = true;
        try
        {
            VideoList videoList = e.Item as VideoList;
            AllVideoData = await new HttpClient().GetStringAsync("https://fclivetool.com/api/APPGetVD?url="+videoList.SourceLink);

            VideoDetailList.ItemsSource= DoRegex(AllVideoData, videoList.RecommendReg);
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

    private void VideoDetailList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoDetailList detail = e.Item as VideoDetailList;

        VideoPrevPage.videoPrevPage.VideoWindow.Source=detail.SourceLink;
        VideoPrevPage.videoPrevPage.VideoWindow.Play();
        VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;
    }

    public async void LoadVideos()
    {
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
    }

    private async void VLJumpBtn_Clicked(object sender, EventArgs e)
    {

        int TargetPage = 1;
        if(!int.TryParse(VLPageTb.Text,out TargetPage))
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }
        if(TargetPage<1||TargetPage>VLMaxPageIndex)
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
}