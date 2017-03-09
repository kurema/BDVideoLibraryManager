set /P DISK_NAME="ディスク番号: "
mkdir %DISK_NAME%
cd %DISK_NAME%
copy E:\BDAV\info.bdav .
mkdir PLAYLIST
copy E:\BDAV\PLAYLIST PLAYLIST\.
