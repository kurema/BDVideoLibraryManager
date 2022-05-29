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
    public partial class LibraryDiscPage : ContentPage
    {
        ViewModels.LibraryViewModel ViewModel { get { return BindingContext as ViewModels.LibraryViewModel; } }


        public LibraryDiscPage()
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
            await LibraryPage.LoadRemote(async (a, b, c) => await DisplayAlert(a, b, c), ViewModel, () => TryLoadLocal());
        }

        private void ListView_Refreshing(object sender, EventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            LoadRemote();
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            var lp = new LibraryPage();
            lp.TargetDisc = Storages.LibraryStorage.Library.Contents.Where((d) => d.DiskName == (e.SelectedItem as VideoLibraryManagerCommon.Library.DiskBD)?.DiskName).First();
            await Navigation.PushAsync(lp);

            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}