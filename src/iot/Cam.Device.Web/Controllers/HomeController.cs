using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Cam.Device.Web.Models;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Cam.Device.Web.AI;
using Cam.Device.Web.Repos;
using Newtonsoft.Json;
using CoreLib.Abstractions;
using System.IO;

namespace Cam.Device.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string deviceId;
        private IIoTHubRepo _iotHub;
        private IStorageRepository _storageRepo;

        public HomeController(ILogger<HomeController> logger, 
            IOptions<AppSettings> settings,
            IIoTHubRepo iotHub,
            IStorageRepository storageRepo,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _appSettings = settings.Value;
            _hostingEnvironment = env;
            deviceId = _appSettings.DeviceId;
            _iotHub = iotHub;
            _storageRepo = storageRepo;
        }

        public IActionResult Index()
        {
            ViewData["DeviceId"] = deviceId;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult CaptureFrame()
        {
            try
            {

                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            // Getting Filename
                            var fileName = file.FileName;
                            
                            // Getting Extension
                            var fileExtension = Path.GetExtension(fileName);
                            string newFileName = "";
                            //If Edge Mode enabled, always save the file with the same name (CamFrames folder will be used as a camera stream to an Edge Device)
                            if (_appSettings.IsEdgeModeEnabled)
                            {
                                // Always save the file with the same name to simulate a stream
                                // Use deviceId + fileExtension as the file name
                                newFileName = $"{deviceId}{fileExtension}";
                            }
                            else
                            {
                                //Create unique file name to be uploaded to storage
                                newFileName = $"{deviceId}-{DateTime.UtcNow.ToString("ddMMyyHHmmss")}{fileExtension}";
                            }

                            //  Generating physical path to store photo 
                            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "CamFrames", newFileName);

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                // Save the frame
                                SaveFrame(file, filePath);

                                // If edge mode is not enabled, send telemetry to IoT Hub and Upload the file
                                if (!_appSettings.IsEdgeModeEnabled)
                                {
                                    //Only send if there are detected faces
                                    //FaceDetectionResult faces = FaceDetector.DetectFaces(filePath, Path.Combine(_hostingEnvironment.WebRootPath, "AI", "haarcascade_frontalface_alt.xml"));
                                    //if(faces != null && faces.DetectedFacesFrames.Length > 0)
                                    //SendFrame(filePath, faces.DetectedFacesFrames.Length);
                                    SendFrame(filePath, 0);
                                }
                            }
                        }
                    }
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Saving captured image into Folder.
        /// </summary>
        /// <param name="file">Actual image file sent in the request</param>
        /// <param name="fileName">File name</param>
        private void SaveFrame(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }

        private void SendFrame(string filePath, int detectedFaces)
        {
            // Using Azure Storage SDK to upload the file is another option
            _storageRepo.CreateFileAsync(Path.GetFileName(filePath), System.IO.File.ReadAllBytes(filePath)).Wait();

            // IoT Hub has built-in support for blobs upload.
            // File will be uploaded to a folder with the device name. This might required some code changes
            //_iotHub.UploadFile(Path.GetFileName(filePath), new MemoryStream(System.IO.File.ReadAllBytes(filePath))).Wait();

            CognitiveRequest req = new CognitiveRequest
            {
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceId,
                FileUrl = Path.GetFileName(filePath),
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                IsDeleted = false,
                IsProcessed = false,
                Origin = "Device.Web.V1.0.0",
                Status = "Submitted",
                TakenAt = DateTime.UtcNow,
                TargetAction = CognitiveTargetAction.CamFrameAnalysis.ToString()
            };

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "DeviceId", deviceId },
                { "DetectedFacesCount", detectedFaces.ToString() }
            };

            _iotHub.SendEventAsync(JsonConvert.SerializeObject(req), properties).Wait();
        }

    }
}
