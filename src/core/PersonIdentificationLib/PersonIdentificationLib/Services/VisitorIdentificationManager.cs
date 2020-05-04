using CognitiveServiceHelpers;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.Documents.Client;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Models;
using PersonIdentificationLib.Repos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            var dbVisitorFactory = new CosmosDbClientFactory(
                    cosmosDbName,
                    new Dictionary<string, string> { { AppConstants.DbColIdentifiedVisitor, AppConstants.DbColIdentifiedVisitorPartitionKey } },
                    dbClient);
            identifiedVisitorRepo = new IdentifiedVisitorRepo(dbVisitorFactory, AppConstants.DbColIdentifiedVisitor);

            var dbVisitorGroupFactory = new CosmosDbClientFactory(
                    cosmosDbName,
                    new Dictionary<string, string> { { AppConstants.DbColIdentifiedVisitor, AppConstants.DbColIdentifiedVisitorPartitionKey } },
                    dbClient);
            identifiedVisitorGroupRepo = new IdentifiedVisitorGroupRepo(dbVisitorGroupFactory, AppConstants.DbColIdentifiedVisitorGroup);

            storageRepo = new AzureBlobStorageRepository(storageConnection, storageContainerName);

            //serviceBusRepo = new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
        }

        public async Task<IdentifiedVisitorGroup> CreateVisitorGroupAsync(string groupId, string groupName)
        {
            IdentifiedVisitorGroup result = null;

            try
            {
                var newItem = new IdentifiedVisitorGroup
                {
                    GroupId = groupId,
                    Name = groupName,
                    Filter = faceWorkspaceDataFilter
                };

                await FaceServiceHelper.CreatePersonGroupAsync(newItem.GroupId, newItem.Name, newItem.Filter);

                result = await identifiedVisitorGroupRepo.AddAsync(newItem);
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

            foreach (var photo in identifiedVisitor.Photos)
            {
                if (!photo.IsSaved)
                {
                    photoData = photo.PhotoData;
                    var photoFileExtension = Path.GetExtension(photo.Name);
                    var newPhotoFileName = $"{identifiedVisitor.Id}-{identifiedVisitor.Photos.IndexOf(photo) + 1}-{photoFileExtension}";

                    //Upload the new photo to storage
                    var photoUrl = await storageRepo.CreateFileAsync(newPhotoFileName, photo.PhotoData);

                    //Only accept photos with single face
                    var detectedFaces = await FaceServiceHelper.DetectWithStreamAsync(GetPhotoStream);
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

                    await AddVisitorPhotoAsync(identifiedVisitor.Id, cognitivePerson.PersonId, photo.Url, detectedFaces[0].FaceRectangle);

                    //Update photo details
                    photo.IsSaved = true;
                    photo.Url = photoUrl;
                    photo.Name = newPhotoFileName;
                }
            }

            //Save the new identified visitor details to database
            var result = await identifiedVisitorRepo.AddAsync(identifiedVisitor);

            return result;
        }

        public async Task<PersistedFace> AddVisitorPhotoAsync(string groupId, Guid cognitivePersonId, string photoUrl, FaceRectangle faceRect)
        {
            var persistedFace = await FaceServiceHelper.AddPersonFaceFromStreamAsync(groupId, cognitivePersonId, GetPhotoStream, photoUrl, faceRect);
            return persistedFace;
        }

        public async Task TrainVisitorGroup(string groupId, bool waitForTrainingToComplete)
        {
            await FaceServiceHelper.TrainPersonGroupAsync(groupId);
            TrainingStatus trainingStatus = null;
            while (waitForTrainingToComplete)
            {
                trainingStatus = await FaceServiceHelper.GetPersonGroupTrainingStatusAsync(groupId);

                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        public async Task<List<IdentifiedVisitor>> GetIdentifiedVisitorsAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<Stream> GetPhotoStream()
        {
            return new MemoryStream(photoData);
        }
    }
}
