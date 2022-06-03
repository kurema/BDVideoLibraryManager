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

            public IEnumerable<MasterMenuItem> MenuItems => MenuItemsFull?.Where(a =>
            {
                var lib = Storages.LibraryStorage.Library;
                return a.Id switch
                {
                    nameof(LibraryPage) => lib?.Contents?.Length > 0,
                    nameof(LibraryDiscPage) => lib?.Contents?.Length > 0,
                    nameof(GenresPage) => lib?.Genres?.Length > 0,
                    "Random" => lib?.Contents?.Length > 0,
                    _ => true
                };

            });
            //public IEnumerable<MasterMenuItem> MenuItems => MenuItemsFull;
            public MasterMenuItem[] MenuItemsFull { get; }

            public MasterViewModel()
            {
                MenuItemsFull = new[]
                {
                    new MasterMenuItem { TargetType=typeof(Views.LibraryPage), Title = HeaderLibraryPage,Description="録画番組一覧" ,Id=nameof(LibraryPage)},
                    new MasterMenuItem { TargetType=typeof(Views.LibraryDiscPage), Title = HeaderLibraryDiscPage,Description="ディスク一覧" ,Id=nameof(LibraryDiscPage)},
                    new MasterMenuItem{TargetType=typeof(Views.GenresPage),Title=HeaderGenresPage,Description="ジャンル検索",Id=nameof(GenresPage)},
                    new MasterMenuItem{Title=HeaderRandomMovie,Description="ランダムで番組選択",Action=async (t)=>{
                        if(t is TopPage top)await top.ChooseRandomPage();
                    } ,Id="Random"},
                    new MasterMenuItem {  Title = HeaderConfig,Description="サーバー設定"
                    ,Action= (t) =>
                    {
                        t.Detail=new NavigationPage( new SettingPage());
                    }
                    ,Id=nameof(SettingPage)
                    //,TargetType=typeof(Views.SettingPage)
                    },
                    new MasterMenuItem{TargetType=typeof(Views.LicensePage),Title="ライセンス",Description="オープンソースライセンス",Id=nameof(LicensePage)}
                };
                Storages.LibraryStorage.LibraryChangedEventHandler += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MenuItems)));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }


        public class MasterMenuItem
        {
            public Action<Xamarin.Forms.MasterDetailPage> Action;

            public string Title { get; set; }
            public string Description { get; set; }
            public Type TargetType { get; set; }
            public string Id { get; set; }
        }
    }
}
