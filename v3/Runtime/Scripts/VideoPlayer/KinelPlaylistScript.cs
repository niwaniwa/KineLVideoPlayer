
using System.Collections.Generic;
using UnityEngine;
using Kinel.VideoPlayer.V3.Scripts.Parameter;
using Kinel.VideoPlayer.V3.Scripts.VideoPlayer;
namespace Kinel.VideoPlayer.V3.Scripts.VideoPlayer
{
    public class KinelPlaylistScript : KinelScriptsModule
    {

        public List<KinelPlaylistItem> playlists;

        public AutoPlay autoPlay;
        public Loop loop;
        public FillResult isAutoFill;
        public string playlistUrl, urlPrefix;
        public bool showPlaylist, storePlaylist;

    }
}