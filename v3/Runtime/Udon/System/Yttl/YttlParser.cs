using System.Linq;
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
    public class YttlParser : KinelSystemBase
    {
        private readonly string _debugPrefix = "[<color=#ff4500>YTTL</color>] ";
        private VRCUrl defineFileUrl = new VRCUrl("https://raw.githubusercontent.com/ureishi/yttl-data/v2/yttl.txt");

        private string rawDataText;

        private string RawDataText => rawDataText;

        private DataDictionary dataJson;

        private YttlManager yttlManager;

        public void Start()
        {
            Log($"{_debugPrefix} Start()");
            SendCustomEventDelayedSeconds(nameof(LoadDefineFile), 5.1f);
        }

        public void LoadDefineFile()
        {
            VRCStringDownloader.LoadUrl(defineFileUrl, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Init(result.Result);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            LogWarning($"定義データがダウンロードできない Error: {result.Error} (ErrorCode: {result.ErrorCode})");
        }

        private void Init(string defineText)
        {
            if (string.IsNullOrEmpty(defineText))
            {
                LogWarning($"{_debugPrefix} 定義データが空");
                return;
            }

            if (!TryParseDefine(defineText))
            {
                LogWarning($"{_debugPrefix} 定義データを解釈できない");
                return;
            }

            Log($"{_debugPrefix} 定義データ読み込み完了");
        }

        private bool TryParseDefine(string tagDefineJsonText)
        {
            if (!VRCJson.TryDeserializeFromJson(tagDefineJsonText, out var token))
            {
                LogWarning($"{_debugPrefix} {token.Error}: {token.String}");
                return false;
            }

            dataJson = token.DataDictionary;
            return true;
        }

        public void SetRawDataText(string rawDataText)
        {
            this.rawDataText = rawDataText;
        }

        private bool TryGetExtractedHost(string urlStr, out string host)
        {
            var index1 = urlStr.IndexOf("://");

            if (index1 == -1)
            {
                LogWarning($"{_debugPrefix} 不正なURL `://`");
                host = default;
                return false;
            }

            index1 += "://".Length;

            if (urlStr.Substring(index1).StartsWith("www."))
            {
                index1 += "www.".Length;
            }

            var index2 = urlStr.IndexOf("/", index1);

            if (index2 == -1)
            {
                index2 = urlStr.IndexOf("?", index1);
            }

            if (index2 == -1)
            {
                LogWarning($"{_debugPrefix} 未対応のURL `/` or `?`");
                host = default;
                return false;
            }

            host = urlStr.Substring(index1, index2 - index1);
            return true;
        }

        private bool TryGetResolvedUrl(string urlStr, DataDictionary resolverParameters, out string resolvedUrl)
        {
            if (!resolverParameters.TryGetValue("s", TokenType.String, out var sToken))
            {
                LogWarning($"{_debugPrefix} パラメータの開始情報がない");
                resolvedUrl = default;
                return false;
            }

            var s = sToken.String;
            resolverParameters.TryGetValue("t", TokenType.String, out var tToken);

            var t = tToken.TokenType == TokenType.String ? tToken.String : string.Empty;

            var index1 = urlStr.IndexOf(s);

            if (index1 == -1)
            {
                resolvedUrl = default;
                return false;
            }

            index1 += s.Length;

            if (string.IsNullOrEmpty(t))
            {
                resolvedUrl = urlStr.Substring(index1);
                return true;
            }
            else
            {
                var index2 = urlStr.IndexOf(t, index1);

                if (index2 == -1)
                {
                    resolvedUrl = default;
                    return false;
                }

                resolvedUrl = urlStr.Substring(index1, index2 - index1);
                return true;
            }
        }

        public bool TryGetSupportedHost(string urlStr, out string supportedHost)
        {
            if (dataJson == null)
            {
                LogWarning($"{_debugPrefix} 初期化前");
                supportedHost = default;
                return false;
            }

            if (!dataJson.TryGetValue("resolver", TokenType.DataDictionary, out var resolversToken))
            {
                LogWarning($"{_debugPrefix} Resolver一覧の取得ができない");
                supportedHost = default;
                return false;
            }

            var resolvers = resolversToken.DataDictionary;

            if (!TryGetExtractedHost(urlStr, out var host))
            {
                LogWarning($"{_debugPrefix} URLが不正 `{urlStr}`");
                supportedHost = default;
                return false;
            }

            bool isResolver = resolvers.TryGetValue(host, TokenType.DataDictionary, out var resolverToken);

            if (!isResolver)
            {
                if (!resolvers.TryGetValue("", TokenType.DataDictionary, out resolverToken))
                {
                    LogWarning($"{_debugPrefix} Resolverの定義が不正 non use resolver");
                    supportedHost = default;
                    return false;
                }
            }

            var resolver = resolverToken.DataDictionary;

            if (isResolver)
            {
                if (!resolver.TryGetValue("parameter", TokenType.DataDictionary, out var parametersToken))
                {
                    LogWarning($"{_debugPrefix} Resolverのパラメータ情報が取得できない");
                    supportedHost = default;
                    return false;
                }

                var parameters = parametersToken.DataDictionary;

                if (!TryGetResolvedUrl(urlStr, parameters, out var resolvedUrl))
                {
                    LogWarning($"{_debugPrefix} 予期しないResolver表現");
                    supportedHost = default;
                    return false;
                }

                if (!TryGetExtractedHost(resolvedUrl, out host))
                {
                    Log($"{_debugPrefix} Resolve対象のURLが不正 `{resolvedUrl}`");
                    supportedHost = default;
                    return false;
                }
            }

            if (!resolver.TryGetValue("site", TokenType.DataList, out var sitesToken))
            {
                LogWarning($"{_debugPrefix} 対応サイト情報が取得できない");
                supportedHost = default;
                return false;
            }

            var sites = sitesToken.DataList;

            if (sites.Contains(host))
            {
                supportedHost = host;
                return true;
            }
            else
            {
                supportedHost = default;
                return false;
            }
        }

        public bool TryGetValue(string host, DataList labels, out DataDictionary result)
        {
            Log($"{_debugPrefix} try get value `{host}`");
            result = default;
            if (dataJson.Count <= 0)
            {
                LogWarning($"{_debugPrefix} 初期化前");
                return false;
            }

            if (string.IsNullOrEmpty(RawDataText))
            {
                LogWarning($"{_debugPrefix} DataTextが空");
                return false;
            }

            if (!dataJson.TryGetValue("site", TokenType.DataDictionary, out var sitesToken))
            {
                LogWarning($"{_debugPrefix} 対応サイト情報が取得できない");
                return false;
            }

            var sites = sitesToken.DataDictionary;

            if (!sites.TryGetValue(host, TokenType.DataDictionary, out var hostDataToken))
            {
                Log($"{_debugPrefix} 対応していないサイト `{host}`");
                return false;
            }

            var hostData = hostDataToken.DataDictionary;

            var resultDict = new DataDictionary();

            for (var labelIndex = 0; labelIndex < labels.Count; labelIndex++)
            {
                if (!labels.TryGetValue(labelIndex, TokenType.String, out var label))
                {
                    LogWarning($"{_debugPrefix} ラベルが取得できない `{label}`");
                    continue;
                }

                if (!hostData.TryGetValue(label, TokenType.DataDictionary, out var labelDataToken))
                {
                    LogWarning($"{_debugPrefix} 対応していないラベル `{label}`");
                    continue;
                }

                var labelData = labelDataToken.DataDictionary;

                var currentIndex = 0;

                var isSucceed = true;

                if (labelData.TryGetValue("middle", TokenType.DataList, out var middleTagsToken))
                {
                    var middleTags = middleTagsToken.DataList;

                    for (var tagIndex = 0; tagIndex < middleTags.Count; tagIndex++)
                    {
                        if (!middleTags.TryGetValue(tagIndex, TokenType.String, out var middleTagToken))
                        {
                            Log($"{_debugPrefix} 中間タグ一覧が取得できない `{tagIndex}`");
                            isSucceed = false;
                            break;
                        }

                        var middleTag = middleTagToken.String;

                        while (true)
                        {
                            var middleTagIndex = RawDataText.IndexOf(middleTag, currentIndex);

                            if (middleTagIndex != -1)
                            {
                                currentIndex = middleTagIndex + middleTag.Length;

                                if (currentIndex > 0 && RawDataText[currentIndex - 1] == '\\')
                                {
                                    continue;
                                }

                                break;
                            }
                            else
                            {
                                Log($"{_debugPrefix} 中間要素が文字列中に見つからない `{middleTag}`");
                                isSucceed = false;
                                break;
                            }
                        }
                    }
                }

                if (!isSucceed)
                {
                    LogWarning($"{_debugPrefix} 対応していないラベル `{label}`");
                    continue;
                }

                if (!labelData.TryGetValue("end", TokenType.DataDictionary, out var endTagsToken))
                {
                    LogWarning($"{_debugPrefix} 最終タグ一覧が取得できない `{label}`");
                    continue;
                }

                var endTags = endTagsToken.DataDictionary;

                if (!endTags.TryGetValue("s", TokenType.String, out var sTagToken))
                {
                    LogWarning($"{_debugPrefix} 始点タグが取得できない `{label}`");
                    continue;
                }

                var sTag = sTagToken.String;

                int sIndex;

                while (true)
                {
                    sIndex = RawDataText.IndexOf(sTag, currentIndex);

                    if (sIndex == -1)
                    {
                        LogWarning($"{_debugPrefix} 始点が見つからない `{sTag}`");
                        isSucceed = false;
                        break;
                    }
                    else if (sIndex != 0 && RawDataText[sIndex - 1] == '\\')
                    {
                        currentIndex = sIndex + 1;
                        continue;
                    }
                    else
                    {
                        sIndex += sTag.Length;
                        currentIndex = sIndex;
                        break;
                    }
                }

                if (!isSucceed)
                {
                    LogWarning($"{_debugPrefix} 対応していないラベル `{label}`");
                    continue;
                }

                if (!endTags.TryGetValue("t", TokenType.String, out var tTagToken))
                {
                    LogWarning($"{_debugPrefix} 終点タグが取得できない `{label}`");
                    continue;
                }

                var tTag = tTagToken.String;

                int tIndex;

                while (true)
                {
                    tIndex = RawDataText.IndexOf(tTag, currentIndex);

                    if (tIndex == -1)
                    {
                        LogWarning($"{_debugPrefix} 終点が見つからない `{tTag}`");
                        isSucceed = false;
                        break;
                    }
                    else if (tIndex != 0 && RawDataText[tIndex - 1] == '\\')
                    {
                        currentIndex = tIndex + 1;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (!isSucceed)
                {
                    LogWarning($"{_debugPrefix} 対応していないラベル `{label}`");
                    continue;
                }

                var resultStr = RawDataText.Substring(sIndex, tIndex - sIndex);

                Log($"{RawDataText.Substring(sIndex, tIndex - sIndex)}");

                if (VRCJson.TryDeserializeFromJson($@"[""{resultStr}""]", out var resultToken))
                {
                    resultStr = resultToken.DataList[0].String;
                }

                resultDict[label] = resultStr;
            }

            result = resultDict;
            return true;
        }
    }
}