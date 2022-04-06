using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Controller
{
    public class KinelMobileScreenController : UdonSharpBehaviour
    {

        [SerializeField] private GameObject uiGameObject;
        [SerializeField] private Canvas ui;
        [SerializeField] private Animator uiAnimator;
        [SerializeField] private GameObject expand, expandless, extraMenu;
        private bool freeze = false, expandActive = false, uiActive = false;
        
        public void Start()
        {
            
        }

        public void OnPointerEntered()
        {
            //uiGameObject.SetActive(true);
            if (freeze)
                return;
            
            uiGameObject.SetActive(true);
            uiAnimator.SetBool("Enable", true);
            Debug.Log("Enter");
        }

        public void Exit()
        {
            
            uiAnimator.SetBool("Enable", false);
            freeze = true;
            uiActive = false;
            SendCustomEventDelayedSeconds(nameof(Thaw), 1);
            
            Debug.Log("Exit");
        }

        public void ToggleExtraMenu()
        {
            if (expandActive)
                CloseExtraMenu();
            else
                ExpandExtraMenu();

            expandActive = !expandActive;
        }

        public void ExpandExtraMenu()
        {
            expand.SetActive(false);
            expandless.SetActive(true);
            extraMenu.SetActive(true);
        }
        
        public void CloseExtraMenu()
        {
            expand.SetActive(true);
            expandless.SetActive(false);
            extraMenu.SetActive(false);
        }

        public void Thaw()
        {
            freeze = false;
        }
    }
}