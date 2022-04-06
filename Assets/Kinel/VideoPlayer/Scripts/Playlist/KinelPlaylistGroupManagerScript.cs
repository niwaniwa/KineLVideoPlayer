using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Controller;
using Kinel.VideoPlayer.Udon.Playlist;
using UnityEngine;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelPlaylistGroupManagerScript : MonoBehaviour
    {

        public KinelVideoPlayer kinelVideoPlayer;
        public KinelPlaylistTabController controller;

        public Canvas[] playlistCanvas;
        public string[] playlists;
        public int enableIndex;
        public bool storePlaylist;

    }
}