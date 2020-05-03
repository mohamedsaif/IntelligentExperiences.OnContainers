using CognitiveOrchestrator.API.Models;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Models;
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

        [HttpPost("{groupId}/{groupName}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreatePersonGroup(string groupId, string groupName)
        {
            // Validation of input
            await visitorIdentificationManager.CreateVisitorGroupAsync(groupId, groupId);
            return Ok("Group created successfully");
        }

        [HttpPost()]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CognitiveRequest), 200)]
        public async Task<IActionResult> SubmitDoc([FromForm]string visitor, IFormFile doc)
        {
            
            return Ok("Visitor created successfully");
        }
    }
}