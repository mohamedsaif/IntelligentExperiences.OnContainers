using CognitiveServiceHelpers.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServiceHelpers
{
    public class CognitiveFacesAnalyzer
    {
        public bool ShowDialogOnFaceApiErrors { get; set; } = false;
        public bool FilterOutSmallFaces { get; set; } = false;
        public int DecodedImageHeight { get; private set; }
        public int DecodedImageWidth { get; private set; }

        private static readonly FaceAttributeType[] DefaultFaceAttributeTypes = new FaceAttributeType[]
        {
            FaceAttributeType.Age,
            FaceAttributeType.Gender,
            FaceAttributeType.Emotion,
            FaceAttributeType.HeadPose
        };

        public event EventHandler FaceDetectionCompleted;
        public event EventHandler FaceRecognitionCompleted;

        public static string PeopleGroupsUserDataFilter = null;
        
        //These represent 4 options to load the analyzer file data
        public Func<Task<Stream>> GetImageStreamCallback { get; set; }
        public string ImageUrl { get; set; }
        public string LocalImagePath { get; set; }
        public byte[] Data { get; set; }

        public CognitiveFacesAnalyzer(string url)
        {
            this.ImageUrl = url;
        }

        public CognitiveFacesAnalyzer(Func<Task<Stream>> getStreamCallback, string path = null)
        {
            this.GetImageStreamCallback = getStreamCallback;
            this.LocalImagePath = path;
        }

        public CognitiveFacesAnalyzer(byte[] data)
        {
            this.Data = data;
            this.GetImageStreamCallback = () => Task.FromResult<Stream>(new MemoryStream(this.Data));
        }

        public IEnumerable<DetectedFace> DetectedFaces { get; set; }

        public IEnumerable<IdentifiedPerson> IdentifiedPersons { get; set; }

        public IEnumerable<SimilarFaceMatch> SimilarFaceMatches { get; set; }

        public static uint MinDetectableFaceCoveragePercentage = 0;

        public async Task DetectFacesAsync(bool detectFaceAttributes = false, bool detectFaceLandmarks = false)
        {
            try
            {
                if (this.ImageUrl != null)
                {
                    this.DetectedFaces = await FaceServiceHelper.DetectWithUrlAsync(
                        this.ImageUrl,
                        returnFaceId: true,
                        returnFaceLandmarks: detectFaceLandmarks,
                        returnFaceAttributes: detectFaceAttributes ? DefaultFaceAttributeTypes : null);
                }
                else if (this.GetImageStreamCallback != null)
                {
                    this.DetectedFaces = await FaceServiceHelper.DetectWithStreamAsync(
                        this.GetImageStreamCallback,
                        returnFaceId: true,
                        returnFaceLandmarks: detectFaceLandmarks,
                        returnFaceAttributes: detectFaceAttributes ? DefaultFaceAttributeTypes : null);
                }

                if (this.FilterOutSmallFaces)
                {
                    this.DetectedFaces = this.DetectedFaces.Where(f => IsFaceBigEnoughForDetection(f.FaceRectangle.Height, this.DecodedImageHeight));
                }
            }
            catch (Exception e)
            {
                ErrorTrackingHelper.TrackException(e, "Face API DetectAsync error");

                this.DetectedFaces = Enumerable.Empty<DetectedFace>();

                if (this.ShowDialogOnFaceApiErrors)
                {
                    await ErrorTrackingHelper.GenericApiCallExceptionHandler(e, "Face API failed.");
                }
            }
            finally
            {
                this.OnFaceDetectionCompleted();
            }
        }

        public async Task IdentifyFacesAsync()
        {
            this.IdentifiedPersons = Enumerable.Empty<IdentifiedPerson>();

            Guid[] detectedFaceIds = this.DetectedFaces?.Where(f => f.FaceId.HasValue).Select(f => f.FaceId.GetValueOrDefault()).ToArray();
            if (detectedFaceIds != null && detectedFaceIds.Any())
            {
                List<IdentifiedPerson> result = new List<IdentifiedPerson>();

                IEnumerable<PersonGroup> personGroups = Enumerable.Empty<PersonGroup>();
                try
                {
                    personGroups = await FaceServiceHelper.ListPersonGroupsAsync(PeopleGroupsUserDataFilter);
                }
                catch (Exception e)
                {
                    ErrorTrackingHelper.TrackException(e, "Face API GetPersonGroupsAsync error");

                    if (this.ShowDialogOnFaceApiErrors)
                    {
                        await ErrorTrackingHelper.GenericApiCallExceptionHandler(e, "Failure getting PersonGroups");
                    }
                }

                foreach (var group in personGroups)
                {
                    try
                    {
                        IList<IdentifyResult> groupResults = await FaceServiceHelper.IdentifyAsync(group.PersonGroupId, detectedFaceIds);
                        foreach (var match in groupResults)
                        {
                            if (!match.Candidates.Any())
                            {
                                continue;
                            }

                            Person person = await FaceServiceHelper.GetPersonAsync(group.PersonGroupId, match.Candidates[0].PersonId);

                            IdentifiedPerson alreadyIdentifiedPerson = result.FirstOrDefault(p => p.Person.PersonId == match.Candidates[0].PersonId);
                            if (alreadyIdentifiedPerson != null)
                            {
                                // We already tagged this person in another group. Replace the existing one if this new one if the confidence is higher.
                                if (alreadyIdentifiedPerson.Confidence < match.Candidates[0].Confidence)
                                {
                                    alreadyIdentifiedPerson.Person = person;
                                    alreadyIdentifiedPerson.Confidence = match.Candidates[0].Confidence;
                                    alreadyIdentifiedPerson.FaceId = match.FaceId;
                                }
                            }
                            else
                            {
                                result.Add(new IdentifiedPerson { Person = person, Confidence = match.Candidates[0].Confidence, FaceId = match.FaceId });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Catch errors with individual groups so we can continue looping through all groups. Maybe an answer will come from
                        // another one.
                        ErrorTrackingHelper.TrackException(e, "Face API IdentifyAsync error");

                        if (this.ShowDialogOnFaceApiErrors)
                        {
                            await ErrorTrackingHelper.GenericApiCallExceptionHandler(e, "Failure identifying faces");
                        }
                    }
                }

                this.IdentifiedPersons = result;
            }

            this.OnFaceRecognitionCompleted();
        }

        public async Task FindSimilarPersistedFacesAsync()
        {
            this.SimilarFaceMatches = Enumerable.Empty<SimilarFaceMatch>();

            if (this.DetectedFaces == null || !this.DetectedFaces.Any())
            {
                return;
            }

            List<SimilarFaceMatch> result = new List<SimilarFaceMatch>();

            foreach (DetectedFace detectedFace in this.DetectedFaces)
            {
                try
                {
                    SimilarFace similarPersistedFace = await FaceListManager.FindSimilarPersistedFaceAsync(this.GetImageStreamCallback, detectedFace.FaceId.GetValueOrDefault(), detectedFace);
                    if (similarPersistedFace != null)
                    {
                        result.Add(new SimilarFaceMatch { Face = detectedFace, SimilarPersistedFace = similarPersistedFace });
                    }
                }
                catch (Exception e)
                {
                    ErrorTrackingHelper.TrackException(e, "FaceListManager.FindSimilarPersistedFaceAsync error");

                    if (this.ShowDialogOnFaceApiErrors)
                    {
                        await ErrorTrackingHelper.GenericApiCallExceptionHandler(e, "Failure finding similar faces");
                    }
                }
            }

            this.SimilarFaceMatches = result;
        }

        private void OnFaceDetectionCompleted()
        {
            this.FaceDetectionCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void OnFaceRecognitionCompleted()
        {
            this.FaceRecognitionCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDecodedImageSize(int height, int width)
        {
            this.DecodedImageHeight = height;
            this.DecodedImageWidth = width;
        }

        public static bool IsFaceBigEnoughForDetection(int faceHeight, int imageHeight)
        {
            if (imageHeight == 0)
            {
                // sometimes we don't know the size of the image, so we assume the face is big enough
                return true;
            }

            double faceHeightPercentage = 100 * ((double)faceHeight / imageHeight);

            return faceHeightPercentage >= MinDetectableFaceCoveragePercentage;
        }
    }
}
