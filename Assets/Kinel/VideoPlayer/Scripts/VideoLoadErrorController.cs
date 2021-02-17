
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

public class VideoLoadErrorController : UdonSharpBehaviour
{

    [SerializeField] private GameObject rootObject;
    [SerializeField] private Text textComponent;
    public void show(VideoError errorMessage)
    {
        rootObject.SetActive(true);
        string message;
        switch (errorMessage)
        {
            case VideoError.AccessDenied:
                message = "ERROR: Access Denied";
                break;
            case VideoError.PlayerError:
                message = "ERROR: Player Error";
                break;
            case VideoError.RateLimited:
                message = "ERROR: Rate Limited.";
                break;
            case VideoError.InvalidURL:
                message = "ERROR: Invalid URL";
                break;
            case VideoError.Unknown:
                message = "ERROR: Unknown error";
                break;
            default:
                message = "ERROR: Unknown error";
                break;
        }
        
        textComponent.text = message;
    }

    public void showMessage(String errorMessage)
    {
        rootObject.SetActive(true);
        textComponent.text = errorMessage;
    }

    public void hide()
    {
        rootObject.SetActive(false);
    }
    
}
