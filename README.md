# BDVideoLibraryManager
## Access
[Androidアプリ](https://play.google.com/store/apps/details?id=com.github.kurema.BDVideoLibraryManager)
## about
このツールは以下の3つの機能を含みます。
* BD-AV形式のBlu-ray Discに含まれる録画内容をディスク毎にcsv化するツール。
* それらの情報をWindowsデスクトップ上で確認・検索するソフト。
* Xmarin Formsを利用し、Android上からSMB共有されたcsvファイルを確認・検索するアプリ。

基本的に私個人が利用する目的であり、汎用性の点で問題がある点も見受けられます。
必要ならば適宜改造したうえで利用してください。
* DIGAの特定機種を前提にしている。
* 日本語での利用しか想定していない。
* Macを持っていないのでXamarin Formsにもかかわらず、Androidでの動作しか確認していない。
* 拡張性は軽視した部分がある。

## How to get
上のreleaseをクリックしてWindows版やAndroid版、ツールなどを入手します。  
Androidでapkファイルを開くには設定が必要です。

## Use case
1. パソコンにBDドライブを接続して管理情報を取り込む。
2. bdavinfo.exeを利用してcsvに変換する。
3. 当該フォルダをファイル共有するかNASに転送する。
4. PC上やAndroidから管理情報を見る(SMB経由。csvのまま)。
5. 見たい映像が含まれるディスクを確認して、探して、見る。

より詳細にWindows上での作業を説明します。

1. Toolsをダウンロードして適当なフォルダに展開(releases→Tools.zip)。
2. フォルダ内の"copy.bat"を編集。E:¥を実際のBDドライブのドライブレターに書き換える。
3. ドライブにBDを一枚づつ挿入。その度にcopy.batを実行する(挿入から読み込みまで多少の時間を要します)。
4. 全てのBDドライブを読み込んだらrpls2csv.batを実行する。
5. できたcsvフォルダを適当な場所に移動して、共有フォルダやNASのCIFS/SMB機能でLan内に公開。
6. csv/の上層フォルダにWindows版(releases→Windows_x64.zipを展開)を配置。これでWindowsで閲覧ができる。
7. あればAndroid版をインストール。設定を選び、サーバー名・パス・ユーザー名・パスワードを設定。これでキャッシュされる。データ更新時には、一覧画面でプルダウンすれば再ダウンロード可能。

## Known Issues
現在以下の問題が確認されています。
* Android版:リモートファイルをコピー中でもおそらく終了できる。直す気はない。容量が小さいのでどうせすぐ終わります。
* ~~Android版:終了時にエラーで落ちる。(Xamarin Formsのバグ[1](https://forums.xamarin.com/discussion/81793/back-button-from-causes-crash-on-android-when-page-is-masterdetail)[2](https://bugzilla.xamarin.com/show_bug.cgi?id=46494)。)そのうち治ると期待。~~　最新ベータ版のXamarin Formsでは問題は発生しませんでした。
* iOS:動作は期待できるが試してはいない。アイコンや名前などの設定はしていない。
* UWP版:ライブラリの関係上動作しません。
* ~~Visual Studio:XAMLの補助が効きません。いくつかエラーが出ます。コンパイルは通ります。~~
* Visual Studio:Android版をコンパイル中にパス名が長すぎるてコンパイルできない事があります。素直にC直下あたりに移動しましょう。
* ジャンルわけ機能とかはDIGAを前提にしている。一部適切に動作しない、常に無効値が表示されるなどの問題が予想されます。
* CSVの列名ではなく、順番固定で解釈しています。これはディスクタイトルを取得する設定にしたからです。
* 端末のフォントサイズによっては期待通りの表示にならない。フォントサイズを固定にすれば対策できますがやめました。

現状で満足しているので、今後改良する気はあまりありません。時間をかけずに作ったので、適当な部分もあります。使っていて落ちたことはないのでかなり安定していると思いますが、csvファイルに問題があればうまく動作しないと思われますので、適宜ファイルの修正・再取得・再変換・Androidではデータの削除などを行ってください。

ニーズは薄いと思っているのでFTP/SFTP/WebDav共有対応は考えてはいませんが、そのうち対応する可能性もあるかもしれません。あるいは誰かがやってください。

## Screenshot
Android:
![screenshot android 1](res/screenshot/01.png)
![screenshot android 2](res/screenshot/02.png)
![screenshot android 3](res/screenshot/03.png)
![screenshot android 4](res/screenshot/04.png)
![screenshot android 5](res/screenshot/05.png)
![screenshot android 6](res/screenshot/06.png)
Windows:
![screenshot windows 1](res/screenshot/desktop01.png)
![screenshot windows 2](res/screenshot/desktop02.png)


## Thanks
以下の協力を以て本ツールは作られています。
* BD-AVに含まれる情報の取得には Vesti La Giubba様の[bdavinfo](http://saysaysay.net/bdavtool/bdavinfo)を利用しています。
* CSVファイルの解析にはJoshClose様のCsvHelperを利用しています。
* SMBへのアクセスには Do-Be's様のSharpCifs.Stdを利用しています。
* ファイル保存にはdsplaisted様のPCLStorageを利用しています。
* 全体的にMicrosoftのおかげでいろいろと動いています。
