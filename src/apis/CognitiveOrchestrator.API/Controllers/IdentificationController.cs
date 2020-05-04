using CognitiveOrchestrator.API.Models;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Controllers
{
    [Route("api/identification")]
    public class Identification : Controller
    {
        IStorageRepository storageRepository;
        IAzureServiceBusRepository serviceBusRepository;
        private IVisitorIdentificationManager visitorIdentificationManager;

        public Identification(IStorageRepository storage, IAzureServiceBusRepository sb, IVisitorIdentificationManager idn)
        {
            storageRepository = storage;
            serviceBusRepository = sb;
            visitorIdentificationManager = idn;
        }

        /// <summary>
        /// Check the health of the service
        /// </summary>
        /// <returns>The status message</returns>
        /// <response code="200">Service is running</response>
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Get()
        {
            return Ok("{\"status\": \"Identification APIs working...\"}");
        }

        [HttpPost("create-group/{groupId}/{groupName}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreatePersonGroup(string groupId, string groupName)
        {
            // Validation of input
            await visitorIdentificationManager.CreateVisitorGroupAsync(groupId, groupName);
            return Ok("Group created successfully");
        }

        [HttpPost("create-visitor/{groupId}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(IdentifiedVisitor), 200)]
        public async Task<IActionResult> CreateVisitorAsync(string groupId, IFormCollection data)
        {
            //var visitorSample = new IdentifiedVisitor
            //{
            //    Age = 36,
            //    Company = "Microsoft",
            //    Photos = new System.Collections.Generic.List<VisitorPhoto> {
            //        new VisitorPhoto { IsSaved = false, Name = "photo1" },
            //        new VisitorPhoto { IsSaved = false, Name = "photo2" }
            //    },
            //    Name = "Mohamed Saif",
            //    ContactPhone = "123456789",
            //    Email = "name@company.com",
            //    Id = Guid.NewGuid().ToString(),
            //    IsConsentGranted = true,
            //    Origin = "Postman",
            //    Title = "Technical Architect"
            //};
            //var visitorSampleJson = JsonConvert.SerializeObject(visitorSample);

            var visitorJson = data["visitor"];
            var newVisitor = JsonConvert.DeserializeObject<IdentifiedVisitor>(visitorJson);
            newVisitor.Photos = new List<VisitorPhoto>();
            foreach(var photo in data.Files)
            {
                VisitorPhoto newPhoto = new VisitorPhoto
                {
                    Name = photo.FileName,
                    IsSaved = false,
                    Status = "Submitted"
                };
                var photoDataStream = new MemoryStream();
                photo.CopyTo(photoDataStream);
                newPhoto.PhotoData = photoDataStream.ToArray();
                newVisitor.Photos.Add(newPhoto);
            }

            //newVisitor = await visitorIdentificationManager.CreateVisitorAsync(newVisitor);

            return Ok(JsonConvert.SerializeObject(newVisitor));
        }
    }
}