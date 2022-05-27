using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.String;

namespace BDVideoLibraryManagerXF.Storages
{
    public static class SettingStorage
    {
        public static bool IsSet => !IsNullOrEmpty(SMBServerName);

        public static string SMBPassword
        {
            get
            {
                var result = GetProperty("SMBPassword") as string;
                if (result == null) return "";
                return result;
            }
            set
            {
                SetProprty("SMBPassword", value);
            }
        }

        public static string SMBID
        {
            get
            {
                var result = GetProperty("SMBID") as string;
                if (result == null) return "";
                return result;
            }
            set
            {
                SetProprty("SMBID", value);
            }
        }

        public static string SMBPath
        {
            get
            {
                var result = GetProperty("SMBPath") as string;
                if (result == null) return "";
                return result;
            }
            set
            {
                SetProprty("SMBPath", value);
            }
        }

        public static string SMBServerName
        {
            get
            {
                var result = GetProperty("SMBServerName") as string;
                if (result == null) return "";
                return result;
            }
            set
            {
                SetProprty("SMBServerName", value);
            }
        }

        public static object GetProperty(string key)
        {
            if (Xamarin.Forms.Application.Current.Properties.ContainsKey(key))
                return Xamarin.Forms.Application.Current.Properties[key];
            else
                return null;

        }

        public static void SetProprty(string key,object value)
        {
            if (Xamarin.Forms.Application.Current.Properties.ContainsKey(key))
                Xamarin.Forms.Application.Current.Properties[key] = value;
            else
                Xamarin.Forms.Application.Current.Properties.Add(key, value);
        }
    }
}
