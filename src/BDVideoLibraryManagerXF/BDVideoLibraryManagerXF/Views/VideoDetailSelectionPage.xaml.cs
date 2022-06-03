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
    public partial class VideoDetailSelectionPage : ContentPage
    {
        public VideoDetailSelectionPage()
        {
            InitializeComponent();
        }

        //選択したテキストに対して色んな操作をしたいけど、カスタムレンダラーなしでは無理なので諦め。
        //カスタムレンダラー使えば簡単だけどね。

        //private string GetSelected()
        //{
        //    try
        //    {
        //        return input.Text.Substring(input.CursorPosition, input.SelectionLength);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //private async Task DoActionToText(Func<string, Task> func)
        //{
        //    var text = GetSelected();
        //    if (string.IsNullOrEmpty(text)) return;
        //    if (func is null) return;
        //    try
        //    {
        //        await func?.Invoke(text);
        //    }
        //    catch { }
        //}

        //private async void ImageButton_Clicked_Search(object sender, EventArgs e) => await DoActionToText(async text => await Xamarin.Essentials.Browser.OpenAsync($"https://www.google.com/search?q={System.Web.HttpUtility.UrlEncode(text)}"));

        //private async void ImageButton_Clicked_Share(object sender, EventArgs e) => await DoActionToText(async text => await Xamarin.Essentials.Share.RequestAsync(text));

        //private async void ImageButton_Clicked_Copy(object sender, EventArgs e)
        //{
        //    await DoActionToText(async text => await Xamarin.Essentials.Clipboard.SetTextAsync(text));
        //}
    }
}