using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Kinel.VideoPlayer.V3.Udon.Yttl;

namespace Kinel.VideoPlayer.V3.Udon.System.Network
{
    public class KinelMediaInfoLoader : KinelSystemBase
    {
        [SerializeField] private VRCUrlInputField urlInputField;
        [SerializeField] private YttlParser parser;

        private Color authorColor = new Color(255f / 255f, 184f / 255f, 100f / 255f, 1f);

        [SerializeField] private Color titleColor = new Color(71f / 255f, 241f / 255f, 255f / 255f, 1f);

        [SerializeField] private Color viewCountColor = new Color(92f / 255f, 92f / 255f, 92f / 255f, 1f);


        public void Start()
        {
        }

        public void OnURLChanged()
        {
            var url = urlInputField.GetUrl();
            Log($"String loading...: {url}");
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
            // yttlManager.LoadData(urlInputField.GetUrl(), this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Log($"OnStringLoadSuccess");
            string resultAsUTF8 = result.Result;
            byte[] resultAsBytes = result.ResultBytes;
            string resultAsASCII = Encoding.ASCII.GetString(resultAsBytes);
            Log($"UTF8: {resultAsUTF8}");

            parser.SetRawDataText(result.Result);

            var labels = new DataList(new DataToken[] { "author", "title", "viewCount", "description" });

            if (!parser.TryGetSupportedHost(result.Url.ToString(), out var host))
            {
                LogWarning("未対応のホスト");
                return;
            }

            if (!parser.TryGetValue(host, labels, out var resultDict))
            {
                LogWarning("情報取得時エラー");
                return;
            }

            string author = string.Empty;
            if (resultDict.TryGetValue("author", TokenType.String, out var authorToken))
            {
                author = authorToken.String;
                Log($"[YTTL] {nameof(author)}: {author}");
            }

            string title = string.Empty;
            if (resultDict.TryGetValue("title", TokenType.String, out var titleToken))
            {
                title = titleToken.String;
                Log($"[YTTL] {nameof(title)}: {title}");
            }

            string viewCount = string.Empty;
            if (resultDict.TryGetValue("viewCount", TokenType.String, out var viewCountToken))
            {
                viewCount = viewCountToken.String;
                if (int.TryParse(viewCount, out var partInt))
                {
                    viewCount = $"{partInt:#,0}";
                }

                Log($"[YTTL] {nameof(viewCount)}: {viewCount}");
            }

            string description = string.Empty;
            if (resultDict.TryGetValue("description", TokenType.String, out var descriptionToken))
            {
                description = descriptionToken.String;
                Log($"[YTTL] {nameof(description)}: {description}");
            }

            Log($"Media info: {title}, {author}, {viewCount}, {description}");
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            LogWarning($"Error loading string: {result.ErrorCode} - {result.Error}");
        }

        [NonSerialized] public string author;

        [NonSerialized] public string title;

        [NonSerialized] public string viewCount;

        [NonSerialized] public string description;

        public void Yttl_OnDataLoaded()
        {
            var resultBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(author))
            {
                resultBuilder.Append($"<color={C2CT(authorColor)}>{author}</color>");
            }

            if (!string.IsNullOrEmpty(title))
            {
                if (resultBuilder.Length > 0)
                {
                    resultBuilder.Append(" ");
                }

                resultBuilder.Append($"<color={C2CT(titleColor)}>{title}</color>");
            }

            if (!string.IsNullOrEmpty(viewCount))
            {
                if (resultBuilder.Length > 0)
                {
                    resultBuilder.Append(" ");
                }

                resultBuilder.Append($"<color={C2CT(viewCountColor)}>({viewCount} views)</color>");
            }

            var result = resultBuilder.ToString();

            Log(result);
        }

        private static string C2CT(Color c)
        {
            var r = Mathf.RoundToInt(c.r * 255);
            var g = Mathf.RoundToInt(c.g * 255);
            var b = Mathf.RoundToInt(c.b * 255);

            return $"#{r:x2}{g:x2}{b:x2}";
        }
    }
}