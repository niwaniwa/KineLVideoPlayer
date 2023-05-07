# Video Player For VRChat

## はじめに

VRChatを対象としたビデオプレイヤーです

## 特徴
特徴
KineL式ビデオプレイヤーは以下のような特徴があります！

✅Youtubeなどのオンラインビデオサービスの動画再生機能

✅直感的なシークバーでの操作

✅再生速度・解像度変更

✅わかりやすいユーザーインターフェース

✅プレイリスト機能
🔍 Youtubeからのプレイリストのインポート機能
🔍 VRC内から編集できるプレイリスト

✅Quest対応

✅YoutubeLiveなどのストリーミングに対応
🔍 VRCDN,TopazChatにも対応しています

✅ミラー内の動画反転システム

などなど...!

## 使い方
- ダウンロードしたunitypackageをUnityで展開し、プロジェクトフォルダの`Assets/Kinel/VideoPlayer/`直下にあるVideoPlayer.prefabを対象シーン内にドラッグ&ドロップしてください。
お好きな場所に設置した後、アップロードを行うのみでVRChat内で動画を見ることが出来ます。

- 更新する際は最新のunitypackageをそのまま上書きするのみで導入できます。但し念の為バックアップすることを強くおすすめします。

## 動作環境
🟦 VRChat SDK3(VCC)
🟦 Udon Sharp(VCC)
🟧 Unity 2019.4.31f1

VCC以外での導入は非推奨となりました。

## 導入方法

1. 最新のVRChat SDK3とUdon SharpをUnityにインポートする。
2. Releaseから最新のunitypackageをダウンロードしUnityにインポートする。
3. `Assets/Kinel/VideoPlayer/`内の`KinelVideoPlayer`を対象シーン内にドラッグアンドドロップし、位置を揃えます。
4. アップロード。

## マニュアル
導入方法や詳しい設定などについては以下のURLからお願いいたします。
https://github.com/niwaniwa/KineLVideoPlayer/wiki

## ライブラリ等
- [Material Icons](https://material.io/resources/icons/)
    - 各UI表示用に使用しており、[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)を適用しています

## 使用ライブラリ等
🟦Material Icons
　・ 各UI表示用に使用しており、Apache License 2.0が適用されています。

🟦 AAChair by 神城工業
・ Editor拡張について参考にさせていただきました。

## License
- Mit License

アセットに組み込んでの配布の際には、既にLicenseフォルダ内とREADME内に記述されているため、そのまま気にすることなく配布できます。

## 何かわからない時
- BoothかGithubなどにてDMやIssueを立てていただけると幸いです。
- Twitter: [@ni_rilana](https://twitter.com/ni_rilana)

## 制作・謝辞など
🟦スクリーン用シェーダー制作
にしおかすみす！さん @nsokSMITHdayo

🟦アドバイスなど
緋狐さん @aki_lua87

デバッグに協力していただいたVRCのみなさん

大変ありがとうございました！


## 更新履歴
- [[2.1.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.0) 再生速度変更のサポート
- [[2.1.1]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.1) 各種不具合の修正
- [[2.1.2]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.2) Editorに関わる不具合の修正
- [[2.1.3]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.1.3) スピード関係の挙動を変更
- [2.1.4] AudioSourceがアタッチされていなかった問題を修正
- [[2.2.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.2.0) PlaylistをImportする際にPrefixを追加できるように修正
- [2.2.1] Stream時に後から人がJoinするとOwnerが切り替わってしまう問題を修正
- [[2.3.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.3.0) 動画解像度の変更を実装しました。
- [[2.4.0]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.4.0) 
  - Editor拡張について適切な動作をするよう修正
  - アップロード時にエラーが発生する問題を低減
  - スクリーンに何らかの原因で不正な画像が存在する問題を修正
  - 内部リファクタ
- [[2.4.1]](https://github.com/niwaniwa/KineLVideoPlayer/releases/tag/2.4.1) Androidビルド時のエラーを修正

2.4.2以降はCHANGELOG.mdを参照してください。