using System;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    // Enum for assigning index of field DataTokens
    enum KinelMediaTrackField
    {
        Url,
        Title,
        Type,

        Count
    }

    public class KinelMediaTrack : DataList
    {
        public static KinelMediaTrack New(VRCUrl url, string title, KinelMediaType type)
        {
            var data = new DataToken[(int)KinelMediaTrackField.Count];

            data[(int)KinelMediaTrackField.Url] = new DataToken(url);
            data[(int)KinelMediaTrackField.Title] = title;
            data[(int)KinelMediaTrackField.Type] = new DataToken(type);

            return (KinelMediaTrack)new DataList(data);
        }

#if !COMPILER_UDONSHARP

        private VRCUrl _url;
        private string _title;
        private KinelMediaType _type;

        public VRCUrl Url()
        {
            return _url;
        }

        public string Title()
        {
            return _title;
        }

        public KinelMediaType Type()
        {
            return _type;
        }

        public KinelMediaTrack(VRCUrl url, string title, KinelMediaType type)
        {
            _url = url;
            _title = title;
            _type = type;
        }
#endif
    }

    public static class KinelMediaTrackExt
    {
#if COMPILER_UDONSHARP
        // Get methods
        public static VRCUrl Url(this KinelMediaTrack instance)
            => (VRCUrl)instance[(int)KinelMediaTrackField.Url].Reference;

        public static string Title(this KinelMediaTrack instance)
            => instance.TryGetValue((int)KinelMediaTrackField.Title, TokenType.String, out var value)
                ? (string)value
                : null;

        public static KinelMediaType Type(this KinelMediaTrack instance)
            => (KinelMediaType)instance[(int)KinelMediaTrackField.Type].Reference;

        // Set methods
        public static void Url(this KinelMediaTrack instance, VRCUrl arg)
            => instance[(int)KinelMediaTrackField.Url] = new DataToken(arg);

        public static void Title(this KinelMediaTrack instance, string arg)
            => instance[(int)KinelMediaTrackField.Title] = arg;

        public static void Type(this KinelMediaTrack instance, KinelMediaType arg)
            => instance[(int)KinelMediaTrackField.Type] = new DataToken(arg);

#endif
    }
}