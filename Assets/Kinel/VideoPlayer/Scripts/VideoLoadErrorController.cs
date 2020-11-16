
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

public class VideoLoadErrorController : UdonSharpBehaviour
{

    public GameObject rootObject;
    public Text textComponent;
    private string message;

    public void show(VideoError errorMessage)
    {
        rootObject.SetActive(true);
        textComponent.text = message + errorMessage;
    }

    public void hide()
    {
        rootObject.SetActive(false);
    }
    
}
