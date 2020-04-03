using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cam.Device.Web.AI
{
    public class FaceDetector
    {
        public static FaceDetectionResult DetectFaces(string imageName, string cascadeModelPath)
        {
            var cascade = new CascadeClassifier(cascadeModelPath);

            FaceDetectionResult result = null;

            using (var src = new Mat(imageName, ImreadModes.Color))
            using (var gray = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                // Detect faces
                Rect[] faces = cascade.DetectMultiScale(
                    gray, 1.08, 2, HaarDetectionType.ScaleImage, new Size(30, 30));

                result = new FaceDetectionResult { DetectedFacesFrames = faces };
            }

            return result;
        }
    }

    public class FaceDetectionResult
    {
        public Rect[] DetectedFacesFrames { get; set; }
    }
}
