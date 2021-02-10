using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using CirrusWebApp.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Azure;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.JSInterop;

namespace CirrusWebApp.Data.Services
{
    public class DataLakeService
    {
        private DataLakeServiceClient ServiceClient;
        private StorageSharedKeyCredential SharedKeyCredential;
        private DataLakeFileSystemClient FileSystemClient;

        private string AccountName = "cirruscspstorageaccount";
        private string AccountKey = "+s6lLIuOQeL0r8EDK+ctWjJDRFFXT2/xy+d10rlDSv4wXz6MDDjx5ZGWCC8a5dYwQUnonBi6s1RcOLnmloaJNA==";
        private string ContainerUri = "https://cirruscspstorageaccount.dfs.core.windows.net/";
        public DataLakeService()
        {
            SharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);
            ServiceClient = new DataLakeServiceClient(new Uri(ContainerUri), SharedKeyCredential);
            FileSystemClient = ServiceClient.GetFileSystemClient("user-files");
        }

        public async Task<string> UploadFile(IBrowserFile WebFile, Models.File CirrusFile)
        {
            var DirectoryClient = FileSystemClient.GetDirectoryClient(CirrusFile.UserId);
            DataLakeFileClient FileClient = null;
            foreach (string Category in CirrusFile.Categories)
            {
                FileClient = DirectoryClient.CreateSubDirectoryAsync(Category).Result.Value.CreateFileAsync(CirrusFile.FileName).Result.Value;
                using (var MS = new MemoryStream())
                {
                    await WebFile.OpenReadStream(10485760).CopyToAsync(MS);
                    MS.Position = 0;
                    await FileClient.AppendAsync(MS, 0);
                    await FileClient.FlushAsync(position: MS.Length);
                }
            }
            return FileClient == null ? null : FileClient.Path + "/" + CirrusFile.FileName;
        }

        public async Task<DataLakeFileClient> DeleteFile(Models.File File)
        {
            var DirectoryClient = FileSystemClient.GetDirectoryClient(File.UserId);
            DataLakeFileClient FileClient = null;
            foreach (string Category in File.Categories)
            {
                FileClient = DirectoryClient.GetFileClient(Category + "/" + File.FileName);
                await FileClient.DeleteAsync(false);
            }

            return FileClient;
        }

        public async Task<MemoryStream> DownloadFiles(List<Models.File> Files)
        {
            var DirectoryClient = FileSystemClient.GetDirectoryClient(Files[0].UserId);
            DataLakeFileClient FileClient = null;
            MemoryStream returnStream = new MemoryStream();
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipOutputStream zipOutputStream = new ZipOutputStream(ms))
                {
                    foreach (Models.File file in Files)
                    {
                        FileClient = DirectoryClient.GetFileClient(file.Categories[0] + "/" + file.FileName);
                        using (Stream stream = await FileClient.OpenReadAsync())
                        {
                            var entry = new ZipEntry(file.FileName);
                            zipOutputStream.PutNextEntry(entry);
                            byte[] bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, (int)stream.Length);
                            await zipOutputStream.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }
                }
                returnStream = ms;
            }
            return returnStream;
        }
        public async Task<DataLakeDirectoryClient> CreateUserDirectory(User user)
        {
            if (FileSystemClient.GetDirectoryClient(user.id) == null)
            {
                var result = await FileSystemClient.CreateDirectoryAsync(user.id);
                return result;
            }
            return null;
        }

        public async Task CreateDirectory(string DirectoryName, string UserId)
        {
            var DirectoryClient = FileSystemClient.GetDirectoryClient(UserId);
            await DirectoryClient.CreateSubDirectoryAsync(DirectoryName);
        }
    }
}
