using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCLiveToolApplication.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FCLiveToolApplication.ViewModel
{
    public partial class VideoSharePageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string m3USourceURL = "";
        [ObservableProperty]
        ObservableCollection<SharedVideo> sharedVideos = new ObservableCollection<SharedVideo>();
        [ObservableProperty]
        public bool videoShareListRingRunning = false;
        [ObservableProperty]
        public int vSLCurrentPageIndex = 1;
        [ObservableProperty]
        public string vSLPageTb = "";
        [ObservableProperty]
        public bool vSLPagePanelVisible = false;
        [ObservableProperty]
        public bool m3UAnalysisBtnEnable = true;
        [ObservableProperty]
        public bool m3URefreshBtnEnable = true;
        [ObservableProperty]
        private bool m3UUpdateEnabled = true;
        [ObservableProperty]
        private bool m3UPlayEnabled = true;
        [ObservableProperty]
        private bool m3UDownloadEnabled = true;


        public static VideoSharePageViewModel videoSharePageViewModel;
        private readonly IDialogService _dialogService;

        public List<string[]> M3U8PlayList = new List<string[]>();
        public bool isVSLRefresh = false;
        public VideoSharePageViewModel() : this(new DialogService())
        {

        }
        public VideoSharePageViewModel(IDialogService dialogService)
        {
            _dialogService=dialogService;
            GetShareData(1);
            videoSharePageViewModel=this;
        }

        [RelayCommand]
        private async Task M3UAnalysisBtn()
        {
            if (string.IsNullOrWhiteSpace(M3USourceURL)||!M3USourceURL.Contains("://"))
            {
                await _dialogService.DisplayAlert("提示信息", "请输入正确的直播源地址！", "确定");
                return;
            }
            if (!M3USourceURL.StartsWith("http://")&&!M3USourceURL.StartsWith("https://"))
            {
                await _dialogService.DisplayAlert("提示信息", "暂时只支持HTTP协议的URL！", "确定");
                return;
            }


            M3UAnalysisBtnEnable=false;
            M3URefreshBtnEnable=false;
            try
            {
                var getresult = await new HttpClient().GetStringAsync("https://fclivetool.com/api/NewShareURL?url="+M3USourceURL);
                CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(new StringReader(getresult));

                if (videoCheckModel is null)
                {
                    await _dialogService.DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                    M3UAnalysisBtnEnable=true;
                    M3URefreshBtnEnable=true;
                    return;
                }

                switch (videoCheckModel.StatusCode)
                {
                    case "0":
                        if (videoCheckModel.Result=="OK")
                        {
                            await _dialogService.DisplayAlert("提示信息", "分享直播源成功，感谢您的分享！\n现在将自动加载第一页。", "确定");

                            VSLCurrentPageIndex=1;
                            await GetShareData(1);
                        }
                        else
                        {
                            await _dialogService.DisplayAlert("提示信息", "分享失败，因为直播源已失效！错误信息："+videoCheckModel.Result, "确定");
                        }

                        break;
                    case "-1":
                        await _dialogService.DisplayAlert("提示信息", "请输入正确的直播源地址！", "确定");
                        break;
                    case "-2":
                        await _dialogService.DisplayAlert("提示信息", "暂时只支持HTTP协议的URL！", "确定");
                        break;
                    default:
                        await _dialogService.DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                        break;
                }


            }
            catch (Exception)
            {
                await _dialogService.DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
            }

            M3UAnalysisBtnEnable=true;
            M3URefreshBtnEnable=true;
        }

        [RelayCommand]
        private async Task M3UUpdate(string sourceLink)
        {
            if (string.IsNullOrWhiteSpace(sourceLink))
                return;

            M3UUpdateEnabled=false;

            try
            {
                var getresult = await new HttpClient().GetStringAsync("https://fclivetool.com/api/UpdateShareURL?url="+sourceLink);
                CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(new StringReader(getresult));

                if (videoCheckModel is null)
                {
                    await _dialogService.DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                    M3UUpdateEnabled=true;
                    return;
                }

                switch (videoCheckModel.StatusCode)
                {
                    case "0":
                        if (videoCheckModel.Result=="OK")
                        {
                            await _dialogService.DisplayAlert("提示信息", "直播源有效。", "确定");

                            //await GetShareData(1);
                        }
                        else
                        {
                            await _dialogService.DisplayAlert("提示信息", "直播源已失效，数据已被移除！错误信息："+videoCheckModel.Result, "确定");

                            VSLCurrentPageIndex=1;
                            await GetShareData(1);
                        }

                        break;
                    default:
                        await _dialogService.DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
                        break;
                }


            }
            catch (Exception)
            {
                await _dialogService.DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
            }

            M3UUpdateEnabled=true;
        }

        [RelayCommand]
        private async Task M3UPlay(string sourceLink)
        {
            if (string.IsNullOrWhiteSpace(sourceLink))
                return;

            M3UPlayEnabled=false;

            try
            {
                string readresult = await new VideoManager().DownloadAndReadM3U8File(M3U8PlayList, new string[] { "", sourceLink});
                if (readresult!="")
                {
                    await _dialogService.DisplayAlert("提示信息", readresult, "确定");
                    M3UPlayEnabled=true;
                    return;
                }


                M3U8PlayList.Insert(0, new string[] { "默认", sourceLink });
                string[] MOptions = new string[M3U8PlayList.Count];
                MOptions[0]="默认\n";
                string WantPlayURL = sourceLink;

                if (M3U8PlayList.Count > 2)
                {
                    for (int i = 1; i<M3U8PlayList.Count; i++)
                    {
                        MOptions[i]="【"+i+"】\n文件名："+M3U8PlayList[i][0]+"\n位率："+M3U8PlayList[i][2]+"\n分辨率："+M3U8PlayList[i][3]+"\n帧率："+M3U8PlayList[i][4]+"\n编解码器："+M3U8PlayList[i][5]+"\n标签："+M3U8PlayList[i][6]+"\n";
                    }

                    string MSelectResult = await _dialogService.DisplayActionSheet("请选择一个直播源：", "取消", null, MOptions);
                    if (MSelectResult == "取消"||MSelectResult is null)
                    {
                        M3UPlayEnabled=true;
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
                M3UPlayEnabled=true;

                var mainpage = ((Shell)App.Current.MainPage);
                mainpage.CurrentItem = mainpage.Items.FirstOrDefault();
                await mainpage.Navigation.PopToRootAsync();

            }
            catch (Exception)
            {
                await _dialogService.DisplayAlert("提示信息", "检测直播源有效性时发生异常，请稍后重试！", "确定");
            }

            M3UPlayEnabled=true;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (isVSLRefresh)
            {
                return;
            }

            M3URefreshBtnEnable=false;
            M3UAnalysisBtnEnable=false;
            isVSLRefresh=true;

            VSLCurrentPageIndex=1;
            await GetShareData(1);

            M3URefreshBtnEnable=true;
            M3UAnalysisBtnEnable=true;
            isVSLRefresh=false;
        }

        [RelayCommand]
        private async Task M3UDownload(string sourceLink)
        {
            if (string.IsNullOrWhiteSpace(sourceLink))
                return;

            M3UDownloadEnabled=false;

            string[] options = new string[3];
            using (Stream stream = await new VideoManager().DownloadM3U8FileToStream(sourceLink, options))
            {
                if (stream is null)
                {
                    await _dialogService.DisplayAlert("提示信息", options[0], "确定");
                    M3UDownloadEnabled=true;
                    return;
                }

                var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, options[1], stream, CancellationToken.None);

                if (fileSaver.IsSuccessful)
                {
                    await _dialogService.DisplayAlert("提示信息", "文件已成功保存至：\n" + fileSaver.FilePath, "确定");
                }
                else
                {
                    await _dialogService.DisplayAlert("提示信息", "您已取消了操作。", "确定");
                }


            }

            M3UDownloadEnabled=true;
        }

        [RelayCommand]
        private async Task M3URefreshBtn()
        {
            if (isVSLRefresh)
            {
                return;
            }

            M3URefreshBtnEnable=false;
            M3UAnalysisBtnEnable=false;
            isVSLRefresh=true;

            VSLCurrentPageIndex =1;
            await GetShareData(1);

            M3URefreshBtnEnable=true;
            M3UAnalysisBtnEnable=true;
            isVSLRefresh=false;
        }
        [RelayCommand]
        private async Task VSLBackBtn()
        {
            if (VSLCurrentPageIndex<=1)
            {
                await _dialogService.DisplayAlert("提示信息", "没有上一页了！", "确定");
                return;
            }

            VSLCurrentPageIndex--;
            await GetShareData(VSLCurrentPageIndex);
        }
        [RelayCommand]
        private async Task VSLJumpBtn()
        {
            int TargetPage = 1;
            if (!int.TryParse(VSLPageTb, out TargetPage))
            {
                await _dialogService.DisplayAlert("提示信息", "请输入正确的页码！", "确定");
                return;
            }
            //暂时不做最大判断
            if (TargetPage<1)
            {
                await _dialogService.DisplayAlert("提示信息", "请输入正确的页码！", "确定");
                return;
            }

            VSLCurrentPageIndex=TargetPage;
            await GetShareData(VSLCurrentPageIndex);
        }
        [RelayCommand]
        private async Task VSLNextBtn()
        {
            //暂时不做最大判断

            VSLCurrentPageIndex++;
            await GetShareData(VSLCurrentPageIndex);
        }


        public async Task GetShareData(int pageindex)
        {
            VideoShareListRingRunning = true;
            VSLPagePanelVisible=false;

            if (pageindex <= 0)
            {
                await _dialogService.DisplayAlert("提示信息", "页码输入不正确！", "确定");
                VideoShareListRingRunning = false;
                return;
            }

            try
            {
                var getresult = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetShareURL?pageindex="+pageindex);
                CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(new StringReader(getresult));
                //CheckValidModel videoCheckModel = (CheckValidModel)new XmlSerializer(typeof(CheckValidModel)).Deserialize(await new HttpClient().GetStreamAsync("https://fclivetool.com/api/GetShareURL?pageindex="+pageindex));

                if (videoCheckModel is null)
                {
                    await _dialogService.DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                    VideoShareListRingRunning = false;
                    return;
                }

                if (videoCheckModel.StatusCode=="0")
                {
                    if (videoCheckModel.Result=="OK")
                    {
                        SharedVideos= new ObservableCollection<SharedVideo>(videoCheckModel.Content);

                        VSLPagePanelVisible=true;
                    }
                }
                else if (videoCheckModel.StatusCode=="-3")
                {
                    await _dialogService.DisplayAlert("提示信息", "没有当前页码。", "确定");

                    VSLCurrentPageIndex=1;
                    await GetShareData(1);
                }
                else
                {
                    await _dialogService.DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
                }

            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("提示信息", "获取数据时发生异常，请稍后重试！", "确定");
            }

            VideoShareListRingRunning = false;

        }
    }
}
