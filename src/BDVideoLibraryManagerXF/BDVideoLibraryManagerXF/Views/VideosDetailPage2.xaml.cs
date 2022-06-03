﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BDVideoLibraryManagerXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosDetailPage2 : ContentPage
    {
        public VideosDetailPage2(IEnumerable<VideoLibraryManagerCommon.Library.DiskVideoPair> list, VideoLibraryManagerCommon.Library.DiskVideoPair targetPair)
        {
            InitializeComponent();

            mainCarousel.ItemsSource = list;
            if (list.Contains(targetPair))
            {
                mainCarousel.CurrentItem = targetPair;
            }
        }

        public VideosDetailPage2(VideoLibraryManagerCommon.Library.DiskVideoPair targetPair)
        {
            InitializeComponent();

            mainCarousel.ItemsSource = new[] { targetPair };
            mainCarousel.CurrentItem = targetPair;
        }


        public VideosDetailPage2()
        {
            InitializeComponent();
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(VideosDetailPage2), Device.GetNamedSize(NamedSize.Small, typeof(Label)));

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            double sizeSmall = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            double sizeMedium = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
            if (FontSize == sizeSmall) FontSize = sizeMedium;
            else if (FontSize == sizeMedium) FontSize = sizeSmall;
        }

        private async void Select_Disc(object sender, EventArgs e)
        {
            if (mainCarousel.CurrentItem is not VideoLibraryManagerCommon.Library.DiskVideoPair bind) return;
            var lp = new LibraryPage();
            lp.TargetDisc = Storages.LibraryStorage.GetLibraryOrLoad().Contents.Where((d) => d.DiskName == bind.Disk.DiskName).First();
            await Navigation.PushAsync(lp);
        }

        private async void ToolbarItem_Clicked_Clipboard(object sender, EventArgs e)
        {
            if (mainCarousel.CurrentItem is not VideoLibraryManagerCommon.Library.DiskVideoPair dvp) return;
            List<(string text, string key)> options = new();
            void addText(string text, string key = null)
            {
                if (string.IsNullOrWhiteSpace(text)) return;
                if (key is null) key = text;
                if (options.Any(a => a.text == text)) return;
                options.Add((text, key));
            }
            addText(dvp.Video?.ProgramTitle);
            if (dvp.Video?.ProgramTitle is not null)
            {
                var temp = dvp.Video?.ProgramTitle;
                temp = System.Text.RegularExpressions.Regex.Replace(temp, @"\[[^\]]+\]", "");
                temp = System.Text.RegularExpressions.Regex.Replace(temp, @"^\s+", "");
                temp = System.Text.RegularExpressions.Regex.Replace(temp, @"\s+$", "");
                addText(temp);
            }
            addText(dvp.Video.ProgramGenre);
            addText(dvp.Video.ChannelName);
            addText(dvp.Disk.DiskTitle);
            addText(dvp.Disk.DiskName);
            foreach (var item in dvp.Video.Links) addText(item.TextFull);
            addText(dvp.Video.ProgramDetail, "[動画情報] 全文");
            const string detailSelect = "[動画情報] 選択";
            addText(detailSelect, detailSelect);
            const string cancel = "キャンセル";

            var action = await DisplayActionSheet("コピー", cancel, null, options.Select(a => a.key).ToArray());
            if (action is null || action == cancel) return;
            if (action == detailSelect)
            {
                await Navigation.PushAsync(new Views.VideoDetailSelectionPage() { BindingContext = dvp.Video });
                return;
            }
            var result = options.FirstOrDefault(a => a.key == action);
            if (result.text is null) return;

            notifyGrid.IsVisible = true;
            //ここでコピーせずダイアログ消えた後にコピーした方が良い(共有や検索した場合キャンセル)？
            //なら「コピーします」？
            //どうせこのタイミングでコピーしてるかは関係ない気がする。
            CopiedText = result.text;
            CopyAbort = false;
            for (int i = 3; i > 0; i--)
            {
                notifyLabel.Text = $"コピーします ({i})。";
                await Task.Delay(1000);
            }
            try
            {
                if (CopyAbort == false)
                {
                    await Xamarin.Essentials.Clipboard.SetTextAsync(result.text);
                    notifyLabel.Text = "コピーしました。";
                }
                //この表示は実際には見えない。
            }
            catch
            {
                notifyLabel.Text = "コピーに失敗しました。";
                await Task.Delay(3000);
            }
            notifyGrid.IsVisible = false;
        }

        private string CopiedText = null;
        private bool CopyAbort = false;

        private async void ImageButton_Clicked_Search(object sender, EventArgs e)
        {
            try
            {
                CopyAbort = true;
                await Xamarin.Essentials.Browser.OpenAsync($"https://www.google.com/search?q={System.Web.HttpUtility.UrlEncode(CopiedText)}");
            }
            catch { }
        }

        private async void ImageButton_Clicked_Share(object sender, EventArgs e)
        {
            try
            {
                CopyAbort = true;
                await Xamarin.Essentials.Share.RequestAsync(CopiedText, "録画情報");
            }
            catch { }
        }
    }
}