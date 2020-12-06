
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InitializeScript : UdonSharpBehaviour
{

    private const int TimeLimit = 1; // seconds
    private const int LoopLimit = 3;
    
    private bool isEndInitialize = false;
    private bool isFast = true;
    private int updateCount = 0;
    private int loopCount = 0;
    private double start = 0, now = 0;
    

    private int userUpdateCount = 100;
    
    public void Start()
    {
        
    }

    public void FixedUpdate()
    {
        if (isEndInitialize)
        {
            return;
        }

        if (isFast)
        {
            start = Networking.GetServerTimeInSeconds();
            isFast = false;
        }
        
        now = Networking.GetServerTimeInSeconds();
        updateCount++;

        if (now - start >= TimeLimit)
        {
            loopCount++;
            if (loopCount >= LoopLimit)
            {
                isEndInitialize = true;
                userUpdateCount = updateCount / LoopLimit;
                return;
            }

            isFast = true;
        }
        
    }

    public int UserUpdateCount()
    {
        return userUpdateCount;
    }
}
