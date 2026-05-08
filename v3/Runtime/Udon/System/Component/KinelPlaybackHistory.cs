using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    public class KinelPlaybackHistory : KinelVideoListener
    {
        [SerializeField] private KinelPlayerController videoPlayer;

        private VRCUrl[] _urls;
        private string[] _titles;
        private KinelMediaType[] _types;

        public VRCUrl[] Urls
        {
            get => _urls;
            set { _urls = value; }
        }

        public int Count => _urls.Length;

        public void Start()
        {
            if (!videoPlayer) return;

            videoPlayer.AddListener(this);
            _urls = new VRCUrl[0];
            _titles = new string[0];
            _types = new KinelMediaType[0];
        }

        public KinelMediaTrack GetTrack(int index)
        {
            if (index < 0 || index >= _urls.Length) return null;
            return KinelMediaTrack.New(_urls[index], _titles[index], _types[index]);
        }

        public void AddTrack(VRCUrl url, string title, KinelMediaType type)
        {
            if (!KinelUtilities.IsValidUrl(url)) return;

            int before = _urls.Length;
            _urls = KinelUtilities.AddArray(_urls, url);
            if (_urls.Length == before) return; // URL 重複でスキップされた

            _titles = KinelUtilities.AppendArray(_titles, title);
            _types = KinelUtilities.AppendArray(_types, type);
        }

        public void RemoveTrackAt(int index)
        {
            if (index < 0 || index >= _urls.Length) return;

            _urls = KinelUtilities.RemoveAtArray(_urls, index);
            _titles = KinelUtilities.RemoveAtArray(_titles, index);
            _types = KinelUtilities.RemoveAtArray(_types, index);
        }

        public override void OnKinelLoadUrl(VRCUrl url)
        {
            AddTrack(url, url.ToString(), videoPlayer.NowSelectedType);
        }
    }
}
