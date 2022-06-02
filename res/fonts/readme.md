# サブセットフォント
## 現在使用中の文字
| code | 説明 |
| -- | -- |
| e8b6 | 虫眼鏡 |
| e14d | コピー |

##コマンド
https://stackoverflow.com/questions/64614572/creating-a-material-icons-subset

```
fonttools subset MaterialIcons-Regular.ttf \
    --unicodes=e8b6,e14d \
    --no-layout-closure \
    --output-file=subset.ttf
```