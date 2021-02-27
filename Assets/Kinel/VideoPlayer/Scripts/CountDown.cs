
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class CountDown : UdonSharpBehaviour
{
    public UdonSharpBehaviour target;

    private bool isSyncWaitCountdown = false;
    private float limitTime = 0;
    private float elapsedTime = 0;
    private String methodName = "";
    private bool isGlobalCountDown;

    public void FixedUpdate()
    {
        if (isSyncWaitCountdown)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= limitTime)
            {
                isSyncWaitCountdown = false;
                limitTime = 0;
                if (isGlobalCountDown)
                {
                    target.SendCustomNetworkEvent(NetworkEventTarget.All, methodName);
                }
                else
                {
                    target.SendCustomEvent(methodName);
                }
            }
        }
    }

    public bool StartSyncWaitCountdown(float time, String methodName, bool isGlobalCountDown)
    {
        if (isSyncWaitCountdown)
        {
            return false;
        }

        this.isGlobalCountDown = isGlobalCountDown;
        this.methodName = methodName;
        limitTime = time;
        isSyncWaitCountdown = true;
        return true;
    }

    public bool IsCountDown()
    {
        return isSyncWaitCountdown;
    }
    
}
