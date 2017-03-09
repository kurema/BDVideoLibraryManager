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
                    new MasterMenuItem { TargetType=typeof(Views.LibraryPage), Title = "一覧" },
                    new MasterMenuItem{TargetType=typeof(Views.GenresPage),Title="分類"},
                    new MasterMenuItem{Title="今日のおまかせ",Action=(t)=>{
                        var lib= Storages.LibraryStorage.Library;
                        var pl=new VideoLibraryManagerCommon.Library.DiskVideoPairList();
                        foreach(var disk in lib.Contents)
                        {
                            foreach(var video in disk.Contents)
                            {
                                pl.Add(new VideoLibraryManagerCommon.Library.DiskVideoPair(disk,video));
                            }
                        }
                        Random rd=new Random((int)(DateTime.Now.Date.Ticks/TimeSpan.FromDays(1).Ticks));
                        var todaysprog= pl[ rd.Next(pl.Count)];

                        var page = (Page)Activator.CreateInstance(typeof(Views.VideoDetailPage),todaysprog);
                        page.Title = "今日のおまかせ";
                        t.Detail = new NavigationPage(page);
                    } },
                    new MasterMenuItem { TargetType=typeof(Views.SettingPage), Title = "設定" },
                    new MasterMenuItem{TargetType=typeof(Views.LicensePage),Title="ライセンス"}
                });
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }


        public class MasterMenuItem
        {
            public Action<Xamarin.Forms.MasterDetailPage> Action;

            public string Title { get; set; }

            public Type TargetType { get; set; }
        }
    }
}
