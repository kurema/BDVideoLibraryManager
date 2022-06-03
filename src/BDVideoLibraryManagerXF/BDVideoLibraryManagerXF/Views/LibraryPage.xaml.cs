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
    public partial class LibraryPage : ContentPage
    {
        ViewModels.LibraryViewModel ViewModel { get { return BindingContext as ViewModels.LibraryViewModel; } }

        public string TargetGenre { get { return ViewModel.SearchGenre; } set { ViewModel.SearchGenre = value; } }

        public VideoLibraryManagerCommon.Library.DiskBD TargetDisc { get { return ViewModel.TargetDisc; } set { ViewModel.TargetDisc = value; } }

        public LibraryPage()
        {
            InitializeComponent();

            //https://qiita.com/amay077/items/4c315e7f212834183316
            //結局Material Iconsからサブセットフォントを作って表示するようにしたのでプラットフォーム依存は不要です。
            //if (Device.RuntimePlatform == Device.Android)
            //{
            //    Search_button.Text = "";
            //    Search_button.IconImageSource = new FontImageSource()
            //    {
            //        FontFamily= "search.ttf#Search",
            //        Glyph= "\ue8b6"
            //    };
            //}

            if (Storages.LibraryStorage.Library != null)
            {
                this.BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = Storages.LibraryStorage.Library };
            }
            else
            {
                TryLoadLocal();
            }
        }

        public async void LoadLocal()
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            var lib = Storages.LibraryStorage.GetLibraryOrLoad();
            if (lib != null)
                BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        public async void TryLoadLocal()
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            try
            {
                var lib = Storages.LibraryStorage.GetLibraryOrLoad();
                if (lib != null) BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            }
            catch { }
            finally { if (ViewModel != null) ViewModel.IsBusy = false; }
        }

        public async Task LoadRemote()
        {
            await LoadRemote(async (a, b, c, d) =>
            {
                if (d is null)
                {
                    await DisplayAlert(a, b, c);
                    return false;
                }
                else return await DisplayAlert(a, b, c, d);
            }, ViewModel, () => TryLoadLocal());
        }

        public async static Task LoadRemote(Func<string, string, string, string, Task<bool>> alert, ViewModels.LibraryViewModel viewModel, Action loadLocal)
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.None
                || (!Xamarin.Essentials.Connectivity.ConnectionProfiles.Any(a => a is Xamarin.Essentials.ConnectionProfile.WiFi or Xamarin.Essentials.ConnectionProfile.Ethernet)))
            {
                //SMBはチャレンジ/レスポンス方式なので、信頼できないWi-Fiに繋いでもパスワードが漏れることはない。
                //とはいえ、Wi-Fiアクセスポイント名(MACアドレス)と紐付けた方が良い気がする。同一名のサーバーがあるかも知れないし。
                if (!await alert?.Invoke("情報取得", "ネットワークに接続されていません。", "続行", "キャンセル")) return;
            }

            if (viewModel != null) viewModel.IsBusy = true;
            try
            {
                if (!await Storages.LibraryStorage.CopyToLocal())
                {
                    await alert?.Invoke("情報取得", "最新情報の取得に失敗しました。", "OK", null);
                }
                loadLocal?.Invoke();
            }
            finally
            {
                if (viewModel != null) viewModel.IsBusy = false;
            }
        }

        private async void ListView_Refreshing(object sender, EventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            await LoadRemote();
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        private void Search_Toggle(object sender, EventArgs e)
        {
            var ts = (DateTime.Now - SearchBarLastClosedTime);
            if (!String.IsNullOrEmpty(SearchBar.Text))
            {
                ViewModel.SearchCommand.Execute(null);
            }
            else if (ts.TotalMilliseconds > 500 || ts.TotalMilliseconds < 0)
            {
                var b = !SearchBar.IsVisible;
                SearchBar.IsVisible = b;
                if (b)
                {
                    SearchBar.Focus();
                }
            }
        }

        private void Clear_Option(object sender, EventArgs e)
        {
            ViewModel.SearchGenre = null;
            var originalDisc = ViewModel.TargetDisc;
            ViewModel.TargetDisc = null;

            async void scroll(VideoLibraryManagerCommon.Library.DiskBD target)
            {
                await Task.Delay(100);
                if (target is not null && target.FirstOrDefault() is not null)
                {
                    LibraryListView.ScrollTo(null, target, ScrollToPosition.Center, false);
                }
            }
            scroll(originalDisc);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            // Manually deselect item
            LibraryListView.SelectedItem = null;

            if (args.SelectedItem is not VideoLibraryManagerCommon.Library.VideoBD item) return;
            if (this.BindingContext is not ViewModels.LibraryViewModel bd) return;

            const int maxVideoCount = 50;

            VideoLibraryManagerCommon.Library.DiskVideoPair result = null;
            var contents = bd.Library.Contents;

            {
                var listFull = new VideoLibraryManagerCommon.Library.DiskVideoPairList();

                foreach (var disc in contents)
                {
                    foreach (var video in disc.Contents)
                    {
                        var temp = new VideoLibraryManagerCommon.Library.DiskVideoPair(disc, video);
                        if (video == item)
                        {
                            result = temp;
                        }
                        listFull.Add(temp);
                        if (listFull.Count() > maxVideoCount) goto toomany;
                    }
                }
                await Navigation.PushAsync(new VideosDetailPage(listFull, result));
                return;
            }
        toomany:

            {
                var disc = contents.FirstOrDefault(a => a.Contains(item));

                if (disc is not null)
                {
                    int index = Array.IndexOf(contents, disc);
                    if (index != -1)
                    {
                        var list = new VideoLibraryManagerCommon.Library.DiskVideoPairList();
                        for (int i = index; i < contents.Length; i++)
                        {
                            foreach (var video in contents[i])
                            {
                                var temp = new VideoLibraryManagerCommon.Library.DiskVideoPair(disc, video);
                                if (video == item)
                                {
                                    result = temp;
                                }
                                list.Add(temp);
                                if (list.Count == maxVideoCount)
                                {
                                    if (list.Contains(result))
                                    {
                                        await Navigation.PushAsync(new VideosDetailPage(list, result));
                                        return;
                                    }
                                    else
                                    {
                                        await Navigation.PushAsync(new VideoDetailPage(result));
                                        return;
                                    }
                                }
                            }
                        }
                        if (list.Count() <= maxVideoCount)
                        {
                            await Navigation.PushAsync(new VideosDetailPage(list, result));
                            return;
                        }

                    }
                }
            }

            await Navigation.PushAsync(new VideoDetailPage(result));
            return;
        }

        //検索ボタンクリック時にSearchBarのフォーカスも外れるので強引に時間差で対応。
        public DateTime SearchBarLastClosedTime = new DateTime();

        protected override void OnAppearing()
        {
            if (Storages.LibraryStorage.Library != null && !Storages.LibraryStorage.Library?.Contents.SequenceEqual(ViewModel?.FullLibrary.Contents) == true)
            {
                this.BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = Storages.LibraryStorage.Library };
            }
            base.OnAppearing();
        }

        private void SearchBar_OnUnfocused(object sender, FocusEventArgs e)
        {
            if (String.IsNullOrEmpty(SearchBar.Text))
            {
                if (SearchBar.IsVisible)
                {
                    SearchBarLastClosedTime = DateTime.Now;
                    SearchBar.IsVisible = false;
                }
            }
            else
            {
                ViewModel.SearchCommand.Execute(null);
            }
        }
    }
}
