using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Playlist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelQueuePlaylist : UdonSharpBehaviour
    {
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        
        [SerializeField] private KinelVideoPlayer videoPlayer;

        [SerializeField] private VRCUrlInputField inputField;
        [SerializeField] private GameObject modeSelectUI;
        [SerializeField] private GameObject urlInputUI;
        [SerializeField] private GameObject playlistPrefab;
        [SerializeField] private GameObject messageUI;
        [SerializeField] private RectTransform content;
        
        [SerializeField] private bool autoPlay;
        // 0 = 入力後自動再生, 1 = Playlistを選択後、選択曲以下を自動再生, 11 = 入力後自動再生、その後プレイリストを自動再生
        [SerializeField] private int autoPlayMode = 0;
        [SerializeField] private bool shouldDelete;

        [UdonSynced, FieldChangeCallback(nameof(IsEditing))]
        private bool isEditing;
        
        [UdonSynced, FieldChangeCallback(nameof(Urls))]
        private VRCUrl[] _urls;
        
        [UdonSynced, FieldChangeCallback(nameof(ModeQueue))]
        private int[] _modeQueue;

        [UdonSynced, FieldChangeCallback(nameof(NowPlayingFlag))]
        private bool nowPlayingFlag = false;

        [UdonSynced] private int index; // このindexはcontent内のchilsの順番ではなく、urlsの順番とする
        
        private int _selectedMode = 0;
        private Text _messageUIPlayerText;
        private GameObject[] _gameObjects;
        private Slider progress;

        public bool IsEditing
        {
            set
            {
                isEditing = value;
                if (value && !Networking.IsOwner(Networking.LocalPlayer, gameObject))
                {
                    messageUI.SetActive(true);
                    _messageUIPlayerText.text = "Queue editing now... " + Networking.GetOwner(this.gameObject).displayName;
                } 
                else if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
                {
                    if (value)
                        return;
                    messageUI.SetActive(false);
                }
                else 
                    messageUI.SetActive(value);
            }
            get => isEditing;
        }

        public VRCUrl[] Urls
        {
            get => _urls;
            set
            {
                _urls = value;
                UpdateQueue();
            }
        }

        public int[] ModeQueue
        {
            get => _modeQueue;
            set
            {
                _modeQueue = value;
            }
        }
        
        public bool NowPlayingFlag
        {
            get => nowPlayingFlag;
            private set
            {
                nowPlayingFlag = value;
                // OnKinelPlaylistStart();
            }
        }
        
        public void Start()
        {
            Debug.Log($"{DEBUG_PREFIX} QueuePlaylist initializing...");
            videoPlayer.RegisterListener(this);
            _urls = new VRCUrl[0];
            _messageUIPlayerText = messageUI.transform.Find("Text").GetComponent<Text>();
        }

        public void FixedUpdate()
        {
            UpdateProgressBar();
        }

        public void AddQueue(VRCUrl url, int mode)
        {
            TakeOwnership();
            QueueIndexTextUpdate();
            var tempUrls = new VRCUrl[_urls.Length + 1];
            var tempModeQueue = new int[_urls.Length + 1];
            //var tempGameObjects = new GameObject[_urls.Length + 1];
            
            for (int i = 0; i < _urls.Length; i++)
            {
                tempUrls[i] = _urls[i];
                tempModeQueue[i] = _modeQueue[i];
                //tempGameObjects[i] = _gameObjects[i];
            }
            
            tempUrls[_urls.Length] = url;
            tempModeQueue[_urls.Length] = mode;
            _urls = tempUrls;
            _modeQueue = tempModeQueue;
            //_gameObjects = tempGameObjects;
            
            
        }

        public void RemoveQueue(int index)
        {
            TakeOwnership();
            var tempVrcUrls = new VRCUrl[_urls.Length - 1];
            var tempModeQueue = new int[_urls.Length  - 1];
            //var tempGameObjects = new GameObject[_gameObjects.Length  - 1];
            int i = 0;
            for (; i < _urls.Length; i++)
            {
                if (i.Equals(index))
                    break;
                
                tempVrcUrls[i] = _urls[i];
                tempModeQueue[i] = _modeQueue[i];
                //tempGameObjects[i] = _gameObjects[i];
            }
            
            for (; i < _urls.Length - 1; i++)
            {
                tempVrcUrls[i] = _urls[i + 1];
                tempModeQueue[i] = _modeQueue[i + 1];
                //tempGameObjects[i] = _gameObjects[i + 1];
            }
            
            _urls = tempVrcUrls;
            _modeQueue = tempModeQueue;
            //_gameObjects = tempGameObjects;
            SendCustomEventDelayedFrames(nameof(QueueIndexTextUpdate), 5);
            Debug.Log($"{DEBUG_PREFIX} Remove queue. now urls length : {_urls.Length}");
        }

        /**
         * index - urlsの0から指定する事
         */
        public void PlayQueue(int index)
        {
            if (_urls.Length <= index)
                return;

            if (videoPlayer.IsLock && !Networking.LocalPlayer.isMaster)
                return;
            
            TakeOwnership();
            PlayerSettings();
            videoPlayer.ChangeMode(_modeQueue[index]);
            videoPlayer.PlayByURL(_urls[index]);
            NowPlayingFlag = true;
            this.index = index;
            RequestSerialization();
        }

        private void PlayerSettings()
        {
            videoPlayer.TakeOwnership();
            videoPlayer.GetVideoPlayerController().Loop(false);
        }

        private void RevertPlayerSettings()
        {
            videoPlayer.GetVideoPlayerController().Loop(videoPlayer.Loop);
        }

        public GameObject CreatePlaylistItem(string description)
        {
            // create playlist item
            GameObject item = VRCInstantiate(playlistPrefab);
            item.transform.SetParent(content);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale    = Vector3.one;
            // set text (url)
            Text text = item.transform.Find("Description").GetComponent<Text>();
            text.text = $"{description}";

            return item;
        }

        public void UpdateQueue()
        {
            for (int i = 1; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
            for (int i = 0; i < _urls.Length; i++)
            {
                GameObject item = CreatePlaylistItem(_urls[i].ToString());
                item.SetActive(true);
            }
        }

        private float lastSliderUpdateTime = -1;

        public void UpdateProgressBar()
        {
            if (!NowPlayingFlag)
                return;

            if (!videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().IsPlaying)
                return;

            if (progress == null)
                return;
            
            progress.value = videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().GetTime();
        }

        public void ProgressBarSetting()
        {
            var trans = content.GetChild(index + 1).Find("Progress/Slider");
            if (trans == null)
            {
                Debug.Log($"null? true {index}");
                SendCustomEventDelayedSeconds(nameof(ProgressBarSetting), 2);
                return;
            }
            trans.gameObject.SetActive(true);
            progress = trans.GetComponent<Slider>();
            progress.maxValue = videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer().GetDuration();
        }

        public int GetNextVideoIndex()
        {
            int nextIndex = 0;
            nextIndex = (shouldDelete ? (index == 0 ? 0 : index - 1) : index++);
            return nextIndex;
        }

        public void OnVideoModeSelect()
        {
            TakeOwnership();
            IsEditing = true;
            _selectedMode = 0;
            modeSelectUI.SetActive(false);
            urlInputUI.SetActive(true);
            RequestSerialization();
        }
        
        public void OnStreamModeSelect()
        {
            TakeOwnership();
            IsEditing = true;
            _selectedMode = 1;
            modeSelectUI.SetActive(false);
            urlInputUI.SetActive(true);
            RequestSerialization();
        }

        public void OnQueueUpdate()
        {
            TakeOwnership();
            
            VRCUrl url = inputField.GetUrl();
            if (!UrlValidCheck(url))
            {
                return;
            }

            IsEditing = false;
            
            // create playlist item
            GameObject item = CreatePlaylistItem($"{_urls.Length + 1}.{url.ToString()}");
            inputField.SetUrl(VRCUrl.Empty);
            
            // ui set active
            item.SetActive(true);
            urlInputUI.SetActive(false);
            modeSelectUI.SetActive(true);
            
            // register queue
            AddQueue(url, _selectedMode);
            
            // sync
            RequestSerialization();
            
            Debug.Log(_urls.Length);
            
            // options
            if (!NowPlayingFlag && !videoPlayer.IsPlaying && autoPlay && _urls.Length == 1)
            {
                if (autoPlayMode == 0)
                {
                    PlayQueue(0);
                    NowPlayingFlag = true;
                    index = 0;
                    RequestSerialization();
                }
            }
        }

        public void OnQueueClick()
        {
            int select = GetSelectedItem(4);
            if (NowPlayingFlag && select == index)
                return;
            TakeOwnership();
            Debug.Log($"{select},");
            Debug.Log($"{select},{_urls[select]}");
            PlayQueue(select);
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            NowPlayingFlag = true;
            index = select;
            RequestSerialization();
        }

        public void OnQueueDelete()
        {
            TakeOwnership();
            int select = GetSelectedItem(2);

            if (NowPlayingFlag && select == index)
            {
                videoPlayer.ResetGlobal();
                if (autoPlay && autoPlayMode == 1)
                {
                    if (index == _urls.Length - 1)
                        return;
                    PlayQueue(GetNextVideoIndex());
                }
            }
            
            RemoveQueue(select);
            Destroy(content.GetChild(select + 1).gameObject);
            RequestSerialization();
            Debug.Log($"{DEBUG_PREFIX} Queue deleted");
        }

        public void QueueIndexTextUpdate()
        {
            for (int i = 1; i < content.childCount; i++)
            {
                Text text = content.GetChild(i).Find("Description").GetComponent<Text>();
                var oldText = text.text;
                text.text = text.text.Remove(0, 1).Insert(0, i.ToString());
                Debug.Log($"index {i}, old {oldText}, new {text.text}");
            }
        }

        public void ResetInputArea()
        {
            inputField.SetUrl(VRCUrl.Empty);
            modeSelectUI.SetActive(true);
            urlInputUI.SetActive(false);
        }
        
        public bool UrlValidCheck(VRCUrl url)
        {
            if (url.Equals(VRCUrl.Empty))
                return false;

            if (!url.Get().StartsWith("https://"))
                return false;

            return true;
        }
        
        /// <summary>
        /// 選択されたplaylistItemのindexを取得
        /// </summary>
        /// <param name="index">判定に使用するGameObjectのindex</param>
        /// <returns>選択されたitemのindexを返却。なお、先頭に存在するURLInputFieldは除いたindexを返す。</returns>
        public int GetSelectedItem(int index)
        {
            var select = 0;
            for (int i = 1; i < content.transform.childCount; i++)
            {
                var button = content.GetChild(i).GetChild(index).GetComponent<UnityEngine.UI.Button>();
                if (!button.interactable){
                    select = i　- 1;
                    button.interactable = true;
                    break;
                }
            }
            return select;
        }

        public void OnKinelUrlUpdate()
        {
            if (!videoPlayer.IsLock)
            {
                _messageUIPlayerText.text = "Loading video...";
                messageUI.SetActive(true);
            }

            if (NowPlayingFlag)
            {
                if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
                    return;
                TakeOwnership();
                RevertPlayerSettings();
                NowPlayingFlag = false;
                RequestSerialization();
            }
        }

        public void OnKinelPlaylistStart()
        {
            
        }

        public void OnKinelVideoReady()
        {
            messageUI.SetActive(false);
        }

        public void OnKinelVideoStart()
        {
            if (!NowPlayingFlag)
                return;

            ProgressBarSetting();
            


        }

        public void OnKinelVideoEnd()
        {

            if (!NowPlayingFlag)
                return;
            
            Debug.Log($"{DEBUG_PREFIX} Video stop.");
            progress = null;
            if (shouldDelete)
            {
                Destroy(content.GetChild(index + 1).gameObject);
            }
            
            if (Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
            {
                TakeOwnership();
                if (shouldDelete)
                {
                    RemoveQueue(index /*== 0 ? 0 : index - 1*/);
                }
                if (autoPlay)
                {
                    if (autoPlayMode == 1)
                    {
                        if (_urls.Length == 0)
                        {
                            nowPlayingFlag = false;
                            RequestSerialization();
                            return;
                        }
                        PlayQueue(GetNextVideoIndex());
                    }
                }

            }
        }

        public void OnKinelVideoError()
        {
            messageUI.SetActive(false);
            
        }
        
        public void OnKinelVideoReset()
        {
            messageUI.SetActive(false);
            ResetState();
        }

        public void OnKinelVideoLoop()
        {
            if (!NowPlayingFlag)
                return;

            if (videoPlayer.IsErrorRetry())
            {
                
            }
        }

        private void TakeOwnership()
        {

            if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                if (!Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                {
                    videoPlayer.TakeOwnership();
                    return;
                }

                return;
            }
            
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            videoPlayer.TakeOwnership();
            
            
        }
        
        public void ResetPlayState()
        {
            TakeOwnership();
            ResetState();
            videoPlayer.ResetGlobal();
        }
        
        private void ResetState()
        {
            index = 0;
            NowPlayingFlag = false;
            IsEditing = false;
            RequestSerialization();
        }
        

    }
}
