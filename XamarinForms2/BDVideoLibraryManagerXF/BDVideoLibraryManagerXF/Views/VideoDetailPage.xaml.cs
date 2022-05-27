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
            if (!(this.BindingContext is VideoLibraryManagerCommon.Library.DiskVideoPair))
                return;
            var bind = (this.BindingContext as VideoLibraryManagerCommon.Library.DiskVideoPair);

            var lp = new LibraryPage();
            lp.TargetDisc = Storages.LibraryStorage.Library.Contents.Where((d) => d.DiskName == bind.Disk.DiskName).First();
            await Navigation.PushAsync(lp);
        }

        public VideoDetailPage()
        {
            InitializeComponent();
        }
    }
}
