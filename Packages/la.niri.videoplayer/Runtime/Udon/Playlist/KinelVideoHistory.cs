using System;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Playlist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoHistory : KinelModule
    {
        [SerializeField] private KinelVideoPlayer videoPlayer;
        [SerializeField] private bool saveErrorUrl;
        [SerializeField] private GameObject playlistPrefab;
        [SerializeField] private GameObject messageUI;
        [SerializeField] private RectTransform content;
        
        private VRCUrl[] _urls;
        
        private int[] _modeQueue;

        [UdonSynced, FieldChangeCallback(nameof(NowPlayingFlag))]
        private bool nowPlayingFlag = false;
        
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
            Debug.Log($"{DEBUG_PREFIX} Video History system initializing...");
            Debug.Log($"{DEBUG_PREFIX} VideoPlayerCheck.... {videoPlayer == null}");
            videoPlayer.RegisterListener(this);
            _urls = new VRCUrl[0];
        }

        public override void OnKinelVideoStart()
        {
            
        }

        public override void OnKinelVideoEnd()
        {
            
        }
        
        public void AddQueue(VRCUrl url, int mode)
        {
            // TakeOwnership();
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
        
        public override void OnKinelUrlUpdate()
        {
            // TakeOwnership();

            VRCUrl url = videoPlayer.SyncedUrl;

            // IsEditing = false;
            
            // create playlist item
            GameObject item = CreatePlaylistItem($"{_urls.Length + 1}.{url.ToString()}");
            
            // ui set active
            item.SetActive(true);
            
            // register queue
            AddQueue(url, videoPlayer.GetCurrentVideoMode());
            
            // sync
            // RequestSerialization();
            
            Debug.Log(_urls.Length);
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
        
    }
}