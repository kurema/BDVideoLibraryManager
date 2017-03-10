using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VideoLibraryManagerCommon.Library;

namespace BDVideoLibraryManagerXF.Storages
{
    public static class LibraryStorage
    {
        public static Library Library { get; private set; }
        private static bool Copying { get; set; }

        public static async Task<Library> GetLocalData()
        {
            if ((await PCLStorage.FileSystem.Current.LocalStorage.CheckExistsAsync("csv")) != PCLStorage.ExistenceCheckResult.FolderExists)
            {
                return null;
            }
            var folder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync("csv", PCLStorage.CreationCollisionOption.OpenIfExists);
            var files = await folder.GetFilesAsync();

            var d = new Queue<DiskBD>();

            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file.Name) == ".csv")
                {
                    using (var stream = await file.OpenAsync(PCLStorage.FileAccess.Read))
                    {
                        using (var fr = new System.IO.StreamReader(stream))
                        {
                            d.Enqueue(new DiskBD(fr, System.IO.Path.GetFileNameWithoutExtension(file.Name)));
                        }
                    }
                }
            }
            return Library = new Library(d.ToArray());
        }

        public static bool TestAccess(string serverName, string remotePath, string UserName, string Password)
        {
            try
            {
                var folder = new SharpCifs.Smb.SmbFile("smb://" + UserName + ":" + Password + "@" + serverName + "/" + remotePath);
                if (!folder.Exists() || !folder.IsDirectory()) { return false; }
            }
            catch { return false; }
            return true;
        }

        public static async Task<bool> CopyToLocal()
        {
            return await CopyToLocal(SettingStorage.SMBServerName, SettingStorage.SMBPath, SettingStorage.SMBID, SettingStorage.SMBPassword);
        }


        public static async Task<bool> CopyToLocal(string serverName, string remotePath,string UserName,string Password)
        {
            SharpCifs.Smb.SmbFile folder;
            if (!remotePath.EndsWith("/")) { remotePath += "/"; }
            try
            {
                folder = new SharpCifs.Smb.SmbFile("smb://" + UserName + ":" + Password + "@" + serverName + "/" + remotePath);
                if (!folder.Exists() || !folder.IsDirectory()) { return false; }
            }
            catch { return false; }
            if (Copying) { return false; }
            Copying = true;

            var destFolder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync("csv", PCLStorage.CreationCollisionOption.ReplaceExisting);

            foreach (var item in folder.ListFiles())
            {
                var name = item.GetName();
                if (item.IsFile() && System.IO.Path.GetExtension(name) == ".csv")
                {
                    using (var stream = item.GetInputStream())
                    {
                        var destFile = await destFolder.CreateFileAsync(name, PCLStorage.CreationCollisionOption.ReplaceExisting);
                        using (var destStr = await destFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                        {
                            var buffer = new byte[1028 * 10];
                            int size;
                            while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await destStr.WriteAsync(buffer, 0, size);
                            }
                        }
                    }
                }
            }
            Copying = false;
            return true;
        }
    }
}
