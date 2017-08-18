using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CognitiveServiceHelpers;

namespace FaceIdentificationProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Initialize();

            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            // TODO: Evaluate passed in argument to determine action
            MainAsync(args, cts.Token).Wait();
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args, CancellationToken token)
        {
            var identityTrainer = new IdentifyAndTrain(token);
            await identityTrainer.TrainFromImageSet();
        }

        private static void Initialize()
        {
            FaceServiceHelper.ApiKey = SettingsHelper.FaceApiKey;
            FaceServiceHelper.Throttled = () => ShowThrottleAlert("Face");
            ErrorTrackingHelper.TrackException = LogException;
            ErrorTrackingHelper.GenericApiCallExceptionHandler = Utilities.GenericApiCallExceptionHandler;
        }

        private static void ShowThrottleAlert(string api)
        {
            ConsoleHelper.WriteLineInColor($"The {api} API is throttling your requests. Consider upgrading to a Premium Key.", ConsoleColor.Yellow);
        }

        private static void LogException(Exception ex, string message)
        {
            Debug.WriteLine("Error detected! Exception: \"{0}\", More info: \"{1}\".", ex.Message, message);
        }
    }
}
