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
    public partial class SettingPage : ContentPage
    {
        private const string TitleResult = "結果";
        public MasterDetailPage ParentMasterDetailPage;

        public SettingPage()
        {
            InitializeComponent();

            {
                Label_Smb_User.Text = Storages.SettingStorage.SMBID;
                Task.Run(async () => Label_Smb_Password.Text = await Storages.SettingStorage.GetSMBPassword());
                Label_Smb_Path.Text = Storages.SettingStorage.SMBPath;
                Label_Smb_Name.Text = Storages.SettingStorage.SMBServerName;
            }

            Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    stackServerSuggestion.Children.Clear();
                    var host = await System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var suggest = ip.ToString();
                            suggest = System.Text.RegularExpressions.Regex.Replace(suggest, @"\.\d+$", ".");

                            var button = new Button() { Text = suggest, VerticalOptions = LayoutOptions.Start };
                            button.Clicked += (s, e) =>
                            {
                                Label_Smb_Name.Text = suggest;
                            };
                            stackServerSuggestion.Children.Add(button);
                        }
                    }
                }
                catch { }
            });
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            string smbName = Label_Smb_Name.Text;
            string smbPath = Label_Smb_Path.Text;
            string smbUser = Label_Smb_User.Text;
            string smbPW = Label_Smb_Password.Text;

            try
            {
                if (button is not null) button.IsEnabled = false;
                if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.None
                    || (!Xamarin.Essentials.Connectivity.ConnectionProfiles.Any(a => a is Xamarin.Essentials.ConnectionProfile.WiFi or Xamarin.Essentials.ConnectionProfile.Ethernet)))
                {
                    var alertResult = await DisplayAlert(TitleResult, "ネットワークに接続されていません。", "続行", "キャンセル");
                    if (alertResult == false) return;
                }

                System.Text.RegularExpressions.Match match;

                //SharpCifs.Smb.SmbFile folder;
                //if (Storages.LibraryStorage.TestAccess(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text,out folder))
                if (await Storages.LibraryStorage.TryCopy(smbName, smbPath, smbUser, smbPW, false))
                {
                    Storages.SettingStorage.SMBID = smbUser;
                    Storages.SettingStorage.SMBPath = smbPath;
                    Storages.SettingStorage.SMBServerName = smbName;
                    await Storages.SettingStorage.SetSMBPassword(smbPW);
                    await DisplayAlert(TitleResult, "アクセスに成功しました。設定を保存します。", "OK");
                    if (button is not null) button.Text = "ダウンロード中";
                    //await Storages.LibraryStorage.CopyToLocal(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text);
                    var result = await Storages.LibraryStorage.TryCopy(smbName, smbPath, smbUser, smbPW, true);
                    if (result)
                    {
                        Storages.LibraryStorage.LoadLocalData();
                        //if (Application.Current?.MainPage is TopPage top && top.Master?.BindingContext is MasterPage.MasterViewModel mvm) { mvm.OnMenuItemsUpdated(); }
                    }
                    else
                    {
                        if (button is not null) button.Text = "保存";
                        await DisplayAlert(TitleResult, "ダウンロードに失敗しました", "OK");
                        return;
                    }

                    if (Parent is MasterDetailPage)
                    {
                        ((MasterDetailPage)Parent).Detail = new NavigationPage(new LibraryPage() { Title = "一覧" });
                    }
                    else if (Parent is NavigationPage ngp && ngp.Parent is MasterDetailPage mdp)
                    {
                        mdp.Detail = new NavigationPage(new LibraryPage() { Title = "一覧" });
                    }
                }
                else if ((match = System.Text.RegularExpressions.Regex.Match(smbPath, "([¥￥])")).Success)
                {
                    await DisplayAlert(TitleResult, $"アクセスに失敗しました。\nパスに「{match.Groups[1].Value}」が含まれています。\nパスの区切りは\"/\"または\"\\\"です。", "OK");
                }
                else
                {
                    await DisplayAlert(TitleResult, "アクセスに失敗しました。", "OK");
                }
            }
            finally
            {
                if (button is not null) button.IsEnabled = true;
            }

        }

        private void Button_Clicked_Tutorial(object sender, EventArgs e)
        {
            if (Parent is MasterDetailPage)
            {
                ((MasterDetailPage)Parent).Detail = new NavigationPage(new TutorialPage() { Title = "チュートリアル" });
            }
            else if (Parent is NavigationPage)
            {
                Navigation.PushAsync(new TutorialPage() { Title = "チュートリアル" });
            }
        }
    }
}
