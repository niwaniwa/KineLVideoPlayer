using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class URLInputModule : KinelModule
    {
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private VRCUrlInputField inputField;
        [SerializeField] private GameObject loadingCover;

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
        }

        public void OnURLChanged()
        {
            var url = inputField.GetUrl();
            var nowUrl = videoPlayerUI.GetVideoPlayer().GetUrl();
            

            if (nowUrl != null)
            {
                if (url.ToString().Equals(nowUrl.ToString()))
                {
                    // reloadを別でするべき?
                    // inputField.SetUrl(VRCUrl.Empty);
                    // videoPlayerUI.GetVideoPlayer().TakeOwnership();
                    // videoPlayerUI.GetVideoPlayer().ReloadGlobal();
                    return;
                }
            }
            
            if (!UrlValidCheck(url))
            {
                return;
            }
            
            inputField.SetUrl(VRCUrl.Empty);
            videoPlayerUI.GetVideoPlayer().TakeOwnership();
            videoPlayerUI.GetVideoPlayer().PlayByURL(url);
        }

        public bool UrlValidCheck(VRCUrl url)
        {
            if (url.Equals(VRCUrl.Empty))
                return false;

            if (url.Get().StartsWith("https://")
                || url.Get().StartsWith("http://")
                || url.Get().StartsWith("rtmp://")
                || url.Get().StartsWith("rtsp://")
                || url.Get().StartsWith("rtspt://")
                || url.Get().StartsWith("rtspu://"))
                return true;
            
            
            return false;
        }

        public override void OnKinelUrlUpdate()
        {
            loadingCover.SetActive(true);
        }

        public override void OnKinelVideoReady()
        {
            loadingCover.SetActive(false);
        }

        public override void OnKinelVideoError()
        {
            loadingCover.SetActive(false);
        }

        public override void OnKinelVideoReset()
        {
            inputField.SetUrl(VRCUrl.Empty);
            loadingCover.SetActive(false);
        }
        
        

    }
}