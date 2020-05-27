using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using PersonIdentificationLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Abstractions
{
    public interface IVisitorIdentificationManager
    {
        Task<PersistedFace> AddVisitorPhotoAsync(string groupId, Guid cognitivePersonId, string photoUrl, FaceRectangle faceRect);
        Task<IdentifiedVisitor> CreateVisitorAsync(IdentifiedVisitor identifiedVisitor);
        Task<IdentifiedVisitorGroup> CreateVisitorsGroupAsync(string groupName);
        Task<ResultStatus> DeleteVisitorAsync(string visitorId, string groupId);
        Task<ResultStatus> DeleteVisitorsGroup(string groupId);
        Task<List<IdentifiedVisitor>> GetAllIdentifiedVisitorsAsync();
        Task<List<IdentifiedVisitorGroup>> GetAllVisitorsGroupsAsync();
        Task<IdentifiedVisitor> GetVisitorByIdAsync(string id);
        Task<IdentifiedVisitor> GetVisitorByPersonIdAsync(Guid personId);
        Task<IdentifiedVisitorGroup> GetVisitorsGroupByIdAsync(string groupId);
        Task<IdentifiedVisitorGroup> GetVisitorsGroupByNameAsync(string groupName);
        Task<TrainingStatus> GetVisitorsGroupTrainingStatusAsync(string groupId);
        Task TrainVisitorGroup(string groupId, bool waitForTrainingToComplete);
        Task<IdentifiedVisitor> UpdateVisitorAsync(IdentifiedVisitor identifiedVisitor);
    }
}