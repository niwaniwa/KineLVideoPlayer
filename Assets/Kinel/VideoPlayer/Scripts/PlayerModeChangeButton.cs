
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerModeChangeButton : UdonSharpBehaviour
{

    public bool videoMode = true;

    public GameObject videoObject, streamObject;
    public GameObject videoButtonObject, streamButtonObject;
    public InitializeScript initializeSystemObject;
    private bool fastSyncEnd = false;
    private int syncUpdateWaitCount = 0;

    [UdonSynced] public bool isStream = false;

    public void FixedUpdate()
    {
        if (fastSyncEnd)
        {
            syncUpdateWaitCount++;
            if (syncUpdateWaitCount >= initializeSystemObject.UserUpdateCount() * 2)
            {
                if (isStream)
                {
                    this.ChangeObjectActive();
                }
                syncUpdateWaitCount = 0;
                fastSyncEnd = false;
            }
            return;
        }
    }

    public void OnModeChange()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SendChangeMessage");

    }

    public void SendChangeMessage()
    {
        ChangeObjectActive();
    }

    private void ChangeObjectActive()
    {
        videoMode = !videoMode;
        videoObject.SetActive(videoMode);
        streamObject.SetActive(!videoMode);
        videoButtonObject.SetActive(videoMode);
        streamButtonObject.SetActive(!videoMode);
        
    }
    
}
