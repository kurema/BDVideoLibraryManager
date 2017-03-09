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
            if(Storages.LibraryStorage.TestAccess(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text))
            {
                Storages.SettingStorage.SMBID = Label_Smb_User.Text;
                Storages.SettingStorage.SMBPassword = Label_Smb_Password.Text;
                Storages.SettingStorage.SMBPath = Label_Smb_Path.Text;
                Storages.SettingStorage.SMBServerName = Label_Smb_Name.Text;
                await DisplayAlert("結果", "アクセスに成功しました。設定を保存します。", "OK");
                await Storages.LibraryStorage.CopyToLocal(Label_Smb_Name.Text, Label_Smb_Path.Text, Label_Smb_User.Text, Label_Smb_Password.Text);

            }
            else
            {
                await DisplayAlert("結果", "アクセスに失敗しました。", "OK");
            }
        }
    }
}
