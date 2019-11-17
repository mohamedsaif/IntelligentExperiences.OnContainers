using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Abstractions
{
    public interface IStorageRepository
    {
        Task<string> CreateFileAsync(string name, byte[] fileData);
        Task<string> CreateFileAsync(string name, Stream fileData);
        Task<string> CreateFileAsync(string containerName, string fileName, Stream fileData);
        Task<byte[]> GetFileAsync(string fileName);
        Task<byte[]> GetFileAsync(string containerName, string fileName);
        string GetFileDownloadUrl(string fileName);
    }
}
