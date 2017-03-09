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

    }
}
