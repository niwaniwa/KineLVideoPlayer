
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
    
    private bool isStreamLocal = false;
    
    
    public void ChangeStreamMode()
    {
        videoPlayer.Reset();
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        SendCustomNetworkEvent(NetworkEventTarget.All, "ChangeStreamModeGlobal");
    }

    public void ChangeStreamModeGlobal()
    {
        if (isStreamLocal != videoPlayer.IsStream())
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                return;
            }
        }

        isStreamLocal = !isStreamLocal;
        
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            videoPlayer.SetStreamModeFlag(isStreamLocal);
        }

        stream.isOn = isStreamLocal;
        video.isOn = !isStreamLocal;
        sliderController.Freeze();
    }
    
}
