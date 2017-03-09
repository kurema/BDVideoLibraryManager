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
                    Title="SharpCifs.Std",
                    Copyright="Copyright (c) 2016 Do-Be's",
                    LicenseUrl="https://github.com/ume05rw/SharpCifs.Std/blob/master/LICENSE",
                    LicenseName="LGPL v2.1 Licence"
                },
                new LicenseViewModel()
                {
                    Title="PCLStorage", 
                    Copyright="Copyright (c) dsplaisted",
                    LicenseUrl="https://github.com/dsplaisted/PCLStorage/blob/master/LICENSE",
                    LicenseName="Microsoft Public License (Ms-PL)"
                },
                new LicenseViewModel()
                {
                    Title="CsvHelper",
                    Copyright="Copyright (c) JoshClose",
                    LicenseUrl="https://raw.githubusercontent.com/JoshClose/CsvHelper/master/LICENSE.txt",
                    LicenseName="Dual licensing under MS-PL and Apache 2.0"
                }

            };
            foreach(var item in items)
            {
                var sl = new StackLayout();
                sl.Children.Add(new Label() { Text = item.Title });
                sl.Children.Add(new Label() { Text = item.Copyright });
                sl.Children.Add(new Label() { Text = item.LicenseName });
                sl.Children.Add(new Label() { Text = item.LicenseUrl });
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
