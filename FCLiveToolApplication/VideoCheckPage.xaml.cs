using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using System.Text;

namespace FCLiveToolApplication;

public partial class VideoCheckPage : ContentPage
{
    public VideoCheckPage()
    {
        InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        if(videoCheckPage!=null)
        {
            return;
        }

        videoCheckPage=this;
        InitRegexList();
        InitErrorCodeList();
    }
    public void InitRegexList()
    {
        List<string> RegexOption = new List<string>() { "规则1", "规则2", "规则3", "规则4", "规则5" };
        RegexSelectBox.ItemsSource = RegexOption;
        RegexSelectBox.SelectedIndex=2;
    }

    public static VideoCheckPage videoCheckPage;
    List<VideoDetailList> CurrentCheckList = new List<VideoDetailList>();
    List<CheckNOKErrorCodeList> CurrentErrorCodeList = new List<CheckNOKErrorCodeList>();
    string AllVideoData;
    public int CheckFinishCount = 0;
    public int CheckOKCount = 0;
    public int CheckNOKCount = 0;
    CancellationTokenSource M3U8ValidCheckCTS;
    public bool ShowLoadOrRefreshDialog = false;
    public bool isFinishCheck = false;
    object errorcodeObj = new object();

    private void M3USourceRBtn_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton entry = sender as RadioButton;

        if (entry.StyleId == "M3USourceRBtn1")
        {
            LocalM3USelectPanel.IsVisible = true;
            M3USourcePanel.IsVisible = false;
            M3UTextPanel.IsVisible = false;
        }
        else if (entry.StyleId == "M3USourceRBtn2")
        {
            LocalM3USelectPanel.IsVisible = false;
            M3USourcePanel.IsVisible = true;
            M3UTextPanel.IsVisible = false;
        }     
        else if (entry.StyleId == "M3USourceRBtn3")
        {
            LocalM3USelectPanel.IsVisible = false;
            M3USourcePanel.IsVisible = false;
            M3UTextPanel.IsVisible = true;
        }
    }

    private async void SelectLocalM3UFileBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult != 0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
{
    { DevicePlatform.iOS, new[] { "com.apple.mpegurl" , "application/vnd.apple.mpegurl" } },
    { DevicePlatform.macOS, new[] {  "application/vnd.apple.mpegurl" } },
    { DevicePlatform.Android, new[] { "audio/x-mpegurl"  } },
    { DevicePlatform.WinUI, new[] { ".m3u"} }
});

        var filePicker = await FilePicker.PickAsync(new PickOptions()
        {
            PickerTitle = "选择M3U文件",
            FileTypes=fileTypes
        });

        if (filePicker is not null)
        {
            LocalMFileTb.Text=filePicker.FullPath;
            AllVideoData =File.ReadAllText(filePicker.FullPath);

            LoadDataToCheckList();
        }
        else
        {
            LocalMFileTb.Text = "已取消选择";
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }

    }
    public async void LoadDataToCheckList()
    {
        if (string.IsNullOrWhiteSpace(AllVideoData))
        {
            await DisplayAlert("提示信息", "什么都没获取到，请检查数据源！", "确定");
            return;
        }
        if (AllVideoData.Contains("tvg-name="))
        {
            RecommendRegexTb.Text = "1或2";
        }
        else if (AllVideoData.Contains("tvg-logo=")&&!AllVideoData.Contains("tvg-name="))
        {
            RecommendRegexTb.Text = "3或4";
        }
        else
        {
            RecommendRegexTb.Text = "-";
        }

        ClearAllCount();

        RegexManager regexManager = new RegexManager();
        CurrentCheckList=regexManager.DoRegex(AllVideoData, regexManager.GetRegexOptionIndex(RegexOptionCB.IsChecked, (RegexSelectBox.SelectedIndex+1).ToString()));
       
        CheckProgressText.Text="0 / "+CurrentCheckList.Count;
        VideoCheckList.ItemsSource= CurrentCheckList;
    }
    private async void M3UAnalysisBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(M3USourceURLTb.Text))
        {
            await DisplayAlert("提示信息", "请输入直播源M3U8地址！", "确定");
            return;
        }
        if (!M3USourceURLTb.Text.Contains("://"))
        {
            await DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
            return;
        }


        string[] options = new string[2];
        using (Stream stream = await new VideoManager().DownloadM3U8FileToStream(M3USourceURLTb.Text, options))
        {
            if (stream is null)
            {
                await DisplayAlert("提示信息", options[0], "确定");
                return;
            }

            string result = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                string r = "";
                while ((r = await sr.ReadLineAsync()) != null)
                {
                    result+=r+"\n";
                }
            }

            AllVideoData=result;
            LoadDataToCheckList();
        }
    }

    private async void StartCheckBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentCheckList.Count<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有直播源，请尝试更换一个解析方案！", "确定");
            return;
        }

        ClearAllCount();
        M3U8ValidCheckCTS=new CancellationTokenSource();
        StopCheckBtn.IsEnabled=true;
        CheckDataSourcePanel.IsEnabled=false;
        ShowLoadOrRefreshDialog=false;
        isFinishCheck = false;
        CurrentErrorCodeList=new List<CheckNOKErrorCodeList>();
        SaveCheckListBtn.IsEnabled=false;
        PrintCheckLogBtn.IsEnabled=false;

        ValidCheck(CurrentCheckList);
        while (CheckFinishCount<CurrentCheckList.Count)
        {
            if (M3U8ValidCheckCTS.IsCancellationRequested)
            {
                break;
            }
            await Task.Delay(1000);
        }

        isFinishCheck=true;
        StopCheckBtn.IsEnabled=false;
        CheckDataSourcePanel.IsEnabled = true;
        RemoveNOKBtn.IsEnabled=true;
        SaveCheckListBtn.IsEnabled=true;
        PrintCheckLogBtn.IsEnabled=true;
        CheckNOKErrorCodeList.ItemsSource=CurrentErrorCodeList.Take(CurrentErrorCodeList.Count);

        if (!M3U8ValidCheckCTS.IsCancellationRequested)
        {
            VideoCheckList.ItemsSource=CurrentCheckList.Take(CurrentCheckList.Count);

            await DisplayAlert("提示信息", "已全部检测完成！", "确定");
        }
        else
        {
            if (ShowLoadOrRefreshDialog)
            {
                bool tresult = await DisplayAlert("提示信息", "是要查看部分检测完的结果还是要重新加载列表？", "查看结果", "重新加载");
                if (tresult)
                {
                    VideoCheckList.ItemsSource=CurrentCheckList.Take(CurrentCheckList.Count);
                }
                else
                {
                    LoadDataToCheckList();
                }
            }
            else
            {
                LoadDataToCheckList();
                await DisplayAlert("提示信息", "您已取消检测！", "确定");
            }

        }

    }
    public void ClearAllCount()
    {
        //CheckProgressText.Text="0 / "+CurrentCheckList.Count;
        CheckOKCountText.Text="0";
        CheckNOKCountText.Text="0";
        //CheckNOKErrorCodeList.ItemsSource=null;
        InitErrorCodeList();
        CheckFinishCount = 0;
        CheckOKCount = 0;
        CheckNOKCount = 0;
    }
    private void VideoCheckList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ItemsSource")
        {
            VideoCheckList.SelectedItem=null;

            if (VideoCheckList.ItemsSource is not null&&VideoCheckList.ItemsSource.Cast<VideoDetailList>().Count()>0)
            {
                VCLIfmText.IsVisible=false;
            }
            else
            {
                VCLIfmText.IsVisible=true;
            }
        }
    }
    public async void ValidCheck(List<VideoDetailList> videodetaillist)
    {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(Preferences.Get("VideoCheckThreadNum", GlobalParameter.VideoCheckThreadNum));

        object obj = new object();
        for (int i = 0; i<videodetaillist.Count; i++)
        {
            var vd = videodetaillist[i];
            Thread thread = new Thread(async () =>
            {
               await semaphoreSlim.WaitAsync();
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout=TimeSpan.FromMinutes(2);

                    int statusCode;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Preferences.Get("VideoCheckUA", GlobalParameter.VideoCheckUA));
                    HttpResponseMessage response = null;

                    try
                    {
                        //取消操作
                        M3U8ValidCheckCTS.Token.ThrowIfCancellationRequested();

                        vd.HTTPStatusCode="Checking...";
                        vd.HTTPStatusTextBKG=Colors.Gray;

                        response = await httpClient.GetAsync(vd.SourceLink, M3U8ValidCheckCTS.Token);

                        statusCode=(int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            vd.HTTPStatusCode="OK";
                            vd.HTTPStatusTextBKG=Colors.Green;

                            lock (obj)
                            {
                                CheckOKCount++;

                                MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    CheckOKCountText.Text=CheckOKCount.ToString();
                                });
                            }
                        }
                        else
                        {
                            vd.HTTPStatusCode=statusCode.ToString();
                            vd.HTTPStatusTextBKG=Colors.Orange;

                            lock (obj)
                            {
                                CheckNOKCount++;

                                MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    CheckNOKCountText.Text=CheckNOKCount.ToString();
                                });
                            }

                            AddToErrorCodeList(new CheckNOKErrorCodeList() {  HTTPStatusCode=statusCode.ToString(), HTTPStatusTextBKG=Colors.Orange });
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        //手动停止和未进行检测的，暂时不统计
                        if (!M3U8ValidCheckCTS.IsCancellationRequested)
                        {
                            vd.HTTPStatusCode="Timeout";
                            vd.HTTPStatusTextBKG=Colors.Purple;

                            lock (obj)
                            {
                                CheckNOKCount++;

                                MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    CheckNOKCountText.Text=CheckNOKCount.ToString();
                                });
                            }

                            AddToErrorCodeList(new CheckNOKErrorCodeList() { HTTPStatusCode="Timeout", HTTPStatusTextBKG=Colors.Purple });
                        }

                    }
                    catch (Exception ex)
                    {
                        vd.HTTPStatusCode="ERROR";
                        vd.HTTPStatusTextBKG=Colors.Red;

                        lock (obj)
                        {
                            CheckNOKCount++;

                            MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                CheckNOKCountText.Text=CheckNOKCount.ToString();
                            });
                        }

                        AddToErrorCodeList(new CheckNOKErrorCodeList() { HTTPStatusCode="ERROR", HTTPStatusTextBKG=Colors.Red });
                    }

                    if (!M3U8ValidCheckCTS.IsCancellationRequested)
                    {
                        lock (obj)
                        {
                            CheckFinishCount++;

                            MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                CheckProgressText.Text=CheckFinishCount+" / "+videodetaillist.Count;
                            });
                        }


                    }
                }

                semaphoreSlim.Release();
            });
            thread.Start();

            await Task.Delay(10);
        }
    }
    private void RegexSelectTipBtn_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("帮助信息", new MsgManager().GetRegexOptionTip(), "关闭");
    }

    private void RegexSelectBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AllVideoData))
        {
            return;
        }

        LoadDataToCheckList();
    }

    private async void StopCheckBtn_Clicked(object sender, EventArgs e)
    {
        if (M3U8ValidCheckCTS!=null)
        {
            bool CancelCheck = await DisplayAlert("提示信息", "您要停止检测吗？停止后暂时不支持恢复进度。", "确定", "取消");
            if (CancelCheck)
            {
                M3U8ValidCheckCTS.Cancel();
                //这句可以注释掉
                StopCheckBtn.IsEnabled=false;
                CheckDataSourcePanel.IsEnabled=true;
                ShowLoadOrRefreshDialog = true;
            }

        }
    }

    private async void RemoveNOKBtn_Clicked(object sender, EventArgs e)
    {
        if (CurrentCheckList.Count<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有直播源，请尝试更换一个解析方案！", "确定");
            return;
        }
        if (!isFinishCheck)
        {
            await DisplayAlert("提示信息", "当前正在执行直播源检测！", "确定");
            return;
        }
        var notokcount = CurrentCheckList.Where(p => (p.HTTPStatusCode!="OK")&&(p.HTTPStatusCode!=null)).Count();
        if (notokcount<1)
        {
            await DisplayAlert("提示信息", "当前列表里没有无效的直播信号，无需操作！", "确定");
            return;
        }


        for (int i = CurrentCheckList.Count-1; i >=0; i--)
        {
            if (CurrentCheckList[i].HTTPStatusCode!="OK"&&CurrentCheckList[i].HTTPStatusCode!=null)
            {
                AllVideoData=AllVideoData.Replace(CurrentCheckList[i].FullM3U8Str, "");
                CurrentCheckList.RemoveAt(i);
            }
        }

        VideoCheckList.ItemsSource = CurrentCheckList.Take(CurrentCheckList.Count);
        await DisplayAlert("提示信息", "已成功从列表里移除所有共"+notokcount+"条无效的直播信号！", "确定");
    }

    private void ShowPopupBtn_Clicked(object sender, EventArgs e)
    {
        var checkPagePopup = new VideoCheckPagePopup();
        checkPagePopup.MainGrid.HeightRequest = Window.Height/1.5;
        checkPagePopup.MainGrid.WidthRequest =Window.Width/1.5;
        this.ShowPopup(checkPagePopup);
    }

    public async void PopShowMsg(string msg)
    {
        await DisplayAlert("提示信息", msg, "确定");
    }    
    public async Task<bool> PopShowMsgAndReturn(string msg)
    {
        return await DisplayAlert("提示信息", msg, "确定","取消");
    }

    public void AddToErrorCodeList(CheckNOKErrorCodeList errorcodeList)
    {
        lock (errorcodeObj)
        {
            var eclist = CurrentErrorCodeList.Where(p => p.HTTPStatusCode==errorcodeList.HTTPStatusCode);
            if (eclist.Count()<1)
            {
                errorcodeList.ErrorCodeCount = 1;
                CurrentErrorCodeList.Add(errorcodeList);
            }
            else
            {
              eclist.FirstOrDefault().ErrorCodeCount+=1;
            }

            //MainThread.InvokeOnMainThreadAsync(() =>
            //{
            //    CheckNOKErrorCodeList.ItemsSource=CurrentErrorCodeList.Take(CurrentErrorCodeList.Count);
            //});
        }

    }

    public void InitErrorCodeList()
    {
        List<CheckNOKErrorCodeList> tlist = new List<CheckNOKErrorCodeList>();
        tlist.Add(new CheckNOKErrorCodeList() { HTTPStatusCode="检测结束后生成" ,HTTPStatusTextBKG=Colors.Black}) ;
        CheckNOKErrorCodeList.ItemsSource=tlist;
    }

    private async void SaveCheckListBtn_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(AllVideoData))
        {
            await DisplayAlert("提示信息", "请先获取直播源！", "确定");
            return;
        }


        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(AllVideoData)))
        {
            var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, ".M3U", ms, CancellationToken.None);

            if (fileSaver.IsSuccessful)
            {
                await DisplayAlert("提示信息", "文件已成功保存至：\n"+fileSaver.FilePath, "确定");
            }
            else
            {
                await DisplayAlert("提示信息", "您已取消了操作。", "确定");
            }

        }
    }

    private async void PrintCheckLogBtn_Clicked(object sender, EventArgs e)
    {
        if(CurrentCheckList.Count<1)
        {
            await DisplayAlert("提示信息", "当前列表为空，请选择直播源并进行检测，之后再输出报告！", "确定");
            return;
        }
        if(CurrentCheckList.Where(p=>!string.IsNullOrWhiteSpace(p.HTTPStatusCode)).Count()<1)
        {
            await DisplayAlert("提示信息", "请先进行检测再输出报告！", "确定");
            return;
        }


        string printStr = "台名,URL,检测结果";
        for(int i=0;i<CurrentCheckList.Count; i++)
        {
            printStr+="\n"+ CurrentCheckList[i].SourceName+","+CurrentCheckList[i].SourceLink+","+CurrentCheckList[i].HTTPStatusCode;
        }

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(printStr)))
        {
            var fileSaver = await FileSaver.SaveAsync(FileSystem.AppDataDirectory, "CheckLog.csv", ms, CancellationToken.None);

            if (fileSaver.IsSuccessful)
            {
                await DisplayAlert("提示信息", "文件已成功保存至：\n"+fileSaver.FilePath, "确定");
            }
            else
            {
                await DisplayAlert("提示信息", "您已取消了操作。", "确定");
            }

        }
    }

    private void M3UAnalysisStringBtn_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(M3UStringTb.Text))
        {
            return;
        }


        AllVideoData=M3UStringTb.Text.Replace("\r","\n");
        LoadDataToCheckList();
    }
}