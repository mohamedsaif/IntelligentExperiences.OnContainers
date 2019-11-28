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

namespace Cam.Device.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string deviceId;

        public HomeController(ILogger<HomeController> logger, IOptions<AppSettings> settings, IWebHostEnvironment env)
        {
            _logger = logger;
            _appSettings = settings;
            _hostingEnvironment = env;
            deviceId = _appSettings.Value.DeviceId;
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

                            // Always save the file with the same name to simulate a stream
                            // Use deviceId + fileExtension as the file name
                            var newFileName = string.Concat(deviceId, fileExtension);
                            //  Generating physical path to store photo 
                            var filepath = Path.Combine(_hostingEnvironment.WebRootPath, "CamFrames") + $@"\{newFileName}";

                            if (!string.IsNullOrEmpty(filepath))
                            {
                                // Save the frame
                                SaveFrame(file, filepath);
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

    }
}
