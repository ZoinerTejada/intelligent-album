using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace FaceIdentificationProcessor
{
    public class SettingsHelper
    {
        public static string FaceApiKey => GetSetting("FaceApiKey");
        public static string WorkspaceKey => GetSetting("WorkspaceKey");
        public static string PersonGroupName => GetSetting("PersonGroupName");
        public static string TrainingImagesPath => GetSetting("TrainingImagesPath");

        private static string GetSetting(string settingName)
        {
            var value = ConfigurationManager.AppSettings[settingName];

            return value;
        }
    }
}
