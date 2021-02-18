# Video Player For VRChat

## はじめに

VRChatを対象としたビデオプレイヤーです。

## 動作環境

- VRChat SDK3(2021.01.28.19.07)
- [Udon Sharp(v0.19.2)](https://github.com/MerlinVR/UdonSharp/releases/tag/v0.19.2)
- Unity 2018.4.20f1

## 特徴
- シークバーを用いた自由な位置からの再生
- `&t=%s`の指定があるURLの場合、指定された時間からの再生
- プレイリスト
- 分かりやすいUI

## 使い方

ダウンロードしたunitypackageをUnityで展開し、プロジェクトフォルダの`Assets/Kinel/VideoPlayer/Prefab/`内にあるVideoPlayer.prefabを対象シーン内にドラッグ&ドロップしてください。
お好きな場所に設置した後、アップロードを行うのみでVRChat内で動画を見ることが出来ます。

## 前提ライブラリ等
### 必須
- Unity 2018.4.20f1
- VRChat SDK3(2021.01.28.19.07)
- [Udon Sharp(v0.19.2)](https://github.com/MerlinVR/UdonSharp/releases/tag/v0.19.2)

### 任意
- [モザイクシェーダー by pya1234様](https://booth.pm/ja/items/1703064) ※1

※1 : UIのデザインで用いている箇所があるため記載しています。主機能を用いるためには必要ないため任意の導入になります。このシェーダーを用いる場合はPrefabフォルダ内の`Blur`と名前が付いているObjectを使用します。

## 導入方法

1. 最新のVRChat SDK3とUdon SharpをUnityにインポートする。
2. Releaseから最新のunitypackageをダウンロードしUnityにインポートする。
3. `Assets/Kinel/VideoPlayer/Prefab/`内の`KinelVideoPlayer`を対象シーン内にドラッグアンドドロップし、位置を揃えます。
4. アップロード。

## 注意点
- VRChatの同期の性質上、プレイヤーが入力したURL文字列の長さが一定以上を超えるとエラーが出力されるように設定されています。現在その条件は85文字以上に設定されています。

## Questについて
- このプレイヤーに用いられているVideoPlayre ComponentはQuestに対応しており、このプレイヤーシステム自体もQuestに対応しておりますが、QuestがAndroid用にエンコードされた動画に対して直リンクのみをサポートしているためYoutubeやその他のサイトに投稿されている動画を見られないことがあります。
- 回避策として直リンクを作成可能なGoogleDriveなどに動画を配置し、リンクを取得することで動画を視聴することが出来ます。(例としてSDK3向けのイベントカレンダー等が挙げられます。)
  - 但し、注意点にも書いてある通り同期に制限があるため、URLが長い動画に関してはUnity上でのアップロード時点でのURL埋め込みが必要です。

## 改変について
- このビデオプレイヤーで実装されている機能は基本的に各GameObject上にアタッチされているComponentで処理されています。比較的分かりやすいように設置していますが処理の都合上各GameObjectが連結されている機能もあるためお気を付けください。
- `Kinel/UI/Icon/Video`内の画像はGoogleの[Material Icons](https://material.io/resources/icons/)です。licenseが異なるため再配布する際はお気を付けください。

## ライブラリ等
- [M+ フォント](https://mplus-fonts.osdn.jp/about.html)
    - 各UI表示フォントとして使用しています。
- [Material Icons](https://material.io/resources/icons/)
    - 各UI表示用に使用しており、[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)を適用しています

## License
- Mit License
  