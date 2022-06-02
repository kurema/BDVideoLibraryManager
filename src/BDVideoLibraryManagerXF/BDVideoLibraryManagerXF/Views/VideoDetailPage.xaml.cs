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

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {

        }
    }
}
