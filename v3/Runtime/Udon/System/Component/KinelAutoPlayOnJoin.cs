using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    /// <summary>
    /// ワールド Join 時に指定のデフォルト URL を自動再生するモジュール
    /// firstPlayerOnly=true のときは 空ワールドへ最初に入った playerのみ発火する
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [KinelModuleAttribute(KinelModuleCategory.Feature, "Auto Play On Join", 35)]
    public class KinelAutoPlayOnJoin : KinelSystemBase
    {
        [SerializeField] private KinelPlayerController controller;

        [Header("Default URL")] [SerializeField]
        private bool enableAutoPlay = true;

        [SerializeField] private VRCUrl defaultUrl;
        [SerializeField] private KinelMediaType mediaType = KinelMediaType.AvPro;

        [Header("Timing")] [Tooltip("Join イベントから再生開始までの遅延 (秒)。ワールドのネット初期化完了を待つ。")] [SerializeField]
        private float delaySeconds = 5f;

        [Tooltip("true: 空ワールドに最初に入った Master のみ発火 (推奨)。" +
                 "false: 自分が join するたびに発火を試みる (既に再生中ならスキップ)。")]
        [SerializeField]
        private bool firstPlayerOnly = true;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!enableAutoPlay) return;
            if (player == null || !player.isLocal) return;
            if (controller == null) return;
            if (defaultUrl == null || defaultUrl.Equals(VRCUrl.Empty)) return;

            if (firstPlayerOnly && !Networking.IsMaster) return;

            if (controller.IsPlaying()) return;

            SendCustomEventDelayedSeconds(nameof(PlayDefault), delaySeconds);
        }

        public void PlayDefault()
        {
            if (controller == null) return;
            if (defaultUrl == null || defaultUrl.Equals(VRCUrl.Empty)) return;
            // 遅延中に他プレイヤーが再生し始めたケース
            if (controller.IsPlaying()) return;

            controller.NowSelectedType = mediaType;
            controller.LoadUrl(defaultUrl);
            Log($"AutoPlay: {defaultUrl} ({mediaType})");
        }
    }
}