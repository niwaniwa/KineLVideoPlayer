using System;
using Kinel.VideoPlayer.V3.Udon.System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.V3.Udon.Yttl
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YttlManager : KinelSystemBase
    {
        private readonly string _debugPrefix = "[<color=#ff4500>YTTL</color>] ";

        [SerializeField] private YttlParser parser;

        [SerializeField] private float loadDelaySeconds = 5.1f;

        [NonSerialized] public VRCUrl url;

        [NonSerialized] public UdonSharpBehaviour listener;

        private void Start()
        {
            SendCustomEventDelayedSeconds(nameof(LoadDefineFile), loadDelaySeconds);
        }

        public void LoadDefineFile()
        {
        }

        public void OnPostLoadDefineFile()
        {
            if (listener != null && !VRCUrl.IsNullOrEmpty(url))
            {
                LoadData(url, listener);
            }
        }

        private void ClearListener()
        {
            url = VRCUrl.Empty;
            listener = null;
        }

        public void LoadData(VRCUrl url, UdonSharpBehaviour listener)
        {
            this.url = url;
            this.listener = listener;

            if (!parser.TryGetSupportedHost(url.Get(), out var _discard))
            {
                LogWarning($"{_debugPrefix} ホストの確認失敗");
                return;
            }

            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            var data = result.Result;

            var url = result.Url.Get();

            if (!parser.TryGetSupportedHost(url, out var host))
            {
                LogWarning($"{_debugPrefix} 未対応のホスト");
                return;
            }

            parser.SetRawDataText(data);

            //var labels = new DataList("author", "title", "viewCount", "description"); // U# bug
            var labels = new DataList(new DataToken[] { "author", "title", "viewCount", "description" });

            if (!parser.TryGetValue(host, labels, out var resultDict))
            {
                LogWarning($"{_debugPrefix} 情報取得時エラー");
                return;
            }

            string author = string.Empty;
            if (resultDict.TryGetValue("author", TokenType.String, out var authorToken))
            {
                author = authorToken.String;
                Log($"{_debugPrefix} {nameof(author)}: {author}");
            }

            string title = string.Empty;
            if (resultDict.TryGetValue("title", TokenType.String, out var titleToken))
            {
                title = titleToken.String;
                Log($"{_debugPrefix} {nameof(title)}: {title}");
            }

            string viewCount = string.Empty;
            if (resultDict.TryGetValue("viewCount", TokenType.String, out var viewCountToken))
            {
                viewCount = viewCountToken.String;
                if (int.TryParse(viewCount, out var partInt))
                {
                    viewCount = $"{partInt:#,0}";
                }

                Log($"{_debugPrefix} {nameof(viewCount)}: {viewCount}");
            }

            string description = string.Empty;
            if (resultDict.TryGetValue("description", TokenType.String, out var descriptionToken))
            {
                description = descriptionToken.String;
                Log($"{_debugPrefix} {nameof(description)}: {description}");
            }

            Yttl_OnDataLoaded(author, title, viewCount, description);

            ClearListener();
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            LogWarning($"{_debugPrefix} 動画情報がダウンロードできない  Error: {result.Error} (ErrorCode: {result.ErrorCode})");
        }

        private void Yttl_OnDataLoaded(string author, string title, string viewCount, string description)
        {
            if (listener != null)
            {
                listener.SetProgramVariable(nameof(author), author);
                listener.SetProgramVariable(nameof(title), title);
                listener.SetProgramVariable(nameof(viewCount), viewCount);
                listener.SetProgramVariable(nameof(description), description);
                listener.SendCustomEvent(nameof(Yttl_OnDataLoaded));
            }
        }
    }
}