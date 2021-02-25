
using System;
using Kinel.VideoPlayer.Scripts;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class InformationManager : UdonSharpBehaviour
{

    [SerializeField] private KinelVideoScript videoPlayer;
    [SerializeField] private VRCUrlInputField inputField;
    [SerializeField] private Text masterTextComponent;
    [SerializeField] private Text ownerTextComponent;

    //[UdonSynced] private bool status = false;

    private VRCPlayerApi master;

    private float elapsedTime = 0f;
    
    public void FixedUpdate()
    {

        elapsedTime += Time.deltaTime;;

        if (elapsedTime <= 5)
            return;

        elapsedTime = 0;
        
        if (videoPlayer.GetVideoPlayer() == null)
            return;

        if(master == null)
            GetMaster();

        masterTextComponent.text = $"{master.displayName}";
        ownerTextComponent.text = $"{Networking.GetOwner(videoPlayer.gameObject).displayName}";
        var url = videoPlayer.GetUrl();
        inputField.textComponent.text = url.Get();
        inputField.SetUrl(url);
    }

    public void GetMaster()
    {
        VRCPlayerApi[] returnArrays = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        master = VRCPlayerApi.GetPlayers(returnArrays)[0];
        foreach (var player in returnArrays)
            if (player.isMaster)
                master = player;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        GetMaster();
    }
}
