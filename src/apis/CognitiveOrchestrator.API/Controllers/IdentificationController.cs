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
    /// <summary>
    /// Provides set of APIs to manage the identification capabilities for the platform (Visitors Groups and Identified Visitors)
    /// </summary>
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
        /// <returns>The status message of the service</returns>
        /// <response code="200">Running Status</response>
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

        /// <summary>
        /// Get Visitors-Group by Id
        /// </summary>
        /// <param name="groupId">Id of the group to be retrieved</param>
        /// <returns>Matched IdentifiedVisitorGroup</returns>
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

        /// <summary>
        /// Get Visitors-Group by Name
        /// </summary>
        /// <param name="groupName">Name of the group to be retrieved</param>
        /// <returns>Matched IdentifiedVisitorGroup</returns>
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

        /// <summary>
        /// Get All Visitors-Groups
        /// </summary>
        /// <returns>List of IdentifiedVisitorGroup</returns>
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

        /// <summary>
        /// Creates new visitors group
        /// </summary>
        /// <param name="groupName">Name of the new group (must be unique)</param>
        /// <returns>Newly created IdentifiedVisitorGroup</returns>
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

        /// <summary>
        /// Submit new Visitors-Group training request
        /// </summary>
        /// <param name="groupId">Id of the group to be trained</param>
        /// <returns>Status message of the training</returns>
        /// <remarks>Training should be called after adding/updating visitors. Call is synchronous</remarks>
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

        /// <summary>
        /// Delete an existing Visitors-Group
        /// </summary>
        /// <param name="groupId">Id of the group to be deleted</param>
        /// <returns>Status message of the deletion</returns>
        /// <remarks>Delete an existing Visitors-Group with its all visitors. This is a permanent operation and can't be reversed</remarks>
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

        /// <summary>
        /// Creates new Identified-Visitor
        /// </summary>
        /// <param name="data">Array of IForm data. [0] in array must be the IdentifiedVisitor json with key "visitor". [1-N] include binary files for person photos</param>
        /// <returns>Newly created IdentifiedVisitor</returns>
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

        /// <summary>
        /// Get Identified-Visitor by Id 
        /// </summary>
        /// <param name="id">Id of the visitor to be retrieved</param>
        /// <returns>Marched IdentifiedVisitor</returns>
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

        /// <summary>
        /// Retrieve list of all Identified-Visitors
        /// </summary>
        /// <returns>List of IdentifiedVisitor</returns>
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

        /// <summary>
        /// Update existing Identified-Visitor
        /// </summary>
        /// <param name="data">Array of IForm data. [0] in array must be the IdentifiedVisitor json with key "visitor". [1-N] include binary files for person photos</param>
        /// <returns>Updated IdentifiedVisitor</returns>
        [HttpPost("visitors/update")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(IdentifiedVisitor), 200)]
        public async Task<IActionResult> UpdateVisitor(IFormCollection data)
        {
            var visitorJson = data["visitor"];
            var visitor = JsonConvert.DeserializeObject<IdentifiedVisitor>(visitorJson);
            if(visitor.Photos == null)
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
            var result = JsonConvert.SerializeObject(visitor);
            return Ok(result);
        }
    }
}