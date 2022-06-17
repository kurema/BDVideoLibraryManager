# サブセットフォント
## 現在使用中の文字
| code | 説明 |
| -- | -- |
| e8b6 | 虫眼鏡 |
| e14d | コピー |
| e80d | 共有 |
| e89e | 開く |

##コマンド
https://stackoverflow.com/questions/64614572/creating-a-material-icons-subset

```
fonttools subset MaterialIcons-Regular.ttf --unicodes=e8b6,e14d,e80d,e89e --no-layout-closure --output-file=subset.ttf
```