円形アイコンがadaptive-iconでは適切に処理されないため、このフォルダ内の`icon.xml`と`icon_round.xml`は「プロジェクトから除外」しています。
有効化する場合は再び追加して(`*.csproj`を直接編集するのが速そう)、以下に設定するのが良いと思います。

| 項目 | 値 |
| -- | -- |
| カスタムツール | MSBuild:UpdateGeneratedFiles |
| ビルドアクション | AndroidResource |

と思いましたがやっぱり辞めました。エミュレータのPixelだと微妙だったので。
妥協として背景を白系に。