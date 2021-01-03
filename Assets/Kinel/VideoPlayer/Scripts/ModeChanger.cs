
using Kinel.VideoPlayer.Scripts;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class ModeChanger : UdonSharpBehaviour
{

    public KinelVideoScript videoPlayer;
    public VideoTimeSliderController sliderController;
    public Toggle video, stream;
    public GameObject timeText;

    public void ChangeStreamModeForVideoButton()
    {
        if (video.isOn)
        {
            return;
        }

        ChangeStreamMode();
    }
    
    public void ChangeStreamModeForStreamButton()
    {
        if (stream.isOn)
        {
            return;
        }

        ChangeStreamMode();
    }

    private void ChangeStreamMode()
    {
        videoPlayer.Reset();
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        SendCustomNetworkEvent(NetworkEventTarget.All, "ChangeStreamModeGlobal");
    }

    public void ChangeStreamModeGlobal()
    {
        videoPlayer.SetStreamModeFlagLocal(!videoPlayer.IsStreamLocal());
        videoPlayer.SetVideoInstance(videoPlayer.IsStreamLocal());
        SetToggleActive();
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            videoPlayer.SetStreamModeFlagGlobal(videoPlayer.IsStreamLocal());
            sliderController.Freeze();
            return;
        }
        sliderController.Freeze();
    }

    public void SetToggleActive()
    {
        stream.isOn = videoPlayer.IsStreamLocal();
        video.isOn = !videoPlayer.IsStreamLocal();
        timeText.SetActive(!timeText.activeSelf);
    }
    
}
