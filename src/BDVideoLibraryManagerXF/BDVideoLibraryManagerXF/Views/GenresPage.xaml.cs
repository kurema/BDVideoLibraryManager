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
    public partial class GenresPage : ContentPage
    {
        public GenresPage()
        {
            InitializeComponent();

            listViewGenre.ItemsSource = Storages.LibraryStorage.GetLibraryOrLoad()?.Genres;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (!(args.SelectedItem is string))
                return;
			if (args.SelectedItem is not string item)
				return;

			var lp = new LibraryPage();
            lp.TargetGenre = item;
            await Navigation.PushAsync(lp);

            listViewGenre.SelectedItem = null;
        }
    }
}
