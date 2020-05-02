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
        Task CreateVisitorGroupAsync(string groupId, string groupName);
        Task<List<IdentifiedVisitor>> GetIdentifiedVisitorsAsync();
        Task TrainVisitorGroup(string groupId, bool waitForTrainingToComplete);
    }
}