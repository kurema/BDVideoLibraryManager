# BDVideoLibraryManager
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

## Known Issues
現在以下の問題が確認されています。
* Android版:リモートファイルをコピー中でもおそらく終了できる。直す気はない。
* Android版:終了時にエラーで落ちる。(Xamarin Formsのバグ[1](https://forums.xamarin.com/discussion/81793/back-button-from-causes-crash-on-android-when-page-is-masterdetail)[2](https://bugzilla.xamarin.com/show_bug.cgi?id=46494)。)そのうち治ると期待。
* UWP版:文句を言われたので除外しましたが、おそらく動くと思います(nugetの再設定などは必要？)。
* Visual Studio:XAMLの補助が効きません。いくつかエラーが出ます。コンパイルは通ります。
* Visual Studio:Android版をコンパイル中にパス名が長すぎるてコンパイルできない事があります。素直にC直下あたりに移動しましょう。

現状で満足しているので、今後改良する気はあまりありません。

## Thanks
以下の協力を以て本ツールは作られています。
* BD-AVに含まれる情報の取得には Vesti La Giubba様の[bdavinfo](http://saysaysay.net/bdavtool/bdavinfo)を利用しています。
* CSVファイルの解析にはJoshClose様のCsvHelperを利用しています。
* SMBへのアクセスには Do-Be's様のSharpCifs.Stdを利用しています。
* ファイル保存にはdsplaisted様のPCLStorageを利用しています。
* 全体的にMicrosoftのおかげでいろいろと動いています。
