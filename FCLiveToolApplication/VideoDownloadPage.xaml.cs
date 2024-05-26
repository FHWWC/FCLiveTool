using CommunityToolkit.Maui.Storage;
using System.Dynamic;

namespace FCLiveToolApplication;

public partial class VideoDownloadPage : ContentPage
{
    public VideoDownloadPage()
    {
        InitializeComponent();
    }
    //public long ReceiveSize = 0;
    //public long AllFilesize = 0;
    //public double DownloadProcess;
    //public int ThreadNum = 1;
    //public List<ThreadInfo> threadinfos;
    public List<DownloadVideoFileList> DownloadFileLists = new List<DownloadVideoFileList>();
    public List<string> LocalM3U8FilesList = new List<string>();

    private async void SelectLocalM3U8FileBtn_Clicked(object sender, EventArgs e)
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
    { DevicePlatform.WinUI, new[] { ".m3u8"} }
});

        var filePicker = await FilePicker.PickMultipleAsync(new PickOptions()
        {
            PickerTitle = "选择M3U8文件",
            FileTypes = fileTypes
        });

        if (filePicker is not null && filePicker.Count() > 0)
        {
            LocalM3U8FilesList = filePicker.Select(p => p.FullPath).ToList();
            LocalM3U8Tb.Text = "已经选择了" + LocalM3U8FilesList.Count + "个文件";
        }
        else
        {
            LocalM3U8Tb.Text = "已取消选择";
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }
    }

    private async void SelectLocalM3U8FolderBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult != 0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        var folderPicker = await FolderPicker.PickAsync(FileSystem.AppDataDirectory, CancellationToken.None);

        if (folderPicker.IsSuccessful)
        {
            List<string> mlist = new List<string>();
            LoadM3U8FileFromSystem(folderPicker.Folder.Path, ref mlist);
            LocalM3U8FilesList=mlist;

            if(LocalM3U8FilesList.Count<1)
            {
                await DisplayAlert("提示信息", "当前选择的目录下没有M3U8文件，请重新选择！", "确定");
            }
            LocalM3U8Tb.Text = "已经选择了" + LocalM3U8FilesList.Count + "个文件";

        }
        else
        {
            LocalM3U8Tb.Text = "已取消选择";
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }


    }
    public void LoadM3U8FileFromSystem(string path,ref List<string> list)
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
                    if (LocalM3U8FilesList.Where(p => p==item).Count()<1)
                    {
                        list.Add(item);
                    }

                }

            }

        }
    }
    private async void M3U8AnalysisBtn_Clicked(object sender, EventArgs e)
    {
        List<VideoAnalysisList> ResultList = new List<VideoAnalysisList>();
        List<string> readresult=new List<string>();

        if (M3U8SourceRBtn1.IsChecked)
        {
            if (string.IsNullOrWhiteSpace(M3U8SourceURLTb.Text))
            {
                await DisplayAlert("提示信息", "请输入直播源M3U8地址！", "确定");
                return;
            }
            if (!M3U8SourceURLTb.Text.Contains("://"))
            {
                await DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
                return;
            }

            //后续添加批量识别，并且加入批量识别的查重
            List<string> M3U8DownloadURLsList = new List<string>();
            M3U8DownloadURLsList.Add(M3U8SourceURLTb.Text);

            readresult = await new VideoManager().DownloadAndReadM3U8FileForDownloadTS(ResultList, M3U8DownloadURLsList, 0);
            if (readresult.Count<1)
            {
                return;
            }

        }
        else if (M3U8SourceRBtn2.IsChecked)
        {
            if (LocalM3U8FilesList.Count < 1)
            {
                await DisplayAlert("提示信息", "当前没有选择任何文件！请先选择文件或文件夹！", "确定");
                return;
            }

            readresult = await new VideoManager().DownloadAndReadM3U8FileForDownloadTS(ResultList,LocalM3U8FilesList , 1);
            if(readresult.Count<1)
            {
                return;
            }
            int needAddServerCount = readresult.Where(p => p.StartsWith("CODE_")).Count();
            if (needAddServerCount> 0)
            {
                bool tresult = await DisplayAlert("提示信息", "当前有"+needAddServerCount+"个本地直播源文件内的分片文件URL是相对地址，程序无法知晓它们的服务器，" +
                    "所以需要你补充直播源对应的服务器地址，你要继续补充还是跳过这些文件？", "继续", "跳过");
                if (tresult)
                {
                    for(int i = 0;i<readresult.Count;i++)
                    {
                        if (readresult[i].StartsWith("CODE_"))
                        {
                            string urlnewvalue = await DisplayPromptAsync("添加服务器", "请输入直播源的服务器地址，该地址除了文件名以外其他都要包含。\n"+
                                "例如文件的地址是 https://example.com/abc/123.ts ，那么你应该填写 https://example.com/abc/", "保存并下一个", "取消", "URL...", -1, Keyboard.Text, "");
                           
                            if (string.IsNullOrWhiteSpace(urlnewvalue))
                            {
                                if (urlnewvalue!=null)
                                {
                                    await DisplayAlert("提示信息", "请输入正确的内容！", "确定");
                                    i--;
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (!urlnewvalue.Contains("://"))
                            {
                                await DisplayAlert("提示信息", "输入的内容不符合URL规范！", "确定");
                                i--;
                                continue;
                            }

                            if(!urlnewvalue.EndsWith("/"))
                            {
                                urlnewvalue+="/";
                            }

                            //将用户输入的内容与之前的地址进行拼接
                            ResultList[i].TS_PARM.ForEach(p=>
                            {
                                p.FullURL=urlnewvalue+p.FullURL;
                            });

                            //重置标识为空值，默认用户输入的是正确的服务器
                            readresult[i]="";
                        }

                    }

                }

                //清除剩余的CODE_标识，因为没有保留的意义
                for (int i = readresult.Count-1; i>=0; i--)
                {
                    if (readresult[i].StartsWith("CODE_"))
                    {
                        readresult.RemoveAt(i);
                        ResultList.RemoveAt(i);
                    }
                }


                //要判断readresult是否为空
                if(readresult.Count<1)
                {
                    await DisplayAlert("提示信息", "本次不会添加新的条目，因为选择的列表里没有有效的直播源！", "确定");
                    return;
                }


            }

        }


        var trList = readresult.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if(trList.Count > 0)
        {
            string tmsg = "";
            for (int i = readresult.Count-1;i>=0 ; i--)
            {
                if (!string.IsNullOrWhiteSpace(readresult[i]))
                {
                    tmsg=ResultList[i].FullURL+"\n"+ readresult[i]+"\n\n"+tmsg;
                    //移除全部失效
                    readresult.RemoveAt(i);
                    ResultList.RemoveAt(i);
                }

            }
            await DisplayAlert("提示信息", "当前有一个或多个直播源存在问题，详细内容如下：\n\n\n"+tmsg, "确定");

            if(ResultList.Count<1)
            {
                await DisplayAlert("提示信息", "本次不会添加新的条目，因为选择的列表里没有有效的直播源！", "确定");
                return;
            }
        }

        

        List<VideoAnalysisList> videoAnalysisLists = new List<VideoAnalysisList>();
        if (VideoAnalysisList.ItemsSource != null)
        {
            videoAnalysisLists = VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().ToList();

            videoAnalysisLists.ForEach(p=>
            {
                var titem = ResultList.FirstOrDefault(p2 => p2.FullURL == p.FullURL);
                if (titem != null)
                {
                    videoAnalysisLists.Remove(p);
                }
            });

        }
        videoAnalysisLists.AddRange(ResultList);

        VideoAnalysisList.ItemsSource = videoAnalysisLists;
    }

    private void M3U8SourceRBtn_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton entry = sender as RadioButton;

        if (entry.StyleId == "M3U8SourceRBtn1")
        {
            M3U8SourceURLTb.IsVisible = true;
            LocalM3U8SelectPanel.IsVisible = false;
        }
        else if (entry.StyleId == "M3U8SourceRBtn2")
        {
            M3U8SourceURLTb.IsVisible = false;
            LocalM3U8SelectPanel.IsVisible = true;
        }

    }

    private async void M3U8DownloadBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SaveDownloadFolderTb.Text))
        {
            await DisplayAlert("提示信息", "请先选择下载文件要保存的位置！", "确定");
            return;
        }
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult != 0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要写入文件！", "确定");
            return;
        }
        if (!Directory.Exists(SaveDownloadFolderTb.Text))
        {
            await DisplayAlert("提示信息", "当前下载文件保存位置的目录不存在，请重新选择！", "确定");
            return;
        }
        if (VideoAnalysisList.ItemsSource is null)
        {
            await DisplayAlert("提示信息", "当前列表为空，请先获取一个M3U8直播源！", "确定");
            return;
        }

        var tlist = VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().Where(p => p.IsSelected).ToList();
        for (int i = 0; i < tlist.Count; i++)
        {
            var tobj = tlist[i];
            VideoManager vmanager = new VideoManager();
            if (DownloadFileLists.Where(p => p.M3U8FullLink == tobj.FullURL && p.CurrentActiveObject.isContinueDownloadStream).Count() > 0)
            {
                await DisplayAlert("提示信息", "你当前正在下载这个M3U8直播流：" + tobj.FileName + " ，不能重复添加任务！", "确定");
                continue;
            }
            DownloadFileLists.Add(new DownloadVideoFileList() { SaveFilePath = SaveDownloadFolderTb.Text, CurrentVALIfm = tobj, CurrentActiveObject = vmanager });

            new Thread(async () =>
            {
                string r="";
#if ANDROID
                r = await vmanager.DownloadM3U8Stream(tobj, SaveDownloadFolderTb.Text + "/", true);
#else
r = await vmanager.DownloadM3U8Stream(tobj, SaveDownloadFolderTb.Text + "\\", true);
#endif

                if (r != "")
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("提示信息", tobj.FileName + "\n" + r, "确定");
                    });

                }
            }).Start();
        }

        DownloadVideoFileList.ItemsSource = DownloadFileLists;

    }

    private void VideoAnalysisListCB_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (VideoAnalysisList.ItemsSource is null)
        {
            return;
        }

        var tlist = VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().ToList();
        tlist.ForEach(p => { p.IsSelected = e.Value; });
        VideoAnalysisList.ItemsSource = tlist;
    }

    /*
         public async Task<string> DownloadM3U8Stream(List<M3U8_TS_PARM> mlist,string savepath,string filename)
        {
            threadinfos= new List<ThreadInfo>();
            int FileIndex = 0;

            foreach (var m in  mlist)
            {
                string url = m.FullURL;
                using (HttpClient httpClient = new HttpClient())
                {
                    int statusCode;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                    HttpResponseMessage response = null;

                    try
                    {
                        response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                        statusCode=(int)response.StatusCode;
                        if (!response.IsSuccessStatusCode)
                        {
                            return "请求失败！"+"HTTP错误代码："+statusCode;
                        }

                        AllFilesize = response.Content.Headers.ContentLength??-1;
                        if (AllFilesize<=0)
                        {
                            return "无法从 ContentLength 中获取有效的文件大小！";
                        }

                    }
                    catch (Exception)
                    {
                        return "请求发生异常！";
                    }


                    List<Task> taskList = new List<Task>();
                    int FinishTaskCount = 0;
                    ReceiveSize=0;

                    //此处要加上连续获取M3U8功能

                    long pieceSize = (long)AllFilesize / ThreadNum + (long)AllFilesize % ThreadNum;
                    for (int i = 0; i < ThreadNum; i++)
                    {
                        ThreadInfo currentThread = new ThreadInfo();
                        currentThread.ThreadId = i;
                        currentThread.ThreadStatus = false;

                        currentThread.TmpFileName = string.Format($"{savepath}TMP{FileIndex}_{filename}.tmp");
                        currentThread.Url = url;
                        currentThread.FileName = filename+".mp4";

                        long startPosition = (i * pieceSize);
                        currentThread.StartPosition = startPosition == 0 ? 0 : startPosition + 1;
                        currentThread.FileSize = startPosition + pieceSize;

                        threadinfos.Add(currentThread);

                        taskList.Add(Task.Factory.StartNew(async () =>
                        {
                            string r = await ReceiveHttp(currentThread);
                            if (r!="")
                            {
                                await DisplayAlert("提示信息", filename+"\n"+ r, "确定");
                            }

                            FinishTaskCount++;
                        }));
                        FileIndex++;
                    }


                    while (true)
                    {
                        if (FinishTaskCount==taskList.Count)
                        {
                            break;
                        }
                    }    

                }
            }




            MergeFile(savepath+filename+".mp4");
            threadinfos.Clear();

            return "";
        }
        public async Task<string> ReceiveHttp(object thread)
        {
            FileStream fs = null;
            Stream ns = null;
            try
            {
                ThreadInfo currentThread = (ThreadInfo)thread;

                //后续加上文件已存在的判断
                if (!File.Exists(currentThread.FileName))
                {
                    fs = new FileStream(currentThread.TmpFileName, FileMode.Create);


                    using (HttpClient httpClient = new HttpClient())
                    {
                        int statusCode;
                        httpClient.DefaultRequestHeaders.Add("Accept-Ranges", "bytes");
                        httpClient.DefaultRequestHeaders.Add("Range", "bytes="+currentThread.StartPosition+"-"+(currentThread.FileSize));
                        HttpResponseMessage response = null;

                        try
                        {
                            response = await httpClient.GetAsync(currentThread.Url);

                            statusCode=(int)response.StatusCode;
                            if (!response.IsSuccessStatusCode)
                            {
                                return "请求失败！"+"HTTP错误代码："+statusCode;
                            }

                        }
                        catch (Exception)
                        {
                            return "请求发生异常！";
                        }


                       ns = await response.Content.ReadAsStreamAsync();
                       ns.CopyTo(fs);
                    }

                    ReceiveSize += ns.Length;
                    double percent = (double)ReceiveSize / (double)AllFilesize * 100;

                    DownloadProcess=percent;


                }
                currentThread.ThreadStatus = true;
            }
            catch 
            {
                return "下载时发生异常";
            }
            finally
            {
                fs?.Close();
                ns?.Close();
            }

            return "";
        }

        public class ThreadInfo
        {
            public int ThreadId { get; set; }
            public bool ThreadStatus { get; set; }
            public long StartPosition { get; set; }
            public long FileSize { get; set; }
            public string Url { get; set; }
            public string TmpFileName { get; set; }
            public string FileName { get; set; }
            public int Times { get; set; }
        }

        private void MergeFile(string filepath)
    {
        string downFileNamePath = filepath;
        int length = 0;
        using (FileStream fs = new FileStream(downFileNamePath, FileMode.Create))
        {
            foreach (var item in threadinfos.OrderBy(o => o.ThreadId))
            {
                if (!File.Exists(item.TmpFileName)) continue;
                var tempFile = item.TmpFileName;

                try
                {
                    using (FileStream tempStream = new FileStream(tempFile, FileMode.Open))
                    {
                        byte[] buffer = new byte[tempStream.Length];

                        while ((length = tempStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, length);
                        }
                        tempStream.Flush();
                    }

                }
                catch
                {

                }

                try
                {
                    File.Delete(item.TmpFileName);
                }
                catch (Exception)
                {

                }

            }
        }

    }
     */

    private async void SelectSaveDownloadFolderBtn_Clicked(object sender, EventArgs e)
    {
        int permResult = await new APPPermissions().CheckAndReqPermissions();
        if (permResult != 0)
        {
            await DisplayAlert("提示信息", "请授权读取和写入权限，程序需要读取文件！", "确定");
            return;
        }

        var folderPicker = await FolderPicker.PickAsync(FileSystem.AppDataDirectory, CancellationToken.None);

        if (folderPicker.IsSuccessful)
        {
            SaveDownloadFolderTb.Text = folderPicker.Folder.Path;
        }
        else
        {
            await DisplayAlert("提示信息", "您已取消了操作。", "确定");
        }
    }

    private void DownloadFileListCB_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (DownloadVideoFileList.ItemsSource is null)
        {
            return;
        }

        var tlist = DownloadVideoFileList.ItemsSource.Cast<DownloadVideoFileList>().ToList();
        tlist.ForEach(p => { p.IsSelected = e.Value; });
        DownloadVideoFileList.ItemsSource = tlist;
    }

    private async void DownloadFileStopBtn_Clicked(object sender, EventArgs e)
    {
        if (DownloadVideoFileList.ItemsSource is null)
        {
            await DisplayAlert("提示信息", "当前列表为空！", "确定");
            return;
        }

        var tlist = DownloadVideoFileList.ItemsSource.Cast<DownloadVideoFileList>().Where(p => p.IsSelected).ToList();
        if (tlist.Count < 1)
        {
            await DisplayAlert("提示信息", "请先至少勾选一条要停止的任务！", "确定");
            return;
        }

        tlist.ForEach(p =>
        {
            var tl = DownloadFileLists.Where(p2 => p2 == p).FirstOrDefault();
            tl.CurrentActiveObject.isContinueDownloadStream = false;
            //DownloadFileLists.Remove(tl);
        });
        DownloadFileLists.RemoveAll(p => !p.CurrentActiveObject.isContinueDownloadStream || p.CurrentActiveObject.isEndList);


        DownloadVideoFileList.ItemsSource = DownloadFileLists;
        await DisplayAlert("提示信息", "已停止选定的任务！", "确定");

    }
}