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

        [SerializeField] public KinelVideoPlayer videoPlayer;

        [SerializeField] public RectTransform content;
        // [SerializeField] public GameObject listPrefab;
        [SerializeField] public GameObject loadingCover;
        // [SerializeField] public GameObject disableCover;
        
        [SerializeField] public string[] titles;
        [SerializeField] public VRCUrl[] urls;
        [SerializeField] public int[] playMode;
        
        // Loopとauto playについて同期するか迷い
        [SerializeField] public int autoPlayMode = 0; // 
        [SerializeField] public int loopMode = 0; // 

        [UdonSynced, FieldChangeCallback(nameof(NowPlayingFlag))]
        private bool nowPlayingFlag = false;

        private bool _loading = false;

        [UdonSynced, FieldChangeCallback(nameof(NowPlaylingItemIndex))]
        private int nowPlayingItemIndex = 0;

        public bool NowPlayingFlag
        {
            get => nowPlayingFlag;
            private set => nowPlayingFlag = value;
        }

        public int NowPlaylingItemIndex
        {
            get => nowPlayingItemIndex;
            set
            {
                nowPlayingItemIndex = value;
            }
        }

        public void Start()
        {
            Debug.Log($"");
            videoPlayer.RegisterListener(this);
            if(autoPlayMode == 1 || autoPlayMode == 2 || autoPlayMode == 3)
                SetLoop(false);
        }

        public void PlayVideo(int index)
        {
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
                var button = content.GetChild(i).GetChild(4).GetComponent<Button>();
                if (!button.interactable){
                    select = i;
                    button.interactable = true;
                    break;
                }
            }
            return select;
        }


        public void SetLoop(bool loop)
        {
            videoPlayer.SetLoop(loop);
            // call event
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (autoPlayMode == 1 || autoPlayMode == 2)
            {
                SendCustomEventDelayedSeconds(nameof(OnPlaylistAutoPlay), 1f);
            }
        }

        public void OnPlaylistAutoPlay()
        {
            if (videoPlayer.IsPlaying)
                return;
            TakeOwnership();
            nowPlayingFlag = true;
            _loading = true;
            RequestSerialization();
            PlayVideo(0);
        }

        public void OnPlaylistSelected()
        {
            int select = GetSelectedItem();
            TakeOwnership();
            nowPlayingFlag = true;
            _loading = true;
            RequestSerialization();
            PlayVideo(select);
        }

        public void OnUrlUpdate()
        {
            loadingCover.SetActive(true);
            if (!_loading)
            {
                ResetState();
            }
        }

        public void OnKinelVideoStart()
        {
            loadingCover.SetActive(false);
            _loading = false;
        }

        public void OnKinelVideoEnd()
        {
            if (!nowPlayingFlag || !Networking.IsOwner(gameObject))
                return;

            if (autoPlayMode == 0 || autoPlayMode == 1)
                return;
            
            // 3 = Playlistを選択後、選択曲以下を自動再生, 2 = Join後自動再生、その後以下自動再生
            if (autoPlayMode == 2 || autoPlayMode == 3)
            {
                nowPlayingItemIndex++;
                if (nowPlayingItemIndex >= urls.Length)
                {
                    nowPlayingItemIndex = 0;
                    if (loopMode != 2)
                        return;
                }
                PlayVideo(nowPlayingItemIndex);
            }
        }

        public void OnKinelVideoLoop()
        {
            
        }

        public void ResetState()
        {
            nowPlayingItemIndex = 0;
            nowPlayingFlag = false;
            _loading = false;
            RequestSerialization();
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
