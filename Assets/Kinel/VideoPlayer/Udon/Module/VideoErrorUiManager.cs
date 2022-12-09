using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class VideoErrorUiManager : KinelModule
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private GameObject errorMessageObject;
        
        private Text errorText;

        public void Start()
        {
            errorText = errorMessageObject.transform.Find("Text").GetComponent<Text>();
            videoPlayerUI.RegisterListener(this);
        }

        public override void OnKinelVideoReady()
        {
            InactiveUiCover();
        }

        public override void OnKinelVideoError()
        {
            if(!videoPlayerUI.GetVideoPlayer().IsErrorRetry())
            {
                errorMessageObject.SetActive(true);
                errorText.text = $"Video loading error. Please try it. : ErrorType {videoPlayerUI.GetVideoPlayer().LastVideoError}";
                return;
            }
            errorMessageObject.SetActive(true);
            errorText.text = $"Retrying... please wait. (click to cancel) : ErrorType {videoPlayerUI.GetVideoPlayer().LastVideoError}";
        }

        public override void OnKinelVideoRetryError()
        {
            errorMessageObject.SetActive(true);
            errorText.text = $"Retry error : ErrorType {videoPlayerUI.GetVideoPlayer().LastVideoError}";
        }

        public void InactiveUiCover()
        {
            errorMessageObject.SetActive(false);
        }
        
        public void CancelRetrying()
        {
            errorMessageObject.SetActive(false);
            // videoPlayerUI.GetVideoPlayer().ResetRetryProcess();
        }

        public override void OnKinelVideoReset()
        {
            CancelRetrying();
        }
        
    }
}