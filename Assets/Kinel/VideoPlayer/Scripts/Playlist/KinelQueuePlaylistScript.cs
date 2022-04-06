using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using UnityEngine;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelQueuePlaylistScript : MonoBehaviour
    {

        public KinelVideoPlayer videoPlayer;
        public AutoPlay autoPlay;
        public Loop loop;
        public FillResult isAutoFill;
        public bool showPlaylist = true, managePlaylistHost, storePlaylist;
        

    }
}