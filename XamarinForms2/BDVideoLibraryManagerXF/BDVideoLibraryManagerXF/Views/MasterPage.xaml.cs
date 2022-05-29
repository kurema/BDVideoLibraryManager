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

        class MasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MasterMenuItem> MenuItems { get; }
            public MasterViewModel()
            {
                MenuItems = new ObservableCollection<MasterMenuItem>(new[]
                {
                    new MasterMenuItem { TargetType=typeof(Views.LibraryPage), Title = "一覧",Description="録画番組一覧" },
                    new MasterMenuItem { TargetType=typeof(Views.LibraryDiscPage), Title = "ディスク一覧",Description="ディスク一覧" },
                    new MasterMenuItem{TargetType=typeof(Views.GenresPage),Title="分類",Description="ジャンル検索"},
                    new MasterMenuItem{Title="今日のおまかせ",Description="ランダムで番組選択",Action=(t)=>{
                        var lib= Storages.LibraryStorage.Library;

                        if (lib?.Contents == null || lib.Contents.Length == 0)
                            return;

                        var pl=new VideoLibraryManagerCommon.Library.DiskVideoPairList();
                        foreach(var disk in lib.Contents)
                        {
                            foreach(var video in disk.Contents)
                            {
                                pl.Add(new VideoLibraryManagerCommon.Library.DiskVideoPair(disk,video));
                            }
                        }
                        Random rd=new Random((int)(DateTime.Now.Date.Ticks/TimeSpan.FromDays(1).Ticks));
                        var todaysprog= pl.OrderBy(a=>a.Video.Length.TotalMilliseconds).ToArray()[ rd.Next(pl.Count)];

                        var page = (Page)Activator.CreateInstance(typeof(Views.VideoDetailPage),todaysprog);
                        page.Title = "今日のおまかせ";
                        if (t.Detail is NavigationPage)
                        {
                            (t.Detail as NavigationPage).PushAsync(page);
                        }
                        else
                        {
                            t.Detail = new NavigationPage(page);
                        }
                    } },
                    new MasterMenuItem {  Title = "設定",Description="サーバー設定"
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
