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

            if (Storages.LibraryStorage.GetLibraryOrLoad() != null)
            {
                this.BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = Storages.LibraryStorage.GetLibraryOrLoad() };
            }
            else
            {
                TryLoadLocal();
            }
        }

        public void TryLoadLocal()
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            try
            {
                var lib = Storages.LibraryStorage.GetLibraryOrLoad();
                if (lib != null)
                    BindingContext = new ViewModels.LibraryViewModel() { FullLibrary = lib };
            }
            catch { }
            finally { if (ViewModel != null) ViewModel.IsBusy = false; }
        }

        private async void ListView_Refreshing(object sender, EventArgs e)
        {
            if (ViewModel != null) ViewModel.IsBusy = true;
            await LibraryPage.LoadRemote(async (a, b, c, d) =>
            {
                if (d is null)
                {
                    await DisplayAlert(a, b, c);
                    return false;
                }
                else return await DisplayAlert(a, b, c, d);
            }, ViewModel, () => TryLoadLocal());
            if (ViewModel != null) ViewModel.IsBusy = false;
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            var lp = new LibraryPage();
            lp.TargetDisc = Storages.LibraryStorage.GetLibraryOrLoad().Contents.Where((d) => d.DiskName == (e.SelectedItem as VideoLibraryManagerCommon.Library.DiskBD)?.DiskName).First();
            await Navigation.PushAsync(lp);

            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}