using System;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using Kinel.VideoPlayer.V3.Udon.System.Sync;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;
using Kinel.VideoPlayer.V3.Udon.Yttl;

namespace Kinel.VideoPlayer.V3.Udon.Interface
{
    /// <summary>
    /// UIの内部システムに関する処理の集約モジュール
    /// </summary>
    [KinelModuleAttribute(KinelModuleCategory.UI, "UI Controller", 90)]
    public class KinelUIController : KinelVideoListener
    {
        #region Udon

        [SerializeField] private KinelPlayerController controller;
        [SerializeField] private KinelPlaylist playlist;
        [SerializeField] private KinelQueueList queueList;
        [SerializeField] private KinelYttlBridge yttlBridge;

        #endregion

        [SerializeField] private Canvas canvas;

        [FormerlySerializedAs("_animator")] [SerializeField]
        private Animator animator;

        [SerializeField] private GameObject queuePrefab;

        [SerializeField, KinelUIEvent(nameof(OnURLChanged), UIEventType.EndEdit)]
        private VRCUrlInputField inputField;

        // inputfieldの後ろにあるボタン
        [SerializeField] private Button inputFieldButton;

        // [SerializeField, KinelUIEvent(nameof(OnQueueAdd), UIEventType.EndEdit)]
        [SerializeField] private VRCUrlInputField queueInputField;

        [SerializeField, KinelUIEvent(nameof(OnURLChanged), UIEventType.EndEdit)]
        private VRCUrlInputField titleInputField;

        [SerializeField] [KinelUIEvent(nameof(OnVolumeChanged), UIEventType.SliderChanged)]
        private Slider volumeSlider;

        [SerializeField] private Slider seekSlider;

        # region [WIP] 本リリースではbuttonをKinelUIButtonに。Canvas関連のコンポーネントはAnimatorで制御

        // 以下は将来的にKinelUIButton.onClickみたいな感じで関数をトリガーできるようにしたい。KinelUIEventはKinelUIButtonで呼び出したい
        [Header("Playback Control")] [SerializeField, KinelUIEvent(nameof(OnResumed), UIEventType.ButtonClick)]
        private Button resumeButton;

        [SerializeField, KinelUIEvent(nameof(OnPaused), UIEventType.ButtonClick)]
        private Button pauseButton;

        [SerializeField, KinelUIEvent(nameof(OnPrevious), UIEventType.ButtonClick)]
        private Button previousButton;

        [SerializeField, KinelUIEvent(nameof(OnNext), UIEventType.ButtonClick)]
        private Button nextButton;

        [SerializeField, KinelUIEvent(nameof(OnLoopToggle), UIEventType.ButtonClick)]
        private Button loopToggleButton;

        [Header("Volume Control")] [SerializeField, KinelUIEvent(nameof(OnVolumeMute), UIEventType.ButtonClick)]
        private Button volumeMute;

        [SerializeField, KinelUIEvent(nameof(OnVolumeUnMute), UIEventType.ButtonClick)]
        private Button volumeUnMute;

        [SerializeField, KinelUIEvent(nameof(OnVolumeMute), UIEventType.ButtonClick)]
        private Button volumeZero;

        [FormerlySerializedAs("volumeDownButton")]
        [SerializeField, KinelUIEvent(nameof(OnVolumeMute), UIEventType.ButtonClick)]
        private Button volumeMedium;

        [FormerlySerializedAs("volumeUpButton")]
        [SerializeField, KinelUIEvent(nameof(OnVolumeMute), UIEventType.ButtonClick)]
        private Button volumeLoud;

        [Header("Menu & Toggles")] [SerializeField, KinelUIEvent(nameof(OnPlaylistToggle), UIEventType.ButtonClick)]
        private Button playlistButton;

        [SerializeField, KinelUIEvent(nameof(OnSettingToggle), UIEventType.ButtonClick)]
        private Button settingsButton;

        [SerializeField, KinelUIEvent(nameof(OnReload), UIEventType.ButtonClick)]
        private Button reloadButton;

        [SerializeField, KinelUIEvent(nameof(OnToggleRemainingTimeMode), UIEventType.ButtonClick)]
        private Button remainingTimeToggle;

        [Header("Playback Speed")]
        [SerializeField, KinelUIEvent(nameof(OnIncreaseSpeedLargeClick), UIEventType.ButtonClick)]
        private Button increaseSpeedLargeButton;

        [SerializeField, KinelUIEvent(nameof(OnDecreaseSpeedLargeClick), UIEventType.ButtonClick)]
        private Button decreaseSpeedLargeButton;

        [SerializeField, KinelUIEvent(nameof(OnIncreaseSpeedSmallClick), UIEventType.ButtonClick)]
        private Button increaseSpeedSmallButton;

        [SerializeField, KinelUIEvent(nameof(OnDecreaseSpeedSmallClick), UIEventType.ButtonClick)]
        private Button decreaseSpeedSmallButton;

        [SerializeField] public float speedIncrementLarge = 0.1f;
        [SerializeField] public float speedIncrementSmall = 0.01f;

        [SerializeField] private TMP_Text speedText;

        [Header("Time Offset")] [SerializeField]
        private KinelVariableSyncer syncer;

        [SerializeField, KinelUIEvent(nameof(OnTimeOffsetPlus), UIEventType.ButtonClick)]
        private Button timeOffsetPlusButton;

        [SerializeField, KinelUIEvent(nameof(OnTimeOffsetMinus), UIEventType.ButtonClick)]
        private Button timeOffsetMinusButton;

        [SerializeField, KinelUIEvent(nameof(OnTimeOffsetReset), UIEventType.ButtonClick)]
        private Button timeOffsetResetButton;

        [SerializeField] private TMP_Text timeOffsetText;

        [Header("Time Display")] [SerializeField]
        private GameObject separate;

        [SerializeField] private TMP_Text duration;
        [SerializeField] private TMP_Text elapsedTime;
        [SerializeField] private TMP_Text remainingTime;
        [SerializeField] private string timeFormat = "hh\\:mm\\:ss\\";

        [Header("Playlist & UI Groups")] [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField] private GameObject playlistUI; //  selectorやqueueなどが入るGameObject
        [SerializeField] private GameObject playlistSelector; // playlistを選択するUIのParent

        [Header("Settings UI")] [SerializeField]
        private GameObject settingsUI;

        [Header("AB Loop")] [SerializeField] private GameObject abLoopPanel;

        [Header("Media Type")] [SerializeField, KinelUIEvent(nameof(OnSelectUnityVideo), UIEventType.ButtonClick)]
        private Button mediaTypeUnityVideoButton;

        [SerializeField, KinelUIEvent(nameof(OnSelectStream), UIEventType.ButtonClick)]
        private Button mediaTypeStreamButton;

        [SerializeField, KinelUIEvent(nameof(OnSelectImage), UIEventType.ButtonClick)]
        private Button mediaTypeImageButton;

        [Header("Resolution")] [SerializeField, KinelUIEvent(nameof(OnSelectResolution144), UIEventType.ButtonClick)]
        private Button resolution144Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution240), UIEventType.ButtonClick)]
        private Button resolution240Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution360), UIEventType.ButtonClick)]
        private Button resolution360Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution480), UIEventType.ButtonClick)]
        private Button resolution480Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution720), UIEventType.ButtonClick)]
        private Button resolution720Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution1080), UIEventType.ButtonClick)]
        private Button resolution1080Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution1440), UIEventType.ButtonClick)]
        private Button resolution1440Button;

        [SerializeField, KinelUIEvent(nameof(OnSelectResolution2160), UIEventType.ButtonClick)]
        private Button resolution2160Button;

        [Header("Screen Settings")]
        [SerializeField, KinelUIEvent(nameof(OnMirrorInversionToggle), UIEventType.ButtonClick)]
        private Button mirrorInversionToggleButton;

        [Header("Others")] [SerializeField] private TMP_Text titleText;

        [SerializeField, KinelUIEvent(nameof(OnQueueAdd), UIEventType.ButtonClick)]
        private Button queueAddButton;

        [FieldChangeCallback(nameof(ErrorText))]
        private string _errorText;

        [SerializeField] private TMP_Text errorTextMesh;

        // [SerializeField] private KinelPlaylist playlist;

        # endregion

        #region Animator Parameters

        private string OnMenuOpenFlag => "OnMenuOpen";
        private string LoadingFlag => "IsLoading";
        private string ErrorTrigger => "ErrorTrigger";

        #endregion

        #region PublicAPI

        [SerializeField] public float seekTimeIncrement = 10f;

        public GameObject PlaylistUI => playlistUI;

        public KinelPlayerController Controller => controller;

        #endregion

        private int _selectedPlaylistIndex = 0;
        private int _selectedTrackIndex = 0;

        private GameObject _selectorParent;

        [SerializeField] private GameObject queueObject;
        [SerializeField] private GameObject queueListUIContent;

        #region FieldChangeCallback

        public String ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value;
                animator.SetTrigger(ErrorTrigger);
                if (errorTextMesh == null) return;
                errorTextMesh.text = value;
            }
        }

        #endregion

        public void Start()
        {
            if (!Validate())
            {
                Log("Validate Failed");
                return;
            }

            controller.AddListener(this);
            // controller.Volume = volumeSlider.value;
            volumeSlider.value = controller.Volume;

            ApplyPauseStateUI();
            ApplyMediaTypeUI();
            ApplyResolutionUI(controller.GetResolution());

            _selectorParent = playlistUI.transform.Find("Playlist Selector/Parent/Viewport/Content").gameObject;
        }

        public void Update()
        {
            UpdateSeekInterface();
            UpdateTimeUI();
        }

        public void ToggleCanvas()
        {
            Log("Toggle Canvas");
            if (canvasGroup.alpha == 0)
                animator.SetBool(OnMenuOpenFlag, true);
            else
                animator.SetBool(OnMenuOpenFlag, false);
        }

        #region Video Events

        public override void OnKinelLoadUrl(VRCUrl url)
        {
            StartLoading();
        }

        public override void OnKinelVideoStart()
        {
            BeginSeekingOnVideoStart();

            ApplyPlayStateUI();
            StopLoading();
        }

        public override void OnKinelVideoReady()
        {
            TimeInitialize();
            StopLoading();
        }

        public override void OnKinelVideoPlay()
        {
            BeginSeekingOnVideoStart();
            ApplyPlayStateUI();
            TimeInitialize();
        }

        public override void OnKinelVideoPause()
        {
            base.OnKinelVideoPause();
            ApplyPauseStateUI();
        }

        public override void OnKinelVideoEnd()
        {
            base.OnKinelVideoEnd();
            ApplyPauseStateUI();
        }

        public override void OnKinelVideoLoop()
        {
            base.OnKinelVideoLoop();
        }

        public override void OnKinelVideoRetry()
        {
            base.OnKinelVideoRetry();
        }

        public override void OnKinelYttlDataLoaded()
        {
            ApplyTitleUI(ReconstructTitleUI());
            RebuildQueueUI();
        }

        public override void OnKinelQueueAdded()
        {
            RebuildQueueUI();
        }

        public override void OnKinelQueueRemoved()
        {
            RebuildQueueUI();
        }

        public override void OnKinelMediaReset()
        {
            TimeInitialize();
        }

        public override void OnKinelVideoError(VideoError videoError)
        {
            ErrorText = $"ERROR: {videoError.ToString()}";
            StopLoading();
        }

        public override void OnKinelVideoSpeedChanged(float speed)
        {
            speedText.text = $"x{speed.ToString("F2")}";
        }

        public override void OnKinelMediaTypeChanged()
        {
            ApplyMediaTypeUI();
        }

        #endregion

        #region Input

        private void InputInitialize()
        {
        }

        public void OnURLChanged()
        {
            var url = inputField.GetUrl();

            if (url.Equals(VRCUrl.Empty))
            {
                url = titleInputField.GetUrl();
            }

            if (url.Equals(VRCUrl.Empty)) return;

            inputField.SetUrl(VRCUrl.Empty);
            titleInputField.SetUrl(VRCUrl.Empty);

            PlayMedia(url);
            ApplyTitleUI("");
        }

        private void PlayMedia(VRCUrl url)
        {
            // モーダル入れるならここ
            controller.LoadUrl(url);
        }

        public void OnPaused()
        {
            controller.NowSelectedMediaModule.Pause();
            ApplyPauseStateUI();
        }

        public void OnResumed()
        {
            controller.NowSelectedMediaModule.Play();
            ApplyPlayStateUI();
        }

        public void OnPrevious()
        {
            float newTime = controller.GetTime() - seekTimeIncrement;
            controller.SetTime(newTime);
            controller.OnKinelSeek(newTime);
        }

        public void OnNext()
        {
            float newTime = controller.GetTime() + seekTimeIncrement;
            controller.SetTime(newTime);
            controller.OnKinelSeek(newTime);
        }

        public void OnReturnToPlaylistSelect()
        {
            playlistSelector.SetActive(true);
            PlaylistUI.transform.GetChild(_selectedPlaylistIndex + 2).gameObject.SetActive(false);
            Log("Return to Playlist Select.");
        }

        public void OnPlaylistSelect()
        {
            var index = _selectorParent.transform.childCount;
            for (int i = 0; i < index; i++)
            {
                var child = _selectorParent.transform.GetChild(i);
                if (!child.gameObject.activeSelf)
                {
                    _selectedPlaylistIndex = i;
                    child.gameObject.SetActive(true);
                    break;
                }
            }

            if (PlaylistUI.transform.childCount < _selectedPlaylistIndex + 2)
            {
                LogWarning("Playlist UI is not found.");
                return;
            }

            playlistSelector.SetActive(false);
            PlaylistUI.transform.GetChild(_selectedPlaylistIndex + 2).gameObject.SetActive(true);
            Log($"Playlist Select. index: {_selectedPlaylistIndex}");
        }

        public void OnPlaylistTrackSelect()
        {
            var selectedPlaylist = PlaylistUI.transform.GetChild(_selectedPlaylistIndex + 2).gameObject.transform
                .Find("Parent/Viewport/Content");
            var index = selectedPlaylist.transform.childCount;
            for (int i = 0; i < index; i++)
            {
                var child = selectedPlaylist.transform.GetChild(i);
                if (!child.gameObject.activeSelf)
                {
                    _selectedTrackIndex = i;
                    child.gameObject.SetActive(true);
                    break;
                }
            }

            if (selectedPlaylist.transform.childCount < _selectedTrackIndex)
            {
                LogWarning("Track is not found.");
                return;
            }

            selectedPlaylist.transform.GetChild(_selectedTrackIndex).gameObject.SetActive(true);
            Log($"Track Select. index: {_selectedTrackIndex}");

            var hoge = playlist.GetPlaylist(_selectedPlaylistIndex - 1)[_selectedTrackIndex];

            Log($"track index: {_selectedTrackIndex}, url: {hoge.Url()}, title: {hoge.Title()}");

            PlayMedia(hoge.Url());
        }

        public void OnQueueSelect()
        {
            queueObject.SetActive(true);
        }

        public void OnQueueAdd()
        {
            var url = queueInputField.GetUrl();
            bool added = queueList.AddTrack(url, url.ToString(), controller.NowSelectedType);
            queueInputField.SetUrl(VRCUrl.Empty);
            if (!added) return;
            Log($"Queue Add. url: {url}, count: {queueList.Count}");
        }

        public void OnQueuePlayByIndex(int index)
        {
            if (index < 0) return;
            controller.LoadUrl(queueList.Urls[index]);
        }

        public void OnQueueRemoveByIndex(int index)
        {
            if (index < 0) return;
            queueList.RemoveTrackAt(index);
        }

        /// <summary>
        /// interactedで確認する
        /// </summary>
        /// <param name="index"></param>
        public int GetSelectedPlaylist()
        {
            for (int i = 0; i < _selectorParent.transform.childCount; i++)
            {
                var child = _selectorParent.transform.GetChild(i);
                if (!child.GetComponentInChildren<Button>().interactable)
                {
                    _selectedPlaylistIndex = i;
                    return i;
                }
            }

            return -1;
        }

        public int GetSelectedTrack()
        {
            var playlistGroup = playlistUI.transform.GetChild(_selectedPlaylistIndex + 1) // プレイリストセレクト分がある
                .Find("PlaylistSelectPanel/Viewport/Content");
            for (int i = 0; i < playlistGroup.childCount; i++)
            {
                var child = playlistGroup.GetChild(i);
                if (!child.GetComponentInChildren<Button>().interactable)
                    return i;
            }

            return -1;
        }

        public void OnLoopToggle()
        {
            controller.SetLoop(!controller.Loop);
        }

        public void OnPlaylistToggle()
        {
            if (playlistUI == null) return;
            playlistUI.SetActive(!playlistUI.activeSelf); // 閉じても開いていたPlaylistは保持しておきたいのですべての親をToggleする

            if (settingsUI == null) return;
            settingsUI.SetActive(false);
        }

        public void OnSettingToggle()
        {
            if (settingsUI == null) return;
            settingsUI.SetActive(!settingsUI.activeSelf);

            if (playlistUI == null) return;
            playlistUI.SetActive(false);
        }

        public void OnABLoopToggle()
        {
            if (abLoopPanel == null) return;
            abLoopPanel.SetActive(!abLoopPanel.activeSelf);
        }

        public void OnMirrorInversionToggle()
        {
            controller.SetNoMirrorInversion(!controller.NoMirrorInversion);
        }

        public void OnReload()
        {
            Log("Reload");
            controller.ReloadMedia();
        }

        public void OnIncreaseSpeedLargeClick()
        {
            controller.NowSelectedMediaModule.SetPlaybackSpeed(controller.NowSelectedMediaModule.GetPlaybackSpeed() +
                                                               speedIncrementLarge);
        }

        public void OnDecreaseSpeedLargeClick()
        {
            controller.NowSelectedMediaModule.SetPlaybackSpeed(controller.NowSelectedMediaModule.GetPlaybackSpeed() -
                                                               speedIncrementLarge);
        }

        public void OnIncreaseSpeedSmallClick()
        {
            controller.NowSelectedMediaModule.SetPlaybackSpeed(controller.NowSelectedMediaModule.GetPlaybackSpeed() +
                                                               speedIncrementSmall);
        }

        public void OnDecreaseSpeedSmallClick()
        {
            controller.NowSelectedMediaModule.SetPlaybackSpeed(controller.NowSelectedMediaModule.GetPlaybackSpeed() -
                                                               speedIncrementSmall);
        }

        public void OnTimeOffsetPlus()
        {
            if (syncer == null) return;
            syncer.SetLocalTimeOffset(syncer.LocalTimeOffset + 0.1f);
            UpdateTimeOffsetUI();
        }

        public void OnTimeOffsetMinus()
        {
            if (syncer == null) return;
            syncer.SetLocalTimeOffset(syncer.LocalTimeOffset - 0.1f);
            UpdateTimeOffsetUI();
        }

        public void OnTimeOffsetReset()
        {
            if (syncer == null) return;
            syncer.SetLocalTimeOffset(0f);
            UpdateTimeOffsetUI();
        }

        private void UpdateTimeOffsetUI()
        {
            if (timeOffsetText == null || syncer == null) return;
            float offset = syncer.LocalTimeOffset;
            timeOffsetText.text = offset >= 0f
                ? $"+{offset:F1}s"
                : $"{offset:F1}s";
        }

        #endregion

        #region Seek

        private bool _isDragging = false;


        private void SeekInitialize()
        {
        }

        public void BeginSeekingOnVideoStart()
        {
            Log($"Begin Seeking On Video Start {controller.GetDuration()}");
#if UNITY_EDITOR
            if (controller.GetDuration() == 0)
            {
                SendCustomEventDelayedFrames(nameof(BeginSeekingOnVideoStart), 5);
                return;
            }
#endif
            SetSeekLength(controller.GetDuration());
        }

        public void StopSeeking()
        {
            SetSeekLength(0);
        }

        public void OnSliderDrag() => _isDragging = true;

        public void OnSliderDrop()
        {
            Log($"On Slider Drop {seekSlider.value}");
            controller.SetTime(seekSlider.value);
            controller.OnKinelSeek(seekSlider.value);
            SendCustomEventDelayedFrames(nameof(SeekWaitCallback), 10);
        }

        public void SeekWaitCallback() => _isDragging = false;

        private void UpdateSeekInterface()
        {
            if (controller.NowSelectedMediaModule.MediaType == KinelMediaType.Image) return;
            if (!controller.IsPlaying() && !controller.IsPaused()) return;
            if (controller.IsStream()) return;
            if (!_isDragging)
                seekSlider.value = controller.GetTime();
        }

        private void SetSeekLength(float time)
        {
            seekSlider.maxValue = time;
        }

        #endregion

        #region Time

        // 時間表示モード（false: 経過時間 Elapsed, true: 残り時間 Remaining）
        private bool _isRemainingTimeMode = false;


        private void TimeInitialize()
        {
            if (controller.NowSelectedMediaModule.MediaType == KinelMediaType.Image) return;

            if (controller.IsStream())
            {
                duration.text = "LIVE";
                return;
            }

            if (controller.GetDuration() >= 3600)
            {
                timeFormat = "hh\\:mm\\:ss";
            }
            else
            {
                timeFormat = "mm\\:ss";
            }

            duration.text = $"{TimeSpan.FromSeconds(controller.GetDuration()).ToString(timeFormat)}";
        }

        public void OnToggleRemainingTimeMode() => OnTimeDisplayToggle();

        public void UpdateTimeUI()
        {
            if (controller.NowSelectedMediaModule.MediaType == KinelMediaType.Image) return;
            if (!controller.IsPlaying() && !controller.IsPaused()) return;
            if (_isRemainingTimeMode) UpdateRemainingTime();
            else UpdateElapsedTime();
        }

        public void UpdateElapsedTime()
        {
            elapsedTime.text = $"{TimeSpan.FromSeconds(controller.GetTime()).ToString(timeFormat)}";
        }

        public void UpdateRemainingTime()
        {
            if (controller.IsStream())
            {
                remainingTime.text =
                    $"{TimeSpan.FromSeconds(controller.GetTime()).ToString(timeFormat)}";
                return;
            }

            var remaining = Mathf.Clamp(controller.GetDuration() - controller.GetTime(), 0, float.MaxValue);
            remainingTime.text =
                $"{TimeSpan.FromSeconds(remaining).ToString(timeFormat)}";
        }

        public void UpdateTimeDisplay()
        {
            duration.gameObject.SetActive(!_isRemainingTimeMode);
            elapsedTime.gameObject.SetActive(!_isRemainingTimeMode);
            separate.SetActive(!_isRemainingTimeMode);
            remainingTime.gameObject.SetActive(_isRemainingTimeMode);
        }

        /// <summary>
        /// 時間表示モードを切り替える（経過時間 <-> 残り時間）
        /// </summary>
        public void OnTimeDisplayToggle()
        {
            _isRemainingTimeMode = !_isRemainingTimeMode;

            // UIの即時更新が必要な場合はここで更新メソッドを呼びます
            UpdateTimeDisplay();
        }

        #endregion

        #region Audio Volume

        public void OnVolumeMute()
        {
            controller.Mute = true;
            ApplyVolumeMute();
        }

        public void OnVolumeUnMute()
        {
            controller.Mute = false;
            ApplyVolumeUnMute();
        }

        public void ApplyVolumeMute()
        {
            if (volumeMute != null) volumeMute.gameObject.SetActive(false);
            if (volumeUnMute != null) volumeUnMute.gameObject.SetActive(true);
            if (volumeZero != null) volumeZero.gameObject.SetActive(false);
            if (volumeMedium != null) volumeMedium.gameObject.SetActive(false);
            if (volumeLoud != null) volumeLoud.gameObject.SetActive(false);
        }

        public void ApplyVolumeUnMute()
        {
            if (volumeMute != null) volumeMute.gameObject.SetActive(true);
            if (volumeUnMute != null) volumeUnMute.gameObject.SetActive(false);
            if (volumeZero != null) volumeZero.gameObject.SetActive(false);
            if (volumeMedium != null) volumeMedium.gameObject.SetActive(false);
            if (volumeLoud != null) volumeLoud.gameObject.SetActive(false);
        }

        public void ApplyVolumeZero()
        {
            if (volumeMute != null) volumeMute.gameObject.SetActive(false);
            if (volumeUnMute != null) volumeUnMute.gameObject.SetActive(false);
            if (volumeZero != null) volumeZero.gameObject.SetActive(true);
            if (volumeMedium != null) volumeMedium.gameObject.SetActive(false);
            if (volumeLoud != null) volumeLoud.gameObject.SetActive(false);
        }

        private void ApplyVolumeMedium()
        {
            if (volumeMute != null) volumeMute.gameObject.SetActive(false);
            if (volumeUnMute != null) volumeUnMute.gameObject.SetActive(false);
            if (volumeZero != null) volumeZero.gameObject.SetActive(false);
            if (volumeMedium != null) volumeMedium.gameObject.SetActive(true);
            if (volumeLoud != null) volumeLoud.gameObject.SetActive(false);
        }

        private void ApplyVolumeLoud()
        {
            if (volumeMute != null) volumeMute.gameObject.SetActive(false);
            if (volumeUnMute != null) volumeUnMute.gameObject.SetActive(false);
            if (volumeZero != null) volumeZero.gameObject.SetActive(false);
            if (volumeMedium != null) volumeMedium.gameObject.SetActive(false);
            if (volumeLoud != null) volumeLoud.gameObject.SetActive(true);
        }

        private void VolumeInitialize()
        {
        }

        public void OnVolumeChanged()
        {
            controller.SetVolume(volumeSlider.value);

            if (controller.Mute) return;

            if (volumeSlider.value <= 0f)
                ApplyVolumeZero();
            else if (volumeSlider.value < 0.5f)
                ApplyVolumeMedium();
            else
                ApplyVolumeLoud();
        }

        #endregion

        #region Modal

        private bool isShowModal = false;

        public void ShowModal(string message, string callbackMethodName)
        {
            isShowModal = true;
        }

        public void CloseModal()
        {
        }

        public bool IsShowModal()
        {
            return isShowModal;
        }

        public void ModalClick()
        {
        }

        #endregion

        #region ViewChanged

        /// <summary>
        /// 再生中状態のUIを適用する（Pauseボタンを表示）
        /// </summary>
        private void ApplyPlayStateUI()
        {
            if (pauseButton != null) pauseButton.gameObject.SetActive(true);
            if (resumeButton != null) resumeButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// 一時停止状態のUIを適用する（Resumeボタンを表示）
        /// </summary>
        private void ApplyPauseStateUI()
        {
            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            if (resumeButton != null) resumeButton.gameObject.SetActive(true);
        }

        private void ApplyTitleUI(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        private string ReconstructTitleUI()
        {
            if (yttlBridge == null) return String.Empty;
            var str = yttlBridge.Title;
            return str;
        }

        private void StartLoading()
        {
            if (animator == null) return;
            animator.SetBool(LoadingFlag, true);
        }

        private void StopLoading()
        {
            if (animator == null) return;
            animator.SetBool(LoadingFlag, false);
        }

        private void RebuildQueueUI()
        {
            if (queueList == null || queueListUIContent == null || queuePrefab == null) return;

            var trans = queueListUIContent.transform;
            int oldCount = trans.childCount;
            for (int i = 0; i < oldCount; i++)
                Destroy(trans.GetChild(i).gameObject);

            int count = queueList.Count;
            string[] titles = queueList.Titles;
            for (int i = 0; i < count; i++)
            {
                var row = Instantiate(queuePrefab, trans);
                var text = row.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = titles[i];
                var caller = row.GetComponent<KinelQueueCall>();
                if (caller != null) caller.UiController = this;
            }
        }


        private void Lock()
        {
            SetInteractable(false);
        }

        private void UnLock()
        {
            SetInteractable(true);
        }

        private void SetInteractable(bool isInteractable)
        {
            if (pauseButton != null) pauseButton.interactable = isInteractable;
            if (resumeButton != null) resumeButton.interactable = isInteractable;
            if (previousButton != null) previousButton.interactable = isInteractable;
            if (nextButton != null) nextButton.interactable = isInteractable;
            if (inputField != null) inputField.interactable = isInteractable;
            if (playlistButton != null) playlistButton.interactable = isInteractable;
            if (inputFieldButton != null) inputFieldButton.interactable = isInteractable;
            if (mediaTypeUnityVideoButton != null) mediaTypeUnityVideoButton.interactable = isInteractable;
            if (mediaTypeStreamButton != null) mediaTypeStreamButton.interactable = isInteractable;
            if (mediaTypeImageButton != null) mediaTypeImageButton.interactable = isInteractable;
        }

        #endregion

        #region MediaType

        public void OnSelectUnityVideo()
        {
            Log("change media: UnityVideo");
            controller.SwitchMediaType(KinelMediaType.UnityVideo);
        }

        public void OnSelectStream()
        {
            Log("change media: Stream");
            controller.SwitchMediaType(KinelMediaType.AvPro);
        }

        public void OnSelectImage()
        {
            Log("change media: Image");
            controller.SwitchMediaType(KinelMediaType.Image);
        }

        private void ApplyMediaTypeUI()
        {
            var currentType = controller.NowSelectedType;
            if (mediaTypeUnityVideoButton != null)
                mediaTypeUnityVideoButton.interactable = currentType != KinelMediaType.UnityVideo;
            if (mediaTypeStreamButton != null)
                mediaTypeStreamButton.interactable = currentType != KinelMediaType.AvPro;
            if (mediaTypeImageButton != null)
                mediaTypeImageButton.interactable = currentType != KinelMediaType.Image;
        }

        #endregion

        #region Resolution

        public void OnSelectResolution144()
        {
            SelectResolution(1);
        }

        public void OnSelectResolution240()
        {
            SelectResolution(2);
        }

        public void OnSelectResolution360()
        {
            SelectResolution(3);
        }

        public void OnSelectResolution480()
        {
            SelectResolution(4);
        }

        public void OnSelectResolution720()
        {
            SelectResolution(5);
        }

        public void OnSelectResolution1080()
        {
            SelectResolution(6);
        }

        public void OnSelectResolution1440()
        {
            SelectResolution(7);
        }

        public void OnSelectResolution2160()
        {
            SelectResolution(8);
        }

        private void SelectResolution(int resolution)
        {
            Log($"resolution changed: {resolution}");
            controller.SetResolution(resolution);
            ApplyResolutionUI(resolution);
        }

        private void ApplyResolutionUI(int resolution)
        {
            if (resolution144Button != null) resolution144Button.interactable = resolution != 1;
            if (resolution240Button != null) resolution240Button.interactable = resolution != 2;
            if (resolution360Button != null) resolution360Button.interactable = resolution != 3;
            if (resolution480Button != null) resolution480Button.interactable = resolution != 4;
            if (resolution720Button != null) resolution720Button.interactable = resolution != 5;
            if (resolution1080Button != null) resolution1080Button.interactable = resolution != 6;
            if (resolution1440Button != null) resolution1440Button.interactable = resolution != 7;
            if (resolution2160Button != null) resolution2160Button.interactable = resolution != 8;
        }

        #endregion

        #region utilities

        /// <summary>
        /// serializefieldの値が正しく投入されている可能確認
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            if (controller == null)
            {
                Log("Controller is null");
                return false;
            }

            if (canvas == null)
            {
                Log("Canvas is null");
                return false;
            }

            if (animator == null)
            {
                Log("Animator is null");
                return false;
            }

            if (inputField == null)
            {
                Log("InputField is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}