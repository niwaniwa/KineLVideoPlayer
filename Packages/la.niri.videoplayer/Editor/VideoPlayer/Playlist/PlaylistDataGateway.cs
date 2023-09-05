using System;
using System.Collections.Generic;
using Kinel.VideoPlayer.Scripts;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Kinel.VideoPlayer.Editor
{
    public static class PlaylistDataGateway
    {

        private const string youtubePlaylistURL = "https://3c8qzpd1vl.execute-api.ap-northeast-1.amazonaws.com/Product/playlist/";/* AWS Url... */

        public static YoutubePlaylistDataResponce GetYoutubePlaylist(string url)
        {
            var data = new YoutubePlaylistDataResponce();

            if (!IsValidURL(url, Service.Youtube))
                return data;
            
            var request = UnityWebRequest.Get($"{youtubePlaylistURL}" +
                                              $"{url.Split('=')[1]}");


            var async = request.SendWebRequest();

            while (true)
            {
                if (async.isDone)
                    break;
            }


            if (request.isHttpError || request.isNetworkError)
            {

                return data;
            }

            var responceData = JsonUtility.FromJson<YoutubePlaylistDataResponce>(request.downloadHandler.text);

            return responceData;
        }

        public static bool IsValidURL(string url, Service service)
        {
            switch (service)
            {
                case Service.Youtube:
                    if(url.StartsWith("https://www.youtube.com/playlist", StringComparison.OrdinalIgnoreCase)
                       || url.StartsWith("https://youtube.com/playlist", StringComparison.OrdinalIgnoreCase))
                        return true;
                    break;
                default:
                    break;
            }

            return false;
        }
        
        [Serializable]
        public class YoutubePlaylistDataResponce
        {

            [SerializeField] public string name;

            [SerializeField] public VideoData[] videos;

        }

        public enum Service
        {
            Youtube
        }
        
    }
}