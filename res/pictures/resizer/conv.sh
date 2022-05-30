#/bin/sh
mkdir mipmap-mdpi
convert -resize 48x48 org.png mipmap-mdpi/icon.png
convert -resize 80x80 -background transparent -gravity center -extent 108x108 org.png mipmap-mdpi/launcher_foreground.png
mkdir mipmap-hdpi
convert -resize 72x72 org.png mipmap-hdpi/icon.png
convert -resize 120x120 -background transparent -gravity center -extent 162x162 org.png mipmap-hdpi/launcher_foreground.png
mkdir mipmap-xhdpi
convert -resize 96x96 org.png mipmap-xhdpi/icon.png
convert -resize 160x160 -background transparent -gravity center -extent 216x216 org.png mipmap-xhdpi/launcher_foreground.png
mkdir mipmap-xxhdpi
convert -resize 144x144 org.png mipmap-xxhdpi/icon.png
convert -resize 240x240 -background transparent -gravity center -extent 324x324 org.png mipmap-xxhdpi/launcher_foreground.png
mkdir mipmap-xxxhdpi
convert -resize 192x192 org.png mipmap-xxxhdpi/icon.png
convert -resize 320x320 -background transparent -gravity center -extent 432x432 org.png mipmap-xxxhdpi/launcher_foreground.png
