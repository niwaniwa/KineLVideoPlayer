using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class VideoErrorUiManager : UdonSharpBehaviour
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private GameObject errorMessageObject;
        

        private Text errorText;

        public void Start()
        {
            errorText = errorMessageObject.transform.Find("Text").GetComponent<Text>();
            videoPlayerUI.RegisterListener(this);
        }

        public void OnKinelVideoReady()
        {
            InactiveUiCover();
        }

        public void OnKinelVideoError()
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

        public void OnKinelVideoRetryError()
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

        public void OnKinelVideoReset()
        {
            CancelRetrying();
        }
        
    }
}