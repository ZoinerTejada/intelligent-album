using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CognitiveServicesFunctionApp
{
    public class SettingsHelper
    {
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}