using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BDVideoLibraryManagerXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoDetailPage : ContentPage
    {
        public VideoDetailPage(VideoLibraryManagerCommon.Library.DiskVideoPair Video)
        {
            InitializeComponent();

            this.BindingContext = Video;
        }

        private async void Select_Disc(object sender, EventArgs e)
        {
            if (this.BindingContext is not VideoLibraryManagerCommon.Library.DiskVideoPair bind) return;
            var lp = new LibraryPage();
            lp.TargetDisc = Storages.LibraryStorage.GetLibraryOrLoad().Contents.Where((d) => d.DiskName == bind.Disk.DiskName).First();
            await Navigation.PushAsync(lp);
        }

        public VideoDetailPage()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (sender is not Label label) return;
            double sizeSmall = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            double sizeMedium = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
            if (label.FontSize == sizeSmall) label.FontSize = sizeMedium;
            else if (label.FontSize == sizeMedium) label.FontSize = sizeSmall;
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if (this.BindingContext is not VideoLibraryManagerCommon.Library.DiskVideoPair dvp) return;
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
            await Xamarin.Essentials.Clipboard.SetTextAsync(result.text);

            notifyGrid.IsVisible = true;
            notifyLabel.Text = "コピーしました。";
            CopiedText = result.text;
            await Task.Delay(3000);
            notifyGrid.IsVisible = false;
            //~~Toastで通知したいけど、DependencyServiceを使うと今も後もめんどくさい。~~
        }

        private string CopiedText=null;

        private async void ImageButton_Clicked_Search(object sender, EventArgs e)
        {
            try
            {
                await Xamarin.Essentials.Browser.OpenAsync($"https://www.google.com/search?q={System.Web.HttpUtility.UrlEncode(CopiedText)}");
            }
            catch { }
        }

        private async void ImageButton_Clicked_Share(object sender, EventArgs e)
        {
            try
            {
                await Xamarin.Essentials.Share.RequestAsync(CopiedText,"録画情報");
            }
            catch { }
        }
    }
}
