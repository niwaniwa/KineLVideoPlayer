using System;
using System.Collections.Generic;
using Kinel.VideoPlayer.V3.Udon.System;

namespace Kinel.VideoPlayer.V3.Scripts.VideoPlayer
{
    [Serializable]
    public class KinelPlaylistItem : KinelScriptsModule
    {
        public string playlistName = "Playlist";
        public List<KinelMediaTrackImpl> Tracks = new List<KinelMediaTrackImpl>();
    }
}