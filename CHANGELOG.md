# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [3.2.0] - 2026-05-28
### Added
- audio moduleを追加 [#167](https://github.com/niwaniwa/KineLVideoPlayer/issues/167)
- Tooltipを追加

### Fixed
- UIを非表示時に、PlaylistのUIなどを押せてしまい誤動作してしまう問題を修正 [#161](https://github.com/niwaniwa/KineLVideoPlayer/issues/161)


## [3.1.0] - 2026-05-25
### Fixed
- Loopモードの修正
  - Playlistを考慮していなかったが、正しくプレイリスト内の動画をループするように
  - Loopを別モジュールに切り出して安定化

## [3.0.1] - 2026-05-20
### Added
- モジュールを一覧・編集するエディタ拡張を追加
- Join時に自動再生設定できるモジュールを追加
- Queueプレイリストに同期処理を追加 ([#150](https://github.com/niwaniwa/KineLVideoPlayer/issues/150))

### Fixed
- UI 向け MSDF シェーダ (`Kinel/MSDF`) で `_ClipRect` 未宣言によりコンパイルエラーが発生する問題を修正 ([#145](https://github.com/niwaniwa/KineLVideoPlayer/issues/145))
- PlaylistのEditorにおいてTrackを手入力しようとしたときに正しく値が反映されない問題を修正 [#152](https://github.com/niwaniwa/KineLVideoPlayer/issues/152)
- 解像度設定変更時、プレイしていない状況でもReloadされてしまう問題を修正 [#149](https://github.com/niwaniwa/KineLVideoPlayer/issues/149)

## [3.0.0] - 2026-05-08

### Added
- v3 アーキテクチャへの移行 (`KineLLocalVideoPlayer` をベースに `v3/` 配下へ実装)
  - Prefab `v3/KineLVideoPlayer-v3.prefab` を追加
  - 同期システムを `KinelVariableSyncer` として独立
  - Listener と controller を分離 (v2 は互換維持のため残置、移行用スクリプトを今後提供)
- AB Loop 機能 (`KinelABLoop` + UI + `KinelMinMaxSlider`)
- ローカル再生時間オフセット機能
- Editor 用基底クラス (`BaseKinelEditor` / `BaseKinelVideoPlayerEditor`)

### Changed
- プレイリストの EditorWindow を Tools 配下に移動
- `Udon.Interface` 内の namespace `Module` を `Interface` に統一
- 速度変更ロジックを堅牢化
- `Kinel.VideoPlayer.V3.Editor` asmdef から v2 Editor (`Kinel.VideoPlayer.Editor`) への参照を独立

### Fixed
- v3 移行直後の prefab とシーンの不整合を整理
  - `v3/KineLVideoPlayer-v3.prefab` の Udon をアップグレード
  - `v3/Runtime/KinelVideoPlayer-V3.unity` から不要参照・オブジェクトを削除
- `v3/` フォルダの .meta を追加
- v3 ソースコード内のコメント文字化けを解消
- `PlaylistEditorWindow` の不要コメントを整理

### Removed
- AVProVideo Trial 版の自動ダウンロード機構 (ライセンス安全性のため)。AVPro は別途 RenderHeads から取得し、Tools メニューの "Setup AvProVideo" でシンボル有効化する運用に変更
- iwaSync 連携の残骸 (関連スクリプト・prefab フィールド・USS スタイル)
- 未使用の `KinelInputModule`
- `Network` フォルダ内の不要な yttl loader

## 2.5.5 - 2023-10-14
### Fixed
- Delete unused using state. [`#120`](https://github.com/niwaniwa/KineLVideoPlayer/issues/120)

## 2.5.4 - 2023-10-04
### Added
- Added LegacyFolders setting. [`#118`](https://github.com/niwaniwa/KineLVideoPlayer/issues/118)

### Fixed
- Fixed Editor set dirty. [`#116`](https://github.com/niwaniwa/KineLVideoPlayer/issues/116)

## 2.5.3 - 2023-09-23
### Fixed
- Fixed default mirror inversion setting. [`#114`](https://github.com/niwaniwa/KineLVideoPlayer/issues/114)

## 2.5.2 - 2023-09-14
### Fixed
- Fixed mirror inversion. [`#112`](https://github.com/niwaniwa/KineLVideoPlayer/issues/112)

## 2.5.1 - 2023-09-12
### Fixed
- Fixed udon asmdef reference. [`#108`](https://github.com/niwaniwa/KineLVideoPlayer/issues/108)
- Fixed PlaylistAPI URL. [`#109`](https://github.com/niwaniwa/KineLVideoPlayer/issues/109)

## 2.5.0 - 2023-09-07
### Changed
- Change to VPM. [`#56`](https://github.com/niwaniwa/KineLVideoPlayer/issues/56)

## 2.4.3 - 2023-05-23
### Fixed
- Avoid errors during upload. [`#96`](https://github.com/niwaniwa/KineLVideoPlayer/pull/96)

## 2.4.2 - 2023-05-08
### Fixed
- Problem with video not appearing on screen during playback [`#92`](https://github.com/niwaniwa/KineLVideoPlayer/issues/92)