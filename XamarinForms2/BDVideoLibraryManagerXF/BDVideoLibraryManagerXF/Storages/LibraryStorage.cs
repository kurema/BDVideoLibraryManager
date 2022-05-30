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

    public static string PathCsv => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "csv");
    public static string PathCsvTemp => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "csv.temp");

    public static Library GetLibraryOrLoad()
    {
        return Library ?? LoadLocalData();
    }

    public static Library LoadLocalData()
    {
        if (!System.IO.Directory.Exists(PathCsv))
        {
            return Library = new Library(new DiskBD[0]);
        }
        var files = System.IO.Directory.GetFiles(PathCsv);

        var d = new Queue<DiskBD>();

        foreach (var file in files.OrderBy(a => a))
        {
            if (System.IO.Path.GetExtension(file) == ".csv" && System.IO.File.Exists(file))
            {
                using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var fr = new System.IO.StreamReader(stream))
                    {
                        d.Enqueue(new DiskBD(fr, System.IO.Path.GetFileNameWithoutExtension(file)));
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
            remotePath = System.Text.RegularExpressions.Regex.Replace(remotePath, @"^[\\/¥]+", "").Replace('\\','/');
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
                            var status = await Task.Run(() => client.Login(string.Empty, UserName, Password));
                            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
                            {
                                try
                                {
                                    fileStore = await Task.Run(() => client.TreeConnect(remotePathTop, out status));
                                    if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
                                    {
                                        string remotePathBottomFixed = remotePathBottom.Replace('/', '\\');
                                        if (!string.IsNullOrEmpty(remotePathBottomFixed) && !remotePathBottomFixed.EndsWith("\\")) remotePathBottomFixed = remotePathBottomFixed + "\\";
                                        if (client is SMB1Client) { remotePathBottomFixed = @"\" + remotePathBottomFixed; }
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
                            await Task.Run(() => client.Logoff());
                            await Task.Run(() => client.Disconnect());
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
        System.Collections.IEnumerable resultList = new object[0];

        System.IO.DirectoryInfo folder = null;
        void UpdatePCLFolderIfNeeded()
        {
            //if (DoCopy) folder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync("csv", PCLStorage.CreationCollisionOption.ReplaceExisting);
            if (folder is null)
            {
                folder = System.IO.Directory.CreateDirectory(PathCsvTemp);
                folder.Delete(true);
                folder.Create();
            }
        }

        if (client is SMB1Client)
        {
            object objectHandle = null;
            var status = await Task.Run(() => fileStore.CreateFile(out objectHandle, out var fileStatus, "\\", SMBLibrary.AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, SMBLibrary.ShareAccess.Read | SMBLibrary.ShareAccess.Write, SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_DIRECTORY_FILE, null));
            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                try
                {
                    //https://github.com/TalAloni/SMBLibrary/issues/78
                    //SMB1の場合はCreateFileを飛ばしてQueryDirectoryが出来るらしい。SMB2みたいにフォルダ指定して"\\*"でQueryDirectoryしても駄目らしい。
                    await Task.Run(() =>
                    {
                        status = ((SMB1FileStore)fileStore).QueryDirectory(out var fileList, path + "*", SMBLibrary.SMB1.FindInformationLevel.SMB_FIND_FILE_DIRECTORY_INFO);
                        resultList = fileList;
                    });
                }
                finally
                {
                    fileStore.CloseFile(objectHandle);
                }
            }
        }
        else if (client is SMB2Client)
        {
            object objectHandle = null;

            var status = await Task.Run(() => fileStore.CreateFile(out objectHandle, out _, path,
                SMBLibrary.AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, SMBLibrary.ShareAccess.Read | SMBLibrary.ShareAccess.Write,
                SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_DIRECTORY_FILE, null));
            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        status = fileStore.QueryDirectory(out var fileList, objectHandle, "*", SMBLibrary.FileInformationClass.FileDirectoryInformation);
                        resultList = fileList;
                    });
                }
                finally
                {
                    fileStore.CloseFile(objectHandle);
                }
            }
        }

        if (resultList is not null)
        {
            bool containsCsv = false;
            bool containsCsvDirectory = false;

            async Task Check(string fileName, bool isDirectory)
            {
                if (System.IO.Path.GetExtension(fileName).ToUpperInvariant() == ".CSV" && !isDirectory)
                {
                    containsCsv = true;
                    if (DoCopy)
                    {
                        UpdatePCLFolderIfNeeded();
                        await TryCopyCopyFile(client, path + fileName, fileStore, folder);
                    }
                }
                if (fileName.ToUpperInvariant() == "CSV" && isDirectory) containsCsvDirectory = true;
            }

            foreach (var item in resultList)
            {
                if (item is SMBLibrary.FileDirectoryInformation info) await Check(info.FileName, info.FileAttributes == SMBLibrary.FileAttributes.Directory);
                else if (item is SMBLibrary.FileFullDirectoryInformation info2) await Check(info2.FileName, info2.FileAttributes == SMBLibrary.FileAttributes.Directory);
                else if (item is SMBLibrary.SMB1.FindFileDirectoryInfo info3) await Check(info3.FileName, info3.ExtFileAttributes == SMBLibrary.SMB1.ExtendedFileAttributes.Directory);
#if DEBUG
                else throw new NotImplementedException();
#endif
            }

            if (folder is not null)
            {
                var pathCsv = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "csv");
                var folder2 = System.IO.Directory.CreateDirectory(pathCsv);
                folder2.Delete(true);
                folder.MoveTo(pathCsv);
            }

            return (containsCsv, containsCsvDirectory);
        }
        return (false, false);
    }

    private static async Task TryCopyCopyFile(ISMBClient client, string path, ISMBFileStore fileStore, System.IO.DirectoryInfo folder)
    {
        object fileHandle = null;
        var status = client switch
        {
            //SMB1でSMBLibrary.CreateOptions.FILE_SYNCHRONOUS_IO_ALERTを設定していると、Invalid Parameterみたいなエラーが出る。
            SMB1Client => await Task.Run(() => fileStore.CreateFile(out fileHandle, out var _, path,
                                        SMBLibrary.AccessMask.GENERIC_READ | SMBLibrary.AccessMask.SYNCHRONIZE,
                                        SMBLibrary.FileAttributes.Normal, SMBLibrary.ShareAccess.Read,
                                        SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_NON_DIRECTORY_FILE, null)),
            SMB2Client => await Task.Run(() => fileStore.CreateFile(out fileHandle, out var _, path,
                                    SMBLibrary.AccessMask.GENERIC_READ | SMBLibrary.AccessMask.SYNCHRONIZE,
                                    SMBLibrary.FileAttributes.Normal, SMBLibrary.ShareAccess.Read,
                                    SMBLibrary.CreateDisposition.FILE_OPEN, SMBLibrary.CreateOptions.FILE_NON_DIRECTORY_FILE | SMBLibrary.CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null)),
            _ => throw new NotImplementedException(),
        };

        try
        {
            if (status == SMBLibrary.NTStatus.STATUS_SUCCESS)
            {
                //var file = await folder.CreateFileAsync(System.IO.Path.GetFileName(path.Replace('\\', '/')), PCLStorage.CreationCollisionOption.ReplaceExisting);
                using var stream = new System.IO.FileStream(System.IO.Path.Combine(folder.FullName, System.IO.Path.GetFileName(path.Replace('\\', '/'))), System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);
                byte[] data = new byte[0];
                long bytesRead = 0;
                while (true)
                {
                    status = await Task.Run(() => fileStore.ReadFile(out data, fileHandle, bytesRead, (int)client.MaxReadSize));
                    if (status is not SMBLibrary.NTStatus.STATUS_SUCCESS and not SMBLibrary.NTStatus.STATUS_END_OF_FILE) throw new Exception("Failed to read from file");
                    if (status is SMBLibrary.NTStatus.STATUS_END_OF_FILE || data.Length == 0) break;
                    bytesRead += data.Length;
                    stream.Write(data, 0, data.Length);
                }
            }
            else
            {
                throw new Exception($"Download failed {path}.");
            }
        }
        finally
        {
            fileStore.CloseFile(fileHandle);
        }
    }

    public static async Task<bool> CopyToLocal()
    {
        //return await CopyToLocal(SettingStorage.SMBServerName, SettingStorage.SMBPath, SettingStorage.SMBID, SettingStorage.SMBPassword);
        var result = await TryCopy(SettingStorage.SMBServerName, SettingStorage.SMBPath, SettingStorage.SMBID, await SettingStorage.GetSMBPassword(), true);
        if (result) LoadLocalData();
        return result;
    }
}
