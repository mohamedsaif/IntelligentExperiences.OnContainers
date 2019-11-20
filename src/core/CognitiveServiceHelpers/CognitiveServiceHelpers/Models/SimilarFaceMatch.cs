using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServiceHelpers.Models
{
    public class SimilarFaceMatch
    {
        public DetectedFace Face
        {
            get; set;
        }

        public SimilarFace SimilarPersistedFace
        {
            get; set;
        }
    }
}
