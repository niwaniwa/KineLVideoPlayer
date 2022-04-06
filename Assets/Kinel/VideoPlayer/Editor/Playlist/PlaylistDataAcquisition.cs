using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Kinel.VideoPlayer.Scripts.v2.Editor
{
    public class PlaylistDataAcquisition : NetworkAccess
    {
        public override async void Connect()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(urlPath); 
            
            webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
                Debug.Log(webRequest.error);
            else
                Debug.Log(webRequest.downloadHandler.text);

            var hoge = JsonUtility.FromJson<KinelPlaylistDataResponse>(webRequest.downloadHandler.text);
        }


        [Serializable]
        internal class KinelPlaylistDataResponse
        {
            public string kind;
            public string etag;
            public string nextPageToken;
            public string regionCode;
            public YoutubeWebResponsePageInfo pageInfo;
            public YoutubeWebResponseItems[] items;

            [Serializable]
            public class YoutubeWebResponsePageInfo
            {
                public int totalResults;
                public int resultsPerPage;
            }

            [Serializable]
            public class YoutubeWebResponseItems
            {
                public string kind;
                public string etag;
                public YoutubeWebResponseItemsId id;
                public YoutubeWebResponseItemsSnippet snippet;

                [Serializable]
                public class YoutubeWebResponseItemsId
                {
                    public string kind;
                    public string videoId;
                }

                [Serializable]
                public class YoutubeWebResponseItemsResourceId
                {
                    public string kind;
                    public string videoId;
                }

                [Serializable]
                public class YoutubeWebResponseItemsSnippet
                {
                    public string publishedAt;
                    public string channelId;
                    public string title;
                    public string description;
                    public string channelTitle;
                    public string liveBroadcastContent;
                    public string publishTime;
                    public YoutubeWebResponseItemsResourceId resourceId;
                }
            }
        }
    }

    
    
}