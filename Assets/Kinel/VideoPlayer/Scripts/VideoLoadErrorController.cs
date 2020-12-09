
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

    public void show(VideoError errorMessage)
    {
        rootObject.SetActive(true);
        textComponent.text = errorMessage.ToString();
    }

    public void hide()
    {
        rootObject.SetActive(false);
    }
    
}
