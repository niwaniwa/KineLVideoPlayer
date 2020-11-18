
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class CountDown : UdonSharpBehaviour
{
    public InitializeScript init;
    public UdonBehaviour target;

    private bool isSyncWaitCountdown = false;
    private float setCountTime = 0;
    private int count = 0;
    private String methodName = "";

    public void FixedUpdate()
    {
        if (isSyncWaitCountdown)
        {
            count++;
            if (count >= setCountTime * init.UserUpdateCount())
            {
                target.SendCustomNetworkEvent(NetworkEventTarget.All, methodName);
                isSyncWaitCountdown = false;
                count = 0;
            }
        }
    }

    public bool StartSyncWaitCountdown(int time, String methodName)
    {
        if (isSyncWaitCountdown)
        {
            return false;
        }

        this.methodName = methodName;
        setCountTime = time;
        isSyncWaitCountdown = true;
        return true;
    }
    
}
