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

        public LibraryPage()
        {
            InitializeComponent();

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
            if(ViewModel!=null) ViewModel.IsBusy = true;
            var lib= await Storages.LibraryStorage.GetLocalData();
            if (lib != null)
                BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        public async void TryLoadLocal()
        {
            if(ViewModel!=null) ViewModel.IsBusy = true;
            try
            {
                var lib = await Storages.LibraryStorage.GetLocalData();
                if (lib != null)
                    BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            }
            catch { }
            if(ViewModel!=null) ViewModel.IsBusy = false;
        }

        public async void LoadRemote()
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            if(! await Storages.LibraryStorage.CopyToLocal())
            {
                await DisplayAlert("情報取得", "最新情報の取得に失敗しました。", "OK");
            }
            TryLoadLocal();
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        private void ListView_Refreshing(object sender, EventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            LoadRemote();
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        private void Search_Toggle(object sender, EventArgs e)
        {
            SearchBar.IsVisible = !SearchBar.IsVisible;
        }

        private void Clear_Genre(object sender, EventArgs e)
        {
            ViewModel.SearchGenre = null;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (!(args.SelectedItem is VideoLibraryManagerCommon.Library.VideoBD))
                return;
            var item = args.SelectedItem as VideoLibraryManagerCommon.Library.VideoBD;
            if (item == null)
                return;


            VideoLibraryManagerCommon.Library.DiskBD result = new VideoLibraryManagerCommon.Library.DiskBD();
            if(this.BindingContext is ViewModels.LibraryViewModel)
            {
                var bd = this.BindingContext as ViewModels.LibraryViewModel;
                foreach(var disk in bd.Library.Contents)
                {
                    if (disk.Contents.Contains(item))
                    {
                        result = disk;
                        break;
                    }
                }
            }

            await Navigation.PushAsync(new VideoDetailPage(new VideoLibraryManagerCommon.Library.DiskVideoPair(result,item)));

            // Manually deselect item
            LibraryListView.SelectedItem = null;
        }
    }
}
