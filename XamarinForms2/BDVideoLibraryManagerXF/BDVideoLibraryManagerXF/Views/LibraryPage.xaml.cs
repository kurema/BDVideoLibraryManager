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
            var lib = await Storages.LibraryStorage.GetLocalData();
            if (lib != null)
                BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        public async void TryLoadLocal()
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            try
            {
                var lib = await Storages.LibraryStorage.GetLocalData();
                if (lib != null)
                    BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            }
            catch { }
            finally { if (ViewModel != null) ViewModel.IsBusy = false; }
        }

        public async void LoadRemote()
        {
            await LoadRemote(async (a, b, c) => await DisplayAlert(a, b, c), ViewModel, () => TryLoadLocal());
        }

        public async static Task LoadRemote(Func<string, string, string, Task> alert, ViewModels.LibraryViewModel viewModel, Action loadLocal)
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.None
                || (!Xamarin.Essentials.Connectivity.ConnectionProfiles.Any(a => a is Xamarin.Essentials.ConnectionProfile.WiFi or Xamarin.Essentials.ConnectionProfile.Ethernet)))
            {
                //SMBはチャレンジ/レスポンス方式なので、信頼できないWi-Fiに繋いでもパスワードが漏れることはない。
                //とはいえ、Wi-Fiアクセスポイント名(MACアドレス)と紐付けた方が良い気がする。同一名のサーバーがあるかも知れないし。
                await alert?.Invoke("情報取得", "ネットワークに接続されていません。", "OK");
                return;
            }

            if (viewModel != null) viewModel.IsBusy = true;
            try
            {
                if (!await Storages.LibraryStorage.CopyToLocal())
                {
                    await alert?.Invoke("情報取得", "最新情報の取得に失敗しました。", "OK");
                }
                loadLocal?.Invoke();
            }
            finally
            {
                if (viewModel != null) viewModel.IsBusy = false;
            }
        }

        private void ListView_Refreshing(object sender, EventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            LoadRemote();
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
            if (!(args.SelectedItem is VideoLibraryManagerCommon.Library.VideoBD))
                return;
            var item = args.SelectedItem as VideoLibraryManagerCommon.Library.VideoBD;

            // Manually deselect item
            LibraryListView.SelectedItem = null;

            int maxVideoCount = 50;

            VideoLibraryManagerCommon.Library.DiskVideoPair result = null;
            var list = new VideoLibraryManagerCommon.Library.DiskVideoPairList();
            if (this.BindingContext is ViewModels.LibraryViewModel)
            {
                var bd = this.BindingContext as ViewModels.LibraryViewModel;
                foreach (var disk in bd.Library.Contents)
                {
                    foreach (var video in disk.Contents)
                    {
                        var temp = new VideoLibraryManagerCommon.Library.DiskVideoPair(disk, video);
                        if (video == item)
                        {
                            result = temp;
                        }
                        list.Add(temp);
                    }
                }
            }

            if (list.Count() > maxVideoCount)
            {
                await Navigation.PushAsync(new VideoDetailPage(result));
            }
            else
            {
                await Navigation.PushAsync(new VideosDetailPage(list, result));
            }


        }

        //検索ボタンクリック時にSearchBarのフォーカスも外れるので強引に時間差で対応。
        public DateTime SearchBarLastClosedTime = new DateTime();

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
