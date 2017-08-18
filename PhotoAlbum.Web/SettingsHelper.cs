using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PhotoAlbum.Web
{
    public class SettingsHelper
    {
        public static string DatabaseConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ToString();
        }
    }
}