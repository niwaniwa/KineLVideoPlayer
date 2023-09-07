using System;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Controller
{
    public class KinelExtraMenuController : UdonSharpBehaviour
    {

        [SerializeField] private GameObject canvas, anchor;
        [SerializeField] private GameObject[] settings;

        private bool isSettingActive = false;
        
        public void Start()
        {
            foreach (var gameObject in settings)
            {
                gameObject.transform.position = anchor.transform.position;
            }
        }

        public void ToggleSettings()
        {
            if (!isSettingActive)
            {
                isSettingActive = true;
                canvas.SetActive(true);
                return;
            }
            
            foreach (var gameObject in settings)
            {
                canvas.SetActive(false);
                isSettingActive = false;
                gameObject.SetActive(false);
            }
        }
    }
}