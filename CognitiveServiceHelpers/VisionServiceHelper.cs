using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Polly;
using Polly.Wrap;

namespace CognitiveServiceHelpers
{
    public static class VisionServiceHelper
    {
        private static VisionServiceClient visionClient { get; set; }

        static VisionServiceHelper()
        {
            InitializeEmotionService();
        }

        public static Action Throttled;

        public static CancellationToken CancellationToken { get; set; }

        private static string apiKey;
        public static string ApiKey
        {
            get
            {
                return apiKey;
            }

            set
            {
                var changed = apiKey != value;
                apiKey = value;
                if (changed)
                {
                    InitializeEmotionService();
                }
            }
        }

        // Define our waitAndRetry policy: keep retrying with exponential backoff.
        private static readonly Policy WaitAndRetryPolicy = Policy
            .Handle<FaceAPIException>(e => e.HttpStatus == (System.Net.HttpStatusCode)429)
            .WaitAndRetryForeverAsync(
                //attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)), // Back off!  2, 4, 8, 16 etc times 1/4-second
                attempt => TimeSpan.FromSeconds(6), // Wait 6 seconds between retries indefinitely
                (exception, calculatedWaitDuration) =>
                {
                    Throttled?.Invoke();
                    Debug.WriteLine(exception.Message);
                }
            );

        // Define our CircuitBreaker policy: Break if the action fails 4 times in a row.
        private static readonly Policy CircuitBreakerPolicy = Policy
            .Handle<FaceAPIException>(e => e.HttpStatus == (System.Net.HttpStatusCode)429)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 4,
                durationOfBreak: TimeSpan.FromSeconds(6),
                onBreak: (ex, breakDelay) =>
                {
                    Throttled?.Invoke();
                },
                onReset: () => Debug.WriteLine(".Breaker logging: Call ok! Closed the circuit again!"),
                onHalfOpen: () => Debug.WriteLine(".Breaker logging: Half-open: Next call is a trial!")
            );

        private static readonly PolicyWrap PolicyWrap = Policy.WrapAsync(WaitAndRetryPolicy, CircuitBreakerPolicy);

        private static void InitializeEmotionService()
        {
            visionClient = new VisionServiceClient(apiKey);
        }

        private static async Task<TResponse> RunTaskWithAutoRetryOnQuotaLimitExceededError<TResponse>(Func<Task<TResponse>> action)
        {
            //return await PolicyWrap.ExecuteAsync<TResponse>(ct => action(), CancellationToken);
            return await WaitAndRetryPolicy.ExecuteAsync<TResponse>(ct => action(), CancellationToken);
        }

        private static async Task RunTaskWithAutoRetryOnQuotaLimitExceededError(Func<Task> action)
        {
            await RunTaskWithAutoRetryOnQuotaLimitExceededError<object>(async () => { await action(); return null; });
        }

        public static async Task<AnalysisResult> DescribeAsync(Func<Task<Stream>> imageStreamCallback)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(async () => await visionClient.DescribeAsync(await imageStreamCallback()));
        }

        public static async Task<AnalysisResult> AnalyzeImageAsync(string imageUrl, IEnumerable<VisualFeature> visualFeatures = null, IEnumerable<string> details = null)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(() => visionClient.AnalyzeImageAsync(imageUrl, visualFeatures, details));
        }

        public static async Task<AnalysisResult> AnalyzeImageAsync(Func<Task<Stream>> imageStreamCallback, IEnumerable<VisualFeature> visualFeatures = null, IEnumerable<string> details = null)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(async () => await visionClient.AnalyzeImageAsync(await imageStreamCallback(), visualFeatures, details));
        }

        public static async Task<AnalysisResult> DescribeAsync(string imageUrl)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(() => visionClient.DescribeAsync(imageUrl));
        }
    }
}
