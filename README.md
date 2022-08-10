# Video Player For VRChat v2.1.3

## はじめに

VRChatを対象としたビデオプレイヤーです。

## 更新履歴
- [[2.1.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.0) 再生速度変更のサポート
- [[2.1.1]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.1) 各種不具合の修正
- [[2.1.2]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.2) Editorに関わる不具合の修正
- [[2.1.3]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.3) スピード関係の挙動を変更
- [2.1.4] AudioSourceがアタッチされていなかった問題を修正
- [[2.2.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.2.0) PlaylistをImportする際にPrefixを追加できるように修正


## 動作環境

v2.2.0
- VRChat SDK3(最新)
- Udon Sharp(最新)
- Unity 2019.4.31f1

VCC(VRChat Creator Companion)には現在対応していません。

## 特徴
- シークバーを用いた自由な位置からの再生
- プレイリスト(Tab着き有り)
- 分かりやすいUI
- Youtubeのplaylistから動画情報をImportする機能

## 使い方

- ダウンロードしたunitypackageをUnityで展開し、プロジェクトフォルダの`Assets/Kinel/VideoPlayer/`直下にあるVideoPlayer.prefabを対象シーン内にドラッグ&ドロップしてください。
お好きな場所に設置した後、アップロードを行うのみでVRChat内で動画を見ることが出来ます。

- 更新する際は最新のunitypackageをそのまま上書きするのみで導入できます。但し念の為バックアップすることを強くおすすめします。
<重要>
- v1.1.3などからアップデータの際はファイルの競合の観点から一度`Assets/Kinel/VideoPlayer/`フォルダを削除してからImportしてください。

## 前提ライブラリ等
### 必須
- Unity 2019.4.31f1
- VRChat SDK3
- Udon Sharp
SDKなどは全て最新版を導入してください。

## 導入方法

1. 最新のVRChat SDK3とUdon SharpをUnityにインポートする。
2. Releaseから最新のunitypackageをダウンロードしUnityにインポートする。
3. `Assets/Kinel/VideoPlayer/`内の`KinelVideoPlayer`を対象シーン内にドラッグアンドドロップし、位置を揃えます。
4. アップロード。

## 注意点
- 稀にPlaylistやdefaultUrlなどでデータが反映されない問題があります。抑制していますが発生する場合は何かしらの項目をOnOffすることやテキストフィールドで編集することで改善する場合があります。


## Questについて
- [こちら](https://github.com/niwaniwa/KineLVideoPlayer/wiki/Quest%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6)を参考にしてください。

## 改変について
- このビデオプレイヤーで実装されている機能は基本的に各GameObject上にアタッチされているComponentで処理されています。比較的分かりやすいように設置していますが処理の都合上各GameObjectが連結されている機能もあるためお気を付けください。
- `Kinel/UI/Icon/Video`内の画像はGoogleの[Material Icons](https://material.io/resources/icons/)です。licenseが異なるため再配布する際はお気を付けください。
- 詳しい動画の資料についてはgithubのwikiを参照ください。

## ライブラリ等
- [Material Icons](https://material.io/resources/icons/)
    - 各UI表示用に使用しており、[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)を適用しています

## リファレンス
- [AAChair](https://github.com/AoiKamishiro/VRChatPrefabs/blob/master/Assets/00Kamishiro/AAChair/AAChair-README_JP.md) by 神城工業
  - アイコン表示用にEditor拡張について使用させていただきました。

## License
- Mit License

## 何かわからない時
- BoothかGithubなどにてDMを投げて頂けると助かります。。。！
- Twitter: [@ni_rilana](https://twitter.com/ni_rilana)