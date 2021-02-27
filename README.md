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

※1 : UIのデザインで用いている箇所があるため記載しています。主機能を用いるためには必要ないため任意の導入になります。このシェーダーを用いる場合はPrefabフォルダ内の`Blur`と名前が付いているPrefabを使用します。

## 導入方法

1. 最新のVRChat SDK3とUdon SharpをUnityにインポートする。
2. Releaseから最新のunitypackageをダウンロードしUnityにインポートする。
3. `Assets/Kinel/VideoPlayer/Prefab/`内の`KinelVideoPlayer`を対象シーン内にドラッグアンドドロップし、位置を揃えます。
4. アップロード。

## 注意点
- VRChatの同期の性質上、プレイヤーが入力したURL文字列の長さが一定以上を超えるとエラーが出力されるように設定されています。現在その条件は85文字以上に設定されています。

## Questについて
- このプレイヤーに用いられているVideoPlayre ComponentはQuestに対応しており、このプレイヤーシステム自体もQuestに対応しておりますが、動画に対して直リンクのみをサポートしているためYoutubeやその他のサイトに投稿されている動画を見ることが出来ない場合があります
- 回避策として直リンクを作成可能なGoogleDriveなどに動画を配置し、リンクを取得することで動画を視聴することが出来ます。(例としてSDK3向けのイベントカレンダー等が挙げられます。)
  - 但し、注意点にも書いてある通り同期に制限があるため、URLが長い動画に関してはUnity上でのアップロード時点でのURL埋め込みが必要です。(2/26 短縮URLを使うと再生できることを確認しました。2021.02.23.11.40)
- Questの仕様上、文字入力は難しいのでPlaylistで初期で入力しておくかPCユーザーがURLを入力する必要があります。
- デスクトップやVRで見ることのできるデバッグ画面にてYoutube-dlが出力したURLを短縮しURLとして貼り付けることで再生できることを確認しました。なお現在動作確認を行ったサイトはYoutubeのみです。(2/26 2021.02.23.11.40)
  - [VRChatのデバッグを有効](https://vrcworld.wiki.fc2.com/wiki/%E3%83%87%E3%83%90%E3%83%83%E3%82%B0%E3%81%AB%E4%BD%BF%E3%81%88%E3%82%8B%E6%8A%80%E8%A1%93)にした状態で``Shift + ` + 3``を押すことでデバッグ画面を出すことが出来ます。
  -   `` [Video Playback] URL '<入力したURL>' resolved to '<動画に対する直リンク>'``
  -   デバッグ画面に動画を入力後上記のようなメッセージが出力されるので`C:\Users\<ユーザー名>\AppData\LocalLow\VRChat\VRChat`に存在する`output_log_<日付>.txt`からコピーを行い、動画プレイヤーに貼付けし再生します。
  -   するとQuestでもURLが同期され再生することが出来ます。
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
  