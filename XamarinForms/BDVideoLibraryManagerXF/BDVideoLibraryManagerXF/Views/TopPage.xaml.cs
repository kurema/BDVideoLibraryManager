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
    public partial class TopPage : Xamarin.Forms.MasterDetailPage
    {
        public TopPage()
        {
            InitializeComponent();

            MasterSide.ListView.ItemSelected += ListView_ItemSelected;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPage.MasterMenuItem;
            if (item == null)
                return;
            if (item.TargetType != null)
            {
                var page = (Page)Activator.CreateInstance(item.TargetType);
                page.Title = item.Title;
                this.Detail = new NavigationPage(page);
            }
            item.Action?.Invoke(this);

            IsPresented = false;
            MasterSide.ListView.SelectedItem = null;
        }
    }
}
