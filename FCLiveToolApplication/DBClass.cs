using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

}
