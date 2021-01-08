using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using CirrusWebApp.Data.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace CirrusWebApp.Data.Services
{
    public class DataLakeSevice
    {
        private DataLakeServiceClient ServiceClient;
        private StorageSharedKeyCredential SharedKeyCredential;
        private DataLakeFileSystemClient FileSystemClient;

        private string AccountName = "cirruscspstorageaccount";
        private string AccountKey = "+s6lLIuOQeL0r8EDK+ctWjJDRFFXT2/xy+d10rlDSv4wXz6MDDjx5ZGWCC8a5dYwQUnonBi6s1RcOLnmloaJNA==";
        private string ContainerUri = "https://cirruscspstorageaccount.dfs.core.windows.net/";

        public DataLakeSevice()
        {
            SharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);
            ServiceClient = new DataLakeServiceClient(new Uri(ContainerUri), SharedKeyCredential);
            FileSystemClient = ServiceClient.GetFileSystemClient("user-files");
        }

        public async Task<bool> UploadFile(IBrowserFile File, Models.File CirrusFile, string UserId)
        {
            var DirectoryClient = FileSystemClient.GetDirectoryClient(UserId);
            foreach (string Category in CirrusFile.Categories)
            {
                var fc = DirectoryClient.CreateSubDirectoryAsync(Category).Result.Value.CreateFileAsync(CirrusFile.FileName).Result.Value;
                using (var MS = new MemoryStream())
                {
                    await File.OpenReadStream().CopyToAsync(MS);
                    MS.Position = 0;
                    await fc.AppendAsync(MS, 0);
                    await fc.FlushAsync(position: MS.Length);
                }
            }
            return true;
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
    }
}
