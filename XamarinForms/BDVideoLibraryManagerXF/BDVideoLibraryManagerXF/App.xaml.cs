using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace BDVideoLibraryManagerXF
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new Views.SettingPage();
            MainPage = new Views.TopPage();
            //MainPage = new MasterDetailPage()
            //{
            //    Master = new Views.MasterPage() { Title = "hello" },
            //    Detail = new NavigationPage(new Views.MasterPage() { Title = "hello" }) { Title="hello"}

            //};
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
