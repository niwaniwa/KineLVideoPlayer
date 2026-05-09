using System;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.Yttl
{
    [KinelModule(KinelModuleCategory.Feature, "VideoTitleViewer", 80)]
    public class KinelYttlBridge : KinelVideoListener
    {
        [SerializeField] private KinelPlayerController controller;
        [SerializeField] private YttlManager manager;

        protected String author;
        protected String title;
        protected String viewCount;
        protected String description;

        public String Author
        {
            get => author;
            private set => author = value;
        }

        public String Title
        {
            get => title;
            private set => title = value;
        }

        public String ViewCount
        {
            get => viewCount;
            private set => viewCount = value;
        }

        public String Description
        {
            get => description;
            private set => description = value;
        }

        public void Start()
        {
            controller.AddListener(this);
            manager.listener = this;
        }

        public override void OnKinelLoadUrl(VRCUrl url)
        {
            manager.LoadData(controller.GetPlayingUrl(), this);
        }

        public override void OnKinelVideoPlay()
        {
            manager.LoadData(controller.GetPlayingUrl(), this);
        }

        public void Yttl_OnDataLoaded()
        {
            controller.OnKinelYttlDataLoaded();
        }
    }
}