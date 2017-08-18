using System.Collections.Generic;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServiceHelpers
{
    public class AnalyzedImageData
    {
        public string FileName { get; set; }
        public IEnumerable<Face> DetectedFaces { get; set; }

        public IEnumerable<IdentifiedPerson> IdentifiedPersons { get; set; }

        public Microsoft.ProjectOxford.Vision.Contract.AnalysisResult AnalysisResult { get; set; }
    }
}
