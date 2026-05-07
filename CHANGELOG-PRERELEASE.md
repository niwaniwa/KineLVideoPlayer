# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added

### Changed
- プレイリストのEditorWindowをtools配下に移動

### Deprecated

### Removed
- AVProVideo Trial版の自動ダウンロード機構を削除。AVPro は別途 RenderHeads から取得し、Tools メニューの "Setup AvProVideo" でシンボルを有効化する運用に変更。
- iwasync由来の参照を削除し正常に動作するように

### Fixed

### Security

## [3.0.0-beta.5] - 2026-05-07

### Changed
- `PlaylistEditorWindow` の不要コメントを整理

### Fixed
- v3 移行直後の Prefabとシーンの不整合を整理
  - `v3/KineLVideoPlayer-v3.prefab`のudonをアップグレード
  - `v3/Runtime/KinelVideoPlayer-V3.unity` から不要参照・オブジェクトを削除
- `Kinel.VideoPlayer.V3.Editor` asmdef更新
  - v2 Editor (`Kinel.VideoPlayer.Editor`) への参照を削除
- `v3/` フォルダの .meta を追加


## [3.0.0-beta.4] - 2026-05-06
### Added
-  `KineLLocalVideoPlayer`をベースに v3 として `v3/`に移行。
  - Prefab `v3/KineLVideoPlayer-v3.prefab`を追加

- 機能実装
  - ローカルで再生時間をオフセット出来るように
  - AB Loop（`KinelABLoop` + UI + `KinelMinMaxSlider`）

### Changed 
- 同期システムを分離 (`KinelVariableSyncer`)
- 速度変更を堅牢化
- Listenerとcontrollerを分離
  - v2に関しては互換性維持のため残しておく。今後移行用スクリプトを提供


## [3.0.0-beta.3] - 2026-03-10

- unreleased

## v3.0.0-beta.2 - 2026-03-11

### Changed
- Bump up from KineL v3.0.0-beta.1

## v3.0.0-beta.1 - 2026-03-10

### Changed
- Bump up from KineL 2.5.5