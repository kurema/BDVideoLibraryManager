using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

//https://dev.to/peedroca/material-font-icons-on-xamarin-forms-2a9h
[assembly: ExportFont("search.ttf", Alias = "MaterialSearch")]
//[assembly: ExportFont("MaterialIcons-Regular.ttf", Alias = "Material")]

namespace BDVideoLibraryManagerXF
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Views.TopPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
