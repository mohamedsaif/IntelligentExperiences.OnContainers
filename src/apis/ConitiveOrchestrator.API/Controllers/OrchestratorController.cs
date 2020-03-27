using CognitiveOrchestrator.API.Models;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Controllers
{
    [Route("api/orchestrator")]
    public class OrchestratorController : Controller
    {
        IStorageRepository storageRepository;
        IAzureServiceBusRepository serviceBusRepository;

        public OrchestratorController(IStorageRepository storage, IAzureServiceBusRepository sb)
        {
            storageRepository = storage;
            serviceBusRepository = sb;
        }

        /// <summary>
        /// Check the health of the service
        /// </summary>
        /// <returns>The status message</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("{\"status\": \"Orchestrator apis working...\"}");
        }

        /// <summary>
        /// Uploads a document to Azure storage
        /// </summary>
        /// <returns>The result of the uploaded document</returns>
        /// <param name="ownerId">Document owner Id</param>
        /// <param name="docType">One of the following ID, StoreShelf, Face, Generic, Unidentified which will determine the cognitive operations to be executed</param>
        /// <param name="isAsync">Flag to indicate if operations need to execute immediately or will be queued</param>
        /// <param name="doc">The binary of the document being processed</param>
        [HttpPost("{ownerId}/{docType}/{isAsync}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitDoc(string deviceId, string docType, IFormFile doc)
        {

            if (doc == null || doc.Length == 0)
                return BadRequest("file not selected");

            var proposedDocType = CognitiveTargetAction.Unidentified;
            var isValidType = Enum.TryParse<CognitiveTargetAction>(docType, out proposedDocType);
            if (!isValidType || proposedDocType == CognitiveTargetAction.Unidentified)
                return BadRequest("Invalid document type");

            long size = doc.Length;

            // full path to file in temp location
            string docName = "NA";
            string docUri = null;

            if (size > 0)
            {
                using (var stream = doc.OpenReadStream())
                {
                    var docExtention = doc.FileName.Substring(doc.FileName.LastIndexOf('.'));
                    docName = $"{deviceId}-{DateTime.UtcNow.ToString("ddMMyyHHmmss")}{docExtention}";
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
                TakenAt = DateTime.UtcNow,
                TargetAction = proposedDocType.ToString()
            };

            var result = await serviceBusRepository.PublishMessage(new Microsoft.Azure.ServiceBus.Message());

            return Ok(result);
        }
    }
}