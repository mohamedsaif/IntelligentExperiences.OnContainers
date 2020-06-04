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
        [ProducesResponseType(typeof(BaseResponse), 200)]
        public IActionResult Get()
        {
            return Ok(new BaseResponse
            {
                IsSuccessful = true,
                Message = "Service is running...",
                StatusCode = "0"
            });
        }

        // Visitors groups
        [HttpGet("groups/getById/{groupId}")]
        [ProducesResponseType(typeof(IdentifiedVisitorGroup), 200)]
        public async Task<IActionResult> GetVisitrosGroupById(string groupId)
        {
            try
            {
                var result = await visitorIdentificationManager.GetVisitorsGroupByIdAsync(groupId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpGet("groups/getByName/{groupName}")]
        [ProducesResponseType(typeof(IdentifiedVisitorGroup), 200)]
        public async Task<IActionResult> GetVisitrosGroupByName(string groupName)
        {
            try
            {
                var result = await visitorIdentificationManager.GetVisitorsGroupByNameAsync(groupName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpGet("groups/getAll")]
        [ProducesResponseType(typeof(List<IdentifiedVisitorGroup>), 200)]
        public async Task<IActionResult> GetAllVisitorsGroups()
        {
            try
            {
                var result = await visitorIdentificationManager.GetAllVisitorsGroupsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpPost("groups/create/{groupName}")]
        [ProducesResponseType(typeof(IdentifiedVisitorGroup), 200)]
        public async Task<IActionResult> CreateVisitorsGroup(string groupName)
        {
            try
            {
                var result = await visitorIdentificationManager.CreateVisitorsGroupAsync(groupName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpPost("groups/train/{groupId}")]
        [ProducesResponseType(typeof(BaseResponse), 200)]
        public async Task<IActionResult> TrainVisitorsGroup(string groupId)
        {
            try
            {
                await visitorIdentificationManager.TrainVisitorGroup(groupId, true);
                return Ok(new BaseResponse
                {
                    IsSuccessful = true,
                    Message = "Training successfully completed.",
                    StatusCode = "0"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
            
        }

        [HttpPost("groups/delete/{groupId}")]
        [ProducesResponseType(typeof(BaseResponse), 200)]
        public async Task<IActionResult> DeleteVisitorsGroup(string groupId)
        {
            // Validation of input
            var result = await visitorIdentificationManager.DeleteVisitorsGroup(groupId);
            if (result.IsSuccessful)
            {
                return Ok(new BaseResponse
                {
                    IsSuccessful = true,
                    Message = "Group deleted successfully completed.",
                    StatusCode = "0"
                });
            }

            return BadRequest(new BaseResponse
            {
                IsSuccessful = false,
                Message = result.Message,
                StatusCode = result.StatusCode,
                ErrorDetails = result.ErrorDetails
            });
        }

        // Visitors
        [HttpPost("visitors/create")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(IdentifiedVisitor), 200)]
        public async Task<IActionResult> CreateVisitor(IFormCollection data)
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
            //    Title = "Technical Architect",
            //    IsActive = true
            //};
            //var visitorSampleJson = JsonConvert.SerializeObject(visitorSample);

            var visitorJson = data["visitor"];
            var newVisitor = JsonConvert.DeserializeObject<IdentifiedVisitor>(visitorJson);
            newVisitor.Photos = new List<VisitorPhoto>();
            foreach (var photo in data.Files)
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

            newVisitor = await visitorIdentificationManager.CreateVisitorAsync(newVisitor);

            return Ok(JsonConvert.SerializeObject(newVisitor));
        }

        [HttpGet("visitors/getById/{id}")]
        [ProducesResponseType(typeof(IdentifiedVisitor), 200)]
        public async Task<IActionResult> GetVisitorById(string id)
        {
            try
            {
                var result = await visitorIdentificationManager.GetVisitorByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpGet("visitors/getAll")]
        [ProducesResponseType(typeof(List<IdentifiedVisitor>), 200)]
        public async Task<IActionResult> GetAllVisitors()
        {
            try
            {
                var result = await visitorIdentificationManager.GetAllIdentifiedVisitorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    StatusCode = "1"
                });
            }
        }

        [HttpPost("visitors/update")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(IdentifiedVisitor), 200)]
        public async Task<IActionResult> UpdateVisitor(IFormCollection data)
        {
            var visitorJson = data["visitor"];
            var visitor = JsonConvert.DeserializeObject<IdentifiedVisitor>(visitorJson);
            visitor.Photos = new List<VisitorPhoto>();
            foreach (var photo in data.Files)
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
                visitor.Photos.Add(newPhoto);
            }

            visitor = await visitorIdentificationManager.UpdateVisitorAsync(visitor);

            return Ok(visitor);
        }
    }
}