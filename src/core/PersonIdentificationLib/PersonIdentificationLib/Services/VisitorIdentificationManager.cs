using CognitiveServiceHelpers;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Models;
using PersonIdentificationLib.Repos;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Services
{
    public class VisitorIdentificationManager : IVisitorIdentificationManager
    {
        private string key = string.Empty;
        private string endpoint = string.Empty;
        private string faceWorkspaceDataFilter;

        private IStorageRepository filesStorageRepo;

        //private ICamFrameAnalysisRepository camFrameAnalysisRepo;

        private IAzureServiceBusRepository serviceBusRepo;
        private CognitiveFacesAnalyzer cognitiveFacesAnalyzer;
        private IdentifiedVisitorRepo identifiedVisitorRepo;
        private IdentifiedVisitorGroupRepo identifiedVisitorGroupRepo;
        private AzureBlobStorageRepository storageRepo;

        private byte[] photoData;

        List<FaceAttributeType> faceAttributes = new List<FaceAttributeType> { FaceAttributeType.Age, FaceAttributeType.Gender };

        public VisitorIdentificationManager(string cognitiveKey,
            string cognitiveEndpoint,
            string faceFilter,
            string cosmosDbEndpoint,
            string cosmosDbKey,
            string cosmosDbName,
            string storageConnection,
            string storageContainerName)
        {
            key = cognitiveKey;
            endpoint = cognitiveEndpoint;
            faceWorkspaceDataFilter = faceFilter;

            FaceServiceHelper.ApiKey = key;
            FaceServiceHelper.ApiEndpoint = endpoint;
            FaceListManager.FaceListsUserDataFilter = faceWorkspaceDataFilter;

            var dbClient = new DocumentClient(
                new Uri(cosmosDbEndpoint),
                cosmosDbKey,
                new ConnectionPolicy { EnableEndpointDiscovery = false });

            var dbFactory = new CosmosDbClientFactory(
                    cosmosDbName,
                    new Dictionary<string, string> { 
                        { AppConstants.DbColIdentifiedVisitor, AppConstants.DbColIdentifiedVisitorPartitionKey },
                        { AppConstants.DbColIdentifiedVisitorGroup, AppConstants.DbColIdentifiedVisitorPartitionKey }
                    },
                    dbClient);
            identifiedVisitorRepo = new IdentifiedVisitorRepo(dbFactory);
            identifiedVisitorGroupRepo = new IdentifiedVisitorGroupRepo(dbFactory);

            storageRepo = new AzureBlobStorageRepository(storageConnection, storageContainerName);

            //serviceBusRepo = new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
        }

        public async Task<IdentifiedVisitorGroup> CreateVisitorsGroupAsync(string groupName)
        {
            IdentifiedVisitorGroup result = null;
            //var isValidGroupId = Regex.IsMatch(groupId, "^[a-z0-9-_]+");
            //if (!isValidGroupId)
            //    throw new InvalidExpressionException("Group id must be only alpha numeric letters with - and _");
            try
            {
                var newItem = new IdentifiedVisitorGroup
                {
                    Name = groupName,
                    Filter = faceWorkspaceDataFilter,
                    PartitionKey = AppConstants.DbColIdentifiedVisitorPartitionKeyValue,
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow, 
                    Origin = AppConstants.Origin
                };

                result = await identifiedVisitorGroupRepo.AddAsync(newItem);
                result.GroupId = result.Id.ToLower();
                await FaceServiceHelper.CreatePersonGroupAsync(result.GroupId, newItem.Name, newItem.Filter);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public async Task<IdentifiedVisitor> CreateVisitorAsync(IdentifiedVisitor identifiedVisitor)
        {
            //TODO: Validate if the visitor exists

            var cognitivePerson = await FaceServiceHelper.CreatePersonAsync(identifiedVisitor.GroupId, identifiedVisitor.Id);
            identifiedVisitor.PersonDetails = cognitivePerson;
            double? age = null;
            Gender? gender = null;
            foreach (var photo in identifiedVisitor.Photos)
            {
                if (!photo.IsSaved)
                {
                    photoData = photo.PhotoData;
                    var photoFileExtension = Path.GetExtension(photo.Name);
                    var newPhotoFileName = $"{identifiedVisitor.Id}-{identifiedVisitor.Photos.IndexOf(photo) + 1}{photoFileExtension}";
                    
                    //Only accept photos with single face
                    var detectedFaces = await FaceServiceHelper.DetectWithStreamAsync(GetPhotoStream, returnFaceAttributes: faceAttributes);
                    if (detectedFaces.Count == 0)
                    {
                        photo.Status = "Invalid: No faces detected in photo";
                        continue;
                    }
                    else if (detectedFaces.Count > 1)
                    {
                        photo.Status = "Invalid: More than 1 face detected in photo. Only photos with single face can be used to train";
                        continue;
                    }

                    //Upload the new photo to storage
                    photo.Url = await storageRepo.CreateFileAsync(newPhotoFileName, photo.PhotoData);
                    age = detectedFaces[0].FaceAttributes.Age;
                    gender = detectedFaces[0].FaceAttributes.Gender;
                    var persistedFace = await AddVisitorPhotoAsync(identifiedVisitor.GroupId, cognitivePerson.PersonId, photo.Url, detectedFaces[0].FaceRectangle);

                    //Update photo details
                    photo.IsSaved = true;
                    photo.Name = newPhotoFileName;
                    photo.Status = "Saved";
                }
            }

            //Save the new identified visitor details to database
            identifiedVisitor.Age = age.HasValue ? age.Value : 0;
            identifiedVisitor.Gender = gender.HasValue ? gender.ToString() : "NA";
            identifiedVisitor.PartitionKey = string.IsNullOrEmpty(identifiedVisitor.PartitionKey) ? "Default" : identifiedVisitor.PartitionKey;
            var result = await identifiedVisitorRepo.AddAsync(identifiedVisitor);

            return result;
        }

        public async Task<PersistedFace> AddVisitorPhotoAsync(string groupId, Guid cognitivePersonId, string photoUrl, FaceRectangle faceRect)
        {
            var persistedFace = await FaceServiceHelper.AddPersonFaceFromStreamAsync(groupId, cognitivePersonId, GetPhotoStream, photoUrl, faceRect);
            return persistedFace;
        }
        
        public async Task<IdentifiedVisitor> GetVisitorByIdAsync(string id)
        {
            var visitor = await identifiedVisitorRepo.GetByIdAsync(id);
            return visitor;
        }

        public async Task<IdentifiedVisitor> GetVisitorByPersonIdAsync(Guid personId)
        {
            var result = await identifiedVisitorRepo.QueryDocuments(
                "visitor", 
                "visitor.PersonDetails.PersonId=@PersonId", 
                new SqlParameterCollection { 
                    new SqlParameter { Name = "@PersonId", Value = personId } 
                });
            
            if(result.Any())
            {
                return result[0];
            }

            return null;
        }

        public async Task<IdentifiedVisitor> UpdateVisitorAsync(IdentifiedVisitor identifiedVisitor)
        {
            if (identifiedVisitor == null)
                throw new InvalidDataException("No visitor data");

            if (identifiedVisitor.Photos != null && identifiedVisitor.Photos.Count > 0)
            {
                double? age = null;
                Gender? gender = null;

                foreach (var photo in identifiedVisitor.Photos)
                {
                    if (!photo.IsSaved)
                    {
                        photoData = photo.PhotoData;
                        var photoFileExtension = Path.GetExtension(photo.Name);
                        var newPhotoFileName = $"{identifiedVisitor.Id}-{identifiedVisitor.Photos.IndexOf(photo) + 1}{photoFileExtension}";

                        //Only accept photos with single face
                        var detectedFaces = await FaceServiceHelper.DetectWithStreamAsync(GetPhotoStream, returnFaceAttributes: faceAttributes);
                        if (detectedFaces.Count == 0)
                        {
                            photo.Status = "Invalid: No faces detected in photo";
                            continue;
                        }
                        else if (detectedFaces.Count > 1)
                        {
                            photo.Status = "Invalid: More than 1 face detected in photo. Only photos with single face can be used to train";
                            continue;
                        }

                        //Upload the new photo to storage
                        photo.Url = await storageRepo.CreateFileAsync(newPhotoFileName, photo.PhotoData);
                        age = detectedFaces[0].FaceAttributes.Age;
                        gender = detectedFaces[0].FaceAttributes.Gender;
                        var persistedFace = await AddVisitorPhotoAsync(identifiedVisitor.GroupId, identifiedVisitor.PersonDetails.PersonId, photo.Url, detectedFaces[0].FaceRectangle);

                        //Update photo details
                        photo.IsSaved = true;
                        photo.Name = newPhotoFileName;
                        photo.Status = "Saved";
                    }
                }
            }
            await identifiedVisitorRepo.UpdateAsync(identifiedVisitor);
            return identifiedVisitor;
        }

        public async Task TrainVisitorGroup(string groupId, bool waitForTrainingToComplete)
        {
            //var group = await identifiedVisitorGroupRepo.GetByIdAsync(groupId);
            //if (group != null)
            //{
                await FaceServiceHelper.TrainPersonGroupAsync(groupId);
                TrainingStatus trainingStatus = null;
                while (waitForTrainingToComplete)
                {
                    trainingStatus = await GetVisitorsGroupTrainingStatusAsync(groupId);

                    if (trainingStatus.Status != TrainingStatusType.Running)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                //group.LastTrainingDate = DateTime.UtcNow;
                //await identifiedVisitorGroupRepo.UpdateAsync(group);
            //}
            //else
            //{
            //    throw new KeyNotFoundException($"Group ({groupId}) not found");
            //}
        }

        public async Task<TrainingStatus> GetVisitorsGroupTrainingStatusAsync(string groupId)
        {
            return await FaceServiceHelper.GetPersonGroupTrainingStatusAsync(groupId);
        }

        public async Task<List<IdentifiedVisitor>> GetIdentifiedVisitorsAsync()
        {
            return await identifiedVisitorRepo.GetAllAsync();
        }

        public async Task<ResultStatus> DeleteVisitorsGroup(string groupId)
        {
            var result = new ResultStatus();
            var group = await identifiedVisitorGroupRepo.GetByIdAsync(groupId);
            if (group != null)
            {
                var visitorsInGroup = (await identifiedVisitorRepo.GetAllAsync()).Where(v => v.GroupId == group.Id);
                foreach (var visitor in visitorsInGroup)
                    await identifiedVisitorRepo.DeleteAsync(visitor);

                await FaceServiceHelper.DeletePersonGroupAsync(groupId);
                await identifiedVisitorGroupRepo.DeleteAsync(group);
                result.StatusCode = "0";
                result.Message = $"Successfully deleted group ({group.Id}) and the associated visitors ({visitorsInGroup.Count()}";
                result.IsSuccessful = true;
            }
            else
            {
                result.StatusCode = "1";
                result.Message = $"Group id ({group.Id}) not found!";
                result.IsSuccessful = false;
            }

            return result;
        }

        public async Task<ResultStatus> DeleteVisitorAsync(string visitorId, string groupId)
        {
            var result = new ResultStatus();
            var visitor = await identifiedVisitorRepo.GetByIdAsync(visitorId);
            if(visitor != null)
            {
                await FaceServiceHelper.DeletePersonAsync(groupId, visitor.PersonDetails.PersonId);
                await identifiedVisitorRepo.DeleteAsync(visitor);
                result.StatusCode = "0";
                result.Message = $"Successfully deleted visitor ({visitor.Id})";
                result.IsSuccessful = true;
            }
            else
            {
                result.StatusCode = "1";
                result.Message = $"Visitor id ({visitor.Id}) not found!";
                result.IsSuccessful = false;
            }

            return result;
        }

        private async Task<Stream> GetPhotoStream()
        {
            return new MemoryStream(photoData);
        }
    }
}
