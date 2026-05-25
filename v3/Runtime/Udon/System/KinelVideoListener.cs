using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    /// <summary>
    /// Listener実装クラス
    /// Listenerを使用し実装したい場合は利用。
    /// </summary>
    public abstract class KinelVideoListener : KinelSystemBase
    {
        #region video player event

        /// <summary>
        /// その動画が初めて再生された際に発火する
        /// pauseなどの後に再生された場合は発火しない
        /// </summary>
        public virtual void OnKinelVideoStart()
        {
        }

        /// <summary>
        /// 動画の読み込みが終わった際に発火する
        /// </summary>
        public virtual void OnKinelVideoReady()
        {
        }

        /// <summary>
        /// 動画が再生された際に発火する
        /// </summary>
        public virtual void OnKinelVideoPlay()
        {
        }

        public virtual void OnKinelVideoPause()
        {
        }

        public virtual void OnKinelVideoEnd()
        {
        }

        public virtual void OnKinelVideoLoop()
        {
        }

        public virtual void OnKinelVideoRetry()
        {
        }

        public virtual void OnKinelVideoError(VideoError videoError)
        {
        }

        #endregion

        #region original video event

        // change: OnKinelVideoModeChange -> OnKinelMediaTypeChanged
        public virtual void OnKinelMediaTypeChanged()
        {
        }

        /// <summary>
        /// URLロードが発火した後のタイミングで発生
        /// </summary>
        /// <param name="url"></param>
        public virtual void OnKinelLoadUrl(VRCUrl url)
        {
        }

        /// <summary>
        /// URLがInputされたときに呼ばれるイベント
        /// </summary>
        /// <param name="url"></param>
        public virtual void OnKinelPostUrlInput(VRCUrl url)
        {
        }

        public virtual void OnKinelVideoTextureUpdated(Texture texture)
        {
        }

        /// <summary>
        /// 動画プレイヤーがリセットされた場合に発火する
        /// </summary>
        public virtual void OnKinelMediaReset()
        {
        }

        public virtual void OnKinelYttlDataLoaded()
        {
        }

        public virtual void OnKinelVideoSpeedChanged(float speed)
        {
        }

        /// <summary>
        /// シーク操作が行われた際に発火する
        /// </summary>
        /// <param name="time">シーク先の時刻（秒）</param>
        public virtual void OnKinelSeek(float time)
        {
        }

        /// <summary>
        /// AB Loopの状態（PointA/PointB/Enabled）が変更された際に発火する
        /// </summary>
        public virtual void OnKinelABLoopStateChanged()
        {
        }

        /// <summary>
        /// ループモードが変更された際に発火する
        /// </summary>
        public virtual void OnKinelLoopModeChanged(LoopMode loopMode)
        {
        }

        /// <summary>
        /// 鏡反転無効（NoMirrorInversion）の設定が変更された際に発火する
        /// </summary>
        public virtual void OnKinelNoMirrorInversionChanged(bool value)
        {
        }

        /// <summary>
        /// KinelPlaylist の再生アクティブ状態が変化した際に発火する
        /// </summary>
        public virtual void OnKinelPlaylistActiveChanged(bool isActive)
        {
        }

        #endregion

        #region queue event

        /// <summary>キューにトラックが追加された際に発火する</summary>
        public virtual void OnKinelQueueAdded()
        {
        }

        /// <summary>キューからトラックが削除された際に発火する（自動送り含む）</summary>
        public virtual void OnKinelQueueRemoved()
        {
        }

        /// <summary>キューの先頭トラックが再生開始した際に発火する</summary>
        public virtual void OnKinelQueueStart()
        {
        }

        #endregion
    }
}