using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;

//https://dev.to/peedroca/material-font-icons-on-xamarin-forms-2a9h
[assembly: ExportFont("search.ttf", Alias = "MaterialSearch")]
//[assembly: ExportFont("MaterialIcons-Regular.ttf", Alias = "Material")]

namespace BDVideoLibraryManagerXF
{
    public partial class App : Application
    {
        const string AppActionHeader = "open_";

        public App()
        {
            InitializeComponent();

            MainPage = new Views.TopPage();

            AppActions.OnAppAction += AppActions_OnAppAction;
        }

        private void AppActions_OnAppAction(object sender, AppActionEventArgs e)
        {
            if (Application.Current != this && Application.Current is App app)
            {
                AppActions.OnAppAction -= app.AppActions_OnAppAction;
                return;
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Current.MainPage is not Views.TopPage top || top.Detail is not NavigationPage navigation) return;

                try
                {
                    switch (e.AppAction.Id)
                    {
                        case AppActionHeader + nameof(Views.LibraryPage):
                            await navigation.PushAsync(new Views.LibraryPage());
                            break;
                        case AppActionHeader + nameof(Views.LibraryDiscPage):
                            await navigation.PushAsync(new Views.LibraryDiscPage());
                            break;
                        case AppActionHeader + nameof(Views.GenresPage):
                            await navigation.PushAsync(new Views.GenresPage());
                            break;
                        case AppActionHeader + "Random":
                            await top.ChooseRandomPage();
                            break;

                    }
                }
                catch
                {
                }
            });
        }

        protected override async void OnStart()
        {
            {
                try
                {
                    string icon = Device.RuntimePlatform switch
                    {
                        Device.Android => "@mipmap/icon",
                        _ => null,
                    };
                    await AppActions.SetAsync(
                        new AppAction(AppActionHeader + nameof(Views.LibraryPage), Views.MasterPage.MasterViewModel.HeaderLibraryPage, icon: icon),
                        new AppAction(AppActionHeader + nameof(Views.LibraryDiscPage), Views.MasterPage.MasterViewModel.HeaderLibraryDiscPage, icon: icon),
                        new AppAction(AppActionHeader + nameof(Views.GenresPage), Views.MasterPage.MasterViewModel.HeaderGenresPage, icon: icon),
                        new AppAction(AppActionHeader + "Random", Views.MasterPage.MasterViewModel.HeaderRandomMovie, icon: icon)
                        );
                }
                catch (FeatureNotSupportedException)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("App Actions not supported");
#endif
                }
                catch
                {
                }
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
