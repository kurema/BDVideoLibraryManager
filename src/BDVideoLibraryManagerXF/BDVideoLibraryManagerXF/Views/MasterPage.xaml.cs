using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.ComponentModel;
using System.Collections.ObjectModel;

namespace BDVideoLibraryManagerXF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        public ListView ListView => ListViewMenuItems;

        public MasterPage()
        {
            InitializeComponent();

            this.BindingContext = new MasterViewModel();
        }

        public class MasterViewModel : INotifyPropertyChanged
        {
            public const string HeaderLibraryPage = "一覧";
            public const string HeaderLibraryDiscPage = "ディスク一覧";
            public const string HeaderGenresPage = "分類";
            public const string HeaderRandomMovie = "今日のおまかせ";
            public const string HeaderConfig = "設定";

            public ObservableCollection<MasterMenuItem> MenuItems { get; }
            public MasterViewModel()
            {
                MenuItems = new ObservableCollection<MasterMenuItem>(new[]
                {
                    new MasterMenuItem { TargetType=typeof(Views.LibraryPage), Title = HeaderLibraryPage,Description="録画番組一覧" },
                    new MasterMenuItem { TargetType=typeof(Views.LibraryDiscPage), Title = HeaderLibraryDiscPage,Description="ディスク一覧" },
                    new MasterMenuItem{TargetType=typeof(Views.GenresPage),Title=HeaderGenresPage,Description="ジャンル検索"},
                    new MasterMenuItem{Title=HeaderRandomMovie,Description="ランダムで番組選択",Action=async (t)=>{
                        if(t is TopPage top)await top.ChooseRandomPage();
                    } },
                    new MasterMenuItem {  Title = HeaderConfig,Description="サーバー設定"
                    ,Action= (t) =>
                    {
                        t.Detail=new NavigationPage( new SettingPage());
                    } 
                    //,TargetType=typeof(Views.SettingPage)
                    },
                    new MasterMenuItem{TargetType=typeof(Views.LicensePage),Title="ライセンス",Description="オープンソースライセンス"}
                });
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }


        public class MasterMenuItem
        {
            public Action<Xamarin.Forms.MasterDetailPage> Action;

            public string Title { get; set; }
            public string Description { get; set; }

            public Type TargetType { get; set; }
        }
    }
}
