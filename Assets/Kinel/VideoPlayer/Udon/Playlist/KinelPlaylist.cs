using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Playlist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelPlaylist : UdonSharpBehaviour
    {
        
        private const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        private const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        [SerializeField] private KinelVideoPlayer videoPlayer;

        [SerializeField] private RectTransform content;
        // [SerializeField] public GameObject listPrefab;
        [SerializeField] private GameObject loadingCover;
        [SerializeField] private GameObject disableCover;
        // [SerializeField] public GameObject disableCover;

        [SerializeField] private string playlistInternalName;
        
        [SerializeField] private string[] titles;
        [SerializeField] private VRCUrl[] urls;
        [SerializeField] private int[] playMode;
        
        // Loopとauto playについて同期するか迷い
        // [SerializeField] private int autoPlayMode = 0; // 
        [SerializeField] private bool autoPlayWhenJoin; // Joinしたときに自動的に再生するか
        [SerializeField] private bool nextPlayVideoWhenPlaySelected; // プレイリストで選択した動画を再生後、次の動画を再生するか
        [SerializeField] private bool shuffle; // playlistをshuffle再生するか
        [SerializeField] private int loopMode = 0; // 
        

        [UdonSynced, FieldChangeCallback(nameof(NowPlayingFlag))]
        private bool nowPlayingFlag = false;

        private bool _loading = false;
        private Slider progress;

        [UdonSynced, FieldChangeCallback(nameof(NowPlaylingItemIndex))]
        private int nowPlayingItemIndex = 0;

        [UdonSynced]
        private bool[] shuffleList;

        public bool NowPlayingFlag
        {
            get => nowPlayingFlag;
            private set
            {
                nowPlayingFlag = value;
                if(!value)
                    ResetProgressBars();
            }
        }

        public int NowPlaylingItemIndex
        {
            get => nowPlayingItemIndex;
            set
            {
                nowPlayingItemIndex = value;
                }
        }

        public bool Shuffle
        {
            get => shuffle;
            set => shuffle = value;
        }

        public void Start()
        {

            if (urls.Length == 0)
            {
                Debug.Log($"{DEBUG_LOG_PREFIX} Playlist disabled... {(String.IsNullOrEmpty(playlistInternalName) ? "" : $"#{playlistInternalName}")}");
                disableCover.SetActive(true);
                return;
            }

            shuffleList = new bool[urls.Length];
            videoPlayer.RegisterListener(this);
            SetLoop(loopMode);
        }
        
        public void FixedUpdate()
        {
            UpdateProgressBar();
        }

        public void PlayVideo(int index)
        {
            if (urls.Length == 0)
            {
                Debug.LogWarning($"{DEBUG_ERROR_PREFIX} Playlist disabled...");
                return;
            }
            TakeOwnership();
            _loading = true;
            videoPlayer.ChangeMode(playMode[index]);
            videoPlayer.PlayByURL(urls[index]);
            nowPlayingItemIndex = index;
            RequestSerialization();
        }

        public int GetSelectedItem()
        {
            var select = 0;
            for (int i = 0; i < content.transform.childCount; i++)
            {
                var button = content.GetChild(i).GetChild(5).GetComponent<UnityEngine.UI.Button>();
                if (!button.interactable){
                    select = i;
                    button.interactable = true;
                    break;
                }
            }
            return select;
        }


        public void SetLoop(int loop)
        {
            loopMode = loop;
            if (loop == 1)
                videoPlayer.SetLoop(true);
            if(loop == 2 || loop == 3)
                videoPlayer.SetLoop(false);
        }

        public int GetRandomVideoIndex()
        {
            var check = false;
            var count = 0;
            var index = 0;

            for (int i = 0; i < shuffleList.Length; i++)
            {
                if (!shuffleList[i])
                {
                    check = true;
                    count++;
                    index = i;
                }
            }

            if (!check)
                return -1;

            if (count == 1)
                return index;
            
            var val = UnityEngine.Random.Range(0, urls.Length - 1);
            if (shuffleList[val])
                return GetRandomVideoIndex();
            return val;
        }
        
        public void UpdateProgressBar()
        {
            if (!NowPlayingFlag)
                return;

            if (!videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().IsPlaying)
                return;

            if (progress == null)
                return;
                
            
            /*if(!progress.gameObject.activeSelf)
                progress.gameObject.SetActive(true);*/
           
            progress.value = videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().GetTime();
        }

        public void ProgressBarSetting()
        {
            var trans = content.GetChild(nowPlayingItemIndex).Find("Progress");
            Debug.Log($"{nowPlayingItemIndex}, {trans == null}");
            if (trans == null)
            {
                Debug.Log($"null? true {nowPlayingItemIndex}");
                SendCustomEventDelayedSeconds(nameof(ProgressBarSetting), 2);
                return;
            }
            trans.gameObject.SetActive(true);
            progress = trans.GetComponent<Slider>();
            progress.maxValue = videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().GetDuration();
        }

        public void ResetProgressBars()
        {
            for (int i = 0; i < content.transform.childCount; i++)
            {
                var trans = content.GetChild(i).GetChild(1);
                if (trans == null)
                {
                    Debug.Log($"{DEBUG_ERROR_PREFIX} KineL Playlist Error. Please report (@ni_rilana) #ProgressNULL");
                    return;
                }
                progress = trans.GetComponent<Slider>();
                progress.value = 0;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player)
                return;

            if (!Networking.LocalPlayer.IsOwner(videoPlayer.gameObject))
                return;
            
            if (autoPlayWhenJoin)
            {
                SendCustomEventDelayedSeconds(nameof(OnPlaylistAutoPlay), 1f);
            }
        }

        public void OnPlaylistAutoPlay()
        {
            if (videoPlayer.IsPlaying)
                return;
            
            TakeOwnership();
            NowPlayingFlag = true;
            _loading = true;
            RequestSerialization();
            if (shuffle)
            {
                var index = GetRandomVideoIndex();
                shuffleList[index] = true;
                PlayVideo(index);
            }
            else
            {
                PlayVideo(0);
            }
            
        }

        public void OnPlaylistSelected()
        {
            int select = GetSelectedItem();
            TakeOwnership();
            NowPlayingFlag = true;
            _loading = true;
            RequestSerialization();
            PlayVideo(select);
        }

        public void OnKinelUrlUpdate()
        {
            if(!videoPlayer.IsLock)
                loadingCover.SetActive(true);
            
            if (!_loading)
            {
                ResetState();
            }
        }

        public void OnKinelVideoStart()
        {
            if(!videoPlayer.IsLock)
                loadingCover.SetActive(false);
            _loading = false;
            
            if(NowPlayingFlag)
                ProgressBarSetting();
        }

        public void OnKinelVideoEnd()
        {
            Debug.Log($"video end");
            progress = null;
            if (!NowPlayingFlag)
            {
                Debug.Log($"sisable");
                ResetProgressBars();
                return;
            }
            Debug.Log($"playlist videoend");

            if (!Networking.IsOwner(videoPlayer.gameObject))
                return;
            Debug.Log($"playlist owner");
            TakeOwnership();
          
            if (loopMode == 0 || loopMode == 2 )
            {
                if (!nextPlayVideoWhenPlaySelected)
                    return;
                
                if (shuffle)
                {
                    var rawIndex = GetRandomVideoIndex();
                    if (rawIndex == -1)
                    {
                        for (int i = 0; i < shuffleList.Length; i++)
                            shuffleList[i] = false;
                        
                        if (loopMode == 2)
                        {
                            rawIndex = GetRandomVideoIndex();
                            shuffleList[rawIndex] = true;
                            PlayVideo(rawIndex);
                            return;
                        }

                        nowPlayingItemIndex = 0;
                        NowPlayingFlag = false;
                        return;
                            
                    }
                    shuffleList[rawIndex] = true;
                    PlayVideo(rawIndex);
                    return;
                }
                
                nowPlayingItemIndex++;
                
                if (nowPlayingItemIndex >= urls.Length)
                {
                    nowPlayingItemIndex = 0;
                    if (loopMode != 2)
                    {
                        NowPlayingFlag = false;
                        return;
                    }
                }
                PlayVideo(nowPlayingItemIndex);
            }
            
        }

        public void OnKinelVideoReset()
        {
            loadingCover.SetActive(false);
            ResetProgressBars();
            ResetState();
        }

        public void OnKinelVideoLoop()
        {
            
        }

        private void ResetState()
        {
            nowPlayingItemIndex = 0;
            NowPlayingFlag = false;
            _loading = false;
            RequestSerialization();
        }

        // 名称check
        public void ResetLocal()
        {
            TakeOwnership();
            ResetState();
            videoPlayer.ResetGlobal();
        }
        

        public void TakeOwnership()
        {
            if (Networking.IsOwner(gameObject))
                return;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            videoPlayer.TakeOwnership();
        }

    }
    
}
