mkdir csv
for /D %%f in (*) do bdavinfo.exe %%f\info.bdav csv\%%f.csv -D -C -fjkdtpzaocnsbieg
