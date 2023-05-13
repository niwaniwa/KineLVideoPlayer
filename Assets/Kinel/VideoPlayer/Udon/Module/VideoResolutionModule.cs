using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class VideoResolutionModule : KinelModule
    {
        [SerializeField] private KinelVideoPlayer videoPlayer;
        [SerializeField] private Animator animator;
        
        [SerializeField] private int[] resolutionArray = new[] { 144, 240, 360, 480, 720, 1080, 1440, 2160 };
        [SerializeField] private Text resolutionText;

        private int resolutionIndex = 4;

        public void Start()
        {
            if (videoPlayer == null) return;
            animator = videoPlayer.GetComponent<Animator>();
            SetResolution(resolutionArray[4]);
        }

        // Udon#1.x.xでEnum使いたい
        public void SetResolution(int resolution)
        {
            if(GetResolutionIndex(resolution) == -1)
            {
                SetResolution(resolutionArray[4]);
                return;
            }
            
            resolutionIndex = GetResolutionIndex(resolution);
            animator.SetInteger("ResolutionIndex", resolutionIndex);
            UpdateUI();
        }

        #region in VRChat

        public void ResolutionUp()
        {
            SetResolution(resolutionArray[GetClampResolutionIndex(++resolutionIndex)]);
        }

        public void ResolutionDown()
        {
            SetResolution(resolutionArray[GetClampResolutionIndex(--resolutionIndex)]);
        }

        public void UpdateUI()
        {
            if (resolutionText == null)
                return;
            resolutionText.text = $"{resolutionArray[resolutionIndex]}";
        }

        public void RestResolution()
        {
            SetResolution(resolutionArray[4]);
            UpdateUI();
        }

        public override void OnKinelVideoError()
        {
            ResolutionDown();
        }
        
        #endregion

        #region Utilities

        private int GetResolutionIndex(int resolution)
        {
            for (int i = 0; i < resolutionArray.Length; i++)
            {
                if (resolutionArray[i].Equals(resolution))
                    return i;
            }
            return -1;
        }

        private int GetClampResolutionIndex(int index)
        {
            return Mathf.Clamp(index, 0, resolutionArray.Length - 1);
        }

        #endregion

    }
}