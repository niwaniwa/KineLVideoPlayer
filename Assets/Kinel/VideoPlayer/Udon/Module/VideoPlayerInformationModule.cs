
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class VideoPlayerInformationModule : UdonSharpBehaviour
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private VRCUrlInputField inputField;
        [SerializeField] private Text masterTextComponent;
        [SerializeField] private Text ownerTextComponent;

        //[UdonSynced] private bool status = false;

        private VRCPlayerApi master;

        private float elapsedTime = 0f;

        public void Start()
        {
            videoPlayerUI.GetVideoPlayer().RegisterListener(this);
        }

        private void UpdateMaster()
        {
#if !UNITY_EDITOR
            VRCPlayerApi[] returnArrays = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            if (returnArrays.Length == 0)
                return;
            master = VRCPlayerApi.GetPlayers(returnArrays)[0];
            foreach (var player in returnArrays)
                if (player.isMaster)
                    master = player;

            if (master == null)
            {
                master = Networking.LocalPlayer;
            }
#endif
        }

        private void UpdateUI()
        {
            UpdateMaster();
            
#if UNITY_EDITOR
            masterTextComponent.text = "UNITY EDITOR";

            ownerTextComponent.text = "UNITY EDITOR";
#else
            if(master == null){
                SendCustomEventDelayedSeconds(nameof(UpdateUI), 5f);
                return;
            }

            masterTextComponent.text = $"{(master == null ? "... loading ..." : master.displayName)}";
            ownerTextComponent.text = $"{(master == null ? "... loading ..." : Networking.GetOwner(videoPlayerUI.GetVideoPlayer().gameObject).displayName)}";
#endif
            var url = videoPlayerUI.GetVideoPlayer().GetUrl();
            if (url != null)
            {
                inputField.textComponent.text = url.Get();
                inputField.SetUrl(url);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer == player)
            {
                SendCustomEventDelayedSeconds(nameof(UpdateUI), 5f);
                return;
            }
            UpdateUI();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer == null)
                return;
            if (Networking.LocalPlayer == (player))
                return;
            UpdateUI();
        }

        public void OnKinelUrlUpdate()
        {
            UpdateUI();
        }

        public void OnKinelVideoReady()
        {
            UpdateUI();
        }
        
        public void OnKinelVideoStart()
        {
            UpdateUI();
        }

        public void OnKinelChangeVideoTime()
        {
            UpdateUI();
        }

        public void OnKinelVideoModeChange()
        {
            UpdateUI();
        }

        public void OnKinelVideoLoop()
        {
            UpdateUI();
        }

        public void OnKinelVideoEnd()
        {
            UpdateUI();
        }

        public VRCPlayerApi GetMaster()
        {
            return master;
        }
        

        
    }
}