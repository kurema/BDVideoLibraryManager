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
    public partial class LicensePage : ContentPage
    {
        public LicensePage()
        {
            InitializeComponent();

            var items = new LicenseViewModel[]
            {
                new LicenseViewModel()
                {
                    Title="SMBLibrary",
                    Copyright="Copyright (c) TalAloni",
                    LicenseUrl="https://github.com/TalAloni/SMBLibrary/blob/master/License.txt",
                    LicenseName="LGPL v3.0 Licence"
                },
                //new LicenseViewModel()
                //{
                //    Title="PCLStorage",
                //    Copyright="Copyright (c) dsplaisted",
                //    LicenseUrl="https://github.com/dsplaisted/PCLStorage/blob/master/LICENSE",
                //    LicenseName="Microsoft Public License (Ms-PL)"
                //},
                new LicenseViewModel()
                {
                    Title="CsvHelper",
                    Copyright="Copyright (c) JoshClose",
                    LicenseUrl="https://raw.githubusercontent.com/JoshClose/CsvHelper/master/LICENSE.txt",
                    LicenseName="Dual licensing under MS-PL and Apache 2.0"
                },
                new LicenseViewModel()
                {
                    Title="Xamarin SDK",
                    Copyright="Copyright (c) .NET Foundation Contributors",
                    LicenseUrl="https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/LICENSE",
                    LicenseName="The MIT License (MIT)"
                },
                new LicenseViewModel()
                {
                    Title=".NET",
                    Copyright="Copyright (c) .NET Foundation and Contributors",
                    LicenseUrl="https://github.com/dotnet/runtime/blob/main/LICENSE.TXT",
                    LicenseName="The MIT License (MIT)"
                },

            };
            foreach (var item in items)
            {
                var sl = new StackLayout();
                sl.Children.Add(new Label() { Text = item.Title, FontAttributes = FontAttributes.Bold });
                sl.Children.Add(new Label() { Text = item.Copyright });
                sl.Children.Add(new Label() { Text = item.LicenseName });
                {
                    var hyperlink = new Label() { Text = item.LicenseUrl, TextDecorations = TextDecorations.Underline };
                    var tapgr = new TapGestureRecognizer();
                    tapgr.Tapped += (_, _) => { Xamarin.Essentials.Launcher.TryOpenAsync(item.LicenseUrl); };
                    hyperlink.GestureRecognizers.Add(tapgr);
                    sl.Children.Add(hyperlink);
                }
                stackLayoutLicense.Children.Add(sl);
            }
        }

        public class LicenseViewModel
        {
            public string Title { get; set; }
            public string Copyright { get; set; }
            public string LicenseUrl { get; set; }
            public string LicenseName { get; set; }
        }
    }
}
