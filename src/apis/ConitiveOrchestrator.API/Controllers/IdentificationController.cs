using CognitiveOrchestrator.API.Models;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Controllers
{
    [Route("api/identification")]
    public class Identification : Controller
    {
        IStorageRepository storageRepository;
        IAzureServiceBusRepository serviceBusRepository;

        public Identification(IStorageRepository storage, IAzureServiceBusRepository sb)
        {
            storageRepository = storage;
            serviceBusRepository = sb;
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

        [HttpPost("{groupId}/{groupName}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CognitiveRequest), 200)]
        public async Task<IActionResult> CreatePersonGroup(string groupId, string groupName)
        {
            // Validation of input
            
            return Ok("Group created successfully");
        }

        [HttpPost("{deviceId}/{docType}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CognitiveRequest), 200)]
        public async Task<IActionResult> SubmitDoc(string deviceId, string docType, [FromForm]string docUTCTime, IFormFile doc)
        {
            // Validation of input
            if (doc == null || doc.Length == 0)
                return BadRequest("file not selected");

            var proposedDocType = CognitiveTargetAction.Unidentified;
            var isValidType = Enum.TryParse<CognitiveTargetAction>(docType, out proposedDocType);
            if (!isValidType || proposedDocType == CognitiveTargetAction.Unidentified)
                return BadRequest("Invalid document type");
            DateTime takenAt = DateTime.UtcNow;
            var isValidTimeProvided = DateTime.TryParse(docUTCTime, out takenAt);

            if (!isValidTimeProvided)
                takenAt = DateTime.UtcNow;

            long size = doc.Length;

            // full path to file in temp location
            string docName = "NA";
            string docUri = null;

            if (size > 0)
            {
                using (var stream = doc.OpenReadStream())
                {
                    var docExtention = doc.FileName.Substring(doc.FileName.LastIndexOf('.'));
                    docName = $"{deviceId}-{takenAt.ToString("ddMMyyHHmmss")}{docExtention}";
                    docUri = await storageRepository.CreateFileAsync(docName, stream);
                }
            }
            else
            {
                return BadRequest("Submitted file size is 0");
            }

            CognitiveRequest req = new CognitiveRequest
            {
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceId,
                FileUrl = docName,
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                IsDeleted = false,
                IsProcessed = false,
                Origin = "CognitiveOrchestrator.API.V1.0.0",
                Status = "Submitted",
                TakenAt = takenAt,
                TargetAction = proposedDocType.ToString()
            };

            var sbMessage = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
            var result = await serviceBusRepository.PublishMessage(sbMessage);

            return Ok(req);
        }
    }
}