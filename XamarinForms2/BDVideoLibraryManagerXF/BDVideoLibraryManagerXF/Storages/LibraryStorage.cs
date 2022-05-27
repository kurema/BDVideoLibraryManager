using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VideoLibraryManagerCommon.Library;
using SMBLibrary.Client;

namespace BDVideoLibraryManagerXF.Storages;

public static class LibraryStorage
{
    public static Library Library { get; private set; }
    private static System.Threading.SemaphoreSlim Semaphore = new(1, 1);

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

    public static async Task<bool> TryCopy(string serverName, string remotePath, string UserName, string Password, bool DoCopy = false)
    {
        //https://github.com/TalAloni/SMBLibrary/blob/master/ClientExamples.md
        ISMBFileStore fileStore = null;

        string remotePathTop, remotePathBottom;
        {
            var match = System.Text.RegularExpressions.Regex.Match(remotePath, "^([^/]+)/(.+)$");
            if (match.Success)
            {
                remotePathTop = match.Groups[1].Value;
                remotePathBottom = match.Groups[2].Value;
            }
            else
            {
                remotePathTop = remotePath;
                remotePathBottom = string.Empty;
            }
        }

        await Semaphore.WaitAsync();
        try
        {
            foreach (var client in new ISMBClient[] { new SMB2Client(), new SMB1Client() })
            {
                try
                {
                    if (client.Connect(serverName, SMBLibrary.SMBTransportType.DirectTCPTransport))
                    {
                        try
                        {
                            var status = client.Login(string.Empty, UserName, Password);
                            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
                            {
                                try
                                {
                                    fileStore = client.TreeConnect(remotePathTop, out status);
                                    if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
                                    {
                                        string remotePathBottomFixed = remotePathBottom.Replace('/', '\\');
                                        if (!string.IsNullOrEmpty(remotePathBottomFixed) && !remotePathBottomFixed.EndsWith("\\")) remotePathBottomFixed = remotePathBottomFixed + "\\";
                                        if (client is SMB1Client) { remotePathBottomFixed = "\\" + remotePathBottomFixed; }
                                        var resultOfCheck = await TryCopyCheckCSV(remotePathBottomFixed, client, fileStore, DoCopy);
                                        if (resultOfCheck.containsCSVFile) return true;
                                        if (resultOfCheck.containsCSVDirectory && (await TryCopyCheckCSV(remotePathBottomFixed + "csv\\", client, fileStore, DoCopy)).containsCSVFile) return true;
                                    }
                                }
                                finally
                                {
                                    fileStore.Disconnect();
                                }
                            }
                        }
                        finally
                        {
                            client.Logoff();
                            client.Disconnect();
                        }
                    }
                }
                catch
                {
                }
            }
        }
        finally
        {
            Semaphore.Release();
        }
        return false;
    }

    private static async Task<(bool containsCSVFile, bool containsCSVDirectory)> TryCopyCheckCSV(string path, ISMBClient client, ISMBFileStore fileStore, bool DoCopy = false)
    {
        var status = fileStore.CreateFile(out var objectHandle, out var fileStatus, path,
            SMBLibrary.AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, SMBLibrary.ShareAccess.Read | SMBLibrary.ShareAccess.Write,
            SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_DIRECTORY_FILE, null);
        if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
        {
            try
            {
                if (client is SMB1Client client1)
                {
                    status = ((SMB1FileStore)fileStore).QueryDirectory(out var fileList, "\\*", SMBLibrary.SMB1.FindInformationLevel.SMB_FIND_FILE_DIRECTORY_INFO);
                    return (false, false);
                }
                else if (client is SMB2Client client2)
                {
                    PCLStorage.IFolder folder = null;
                    if (DoCopy) folder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync("csv", PCLStorage.CreationCollisionOption.ReplaceExisting);

                    status = fileStore.QueryDirectory(out var fileList, objectHandle, "*", SMBLibrary.FileInformationClass.FileDirectoryInformation);
                    bool containsCsv = false;
                    bool containsCsvDirectory = false;
                    foreach (var item in fileList)
                    {
                        if (item is not SMBLibrary.FileDirectoryInformation info) continue;
                        if (System.IO.Path.GetExtension(info.FileName).ToUpperInvariant() == ".CSV" && info.FileAttributes != SMBLibrary.FileAttributes.Directory)
                        {
                            containsCsv = true;
                            if (DoCopy)
                            {
                                await TryCopyCopyFile(client, path + info.FileName, fileStore, folder);
                            }
                        }
                        if (info.FileName.ToUpperInvariant() == "CSV" && info.FileAttributes == SMBLibrary.FileAttributes.Directory) containsCsvDirectory = true;
                    }
                    return (containsCsv, containsCsvDirectory);
                }
            }
            finally
            {
                fileStore.CloseFile(objectHandle);
            }
        }
        return (false, false);
    }

    private static async Task TryCopyCopyFile(ISMBClient client, string path, ISMBFileStore fileStore, PCLStorage.IFolder folder)
    {
        var status = fileStore.CreateFile(out var fileHandle, out var fileStatus, path,
            SMBLibrary.AccessMask.GENERIC_READ | SMBLibrary.AccessMask.SYNCHRONIZE,
            SMBLibrary.FileAttributes.Normal, SMBLibrary.ShareAccess.Read,
            SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_NON_DIRECTORY_FILE | SMBLibrary.CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);

        try
        {
            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                var file = await folder.CreateFileAsync(System.IO.Path.GetFileName(path), PCLStorage.CreationCollisionOption.ReplaceExisting);
                using var stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite);
                byte[] data;
                long bytesRead = 0;
                while (true)
                {
                    status = fileStore.ReadFile(out data, fileHandle, bytesRead, (int)client.MaxReadSize);
                    if (status is not SMBLibrary.NTStatus.STATUS_SUCCESS and not SMBLibrary.NTStatus.STATUS_END_OF_FILE) throw new Exception("Failed to read from file");
                    if (status is SMBLibrary.NTStatus.STATUS_END_OF_FILE || data.Length == 0) break;
                    bytesRead += data.Length;
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
        }
    }


    //public static bool TestAccess(string serverName, string remotePath, string UserName, string Password, out SharpCifs.Smb.SmbFile smbFolder)
    //{
    //    if (!remotePath.EndsWith("/")) { remotePath += "/"; }
    //    if (!TestAccessSingle(serverName, remotePath, UserName, Password, out smbFolder))
    //    {
    //        remotePath += "csv/";
    //        if (!TestAccessSingle(serverName, remotePath, UserName, Password, out smbFolder))
    //        {
    //            smbFolder = null;
    //            return false;
    //        }
    //        else
    //        {
    //            return true;
    //        }
    //    }
    //    return true;
    //}

    //public static bool TestAccessSingle(string serverName, string remotePath, string UserName, string Password, out SharpCifs.Smb.SmbFile smbFolder)
    //{
    //    try
    //    {
    //        //{
    //        //    var client = new SMB1Client();
    //        //    if (client.Connect(serverName, SMBLibrary.SMBTransportType.DirectTCPTransport))
    //        //    {
    //        //        var status = client.Login(string.Empty, UserName, Password);
    //        //        if(status == SMBLibrary.NTStatus.STATUS_SUCCESS)
    //        //        {
    //        //            var fileStore = client.TreeConnect(remotePath, out var status2);
    //        //            if(status2 == SMBLibrary.NTStatus.STATUS_SUCCESS)
    //        //            {

    //        //            }
    //        //        }
    //        //    }
    //        //}


    //        var folder = new SharpCifs.Smb.SmbFile("smb://" + UserName + ":" + Password + "@" + serverName + "/" + remotePath);
    //        if (!folder.Exists() || !folder.IsDirectory())
    //        {
    //            smbFolder = null;
    //            return false;
    //        }

    //        bool Sucess = false;
    //        foreach (var item in folder.ListFiles())
    //        {
    //            if (item.IsFile() && System.IO.Path.GetExtension(item.GetName()) == ".csv")
    //            {
    //                Sucess = true;
    //                break;
    //            }
    //        }
    //        if (!Sucess)
    //        {
    //            smbFolder = null;
    //            return false;
    //        }
    //        smbFolder = folder;
    //    }
    //    catch (Exception e)
    //    {
    //        smbFolder = null;
    //        return false;
    //    }
    //    return true;
    //}

    public static async Task<bool> CopyToLocal()
    {
        //return await CopyToLocal(SettingStorage.SMBServerName, SettingStorage.SMBPath, SettingStorage.SMBID, SettingStorage.SMBPassword);
        return await TryCopy(SettingStorage.SMBServerName, SettingStorage.SMBPath, SettingStorage.SMBID, SettingStorage.SMBPassword, true);
    }

    //public static async Task<bool> CopyToLocal(string serverName, string remotePath, string UserName, string Password)
    //{
    //    SharpCifs.Smb.SmbFile folder;
    //    if (!remotePath.EndsWith("/")) { remotePath += "/"; }
    //    if (!TestAccess(serverName, remotePath, UserName, Password, out folder))
    //    {
    //        return false;
    //    }
    //    if (Copying) { return false; }
    //    Copying = true;

    //    var destFolder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync("csv", PCLStorage.CreationCollisionOption.ReplaceExisting);

    //    foreach (var item in folder.ListFiles())
    //    {
    //        var name = item.GetName();
    //        if (item.IsFile() && System.IO.Path.GetExtension(name) == ".csv")
    //        {
    //            using (var stream = item.GetInputStream())
    //            {
    //                var destFile = await destFolder.CreateFileAsync(name, PCLStorage.CreationCollisionOption.ReplaceExisting);
    //                using (var destStr = await destFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
    //                {
    //                    var buffer = new byte[1028 * 10];
    //                    int size;
    //                    while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
    //                    {
    //                        await destStr.WriteAsync(buffer, 0, size);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    Copying = false;
    //    return true;
    //}
}
