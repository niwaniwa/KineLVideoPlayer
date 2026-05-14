using System;
using Kinel.VideoPlayer.V3.Udon.System;
using UdonSharp;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon
{
    public class KinelUtilities : UdonSharpBehaviour
    {
        /// <summary>
        /// 配列の末尾に item を必ず追加して新しい配列を返す
        /// 重複検査は行わないため、並列配列の同時更新で長さを揃えたい場合に使う
        /// </summary>
        public static T[] AppendArray<T>(T[] array, T item)
        {
            var temp = new T[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
                temp[i] = array[i];
            temp[array.Length] = item;
            return temp;
        }

        /// <summary>
        /// 配列に item が存在しない場合のみ末尾に追加して新しい配列を返す  
        /// Listener 登録や URL のように同値の重複を許容しない用途で使う
        /// 内部実装は重複検査 + <see cref="AppendArray{T}"/>
        /// </summary>
        public static T[] AddArray<T>(T[] array, T item)
        {
            if (Array.IndexOf(array, item) >= 0)
                return array;
            return AppendArray(array, item);
        }

        /// <summary>
        /// 配列の指定インデックスを削除して新しい配列を返す。
        /// 範囲外は元の配列をそのまま返す。
        /// </summary>
        public static T[] RemoveAtArray<T>(T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return array;

            var temp = new T[array.Length - 1];
            Array.Copy(array, 0, temp, 0, index);
            Array.Copy(array, index + 1, temp, index, array.Length - index - 1);
            return temp;
        }

        /// <summary>
        /// 配列から item と一致する最初の要素を削除して新しい配列を返す。
        /// 値で削除したい単独配列向け。見つからない場合は元の配列をそのまま返す。
        /// 内部実装は <see cref="Array.IndexOf"/> + <see cref="RemoveAtArray{T}"/>。
        /// </summary>
        public static T[] RemoveArray<T>(T[] array, T item)
        {
            return RemoveAtArray(array, Array.IndexOf(array, item));
        }

        public static KinelMediaType ConvertToMediaMode(int value)
        {
            if (Enum.IsDefined(typeof(KinelMediaType), value))
            {
                return (KinelMediaType)value;
            }

            return KinelMediaType.AvPro;
        }

        public static bool IsValidUrl(VRCUrl url)
        {
            return url.Get().StartsWith("http://") || url.Get().StartsWith("https://") || url.Get().StartsWith("rtsp://") || url.Get().StartsWith("rtspt://");
        }
    }
}
