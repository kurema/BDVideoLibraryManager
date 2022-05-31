custom.aprofは「スタートアップトレース」という起動高速化技法に関するものです。
詳しくは以下。

* [スタートアップトレースを使ってXamarin.Androidの起動速度を向上させる](https://zenn.dev/muak/articles/d364cb4ee7a890)

# ポートを変更する場合
今回デフォルトのポート9999が取得できなかったので、別ポートに変更しました。
その際、以下のようにBuildとFinishのどちらでもポートを指定しないといけない点に注意してください。
ビルド時に指定ポートを埋め込むようです。

```
$ msbuild /t:BuildAndStartAotProfiling /p:AndroidAotProfilerPort=12345
$ msbuild /t:FinishAotProfiling xxxxx.csproj /p:AndroidAotProfilerPort=12345
```
