using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cam.Device.Web.Repos
{
    public class IoTHubRepo : IIoTHubRepo
    {
        private DeviceClient _deviceClient;
        private static TransportType _transportType = TransportType.Amqp;
        private AppSettings _appSettings;

        public IoTHubRepo(IOptions<AppSettings> settings, IWebHostEnvironment env)
        {
            _appSettings = settings.Value;
            _deviceClient = DeviceClient.CreateFromConnectionString(_appSettings.DeviceConnectionString, _transportType);
            if (_deviceClient == null)
                throw new NullReferenceException("Device client failed to initialize");
        }

        /// <summary>
        /// Upload file through IoT Hub File Upload endpoint. 
        /// Please note that this will be uploaded in a folder with the device name
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public async Task UploadFile(string fileName, Stream fileStream)
        {
            await _deviceClient.UploadToBlobAsync(fileName, fileStream);
        }

        public async Task SendEventAsync(string message, Dictionary<string, string> properties)
        {
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(message));
            eventMessage.ContentEncoding = "utf-8";
            eventMessage.ContentType = "application/json";

            foreach (var prop in properties)
                eventMessage.Properties.Add(prop.Key, prop.Value);
            await _deviceClient.SendEventAsync(eventMessage);
        }


    }
}
