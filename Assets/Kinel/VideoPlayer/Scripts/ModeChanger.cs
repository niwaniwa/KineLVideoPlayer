
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
using VRC.Udon.Serialization.OdinSerializer.Utilities;

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
    [SerializeField] private GameObject subVideoScree;
    [SerializeField] private GameObject subStreanScreen;
    [SerializeField] private GameObject[] masterOnlyToggleUIObjects;
    
    private const int VIDEO_MODE = 0;
    private const int STREAM_MODE = 1;
    

    public void ChangeMode(int playMode)
    {
        if (videoPlayer.GetVideoPlayer() != null)
            if (Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                videoPlayer.ResetGlobal();
            

        if (playMode == VIDEO_MODE)
        {
            videoPlayer.SetVideoInstance(video);
            textUpdater.SetActive(true);
            videoScreen.SetActive(true);
            subVideoScree.SetActive(true);
            streamScreen.SetActive(false);
            subStreanScreen.SetActive(false);
            sliderController.UnFreeze();
            togglePlayButton.gameObject.SetActive(true);
            animator.SetInteger("PlayMode", VIDEO_MODE);
        }

        if (playMode == STREAM_MODE)
        {
            videoPlayer.SetVideoInstance(stream);
            textUpdater.SetActive(false);
            videoScreen.SetActive(false);
            subVideoScree.SetActive(false);
            streamScreen.SetActive(true);
            subStreanScreen.SetActive(true);
            sliderController.Freeze();
            togglePlayButton.gameObject.SetActive(false);
            animator.SetInteger("PlayMode", STREAM_MODE);
        }
        
        videoPlayer.ResetLocal();
        
    }

    public void ChangeModeForSlider()
    {
        if (videoPlayer.masterOnly && !Networking.LocalPlayer.isMaster)
            return;
        
        if(videoPlayer.GetList() != null)
            videoPlayer.GetList().AutoPlayDisable();
        
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


    public void MasterOnlyForButton()
    {
        if (Networking.LocalPlayer.isMaster)
        {
            ToggleMasterOnly();
        }
    }
    
    public void ToggleMasterOnly()
    {
        if (videoPlayer.masterOnlyLocal)
        {
            if (Networking.LocalPlayer.isMaster)
            {
                Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);
                videoPlayer.masterOnly = false;
            }
            
            videoPlayer.masterOnlyLocal = false;
            animator.SetBool("MasterOnly", false);
            if (!Networking.LocalPlayer.isMaster)
            {
                ToggleWaringUIObject(false);
            }

            return;
        }

        if (!videoPlayer.masterOnlyLocal)
        {
            if (Networking.LocalPlayer.isMaster)
            {
                Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);
                videoPlayer.masterOnly = true;
            }
            
            videoPlayer.masterOnlyLocal = true;
            animator.SetBool("MasterOnly", true);
            if (!Networking.LocalPlayer.isMaster)
            {
                ToggleWaringUIObject(true);
            }
        }
    }

    public void ToggleWaringUIObject(bool b)
    {
        for (int i = 0; i < masterOnlyToggleUIObjects.Length; i++)
        {
            masterOnlyToggleUIObjects[i].SetActive(b);
        }
    }

}
