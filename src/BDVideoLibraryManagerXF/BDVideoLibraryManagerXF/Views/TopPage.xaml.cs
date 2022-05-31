using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDVideoLibraryManagerXF.Storages;
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

            if (SettingStorage.IsSet)
                //Navigation.PushAsync(new TutorialPage() { Title = "チュートリアル" });
                Navigation.PushAsync(new LibraryPage() { Title = "一覧" });
            else
                //Navigation.PushAsync(new SettingPage() { Title = "設定" });
                Navigation.PushAsync(new TutorialPage() { Title = "チュートリアル" });

            MasterSide.ListView.ItemSelected += ListView_ItemSelected;
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPage.MasterMenuItem;
            if (item == null)
                return;
            if (item.TargetType != null)
            {
                var page = (Page)Activator.CreateInstance(item.TargetType);
                page.Title = item.Title;
                if (this.Detail is NavigationPage nvp && nvp.CurrentPage is not SettingPage and not LicensePage)
                {
                    await ((NavigationPage)this.Detail).Navigation.PushAsync(page);
                }
                else
                {
                    this.Detail = new NavigationPage(page);
                }
            }
            item.Action?.Invoke(this);

            IsPresented = false;
            MasterSide.ListView.SelectedItem = null;
        }
    }
}
