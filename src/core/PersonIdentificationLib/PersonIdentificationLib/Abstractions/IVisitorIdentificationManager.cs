using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using PersonIdentificationLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Abstractions
{
    public interface IVisitorIdentificationManager
    {
        //Visitors
        Task<IdentifiedVisitor> CreateVisitorAsync(IdentifiedVisitor identifiedVisitor);
        //Task<PersistedFace> AddVisitorPhotoAsync(string groupId, Guid cognitivePersonId, string photoUrl, FaceRectangle faceRect);
        Task<IdentifiedVisitor> UpdateVisitorAsync(IdentifiedVisitor identifiedVisitor);
        Task<IdentifiedVisitor> GetVisitorByIdAsync(string id);
        Task<IdentifiedVisitor> GetVisitorByPersonIdAsync(Guid personId);
        Task<ResultStatus> DeleteVisitorAsync(string visitorId, string groupId);

        //Visitors Group
        Task<IdentifiedVisitorGroup> CreateVisitorsGroupAsync(string groupName);
        Task<List<IdentifiedVisitor>> GetIdentifiedVisitorsAsync();
        Task TrainVisitorGroup(string groupId, bool waitForTrainingToComplete);
        Task<TrainingStatus> GetVisitorsGroupTrainingStatusAsync(string groupId);
        Task<ResultStatus> DeleteVisitorsGroup(string groupId);
    }
}