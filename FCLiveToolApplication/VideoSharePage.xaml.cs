using CommunityToolkit.Maui.Storage;
using System.Text;
using System.Xml.Serialization;

namespace FCLiveToolApplication;

public partial class VideoSharePage : ContentPage
{
	public VideoSharePage()
	{
		InitializeComponent();
	}
    public static VideoSharePage videoSharePage;
    public int VSLCurrentPageIndex = 1;
    public List<string[]> M3U8PlayList = new List<string[]>();
    private async void M3UAnalysisBtn_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(M3USourceURLTb.Text)||!M3USourceURLTb.Text.Contains("://"))
        {
            await DisplayAlert("提示信息", "请输入正确的直播源地址！", "确定");
            return;
        }
        if (!M3USourceURLTb.Text.StartsWith("http://")&&!M3USourceURLTb.Text.StartsWith("https://"))
        {
            await DisplayAlert("提示信息", "暂时只支持HTTP协议的URL！", "确定");
            return;
        }


        M3UAnalysisBtn.IsEnabled=false;
        try
        {
            CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(
await new HttpClient().GetStreamAsync("https://fclivetool.com/api/NewShareURL?url="+M3USourceURLTb.Text));

            if (videoCheckModel is null)
            {
                await DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                M3UAnalysisBtn.IsEnabled=true;
                return;
            }

            switch(videoCheckModel.StatusCode)
            {
                case "0":
                    if (videoCheckModel.Result=="OK")
                    {
                        await DisplayAlert("提示信息", "分享直播源成功，感谢您的分享！\n现在将自动加载第一页。", "确定");

                        VSLCurrentPageIndex=1;
                        await GetShareData(1);
                    }
                    else
                    {
                        await DisplayAlert("提示信息", "分享失败，因为直播源已失效！错误信息："+videoCheckModel.Result, "确定");
                    }

                    break;         
                case "-1":
                    await DisplayAlert("提示信息", "请输入正确的直播源地址！", "确定");
                    break;     
                case "-2":
                    await DisplayAlert("提示信息", "暂时只支持HTTP协议的URL！", "确定");
                    break;     
                default:
                    await DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                    break;
            }


        }
        catch (Exception)
        {
            await DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
        }

        M3UAnalysisBtn.IsEnabled=true;
    }

    private async void M3UItemRightBtn_Clicked(object sender, EventArgs e)
    {
        Button UpdBtn = sender as Button;

        if (!string.IsNullOrWhiteSpace(UpdBtn.CommandParameter.ToString()))
        {
            UpdBtn.IsEnabled=false;

            if (UpdBtn.StyleId=="M3UStateUpdateBtn")
            {
                try
                {
                    CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(
        await new HttpClient().GetStreamAsync("https://fclivetool.com/api/UpdateShareURL?url="+UpdBtn.CommandParameter.ToString()));

                    if (videoCheckModel is null)
                    {
                        await DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                        UpdBtn.IsEnabled=true;
                        return;
                    }

                    switch (videoCheckModel.StatusCode)
                    {
                        case "0":
                            if (videoCheckModel.Result=="OK")
                            {
                                await DisplayAlert("提示信息", "直播源有效。", "确定");

                                //await GetShareData(1);
                            }
                            else
                            {
                                await DisplayAlert("提示信息", "直播源已失效，数据已被移除！错误信息："+videoCheckModel.Result, "确定");

                                VSLCurrentPageIndex=1;
                                await GetShareData(1);
                            }

                            break;
                        default:
                            await DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                            break;
                    }


                }
                catch (Exception)
                {
                    await DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                }

            }
            else if (UpdBtn.StyleId=="M3UPlayBtn")
            {
                try
                {
                    string readresult = await new VideoManager().DownloadAndReadM3U8File(M3U8PlayList, new string[] { "", UpdBtn.CommandParameter.ToString() });
                    if (readresult!="")
                    {
                        await DisplayAlert("提示信息", readresult, "确定");
                        UpdBtn.IsEnabled=true;
                        return;
                    }


                    M3U8PlayList.Insert(0, new string[] { "默认", UpdBtn.CommandParameter.ToString() });
                    string[] MOptions = new string[M3U8PlayList.Count];
                    MOptions[0]="默认\n";
                    string WantPlayURL = UpdBtn.CommandParameter.ToString();

                    if (M3U8PlayList.Count > 2)
                    {
                        for (int i = 1; i<M3U8PlayList.Count; i++)
                        {
                            MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
                        }

                        string MSelectResult = await DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
                        if (MSelectResult == "取消"||MSelectResult is null)
                        {
                            UpdBtn.IsEnabled=true;
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
                    VideoPrevPage.videoPrevPage.NowPlayingTb.Text="共享直播源";

                    //滚动条在跳转页面前需要先禁用，否则当用户再次返回到分享页面时，滚动条才会禁用
                    UpdBtn.IsEnabled=true;

                    var mainpage = ((Shell)App.Current.MainPage);
                    mainpage.CurrentItem = mainpage.Items.FirstOrDefault();
                    await mainpage.Navigation.PopToRootAsync();

                }
                catch (Exception)
                {
                    await DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                }

            }
            else if (UpdBtn.StyleId=="M3UDownloadBtn")
            {
                string[] options = new string[2];
                using (Stream stream = await new VideoManager().DownloadM3U8FileToStream(UpdBtn.CommandParameter.ToString(), options))
                {
                    if (stream is null)
                    {
                        await DisplayAlert("提示信息", options[0], "确定");
                        UpdBtn.IsEnabled=true;
                        return;
                    }

#if ANDROID
                        await DisplayAlert("提示信息", "安卓暂未开发", "确定");
                        UpdBtn.IsEnabled=true;
                        return;
#else
                    var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, options[1], stream, CancellationToken.None);

                    if (fileSaver.IsSuccessful)
                    {
                        await DisplayAlert("提示信息", "文件已成功保存至：\n" + fileSaver.FilePath, "确定");
                    }
                    else
                    {
                        await DisplayAlert("提示信息", "您已取消了操作。", "确定");
                    }

#endif

                }

            }


            UpdBtn.IsEnabled=true;
        }

    }

    private async void VideoShareList_Refreshing(object sender, EventArgs e)
    {
        VSLCurrentPageIndex=1;
        await GetShareData(1);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        if(videoSharePage!=null)
        {
            return;
        }
        videoSharePage=this;

        await GetShareData(1);
    }

    public async Task GetShareData(int pageindex)
    {
        VideoShareListRing.IsRunning = true;
        VSLPagePanel.IsVisible=false;

        if (pageindex <= 0)
        {
            await DisplayAlert("提示信息", "页码输入不正确！", "确定");
            VideoShareListRing.IsRunning = false;
            return;
        }

        try
        {
            var getresult = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetShareURL?pageindex="+pageindex);
            CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(new StringReader(getresult));
            //CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(await new HttpClient().GetStreamAsync("https://fclivetool.com/api/GetShareURL?pageindex="+pageindex));

            if (videoCheckModel is null)
            {
                await DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                VideoShareListRing.IsRunning = false;
                return;
            }

            if(videoCheckModel.StatusCode=="0")
            {
                if(videoCheckModel.Result=="OK")
                {
                    VideoShareList.ItemsSource= videoCheckModel.Content;

                    VSLPagePanel.IsVisible=true;
                }
            }
            else if(videoCheckModel.StatusCode=="-3")
            {
                await DisplayAlert("提示信息", "没有当前页码。", "确定");

                VSLCurrentPageIndex=1;
                await GetShareData(1);
            }
            else
            {
                await DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
        }

        VideoShareListRing.IsRunning = false;

    }

    private async void M3URefreshBtn_Clicked(object sender, EventArgs e)
    {
        VSLCurrentPageIndex=1;
        await GetShareData(1);
    }

    private async void VSLBackBtn_Clicked(object sender, EventArgs e)
    {
        if (VSLCurrentPageIndex<=1)
        {
            await DisplayAlert("提示信息", "没有上一页了！", "确定");
            return;
        }

        VSLCurrentPageIndex--;
        await GetShareData(VSLCurrentPageIndex);
    }

    private async void VSLJumpBtn_Clicked(object sender, EventArgs e)
    {
        int TargetPage = 1;
        if (!int.TryParse(VSLPageTb.Text, out TargetPage))
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }
        //暂时不做最大判断
        if (TargetPage<1)
        {
            await DisplayAlert("提示信息", "请输入正确的页码！", "确定");
            return;
        }

        VSLCurrentPageIndex=TargetPage;
        await GetShareData(VSLCurrentPageIndex);
    }

    private async void VSLNextBtn_Clicked(object sender, EventArgs e)
    {
        //暂时不做最大判断

        VSLCurrentPageIndex++;
        await GetShareData(VSLCurrentPageIndex);
    }
}