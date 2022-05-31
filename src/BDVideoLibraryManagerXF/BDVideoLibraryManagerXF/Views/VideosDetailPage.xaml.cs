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
    public partial class VideosDetailPage : CarouselPage
    {
        public VideosDetailPage(VideoLibraryManagerCommon.Library.DiskVideoPairList list, VideoLibraryManagerCommon.Library.DiskVideoPair targetPair)
        {
            InitializeComponent();

            this.ItemsSource = list;
            if (list.Contains(targetPair))
            {
                this.SelectedItem = targetPair;
            }
        }
    }
}
