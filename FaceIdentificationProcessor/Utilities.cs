using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;

namespace FaceIdentificationProcessor
{
    internal static class Utilities
    {
        public static string CapitalizeString(string s)
        {
            return string.Join(" ",
                s.Split(' ')
                    .Select(
                        word => !string.IsNullOrEmpty(word) ? char.ToUpper(word[0]) + word.Substring(1) : string.Empty));
        }

        internal static async Task GenericApiCallExceptionHandler(Exception ex, string errorTitle)
        {
            var errorDetails = GetMessageFromException(ex);
            ConsoleHelper.WriteLineInColor($"{errorTitle}: {errorDetails}", ConsoleColor.Red);
        }

        internal static void DisplayGenericError(string errorMessage)
        {
            ConsoleHelper.WriteLineInColor(errorMessage, ConsoleColor.Red);
        }

        internal static string GetMessageFromException(Exception ex)
        {
            string errorDetails = ex.Message;

            FaceAPIException faceApiException = ex as FaceAPIException;
            if (faceApiException?.ErrorMessage != null)
            {
                errorDetails = faceApiException.ErrorMessage;
            }

            Microsoft.ProjectOxford.Common.ClientException commonException =
                ex as Microsoft.ProjectOxford.Common.ClientException;
            if (commonException?.Error?.Message != null)
            {
                errorDetails = commonException.Error.Message;
            }

            return errorDetails;
        }
    }
}
