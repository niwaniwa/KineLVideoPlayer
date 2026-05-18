using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System.Sync;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    [KinelModuleAttribute(KinelModuleCategory.Feature, "Queue", 50)]
    public class KinelQueueList : KinelVideoListener
    {
        [SerializeField] private KinelQueueSyncer syncer;
        [SerializeField] private KinelPlayerController controller;

        private VRCUrl[] _localUrls = new VRCUrl[0];
        private string[] _localTitles = new string[0];
        private KinelMediaType[] _localTypes = new KinelMediaType[0];

        private bool IsLocalMode => syncer == null;

        public int Count => IsLocalMode ? _localUrls.Length : syncer.Count;
        public VRCUrl[] Urls => IsLocalMode ? _localUrls : syncer.Urls;
        public string[] Titles => IsLocalMode ? _localTitles : syncer.Titles;

        public void Start()
        {
            if (IsLocalMode && controller != null)
            {
                controller.AddListener(this);
            }
        }

        /// <summary>
        /// queue の index 番目のトラックを取得。範囲外なら null。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public KinelMediaTrack GetTrack(int index)
        {
            if (IsLocalMode)
            {
                if (index < 0 || index >= _localUrls.Length) return null;
                return KinelMediaTrack.New(_localUrls[index], _localTitles[index], _localTypes[index]);
            }

            if (index < 0 || index >= syncer.Count) return null;
            return KinelMediaTrack.New(syncer.Urls[index], syncer.Titles[index], (KinelMediaType)syncer.Types[index]);
        }

        /// <summary>
        /// queue 末尾に Track を追加。sync mode なら Owner へ RPC、local mode ならローカル配列に append。
        /// </summary>
        public bool AddTrack(VRCUrl url, string title, KinelMediaType type)
        {
            if (!KinelUtilities.IsValidUrl(url)) return false;
            if (IsLocalMode)
            {
                _localUrls = KinelUtilities.AppendArray(_localUrls, url);
                _localTitles =
                    KinelUtilities.AppendArray(_localTitles, string.IsNullOrEmpty(title) ? url.ToString() : title);
                _localTypes = KinelUtilities.AppendArray(_localTypes, type);
                if (controller != null) controller.OnKinelQueueAdded();
                return true;
            }

            syncer.RequestAdd(url, type);
            return true;
        }

        /// <summary>
        /// queue の index 番目を削除。sync mode なら Owner へ RPC、local mode ならローカル配列から削除。
        /// </summary>
        public void RemoveTrackAt(int index)
        {
            if (IsLocalMode)
            {
                if (index < 0 || index >= _localUrls.Length) return;
                _localUrls = KinelUtilities.RemoveAtArray(_localUrls, index);
                _localTitles = KinelUtilities.RemoveAtArray(_localTitles, index);
                _localTypes = KinelUtilities.RemoveAtArray(_localTypes, index);
                if (controller != null) controller.OnKinelQueueRemoved();
                return;
            }

            syncer.RequestRemove(index);
        }

        /// <summary>
        /// 現曲終了時。local mode のときのみ先頭 pop + controller.LoadUrl で auto-advance する。
        /// sync mode では KinelQueueSyncer 側が処理するため、こちらの listener は未登録で発火しない。
        /// </summary>
        public override void OnKinelVideoEnd()
        {
            if (!IsLocalMode) return;
            if (controller == null || _localUrls.Length == 0) return;

            var url = _localUrls[0];
            var type = _localTypes[0];
            _localUrls = KinelUtilities.RemoveAtArray(_localUrls, 0);
            _localTitles = KinelUtilities.RemoveAtArray(_localTitles, 0);
            _localTypes = KinelUtilities.RemoveAtArray(_localTypes, 0);

            controller.OnKinelQueueRemoved();
            controller.OnKinelQueueStart();

            controller.NowSelectedType = type;
            controller.LoadUrl(url);
        }
    }
}