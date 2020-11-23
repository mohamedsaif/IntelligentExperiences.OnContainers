using MaskDetector.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Services
{
    public class UrlBody
    {
        public string url { get; set; }
    }
    public interface IMaskDetectionAPI
    {
        [Multipart]
        [Post("/image")]
        //Task<MaskDetectionResult> DetectImage([Body(BodySerializationMethod.Default)] Dictionary<string, Stream> data);
        Task<MaskDetectionResult> DetectImage([AliasAs("imageData")] StreamPart imageData);
        
        [Post("/url")]
        Task<MaskDetectionResult> DetectImageUrl([Body] string url);

        [Get("/")]
        Task<string> GetStatus();
    }
}
