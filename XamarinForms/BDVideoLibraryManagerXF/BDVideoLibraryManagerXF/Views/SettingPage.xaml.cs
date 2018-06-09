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
        public MasterDetailPage ParentMasterDetailPage;

        public SettingPage()
        {
            InitializeComponent();

            Label_Smb_User.Text= Storages.SettingStorage.SMBID;
            Label_Smb_Password.Text = Storages.SettingStorage.SMBPassword;
            Label_Smb_Path.Text = Storages.SettingStorage.SMBPath;
            Label_Smb_Name.Text = Storages.SettingStorage.SMBServerName;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            SharpCifs.Smb.SmbFile folder;
            if (Storages.LibraryStorage.TestAccess(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text,out folder))
            {
                Storages.SettingStorage.SMBID = Label_Smb_User.Text;
                Storages.SettingStorage.SMBPassword = Label_Smb_Password.Text;
                Storages.SettingStorage.SMBPath = Label_Smb_Path.Text;
                Storages.SettingStorage.SMBServerName = Label_Smb_Name.Text;
                await DisplayAlert("結果", "アクセスに成功しました。設定を保存します。", "OK");
                await Storages.LibraryStorage.CopyToLocal(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text);

                if (Parent is MasterDetailPage)
                {
                    ((MasterDetailPage)Parent).Detail = new NavigationPage(new LibraryPage() { Title = "一覧" });
                }
            }
            else
            {
                await DisplayAlert("結果", "アクセスに失敗しました。", "OK");
            }
        }

        private void Button_Clicked_Tutorial(object sender, EventArgs e)
        {
            if (Parent is MasterDetailPage)
            {
                ((MasterDetailPage)Parent).Detail = new NavigationPage(new TutorialPage() { Title = "チュートリアル" });
            }else if(Parent is NavigationPage)
            {
                Navigation.PushAsync(new TutorialPage() { Title = "チュートリアル" });
            }
        }
    }
}
