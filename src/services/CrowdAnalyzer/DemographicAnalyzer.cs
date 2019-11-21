using CamFrameAnalyzer.Abstractions;
using CamFrameAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamFrameAnalyzer.Utils
{
    public class DemographicAnalyzer
    {
        //public async Task UpdateDemographics(CamFrameAnalysis analysis, IVisitorsRepository visitorRepo, ICamFrameAnalysisRepository camFrameRepo)
        //{
        //    var analysisTakenAt = analysis.Request.TakenAt;
            
        //    // aggregate window start from begging of the hour and continue aggregating till 59min:59sec.
        //    var currentAggregateWindow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);

        //    if (analysis.SimilarFaces != null)
        //    {
        //        // Update the Visitor collection (either add new entry or update existing)
        //        foreach (var item in analysis.SimilarFaces)
        //        {
        //            Visitor visitor;
        //            Guid persistedFaceId = item.SimilarPersistedFace.PersistedFaceId.GetValueOrDefault();
        //            var existingVisitor = await visitorRepo.GetByIdAsync(persistedFaceId.ToString());
        //            if (existingVisitor != null)
        //            {
        //                existingVisitor.VisitsCount++;
        //                existingVisitor.LastVisitDate = analysisTakenAt;
        //                await visitorRepo.UpdateAsync(existingVisitor);
        //                analysis.Summary.TotalSimilarFaces++;
        //            }
        //            else
        //            {
        //                visitor = new Visitor { Id = persistedFaceId.ToString(), VisitsCount = 1, LastVisitDate = analysisTakenAt };
        //                await visitorRepo.AddAsync(visitor);
                        
        //                // Upon adding a new visitor, this will change the associated demographics totals
        //                // We only do it for new visitors to avoid double counting. 
        //                AgeDistribution genderBasedAgeDistribution = null;
        //                if (item.Face.FaceAttributes.Gender == Gender.Male)
        //                {
        //                    this.demographics.OverallMaleCount++;
        //                    genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.MaleDistribution;
        //                }
        //                else
        //                {
        //                    this.demographics.OverallFemaleCount++;
        //                    genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.FemaleDistribution;
        //                }

        //                if (item.Face.FaceAttributes.Age < 16)
        //                {
        //                    genderBasedAgeDistribution.Age0To15++;
        //                }
        //                else if (item.Face.FaceAttributes.Age < 20)
        //                {
        //                    genderBasedAgeDistribution.Age16To19++;
        //                }
        //                else if (item.Face.FaceAttributes.Age < 30)
        //                {
        //                    genderBasedAgeDistribution.Age20s++;
        //                }
        //                else if (item.Face.FaceAttributes.Age < 40)
        //                {
        //                    genderBasedAgeDistribution.Age30s++;
        //                }
        //                else if (item.Face.FaceAttributes.Age < 50)
        //                {
        //                    genderBasedAgeDistribution.Age40s++;
        //                }
        //                else
        //                {
        //                    genderBasedAgeDistribution.Age50sAndOlder++;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
