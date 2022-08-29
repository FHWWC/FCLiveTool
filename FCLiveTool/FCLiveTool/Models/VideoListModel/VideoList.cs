using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FCLiveTool.Models.VideoListModel
{
    public class AllModel
    {
        public List<VideoList> TVideos { get; set; }
        public List<RecentVList> RLists { get; set; }

        public class VideoList
        {
            public int Id { get; set; }
            public string SourceName { get; set; }
            public string SourceLink { get; set; }
            public string Description { get; set; }
        }
        public class RecentVList
        {
            public int Id { get; set; }

            public string SourceName { get; set; }
            public string LogoLink { get; set; }
            public string SourceLink { get; set; }
            public string Description { get; set; }
            public DateTime AddDT { get; set; }
        }
    }
}
