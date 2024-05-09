using System.Text.RegularExpressions;

namespace FCLiveToolApplication
{
    public class HamListClass
    {
        public ImageSource ItemLogo { get; set; }
        public string ItemTitle { get; set; }
    }
    public class VideoList
    {
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
        public string Description { get; set; }
        public string RecommendReg { get; set; }
        public string LiveType { get; set; }
    }
    public class VideoDetailList
    {
        public int Id { get; set; }
        public string LogoLink { get; set; }
        public string SourceName { get; set; }
        public string SourceLink { get; set; }
        //客户端专用
        public string FullM3U8Str { get; set; }
        public bool isHTTPS { get; set; }
        public string FileName { get; set; }
        public string HTTPStatusCode { get; set; }
        public Color HTTPStatusTextBKG { get; set; }
    }
    public class RecentVList
    {
        public int Id { get; set; }

        public string SourceName { get; set; }
        public string LogoLink { get; set; }
        public string SourceLink { get; set; }
        public string Description { get; set; }
        public DateTime AddDT { get; set; }
        //客户端专用
        public string PastTime { get; set; }
    }
    public class LocalM3U8List
    {
        public string ItemId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FullFilePath { get; set; }
    }
    public class LocalM3UList
    {
        public string ItemId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FullFilePath { get; set; }
    }
    public abstract class VideoEditList
    {    /// <summary>
         /// 当前项使用的数据模板ID
         /// </summary>
         /// <value>
         /// ID对应的数据模板：
         /// <para>1：VideoEditListEXT；2：VideoEditListTVG；3：VideoEditListEXT_Readonly；4：VideoEditListSourceLink；5：VideoEditListOtherString。</para>
         /// </value>
        public int ItemTypeId { get; set; }
        /// <summary>
        /// 当前项的名称
        /// </summary>
        /// <value>
        /// 名称在集合里面是唯一的。
        /// </value>
        public string ItemName { get; set; }
        public bool IsSelected { get; set; }
    }
    public class VideoEditListEXT : VideoEditList
    {
        public string EXTTag { get; set; }
    }
    public class VideoEditListEXT_Readonly : VideoEditList
    {
        public string EXTTag { get; set; }
    }
    public class VideoEditListTVG : VideoEditList
    {
        public string AllStr { get; set; }
        public string GroupTitle { get; set; }
        public string TVGGroup { get; set; }
        public string TVGID { get; set; }
        public string TVGLogo { get; set; }
        public string TVGCountry { get; set; }
        public string TVGLanguage { get; set; }
        public string TVGName { get; set; }
        public string TVGURL { get; set; }

    }
    public class VideoEditListSourceLink : VideoEditList
    {
        public string SourceLink { get; set; }
    }
    public class VideoEditListOtherString : VideoEditList
    {
        public string AllStr { get; set; }
    }

    public class VideoAnalysisList
    {
        public string ItemId { get; set; }
        public string AllowCache { get; set; }
        public string TargetDuration { get; set; }
        public string MediaSequence { get; set; }
        public List<M3U8_TS_PARM> TS_PARM { get; set; }
        //非M3U8文件内参数
        public string M3U8URL { get; set; }
        public string FileName { get; set; }
        public string FullURL { get; set; }
        public bool IsSelected { get; set; }

    }
    public class M3U8_TS_PARM
    {
        public int ItemId { get; set; }
        public double Time { get; set; }
        public string FullURL { get; set; }
        public string FileName { get; set; }
    }
    public class DownloadTempFileList
    {
        public int ItemId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long Filesize { get; set; }
        public string FullLink { get; set; }
    }
    public class DownloadVideoFileList
    {
        public int ItemId { get; set; }
        public string SaveFileName { get { return CurrentVALIfm.FileName; } }
        public string SaveFilePath { get; set; }
        public string M3U8FullLink { get { return CurrentVALIfm.FullURL; } }
        public int TS_FileCount { get { return CurrentVALIfm.TS_PARM.Count; } }
        public VideoManager CurrentActiveObject { get; set; }
        public VideoAnalysisList CurrentVALIfm { get; set; }
        public bool IsSelected { get; set; }
    }
    public class VideoEditListDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is VideoEditListEXT)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListEXTStyle"];
            }
            else if (item is VideoEditListTVG)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListTVGStyle"];
            }
            else if(item is VideoEditListEXT_Readonly)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListEXT_ReadonlyStyle"];
            }
            else if (item is VideoEditListSourceLink)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListSourceLinkStyle"];
            }
            else if (item is VideoEditListOtherString)
            {
                return (DataTemplate)VideoEditPage.videoEditPage.Resources["VideoEditListOtherStringStyle"];
            }

            return null;
        }
    }
    public class APPPermissions
    {
        /// <summary>
        /// 检查是否有文件读取的权限
        /// </summary>
        /// <returns>状态码。0：已经有权限或已成功获取到权限；1：读取没有权限；2：写入没有权限；3：读写都没有权限或其他异常情况；</returns>
        public async Task<int> CheckAndReqPermissions()
        {
            //Windows操作系统不需要这种单独获取权限
#if WINDOWS
            return 0;
#else
            var checkRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var checkWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (checkRead == PermissionStatus.Granted&&checkWrite == PermissionStatus.Granted)
            {
                return 0;
            }
            else
            {
                var reqRead = await Permissions.RequestAsync<Permissions.StorageRead>();
                var reqWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (reqRead == PermissionStatus.Granted&&reqWrite == PermissionStatus.Granted)
                {
                    return 0;
                }
                else if (reqRead != PermissionStatus.Granted &&reqWrite == PermissionStatus.Granted)
                {
                    return 1;
                }
                else if (reqRead == PermissionStatus.Granted &&reqWrite != PermissionStatus.Granted)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }

            }
#endif
        }

    }
    public class VideoManager
    {
        public List<DownloadTempFileList> TempFileList;
        public bool isContinueDownloadStream = true;
        public bool isEndList = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="M3U8PlayList">M3U8文件内的所有直播信号</param>
        /// <param name="VideoIfm">1：名称；2：URL；</param>
        /// <returns></returns>
        public async Task<string> DownloadAndReadM3U8File(List<string[]> M3U8PlayList, string[] VideoIfm)
        {
            M3U8PlayList.Clear();
            using (HttpClient httpClient = new HttpClient())
            {
                int statusCode;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                HttpResponseMessage response = null;

                try
                {
                    //detail.SourceLink 不清除\n和\r符可能会带来影响，这里暂时不清除
                    response = await httpClient.GetAsync(VideoIfm[1]);

                    statusCode=(int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        return "获取文件失败，请稍后重试！\n"+"HTTP错误代码："+statusCode;
                    }
                }
                catch (Exception)
                {
                    return "无法连接到对方服务器，请检查您的网络或者更换一个直播源！";
                }


                VideoIfm[0]=VideoIfm[0].Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
                try
                {
                    using (StreamReader sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        string r = "";
                        string tProperties = "";
                        while ((r=await sr.ReadLineAsync())!=null)
                        {
                            if (r.StartsWith("#"))
                            {
                                tProperties=r;
                                continue;
                            }
                            else if (String.IsNullOrWhiteSpace(r))
                                continue;
                            else if (r.Contains(".m3u8"))
                            {
                                if (!r.Contains("://"))
                                {
                                    r=VideoIfm[1].Substring(0, VideoIfm[1].LastIndexOf("/")+1)+r;
                                }


                                //string m3u8name = r[new Range(r.LastIndexOf("/")+1, r.LastIndexOf(".m3u8")+5)];                                
                                Match m3u8nameResult = Regex.Match(r, @"\/([^\/]+\.m3u8)");
                                string m3u8name = m3u8nameResult.Groups[1].Value=="" ? "未解析到文件名" : m3u8nameResult.Groups[1].Value;

                                Match tPResult = Regex.Match(tProperties, @"(?:,|:)BANDWIDTH=(.*?)(,|\n|$)");
                                string tBandwidth = tPResult.Groups[1].Value.Replace("\"", "");
                                tBandwidth=tBandwidth=="" ? "---" : tBandwidth;
                                Match tPResult2 = Regex.Match(tProperties, @"RESOLUTION=(.*?)(,|\n|$)");
                                string tResolution = tPResult2.Groups[1].Value.Replace("\"", "");
                                tResolution=tResolution=="" ? "---" : tResolution;
                                Match tPResult3 = Regex.Match(tProperties, @"FRAME-RATE=(.*?)(,|\n|$)");
                                string tFrameRate = tPResult3.Groups[1].Value.Replace("\"", "");
                                tFrameRate=tFrameRate=="" ? "---" : tFrameRate;
                                Match tPResult4 = Regex.Match(tProperties, @"CODECS=""(.*?)""(,|\n|$)");
                                string tCodecs = tPResult4.Groups[1].Value.Replace("\"", "");
                                tCodecs=tCodecs=="" ? "---" : tCodecs;
                                Match tPResult5 = Regex.Match(tProperties, @"NAME=(.*?)(,|\n|$)");
                                string tName = tPResult5.Groups[1].Value.Replace("\"", "");
                                tName=tName=="" ? "---" : tName;


                                M3U8PlayList.Add(new string[] { m3u8name, r, tBandwidth, tResolution, tFrameRate, tCodecs, tName });

                            }
                            else if (r.Contains(".ts"))
                            {
                                //备用
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    return "解析出现问题，可能是M3U8文件数据格式有问题。";
                }


            }


            return "";
        }
        /// <summary>
        ///  下载M3U8文件并解析所有TS分片文件，该方法会返回一个TS列表，用于下载分片文件
        /// </summary>
        /// <param name="videoAnalysisList">从M3U8文件里解析到的所有信息</param>
        /// <param name="VideoIfm">1：URL；</param>
        /// <returns></returns>
        public async Task<string> DownloadAndReadM3U8FileForDownloadTS(VideoAnalysisList videoAnalysisList, string[] VideoIfm)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                int statusCode;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                HttpResponseMessage response = null;

                try
                {
                    response = await httpClient.GetAsync(VideoIfm[0]);

                    statusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        return "获取文件失败，请稍后重试！\n" + "HTTP错误代码：" + statusCode;
                    }
                }
                catch (Exception)
                {
                    return "无法连接到对方服务器，请检查您的网络或者更换一个直播源！";
                }

                try
                {
                    using (StreamReader sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        string r = "";
                        string tAllowCache = "---";
                        string tTargetDuration = "---";
                        string tMediaSequence = "---";
                        List<M3U8_TS_PARM> tMTP = new List<M3U8_TS_PARM>();
                        double tTime = 0;
                        string tsurl = VideoIfm[0].Substring(0, VideoIfm[0].LastIndexOf("/") + 1);
                        int ts_index = 0;

                        while ((r = await sr.ReadLineAsync()) != null)
                        {                       
                            //如果下载M3U8文件后里面还是个M3U8文件，则继续下载并解析
                            if (r.Contains(".m3u8"))
                            {
                                if (!r.Contains("://"))
                                {
                                    r =tsurl + r;
                                }

                                return await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { r });
                            }
                            //开始读取带TS分片信息的M3U8
                            else if (r.StartsWith("#EXT-X-ALLOW-CACHE"))
                            {
                                tAllowCache=r.Replace("#EXT-X-ALLOW-CACHE:","");
                            }                       
                            else if (r.StartsWith("#EXT-X-TARGETDURATION"))
                            {
                                tTargetDuration=r.Replace("#EXT-X-TARGETDURATION:", "");
                            }
                            else if (r.StartsWith("#EXT-X-MEDIA-SEQUENCE"))
                            {
                                tMediaSequence=r.Replace("#EXT-X-MEDIA-SEQUENCE:", "");
                            }
                            else if (r.Contains("#EXTINF:"))
                            {
                                var tvalue = r.Replace("#EXTINF:", "");
                                tvalue = tvalue.Substring(0, tvalue.IndexOf(","));
                                
                                double.TryParse(tvalue, out tTime);
                            }
                            else if (r.Contains(".ts"))
                            {
                                if (!r.Contains("://"))
                                {
                                    r = tsurl+ r;
                                }

                                tMTP.Add(new M3U8_TS_PARM() { ItemId=ts_index, FullURL=r,Time=tTime});
                                ts_index++;
                            }
                            else if (r.Contains("#EXT-X-ENDLIST"))
                            {
                                //添加一个标识，因为直播已结束，服务器不再提供新的数据流，所以程序将不再向服务器发请求
                                //tMTP.Add(new M3U8_TS_PARM() { FullURL = "", Time = 0 });
                                isEndList = true;
                                //return "直播源已播放完毕";
                            }

                        }


                        videoAnalysisList.AllowCache = tAllowCache;
                        videoAnalysisList.MediaSequence = tMediaSequence;
                        videoAnalysisList.TargetDuration = tTargetDuration;
                        videoAnalysisList.FullURL = VideoIfm[0];
                        videoAnalysisList.M3U8URL = tsurl;
                        videoAnalysisList.FileName = VideoIfm[0].Replace(tsurl,"");
                        videoAnalysisList.TS_PARM = tMTP;
                    }

                }
                catch (Exception)
                {
                    return "解析出现问题，可能是M3U8文件数据格式有问题。";
                }


            }


            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="M3U8PlayList">M3U8文件内的所有直播信号</param>
        /// <param name="VideoIfm">1：文件名；2：文件的路径；</param>
        /// <returns></returns>
        public async Task<string> ReadLocalM3U8File(List<string[]> M3U8PlayList, string[] VideoIfm)
        {
            M3U8PlayList.Clear();

            if (!File.Exists(VideoIfm[1]))
            {
                return "程序找不到该直播源本地文件，可能是该文件已被删除或移动。";
            }

            try
            {
                using (StringReader sr = new StringReader(File.ReadAllText(VideoIfm[1])))
                {
                    string r = "";
                    string tProperties = "";
                    while ((r=await sr.ReadLineAsync())!=null)
                    {
                        if (r.StartsWith("#"))
                        {
                            tProperties=r;
                            continue;
                        }
                        else if (String.IsNullOrWhiteSpace(r))
                            continue;
                        else if (r.Contains(".m3u8"))
                        {
                            //string m3u8name = r[new Range(r.LastIndexOf("/")+1, r.LastIndexOf(".m3u8")+5)];
                            //暂时给表达式加?
                            Match m3u8nameResult = Regex.Match(r, @"\/?([^\/]+\.m3u8)");
                            string m3u8name = m3u8nameResult.Groups[1].Value=="" ? "未解析到文件名" : m3u8nameResult.Groups[1].Value;

                            Match tPResult = Regex.Match(tProperties, @"(?:,|:)BANDWIDTH=(.*?)(,|\n|$)");
                            string tBandwidth = tPResult.Groups[1].Value.Replace("\"", "");
                            tBandwidth=tBandwidth=="" ? "---" : tBandwidth;
                            Match tPResult2 = Regex.Match(tProperties, @"RESOLUTION=(.*?)(,|\n|$)");
                            string tResolution = tPResult2.Groups[1].Value.Replace("\"", "");
                            tResolution=tResolution=="" ? "---" : tResolution;
                            Match tPResult3 = Regex.Match(tProperties, @"FRAME-RATE=(.*?)(,|\n|$)");
                            string tFrameRate = tPResult3.Groups[1].Value.Replace("\"", "");
                            tFrameRate=tFrameRate=="" ? "---" : tFrameRate;
                            Match tPResult4 = Regex.Match(tProperties, @"CODECS=""(.*?)""(,|\n|$)");
                            string tCodecs = tPResult4.Groups[1].Value.Replace("\"", "");
                            tCodecs=tCodecs=="" ? "---" : tCodecs;
                            Match tPResult5 = Regex.Match(tProperties, @"NAME=(.*?)(,|\n|$)");
                            string tName = tPResult5.Groups[1].Value.Replace("\"", "");
                            tName=tName=="" ? "---" : tName;

                            if (!r.Contains("://"))
                            {
                                r="";
                            }

                            M3U8PlayList.Add(new string[] { m3u8name, r, tBandwidth, tResolution, tFrameRate, tCodecs, tName });

                        }
                        else if (r.Contains(".ts"))
                        {
                            //备用
                        }
                    }
                }

            }
            catch (Exception)
            {
                return "解析出现问题，可能是M3U8文件数据格式有问题。";
            }

            return "";
        }
        public async Task<string> DownloadM3U8Stream(VideoAnalysisList valist, string savepath)
        {
            TempFileList=new List<DownloadTempFileList>();
            //int FileIndex = 0;
            string filename = valist.FileName.Substring(0, valist.FileName.LastIndexOf("."));
            int FinishCount = 0;
            string dresult = "";

            foreach (var m in valist.TS_PARM)
            {
                new Thread(async () =>
                {
                    string url = m.FullURL;
                    long Filesize = 0;
                    string FileID = Guid.NewGuid().ToString();
                    string TempFilepath = string.Format($"{savepath}{FileID}_{filename}.tmp");


                    using (HttpClient httpClient = new HttpClient())
                    {
                        int statusCode;
                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                        HttpResponseMessage response = null;

                        try
                        {
                            response = await httpClient.GetAsync(url);

                            statusCode = (int)response.StatusCode;
                            if (!response.IsSuccessStatusCode)
                            {
                                dresult= "请求失败！" + "HTTP错误代码：" + statusCode;
                                isContinueDownloadStream = false;
                                FinishCount++;
                                return;
                            }

                            Filesize = response.Content.Headers.ContentLength ?? -1;
                            if (Filesize <= 0)
                            {
                                dresult= "无法从 ContentLength 中获取有效的文件大小！";
                                isContinueDownloadStream = false;
                                FinishCount++;
                                return;
                            }

                        }
                        catch (Exception)
                        {
                            dresult = "请求发生异常！";
                            isContinueDownloadStream = false;
                            FinishCount++;
                            return;
                        }

                        FileStream fs = null;
                        Stream ns = null;
                        try
                        {
                            fs = new FileStream(TempFilepath, FileMode.Create);

                            ns = await response.Content.ReadAsStreamAsync();
                            ns.CopyTo(fs);
                        }
                        catch(Exception)
                        {
                            dresult = "向临时文件写入发生异常！";
                            isContinueDownloadStream = false;
                            FinishCount++;
                            return;
                        }
                        finally
                        {
                            fs?.Close();
                            ns?.Close();
                        }


                    }

                    TempFileList.Add(new DownloadTempFileList() { ItemId = m.ItemId, FileName = TempFilepath.Replace(savepath, ""), FilePath = TempFilepath, Filesize = Filesize, FullLink = url });
                
                    FinishCount++;
                }).Start();


            }

            while(true)
            {
                if(FinishCount == valist.TS_PARM.Count)
                {
                    break;
                }
            }

            if(!isEndList&&isContinueDownloadStream)
            {
                MergeTempFile(savepath + filename + ".mp4",true);

                double TS_AllTime = valist.TS_PARM.Sum(p=>p.Time)*1000;
                await Task.Delay((int)TS_AllTime);

                VideoAnalysisList videoAnalysisList = new VideoAnalysisList();
                dresult = await DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { valist.FullURL });
                if (string.IsNullOrEmpty(dresult))
                {


                    dresult = await DownloadM3U8Stream(videoAnalysisList, savepath);
 
                }


            }
            else
            {
                MergeTempFile(savepath + filename + ".mp4",false);
            }


            return dresult;
        }
        /// <summary>
        /// 将当前播放列表更新到直播源预览页面
        /// </summary>
        /// <param name="playlist">播放列表</param>
        public void UpdatePrevPagePlaylist(List<string[]> playlist)
        {
            VideoPrevPage.videoPrevPage.M3U8PlayList.Clear();
            playlist.ForEach(p =>
            {
                VideoPrevPage.videoPrevPage.M3U8PlayList.Add(p);
            });
        }

        /*
               public string GetReadM3U8FileErrorMsg(int index)
              {
                  switch (index)
                  {
                      case 1:

                          break;
                      case 2:
                          break;
                      case 3:
                          break;
                  }
              }
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m3uStr"></param>
        /// <returns></returns>
        public async Task<List<VideoEditList>> ReadM3UString(string m3uStr)
        {
            List<VideoEditList> videoStrList = new List<VideoEditList>();

            try
            {
                using (StringReader sr = new StringReader(m3uStr))
                {
                    string r = "";
                    while ((r=await sr.ReadLineAsync())!=null)
                    {
                        if (r.StartsWith("#"))
                        {
                            if(r.StartsWith("#EXTM3U"))
                            {
                                videoStrList.Add(new VideoEditListEXT_Readonly() { ItemTypeId=3, EXTTag=r });
                            }
                            else if (r.StartsWith("#EXTINF"))
                            {
                                string groupTitle="";
                                string tvgGroup="";
                                string tvgID="";
                                string tvgLogo = "";
                                string tvgCountry = "";
                                string tvgLanguage = "";
                                string tvgName = "";
                                string tvgURL = "";

                                if(r.Contains("group-title="))
                                {
                                    Match tResult = Regex.Match(r, @"group-title=""(.*?)""");
                                    groupTitle=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-group="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-group=""(.*?)""");
                                    tvgGroup=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-logo="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-logo=""(.*?)""");
                                    tvgLogo=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-id="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-id=""(.*?)""");
                                    tvgID=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-country="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-country=""(.*?)""");
                                    tvgCountry=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-language="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-language=""(.*?)""");
                                    tvgLanguage=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-name="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-name=""(.*?)""");
                                    tvgName=tResult.Groups[1].Value;
                                }
                                if (r.Contains("tvg-url="))
                                {
                                    Match tResult = Regex.Match(r, @"tvg-url=""(.*?)""");
                                    tvgURL=tResult.Groups[1].Value;
                                }

                                videoStrList.Add(new VideoEditListTVG() {ItemTypeId=2, AllStr=r ,GroupTitle=groupTitle, TVGGroup=tvgGroup,TVGID=tvgID,TVGLogo=tvgLogo,TVGCountry=tvgCountry,TVGLanguage=tvgLanguage,TVGName=tvgName,TVGURL=tvgURL});

                            }
                            else
                            {
                                videoStrList.Add(new VideoEditListEXT() {ItemTypeId=1, EXTTag=r });
                            }

                        }
                        //暂时不对空格作出处理
                        else if (String.IsNullOrWhiteSpace(r))
                            continue;
                        else if (r.Contains(".m3u8"))
                        {
                            videoStrList.Add(new VideoEditListSourceLink() {ItemTypeId=4, SourceLink=r });
                        }
                        else
                        {
                            videoStrList.Add(new VideoEditListOtherString() {ItemTypeId=5, AllStr=r });
                        }
                    }
                }

            }
            catch (Exception)
            {

            }

            return videoStrList;
        }

        /*
                 public string CheckAndRandomGUID(List<VideoEditList> videolist)
                {
                    string guid = Guid.NewGuid().ToString();
                    if(videolist.Where(p=>p.ItemName==guid).Count() > 0)
                    {
                       return CheckAndRandomGUID(videolist);
                    }

                    return guid;
                }
         */
        private void MergeTempFile(string finalFilepath,bool isNeedJoinFile)
        {
            int length = 0;

            FileMode fileMode;
            if(isNeedJoinFile)
            {
                fileMode = FileMode.Append;
            }
            else
            {
                fileMode = FileMode.Create;
            }

            try
            {
                long fsLength = 0;
                using (FileStream fs = new FileStream(finalFilepath, fileMode))
                {
                    foreach (var item in TempFileList.OrderBy(p => p.ItemId))
                    {
                        string tempFilePath = item.FilePath;
                        if (!File.Exists(tempFilePath)) continue;

                        try
                        {
                            using (FileStream tempStream = new FileStream(tempFilePath, FileMode.Open))
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

                        File.Delete(tempFilePath);
                    }

                    fsLength=fs.Length;
                }

                if (fsLength<=0)
                {
                    File.Delete(finalFilepath);
                }
            }
            catch (Exception)
            {

            }

        }
    }
    public class APPFileManager
    {
        public string GetOrCreateAPPDirectory(string foldername)
        {
            //根据不同平台选择不同的缓存方式
            string dataPath;
#if WINDOWS
            dataPath = Path.Combine(FileSystem.AppDataDirectory+"/"+foldername);
#elif ANDROID
            //var test= Directory.CreateDirectory(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.AbsolutePath)+"/LiveStreamCache");
            dataPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android", "data", Android.App.Application.Context.PackageName+"/"+foldername);
#else
            //暂时不对苹果设备以及其他平台进行直播源缓存

            //await DisplayAlert("提示信息", "当前平台暂不支持保存操作！", "确定");
            return null;
#endif

            try
            {
                Directory.CreateDirectory(dataPath);
            }
            catch (Exception)
            {
                //await DisplayAlert("提示信息", "保存文件失败！可能是没有权限。", "确定");
                return null;
            }

            return dataPath;
        }
    }
}
