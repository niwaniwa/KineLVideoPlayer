using System;
using System.Collections.Generic;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [KinelModuleAttribute(KinelModuleCategory.Feature, "Playlist", 40)]
    public class KinelPlaylist : KinelVideoListener
    {
        public const string ModuleName = "KinelPlaylist";

        [SerializeField] private KinelPlayerController controller;

        [SerializeField] private string playlistIdentifyName = "Playlist";
        [SerializeField] private string[] playlistNames;
        [SerializeField] private VRCUrl[] urls;
        [SerializeField] private string[] titles;
        [SerializeField] private KinelMediaType[] types;
        [SerializeField] private int[] playlistIndex; // 各配列の開始位置を示す。

        private int _currentIndex = -1;

        public String[] PlaylistNames => playlistNames;

        public KinelMediaTrack[] GetPlaylist(int index)
        {
            Log($"GetPlaylist index {index}, playlistIndex length {playlistIndex.Length}");
            if (playlistIndex.Length <= index) return null;

            Log($"playlistIndex {playlistIndex[index]}");

            var startPos = playlistIndex[index];
            var endPos = (index + 1 < playlistIndex.Length) ? playlistIndex[index + 1] : urls.Length;
            var length = endPos - startPos;

            var result = new KinelMediaTrack[length];
            for (int i = 0; i < length; i++)
            {
                var arrayIndex = startPos + i;
#if !COMPILER_UDONSHARP
                result[i] =　new KinelMediaTrack(urls[arrayIndex], titles[arrayIndex], types[arrayIndex]);
#else
                result[i] =　KinelMediaTrack.New(urls[arrayIndex], titles[arrayIndex], types[arrayIndex]);
#endif
            }

            return result;
        }

        public int GetPlaylistCount()
        {
            return playlistIndex.Length;
        }

        public int Count => urls.Length;

        public KinelMediaTrack GetTrack(int index)
        {
            if (index < 0 || index >= urls.Length) return null;
#if !COMPILER_UDONSHARP
            return new KinelMediaTrack(urls[index], titles[index], types[index]);
#else
            return KinelMediaTrack.New(urls[index], titles[index], types[index]);
#endif
        }

        public DataList GetPlaylists()
        {
            return null;
        }

        public void PlayFromIndex(int index)
        {
            var track = GetTrack(index);
            if (track == null) return;
            _currentIndex = index;
            controller.NowSelectedType = track.Type();
            controller.LoadUrl(track.Url());
        }

        public override void OnKinelVideoEnd()
        {
            if (_currentIndex < 0) return;

            var nextIndex = _currentIndex + 1;
            if (nextIndex >= Count)
            {
                if (controller.LoopMode == LoopMode.Playlist)
                {
                    PlayFromIndex(0);
                }
                else
                {
                    _currentIndex = -1;
                }
                return;
            }

            PlayFromIndex(nextIndex);
        }

        public override void OnKinelQueueStart()
        {
            _currentIndex = -1;
        }

        public void Start()
        {
            if (controller != null)
                controller.AddListener(this);
        }
    }
}
