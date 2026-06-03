using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.V3.Udon.System.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [KinelModuleAttribute(KinelModuleCategory.Sync, "Queue Syncer", 50)]
    public class KinelQueueSyncer : KinelVideoListener
    {
        [SerializeField] private KinelPlayerController controller;

        [UdonSynced, FieldChangeCallback(nameof(Urls))]
        private VRCUrl[] _urls = new VRCUrl[0];

        [UdonSynced, FieldChangeCallback(nameof(Titles))]
        private string[] _titles = new string[0];

        [UdonSynced] private int[] _types = new int[0];
        [UdonSynced] private string[] _addedBy = new string[0];

        public int Count => _urls.Length;
        public int[] Types => _types;
        public string[] AddedBy => _addedBy;

        public VRCUrl[] Urls
        {
            get => _urls;
            set
            {
                var prevCount = _urls.Length;
                _urls = value;
                if (Networking.IsOwner(gameObject)) return;
                if (_urls.Length > prevCount)
                    controller.OnKinelQueueAdded();
                else
                    controller.OnKinelQueueRemoved();
            }
        }

        public string[] Titles
        {
            get => _titles;
            set
            {
                _titles = value;
                if (Networking.IsOwner(gameObject)) return;
                // if (_titles.Length == _urls.Length)
                //     controller.OnKinelQueueAdded();
            }
        }


        public void Start()
        {
            if (controller != null) controller.AddListener(this);
        }

        /// <summary>
        /// ローカルから Owner へ AddTrack を依頼する。
        /// </summary>
        public void RequestAdd(VRCUrl url, KinelMediaType type)
        {
            Log("RequestAdd: " + url);
            if (!KinelUtilities.IsValidUrl(url)) return;
            var local = Networking.LocalPlayer;
            var who = local != null ? local.displayName : "";
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(AddTrack),
                url, (int)type, who);
        }

        /// <summary>
        /// ローカルから Owner へ RemoveAt を依頼する
        /// </summary>
        public void RequestRemove(int index)
        {
            var local = Networking.LocalPlayer;
            var who = local != null ? local.displayName : "";
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RemoveAt),
                index, who);
        }

        /// <summary>
        /// Yttl 等から Owner へ Title 差し替えを依頼する
        /// </summary>
        public void RequestUpdateTitle(int index, string newTitle)
        {
            if (index < 0 || index >= _urls.Length) return;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(UpdateTitle),
                index, _addedBy[index], newTitle ?? "");
        }

        /// <summary>
        /// Owner で実行される追加処理。配列に append + RequestSerialization
        /// </summary>
        [NetworkCallable]
        public void AddTrack(VRCUrl url, int typeInt, string addedBy)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (url == null || url.Equals(VRCUrl.Empty)) return;

            _urls = AppendUrl(_urls, url);
            _titles = AppendString(_titles, url.ToString());
            _types = AppendInt(_types, typeInt);
            _addedBy = AppendString(_addedBy, addedBy ?? "");
            RequestSerialization();
            controller.OnKinelQueueAdded();
        }

        /// <summary>
        /// Owner で実行される削除処理。本人 or Owner 自身のリクエストのみ通る。
        /// </summary>
        [NetworkCallable]
        public void RemoveAt(int index, string requesterDisplayName)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (index < 0 || index >= _urls.Length) return;

            var requester = requesterDisplayName ?? "";
            var localName = Networking.LocalPlayer != null ? Networking.LocalPlayer.displayName : "";
            bool isAuthor = _addedBy[index] == requester;
            bool isQueueOwnerSelf = localName == requester;
            if (!isAuthor && !isQueueOwnerSelf) return;

            _urls = RemoveAtUrl(_urls, index);
            _titles = RemoveAtString(_titles, index);
            _types = RemoveAtInt(_types, index);
            _addedBy = RemoveAtString(_addedBy, index);
            RequestSerialization();
            controller.OnKinelQueueRemoved();
        }

        /// <summary>
        /// Owner で実行される Title 更新。addedBy が一致する場合のみ差し替える
        /// </summary>
        [NetworkCallable]
        public void UpdateTitle(int index, string expectedAddedBy, string newTitle)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (index < 0 || index >= _urls.Length) return;
            if (_addedBy[index] != (expectedAddedBy ?? "")) return;

            _titles[index] = newTitle ?? "";
            RequestSerialization();
        }

        /// <summary>
        /// 現動画が終了で queue 先頭を pop して LoadUrl する (Owner のみ)
        /// </summary>
        public override void OnKinelVideoEnd()
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (controller == null || _urls.Length == 0) return;
            if (controller.IsReloading) return; // reloadに伴うStop()のOnKinelVideoEndではpop(次へ再生)しない

            _urls = RemoveAtUrl(_urls, 0);
            _titles = RemoveAtString(_titles, 0);
            _types = RemoveAtInt(_types, 0);
            _addedBy = RemoveAtString(_addedBy, 0);
            RequestSerialization();
            controller.OnKinelQueueRemoved();

            if (_urls.Length == 0) return;

            var url = _urls[0];
            var type = _types[0];
            controller.OnKinelQueueStart();
            controller.NowSelectedType = (KinelMediaType)type;
            controller.LoadUrl(url);
        }

        private static VRCUrl[] AppendUrl(VRCUrl[] a, VRCUrl v)
        {
            var n = new VRCUrl[a.Length + 1];
            for (int i = 0; i < a.Length; i++) n[i] = a[i];
            n[a.Length] = v;
            return n;
        }

        private static string[] AppendString(string[] a, string v)
        {
            var n = new string[a.Length + 1];
            for (int i = 0; i < a.Length; i++) n[i] = a[i];
            n[a.Length] = v;
            return n;
        }

        private static int[] AppendInt(int[] a, int v)
        {
            var n = new int[a.Length + 1];
            for (int i = 0; i < a.Length; i++) n[i] = a[i];
            n[a.Length] = v;
            return n;
        }

        private static VRCUrl[] RemoveAtUrl(VRCUrl[] a, int index)
        {
            var n = new VRCUrl[a.Length - 1];
            for (int i = 0, j = 0; i < a.Length; i++)
            {
                if (i == index) continue;
                n[j++] = a[i];
            }

            return n;
        }

        private static string[] RemoveAtString(string[] a, int index)
        {
            var n = new string[a.Length - 1];
            for (int i = 0, j = 0; i < a.Length; i++)
            {
                if (i == index) continue;
                n[j++] = a[i];
            }

            return n;
        }

        private static int[] RemoveAtInt(int[] a, int index)
        {
            var n = new int[a.Length - 1];
            for (int i = 0, j = 0; i < a.Length; i++)
            {
                if (i == index) continue;
                n[j++] = a[i];
            }

            return n;
        }
    }
}