# Video Player For VRChat v2.1.2

## はじめに

VRChatを対象としたビデオプレイヤーです。

## 更新履歴
- [[2.1.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.0) 再生速度変更のサポート
- [[2.1.1]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.1) 各種不具合の修正
- [[2.1.2]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.2) Editorに関わる不具合の修正

## 動作環境

v2.1.2
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
- このプレイヤーに用いられているVideoPlayer ComponentはQuestに対応しており、このプレイヤーシステム自体もQuestに対応しておりますが、動画に対して直リンクのみをサポートしているためYoutubeやその他のサイトに投稿されている動画を見ることが出来ない場合があります
- 回避策として直リンクを作成可能なGoogleDriveなどに動画を配置し、リンクを取得することで動画を視聴することが出来ます。(例としてSDK3向けのイベントカレンダー等が挙げられます。)
  - 但し、注意点にも書いてある通り同期に制限があるため、URLが長い動画に関してはUnity上でのアップロード時点でのURL埋め込みが必要です。(2/26 短縮URLを使うと再生できることを確認しました。2021.02.23.11.40)
- Questの仕様上、文字入力は難しいのでPlaylistで初期で入力しておくかPCユーザーがURLを入力する必要があります。
- デスクトップやVRで見ることのできるデバッグ画面にてYoutube-dlが出力したURLを短縮しURLとして貼り付けることで再生できることを確認しました。なお現在動作確認を行ったサイトはYoutubeのみです。(2/26 2021.02.23.11.40)
  - [VRChatのデバッグを有効](https://vrcworld.wiki.fc2.com/wiki/%E3%83%87%E3%83%90%E3%83%83%E3%82%B0%E3%81%AB%E4%BD%BF%E3%81%88%E3%82%8B%E6%8A%80%E8%A1%93)にした状態で``Shift + ` + 3``を押すことでデバッグ画面を出すことが出来ます。
  -   `` [Video Playback] URL '<入力したURL>' resolved to '<動画に対する直リンク>'``
  -   デバッグ画面に動画を入力後上記のようなメッセージが出力されるので`C:\Users\<ユーザー名>\AppData\LocalLow\VRChat\VRChat`に存在する`output_log_<日付>.txt`からコピーを行い、動画プレイヤーに貼付けし再生します。
  -   するとQuestでもURLが同期され再生することが出来ます。
  -   

- Youtube-dlの直リンクを用いた場合はURLの都合上数時間程度でリンクが切れるため長期の利用の際は従来の方法を用いる必要があります

- 今後の更新で対応できるようシステム開発中です。

## 改変について
- このビデオプレイヤーで実装されている機能は基本的に各GameObject上にアタッチされているComponentで処理されています。比較的分かりやすいように設置していますが処理の都合上各GameObjectが連結されている機能もあるためお気を付けください。
- `Kinel/UI/Icon/Video`内の画像はGoogleの[Material Icons](https://material.io/resources/icons/)です。licenseが異なるため再配布する際はお気を付けください。
- 詳しい動画の資料についてはgithubのwikiを参照ください。

## ライブラリ等
- [Material Icons](https://material.io/resources/icons/)
    - 各UI表示用に使用しており、[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)を適用しています
- [AAChair](https://github.com/AoiKamishiro/VRChatPrefabs/blob/master/Assets/00Kamishiro/AAChair/AAChair-README_JP.md) by 神城工業
  - Editor拡張について参考にさせていただきました。

## License
- Mit License

## 何かわからない時
- BoothかGithubなどにてDMを投げて頂けると助かります。。。！
- Twitter: [@ni_rilana](https://twitter.com/ni_rilana)