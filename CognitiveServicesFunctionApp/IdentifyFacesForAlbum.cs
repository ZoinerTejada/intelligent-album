using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.ProjectOxford.Face.Contract;
using ImageResizer;
using CognitiveServiceHelpers;
using ImageResizer.ExtensionMethods;
using System.Text;

namespace CognitiveServicesFunctionApp
{
    public class IdentifyFacesForAlbum
    {
        public static async Task Run(Stream image, string filename, TraceWriter log)
        {
            log.Info($"{filename}");
            var imageBuilder = ImageBuilder.Current;
            byte[] imageSmall;
            // Resize incoming image to avoid Cognitive API errors.
            using (var stream = new MemoryStream())
            {
                imageBuilder.Build(
                    image, stream,
                    new ResizeSettings(1200, 1200, FitMode.Max, null), false);
                imageSmall = stream.ToArray();
            }

            // Set Face API key.
            FaceServiceHelper.ApiKey = SettingsHelper.GetEnvironmentVariable("FaceApiKey");
            // Set Vision API key.
            VisionServiceHelper.ApiKey = SettingsHelper.GetEnvironmentVariable("VisionApiKey");

            // Detect and identify faces.
            var imageWithFace = await AnalyzeImage(imageSmall);
            var analyzed = new AnalyzedImageData
            {
                AnalysisResult = imageWithFace.AnalysisResult,
                DetectedFaces = imageWithFace.DetectedFaces,
                IdentifiedPersons = imageWithFace.IdentifiedPersons,
                FileName = filename.EndsWith(".jpg") ? filename : $"{filename}.jpg"
            };

            // Save analyzed image data to our data store.http://localhost:16294/
            WriteToDatabase(analyzed);
        }

        private static async Task<ImageAnalyzer> AnalyzeImage(byte[] image)
        {
            var imageWithFace = new ImageAnalyzer(image);
            {
                if (imageWithFace.DetectedFaces == null)
                {
                    await imageWithFace.DetectFacesAsync(detectFaceAttributes: true, detectFaceLandmarks: false);
                }

                if (imageWithFace.IdentifiedPersons == null)
                {
                    await imageWithFace.IdentifyFacesAsync();
                }

                if (imageWithFace.AnalysisResult == null)
                {
                    await imageWithFace.IdentifyImageObjectsAsync();
                }
            }

            return imageWithFace;
        }

        private static void WriteToDatabase(AnalyzedImageData data)
        {
            var request = WebRequest.Create(SettingsHelper.GetEnvironmentVariable("SaveAlbumImageDataUrl"));

            request.Method = "POST";

            var postData = JsonConvert.SerializeObject(data);
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();
            dataStream = response.GetResponseStream();
            if (dataStream != null)
            {
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();

                reader.Close();
            }
            dataStream?.Close();
            response.Close();
        }

    }
}