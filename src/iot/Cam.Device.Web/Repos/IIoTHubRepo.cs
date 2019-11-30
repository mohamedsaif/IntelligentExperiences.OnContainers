using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cam.Device.Web.Repos
{
    public interface IIoTHubRepo
    {
        Task SendEventAsync(string message, Dictionary<string, string> properties);
        Task UploadFile(string fileName, FileStream fileStream);
    }
}