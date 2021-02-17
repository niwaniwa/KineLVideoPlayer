
using Kinel.VideoPlayer.Scripts;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class ModeChanger : UdonSharpBehaviour
{

    [SerializeField] private VRCUnityVideoPlayer video;
    [SerializeField] private VRCAVProVideoPlayer stream;
    [SerializeField] private KinelVideoScript videoPlayer;
    [SerializeField] private VideoTimeSliderController sliderController;
    //public VideoPlayerTimeTextUpdater textUpdater;
    [SerializeField] private TogglePlayButton togglePlayButton;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject textUpdater;

    [SerializeField] private GameObject videoScreen;
    [SerializeField] private GameObject streamScreen;
    
    private const int VIDEO_MODE = 0;
    private const int STREAM_MODE = 1;
    

    public void ChangeMode(int playMode)
    {
        if (videoPlayer.GetVideoPlayer() != null)
            videoPlayer.ResetLocal();

        if (playMode == VIDEO_MODE)
        {
            videoPlayer.SetVideoInstance(video);
            textUpdater.SetActive(true);
            videoScreen.SetActive(true);
            streamScreen.SetActive(false);
            sliderController.UnFreeze();
            togglePlayButton.gameObject.SetActive(true);
            animator.SetInteger("PlayMode", VIDEO_MODE);
        }

        if (playMode == STREAM_MODE)
        {
            videoPlayer.SetVideoInstance(stream);
            textUpdater.SetActive(false);
            videoScreen.SetActive(false);
            streamScreen.SetActive(true);
            sliderController.Freeze();
            togglePlayButton.gameObject.SetActive(false);
            animator.SetInteger("PlayMode", STREAM_MODE);
        }
        
        videoPlayer.ResetLocal();
        
    }

    public void ChangeModeForSlider()
    {
        if (videoPlayer.masterOnly)
            return;
        
        if (videoPlayer.GetPlayMode() == VIDEO_MODE)
            ChangeStreamModeForStreamButton();
        else
            ChangeStreamModeForVideoButton();
        
    }
    
    public void ChangeStreamModeForVideoButton()
    {
        if (videoPlayer.GetPlayMode() == VIDEO_MODE)
            return;
        
        Debug.Log($"[KineL] Mode changed from button. to [video]");
        ChangeMode(VIDEO_MODE);
        videoPlayer.GlobalChangeMode(VIDEO_MODE);
    }
    
    public void ChangeStreamModeForStreamButton()
    {
        if (videoPlayer.GetPlayMode() == STREAM_MODE)
            return;
        
        Debug.Log($"[KineL] Mode changed from button. to [stream]");
        ChangeMode(STREAM_MODE);
        videoPlayer.GlobalChangeMode(STREAM_MODE);
    }


}
