
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
    private bool isGlobalCountDown;

    public void FixedUpdate()
    {
        if (isSyncWaitCountdown)
        {
            count++;
            if (count >= setCountTime * init.UserUpdateCount())
            {
                if (isGlobalCountDown)
                {
                    target.SendCustomNetworkEvent(NetworkEventTarget.All, methodName);
                }
                else
                {
                    target.SendCustomEvent(methodName);
                }
                isSyncWaitCountdown = false;
                count = 0;
            }
        }
    }

    public bool StartSyncWaitCountdown(int time, String methodName, bool isGlobalCountDown)
    {
        if (isSyncWaitCountdown)
        {
            return false;
        }

        this.isGlobalCountDown = isGlobalCountDown;
        this.methodName = methodName;
        setCountTime = time;
        isSyncWaitCountdown = true;
        return true;
    }
    
}
