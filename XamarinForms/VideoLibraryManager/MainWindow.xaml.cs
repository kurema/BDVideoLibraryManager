using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using VideoLibraryManagerCommon.Library;

namespace VideoLibraryManager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Library Library;

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        public void Init()
        {
            var d = new Queue<DiskBD>();

            if (!System.IO.Directory.Exists(@".\csv"))
            {
                System.IO.Directory.CreateDirectory(@".\csv");
            }

            foreach (var f in System.IO.Directory.GetFiles(@".\csv"))
            {
                if (System.IO.Path.GetExtension(f) == ".csv")
                {
                    using (var fr = new System.IO.StreamReader(f))
                    {
                        d.Enqueue(new DiskBD(fr, System.IO.Path.GetFileNameWithoutExtension(f)));
                    }
                }
            }
            Library = new Library(d.ToArray());

            ComboBoxGenres.Items.Add("全て");
            ComboBoxGenres.SelectedIndex = 0;
            foreach (var genre in Library.Genres)
            {
                ComboBoxGenres.Items.Add(genre);
            }

            ExecuteSearch("", "", SearchType.全て);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSearch(TextBoxSearchText.Text, (string)ComboBoxGenres.SelectedItem, (SearchType)ComboBoxSearchType.SelectedItem);
            //DataGridSearchResult.ItemsSource = Library.Contents.Where(a => a.Contents.Any(b => b.ProgramTitle.Contains(TextBoxSearchText.Text)));
        }


        public void ExecuteSearch(string searchText,string searchGenre ,SearchType searchType)
        {
            var queue = new DiskVideoPairList();
            var normalStrs = VideoBD.NormalizeText(searchText).Split(' ');

            foreach (var disk in Library.Contents)
            {
                IEnumerable<VideoBD> videos = disk.Contents;
                foreach (var word in normalStrs)
                {
                    videos = videos.Where(b => {
                        string text = "";
                        switch (searchType)
                        {
                            case SearchType.全て: text = b.ProgramTitleNormalized + " " + b.ProgramDetailNormalized; break;
                            case SearchType.番組名: text = b.ProgramTitleNormalized; break;
                            case SearchType.番組紹介: text = b.ProgramDetailNormalized; break;
                            case SearchType.ディスク番号: text = disk.DiskName + " " + disk.DiskTitle; break;
                        }
                        return ContainsAmbiguous(text,word);
                    });
                }
                if (ComboBoxGenres.SelectedIndex != 0)
                {
                    videos = videos.Where(b => ContainsAmbiguous(b.ProgramGenre, searchGenre));
                }
                foreach (var video in videos)
                {
                    queue.Add(new DiskVideoPair(disk, video));
                }
            }
            ResultView.DataContext = queue;
        }

        public static bool ContainsAmbiguous(string text,string word)
        {
            if(word==null || word == "") { return true; }
            var ci = System.Globalization.CultureInfo.CurrentCulture.CompareInfo;
            return ci.IndexOf(text, word, System.Globalization.CompareOptions.IgnoreKanaType | System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreWidth
                |System.Globalization.CompareOptions.IgnoreNonSpace | System.Globalization.CompareOptions.IgnoreSymbols) != -1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(ResultView.SelectedItem is DiskVideoPair)
            {
                var video = (DiskVideoPair)ResultView.SelectedItem;
                ExecuteSearch(video.Disk.DiskName, "", SearchType.ディスク番号);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ExecuteSearch("", "", SearchType.全て);
        }

        private string CurrentSortTitle = "DiskName";
        private bool CurrentSortOrder = true;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader columnHeader = sender as GridViewColumnHeader;
            string columnTagName = columnHeader.Tag as string;

            var data = ResultView.DataContext as DiskVideoPairList;

            string newSortTitle;
            bool newSortOrder;
            if (CurrentSortTitle == columnTagName && CurrentSortOrder == true)
            {
                newSortTitle = columnTagName;
                newSortOrder = false;
            }else if (CurrentSortTitle == columnTagName)
            {
                newSortTitle = "DiskName";
                newSortOrder = true;
            }
            else {
                newSortTitle = columnTagName;
                newSortOrder = true;
            }
            CurrentSortOrder = newSortOrder;
            CurrentSortTitle = newSortTitle;

            IEnumerable<DiskVideoPair> result = null;
            
            switch (newSortTitle)
            {
                case "DiskName":
                    if(newSortOrder) result=data.OrderBy((a) => a.Disk.DiskName);
                    else result = data.OrderByDescending((a) => a.Disk.DiskName);
                    break;
                case "ProgramTitle":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.ProgramTitle);
                    else result = data.OrderByDescending((a) => a.Video.ProgramTitle);
                    break;
                case "Length":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.Length);
                    else result = data.OrderByDescending((a) => a.Video.Length);
                    break; 
                case "ProgramGenre":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.ProgramGenre);
                    else result = data.OrderByDescending((a) => a.Video.ProgramGenre);
                    break;
                case "RecordDateTime":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.RecordDateTime);
                    else result = data.OrderByDescending((a) => a.Video.RecordDateTime);
                    break;
                case "ChannelNumber":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.ChannelNumber);
                    else result = data.OrderByDescending((a) => a.Video.ChannelNumber);
                    break;
                case "ChannelName":
                    if (newSortOrder) result = data.OrderBy((a) => a.Video.ChannelName);
                    else result = data.OrderByDescending((a) => a.Video.ChannelName);
                    break;
            }
            if (result != null) data.SetContents(result);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var win=new LoadDiskWindow();
            win.ParentMainWindow = this;
            win.Show();
        }
    }

    public enum SearchType
    {
        全て, 番組名, 番組紹介, ディスク番号
    }

}
