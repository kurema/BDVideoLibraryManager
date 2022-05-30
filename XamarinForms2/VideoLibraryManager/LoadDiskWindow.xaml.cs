using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VideoLibraryManager
{
    /// <summary>
    /// LoadDiskWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoadDiskWindow : Window
    {
        public MainWindow ParentMainWindow { get; set; }

        public LoadDiskWindow()
        {
            InitializeComponent();

            var drives= GetAvailableDrives();
            ComboBoxDrives.ItemsSource = drives;
            if (drives.Any()) ComboBoxDrives.SelectedIndex = 0;
        }

        public IEnumerable<DriveInfo> GetAvailableDrives()
        {
            return DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Object selected= ComboBoxDrives.SelectionBoxItem;
            if (selected == null || !(selected is DriveInfo)) return;
            var sel = (DriveInfo) selected;
            if (sel == null) return;

            var infoBdav = System.IO.Path.Combine(sel.Name, "BDAV", "info.bdav");
            if (!File.Exists(infoBdav))
            {
                WriteLine("ファイルがありません：" + infoBdav);
                return;
            }

            var playlist = System.IO.Path.Combine(sel.Name, "BDAV", "PLAYLIST");
            if (!Directory.Exists(playlist))
            {
                WriteLine("フォルダがありません：" + playlist);
                return;
            }

            var diskName = TextBoxDiscName.Text;
            if (String.IsNullOrWhiteSpace(diskName))
            {
                WriteLine("無効な名前です。");
                return;
            }
            var csvName = System.IO.Path.Combine("csv", diskName + ".csv");
            if (File.Exists(csvName))
            {
                WriteLine("ディスク名が登録済みです。");
                return;
            }
            foreach (var item in System.IO.Path.GetInvalidFileNameChars())
            {
                if (diskName.Contains(item))
                {
                    WriteLine("ディスク名に禁止文字が含まれています。："+item);
                    return;
                }
            }

            var p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
            p.StartInfo.FileName = "bdavinfo.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.Arguments = infoBdav +" \""+csvName+ "\" -D -C -fjkdtpzaocnsbieg";
            p.StartInfo.CreateNoWindow = true;
            p.OutputDataReceived += (s, e2) =>
            {
                if (e2.Data == null) return;
                WriteLine(e2.Data);
            };
            p.ErrorDataReceived += (s, e2) =>
            {
                if (e2.Data == null) return;
                WriteLine("エラー：" + e2.Data);
            };
            p.Exited += (s, e2) =>
            {
                WriteLine("完了しました。");
                Dispatcher.Invoke(() =>
                {
                    ParentMainWindow?.Init();
                });
            };
            p.EnableRaisingEvents = true;
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

        }

        public void WriteLine(string text)
        {
            Dispatcher.Invoke(() =>
            {
                TextBoxOutput.Text += text + Environment.NewLine;
                TextBoxOutput.Focus();
                TextBoxOutput.ScrollToEnd();
            });
        }
    }
}
