
using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelPlaylistScript : KinelScriptBase
    {
        public KinelVideoPlayer videoPlayer;
        
        public VideoData[] videoDatas;

        public AutoPlay autoPlay;
        public Loop loop;
        public FillResult isAutoFill;
        public bool autoPlayWhenJoin, nextPlayVideoWhenPlaySelected, shuffle; // autoplay etc..
        public string playlistUrl;
        public bool generateInUnity = false, showPlaylist = true, managePlaylistTabHost, storePlaylist;
    }
}