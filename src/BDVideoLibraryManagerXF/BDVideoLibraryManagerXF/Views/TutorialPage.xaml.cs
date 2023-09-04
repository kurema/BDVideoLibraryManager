using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;

namespace BDVideoLibraryManagerXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TutorialPage : CarouselPage
    {
        public TutorialPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked_NextPage(object sender, EventArgs e)
        {
            if (!this.Children.Contains(this.CurrentPage)) { return; }
            var cnt = this.Children.IndexOf(this.CurrentPage);
            this.CurrentPage = this.Children[Math.Min(this.Children.Count - 1, cnt + 1)];
        }

        private async void Button_Clicked_Kakaku_BDD(object sender, EventArgs e)
        {
			await OpenUrlAsync("http://kakaku.com/pc/blu-ray-drive/itemlist.aspx?pdf_so=p1");
		}

        private async void Button_Clicked_Map_BDD(object sender, EventArgs e)
        {
			await OpenUrlAsync("https://www.google.co.jp/maps/search/家電量販店/");
        }

        private async void Button_Clicked_Amazon_BDD(object sender, EventArgs e)
        {
			await OpenUrlAsync("https://www.amazon.co.jp/%E5%A4%96%E4%BB%98%E8%A8%98%E9%8C%B2%E5%9E%8B%E3%83%96%E3%83%AB%E3%83%BC%E3%83%AC%E3%82%A4%E3%83%89%E3%83%A9%E3%82%A4%E3%83%96-%E9%80%9A%E8%B2%A9/b/ref=as_li_ss_tl?ie=UTF8&node=2151956051&linkCode=ll2&tag=randomfisher-22&linkId=4a9102e296e2b39eabc488982f59a38a");
		}


        private async void Button_Clicked_Pocket(object sender, EventArgs e)
        {
			await OpenUrlAsync("https://getpocket.com/edit.php?url=https%3A%2F%2Fgithub.com%2Fkurema%2FBDVideoLibraryManager");
        }

        private async void Button_Clicked_Open(object sender, EventArgs e)
        {
			await OpenUrlAsync("https://github.com/kurema/BDVideoLibraryManager");
		}

        private async void Button_Clicked_Share(object sender, EventArgs e)
        {
			await Share.RequestAsync(new ShareTextRequest()
            {
                Uri = "https://github.com/kurema/BDVideoLibraryManager",
                Title = "BD録画管理ソフト",
                Text= "BD録画管理ソフト\n「Windows版」からダウンロードしてください。"
            });
        }

        public static async Task OpenUrlAsync(string url)
        {
            try { if (await Launcher.TryOpenAsync(url)) return; } catch { }
            try { await Browser.OpenAsync(url); } catch { }
        }

        private void Button_Clicked_Go_Setting(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingPage() { Title = "設定" });
        }

    }
}