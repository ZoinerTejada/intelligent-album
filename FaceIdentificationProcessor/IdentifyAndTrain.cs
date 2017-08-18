using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Threading;
using Windows.Storage;
using CognitiveServiceHelpers;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceIdentificationProcessor
{
    public class IdentifyAndTrain
    {
        private IEnumerable<PersonGroup> PersonGroups;
        private PersonGroup CurrentPersonGroup { get; set; }
        private ObservableCollection<Person> PersonsInCurrentGroup { get; set; }

        public IdentifyAndTrain(CancellationToken cancellationToken)
        {
            FaceServiceHelper.CancellationToken = cancellationToken;
        }

        public async Task TrainFromImageSet()
        {
            this.PersonsInCurrentGroup = new ObservableCollection<Person>();

            // First load any registered person groups.
            await LoadPersonGroupsFromService();
            // Next, create a new person group if needed.
            await CreatePersonGroup();
            // Add people's faces from training images stored in the file system.
            await AddPeoplesFacesFromImages();
            // Train based off of added faces.
            await StartTraining();

            Console.WriteLine("");
            ConsoleHelper.WriteLineInColor("Finished training your person group!", ConsoleColor.Green);
        }

        private async Task LoadPersonGroupsFromService()
        {
            try
            {
                PersonGroups = await FaceServiceHelper.GetPersonGroupsAsync(SettingsHelper.WorkspaceKey);
                var personGroups = PersonGroups as PersonGroup[] ?? PersonGroups.ToArray();
                if (personGroups.Any())
                {
                    CurrentPersonGroup = personGroups.First(p => p.Name == SettingsHelper.PersonGroupName);
                }
            }
            catch (Exception ex)
            {
                await Utilities.GenericApiCallExceptionHandler(ex, "Failure dowloading groups");
            }
        }

        private async Task CreatePersonGroup()
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsHelper.WorkspaceKey))
                {
                    throw new InvalidOperationException("Before you can create a person group, you need to define a Workspace Key in App.config.");
                }
                if (string.IsNullOrEmpty(SettingsHelper.PersonGroupName))
                {
                    throw new InvalidOperationException("You must specify a person group name by setting the PersonGroupName value in App.config.");
                }

                if (!PersonGroups.Any(p => p.Name == SettingsHelper.PersonGroupName))
                {
                    await FaceServiceHelper.CreatePersonGroupAsync(Guid.NewGuid().ToString(),
                        SettingsHelper.PersonGroupName, SettingsHelper.WorkspaceKey);
                    await this.LoadPersonGroupsFromService();
                }
            }
            catch (Exception ex)
            {
                await Utilities.GenericApiCallExceptionHandler(ex, "Failure creating group");
            }
        }

        private async Task LoadPersonsInCurrentGroup()
        {
            this.PersonsInCurrentGroup.Clear();

            try
            {
                var personsInGroup = await FaceServiceHelper.GetPersonsAsync(this.CurrentPersonGroup.PersonGroupId);
                foreach (var person in personsInGroup.OrderBy(p => p.Name))
                {
                    this.PersonsInCurrentGroup.Add(person);
                }
            }
            catch (Exception e)
            {
                await Utilities.GenericApiCallExceptionHandler(e, "Failure loading people in the group");
            }
        }

        private async Task AddPeoplesFacesFromImages()
        {
            if (string.IsNullOrEmpty(SettingsHelper.TrainingImagesPath))
            {
                throw new InvalidOperationException("You must set the TrainingImagesPath value in App.config before you can train from images.");
            }
            var errors = new List<string>();
            await this.LoadPersonsInCurrentGroup();

            try
            {
                var autoTrainFolder = await StorageFolder.GetFolderFromPathAsync(SettingsHelper.TrainingImagesPath);
                var folders = await autoTrainFolder.GetFoldersAsync();
                foreach (var folder in folders)
                {
                    var personName = Utilities.CapitalizeString(folder.Name.Trim());
                    ConsoleHelper.WriteLineInColor($"Adding {personName}'s face from the following images:", ConsoleColor.Green);
                    var isNewPerson = false;
                    if (string.IsNullOrEmpty(personName))
                    {
                        continue;
                    }

                    if (!this.PersonsInCurrentGroup.Any(p => p.Name == personName))
                    {
                        await FaceServiceHelper.CreatePersonAsync(this.CurrentPersonGroup.PersonGroupId, personName);
                        isNewPerson = true;
                    }

                    var newPerson = (await FaceServiceHelper.GetPersonsAsync(this.CurrentPersonGroup.PersonGroupId)).First(p => p.Name == personName);
                    var files = await folder.GetFilesAsync();
                    foreach (var photoFile in files)
                    {
                        try
                        {
                            ConsoleHelper.WriteLineInColor($"  - {photoFile.Name}", ConsoleColor.Gray);
                            await FaceServiceHelper.AddPersonFaceAsync(
                                this.CurrentPersonGroup.PersonGroupId,
                                newPerson.PersonId,
                                imageStreamCallback: photoFile.OpenStreamForReadAsync,
                                userData: photoFile.Path,
                                targetFace: null);

                            // Force a delay to reduce the chance of hitting API call rate limits 
                            await Task.Delay(250);
                        }
                        catch (Exception e)
                        {
                            errors.Add(photoFile.Path);
                        }
                    }

                    if (isNewPerson) this.PersonsInCurrentGroup.Add(newPerson);
                }
            }
            catch (Exception ex)
            {
                await Utilities.GenericApiCallExceptionHandler(ex, "Failure processing the folder and files");
            }

            if (errors.Any())
            {
                Utilities.DisplayGenericError($"Failure importing the following photos: {string.Join("\n", errors)}");
            }
        }

        private async Task StartTraining()
        {
            TrainingStatus trainingStatus = null;
            try
            {
                ConsoleHelper.WriteLineInColor($"Beginning person group training process...", ConsoleColor.Green);
                await FaceServiceHelper.TrainPersonGroupAsync(this.CurrentPersonGroup.PersonGroupId);

                while (true)
                {
                    trainingStatus = await FaceServiceHelper.GetPersonGroupTrainingStatusAsync(this.CurrentPersonGroup.PersonGroupId);

                    if (trainingStatus.Status != Status.Running)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                await Utilities.GenericApiCallExceptionHandler(ex, "Failure requesting training");
            }

            if (trainingStatus.Status != Status.Succeeded)
            {
                ConsoleHelper.WriteLineInColor("Training finished with failure", ConsoleColor.Magenta);
            }
            else
            {
                ConsoleHelper.WriteLineInColor("Training completed successfully", ConsoleColor.Cyan);
            }
        }

    }
}
