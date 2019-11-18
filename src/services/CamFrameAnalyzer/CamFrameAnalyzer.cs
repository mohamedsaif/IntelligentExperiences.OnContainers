using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using CamFrameAnalyzer.Models;

namespace CamFrameAnalyzer.Functions
{
    public static class CamFrameAnalyzer
    {
        static string key = string.Empty;
        static string endpoint = string.Empty;
        static IFaceClient faceClient;

        [FunctionName("CamFrameAnalyzer")]
        public static async Run(
            [ServiceBusTrigger("camframe-analysis", "camframe-analyzer", Connection = "SB_Connection")]string request, 
            ILogger log)
        {
            try
            {
                var cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }

            try
            {
                key = GlobalSettings.GetKeyValue("cognitiveKey");
                endpoint = GlobalSettings.GetKeyValue("cognitiveEndpoint");

                faceClient = new FaceClient(
                        new Microsoft.Azure.CognitiveServices.Vision.Face.ApiKeyServiceClientCredentials(key),
                        new System.Net.Http.DelegatingHandler[] { })
                        { Endpoint = endpoint };

                //var data = await filesStorageRepo.GetFileAsync(input.FileUrl);

                var frameAnalysis = new CamFrameAnalysis();
                

            }
            catch (Exception ex)
            {

            }

            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
        }

        public static async Task<CamFrameAnalysis> FaceDetection(CamFrameAnalysis input, ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Detection");

            IList<FaceAttributeType> faceAttributes = new FaceAttributeType[]
                                           {
                                                FaceAttributeType.Gender, FaceAttributeType.Age,
                                                FaceAttributeType.Smile, FaceAttributeType.Emotion,
                                                FaceAttributeType.Glasses, FaceAttributeType.Hair
                                           };
            try
            {
                using (Stream imageFileStream = new MemoryStream(data))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);

                    input.DetectedFaces = faceList;
                    input.IsSuccessfull = true;
                    input.Status = $"Detected Faces: {faceList.Count}";
                    return input;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException e)
            {
                log.LogError($"####### Failed to detect faces: {e.Message}");
                input.IsSuccessfull = false;
                input.Status = e.message;
                return input;
            }
        }
    }
}
