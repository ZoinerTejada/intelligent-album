using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CognitiveServiceHelpers;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using Dapper;
using PhotoAlbum.DTO;
using PhotoAlbum.Data;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Store.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace CognitiveServicesFunctionApp
{
    public class SaveAlbumImageData
    {
        private static DataLakeStoreAccountManagementClient _adlsClient;
        private static DataLakeStoreFileSystemManagementClient _adlsFileSystemClient;

        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            // Retrieve Object and Convert.
            var myData = await req.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(myData))
                return req.CreateResponse(HttpStatusCode.BadRequest, "A valid AnalyzedImageData data object must be passed to this function.");

            var imageData = JsonConvert.DeserializeObject<AnalyzedImageData>(myData);

            // Debugging 
            log.Info($"Filename: {imageData.FileName}");

            await Authenticate();
            await SaveDataToAdls(PopulateImageFromAnalyzedImageData(imageData));

            //UpdateDatabase(imageData);

            return req.CreateResponse(HttpStatusCode.Created);
        }

        private static async Task SaveDataToAdls(Image imageData)
        {
            // Round current date/time to nearest 15 minutes.
            var date = RoundToNearest(DateTime.UtcNow, TimeSpan.FromMinutes(15));
            var filePath = $"PhotoData/{date:yyyy}/{date:MM}/{date:dd}/{date:HH}/{date:mm}/{imageData.ImageName}.json";
            var json = JsonConvert.SerializeObject(imageData);
            // Convert string to stream.
            var byteArray = Encoding.ASCII.GetBytes(json);
            var stream = new MemoryStream(byteArray);
            // Upload to ADLS.
            await _adlsFileSystemClient.FileSystem.CreateAsync(SettingsHelper.GetEnvironmentVariable("ADLSAccountName"), filePath, stream, overwrite: true);
        }

        private static async Task Authenticate()
        {
            var clientCredential = new ClientCredential(SettingsHelper.GetEnvironmentVariable("WebApp_ClientId"), SettingsHelper.GetEnvironmentVariable("ClientSecret"));
            var creds = await ApplicationTokenProvider.LoginSilentAsync(SettingsHelper.GetEnvironmentVariable("Domain"), clientCredential);

            // Create client objects and set the subscription ID.
            _adlsClient = new DataLakeStoreAccountManagementClient(creds) { SubscriptionId = SettingsHelper.GetEnvironmentVariable("SubId") };
            _adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(creds);
        }

        private static Image PopulateImageFromAnalyzedImageData(AnalyzedImageData imageData)
        {
            var newDescription = imageData.AnalysisResult?.Description?.Captions[0]?.Text;

            var image = new Image
            {
                Id = Guid.NewGuid(),
                ImageName = imageData.FileName,
                Description = newDescription,
                People = new List<ImagePerson>(),
                Tags = new List<ImageTag>()
            };
            // Add identified person/people.
            if (imageData.IdentifiedPersons.Any())
            {
                foreach (var person in imageData.IdentifiedPersons)
                {
                    // Add the person if the confidence level is > 50%
                    if (person.Person != null && person.Confidence > .5)
                    {
                        image.People.Add(new ImagePerson
                        {
                            ImageId = image.Id,
                            Name = person.Person.Name,
                            PersonId = person.Person.PersonId
                        });
                    }
                }
            }
            // Add image tags.
            if (imageData.AnalysisResult != null && imageData.AnalysisResult.Tags.Any())
            {
                foreach (var tag in imageData.AnalysisResult.Tags)
                {
                    image.Tags.Add(new ImageTag
                    {
                        ImageId = image.Id,
                        Tag = tag.Name
                    });
                }
            }

            return image;
        }

        private static DateTime RoundToNearest(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            var roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }

        private static void UpdateDatabase(AnalyzedImageData imageData)
        {
            var imageRepo = new ImageRepository(SettingsHelper.GetEnvironmentVariable("DatabaseConnectionString"));
            var imagePersonRepo = new ImagePersonRepository(SettingsHelper.GetEnvironmentVariable("DatabaseConnectionString"));
            var imageTagRepo = new ImageTagRepository(SettingsHelper.GetEnvironmentVariable("DatabaseConnectionString"));
            Image existingImage = null;
            var newDescription = imageData.AnalysisResult?.Description?.Captions[0]?.Text;
            if (imageData.IdentifiedPersons.Any(p => p.Person != null))
            {
                existingImage = imageRepo.GetByImageName(imageData.FileName);
            }
            if (existingImage == null || existingImage.Id == Guid.Empty)
            {
                // Insert new records.
                existingImage = new Image
                {
                    Id = Guid.NewGuid(),
                    ImageName = imageData.FileName,
                    Description = newDescription
                };
                imageRepo.Add(existingImage);
                if (existingImage.Id != Guid.Empty)
                {
                    // Add identified person/people.
                    if (imageData.IdentifiedPersons.Any())
                    {
                        foreach (var person in imageData.IdentifiedPersons)
                        {
                            // Add the person if the confidence level is > 50%
                            if (person.Person != null && person.Confidence > .5)
                            {
                                imagePersonRepo.Add(new ImagePerson
                                {
                                    ImageId = existingImage.Id,
                                    Name = person.Person.Name,
                                    PersonId = person.Person.PersonId
                                });
                            }
                        }
                    }
                }
                // Add image tags.
                if (imageData.AnalysisResult != null && imageData.AnalysisResult.Tags.Any())
                {
                    foreach (var tag in imageData.AnalysisResult.Tags)
                    {
                        imageTagRepo.Add(new ImageTag
                        {
                            ImageId = existingImage.Id,
                            Tag = tag.Name
                        });
                    }
                }
            }
            else
            {
                // Update existing records.
                existingImage.Description = newDescription;
            }
        }
    }
}