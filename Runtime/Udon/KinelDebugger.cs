using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

public class KinelDebugger : UdonSharpBehaviour
{

    private const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
    private const string DEBUG_WARN_PREFIX = "[<color=#ff9900>KineL</color>]";
    private const string DEBUG_ERROR_PREFIX = "[<color=#ff0066>KineL</color>]";

    public static void Log(string message)
    {
        Debug.Log($"{DEBUG_LOG_PREFIX} {message}");
    }
    
    public static void Warn(string message)
    {
        Debug.LogWarning($"{DEBUG_LOG_PREFIX} {message}");
    }
    
    public static void Error(string message)
    {
        Debug.LogError($"{DEBUG_LOG_PREFIX} {message}");
    }
    
}
