using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoAlbum.Web
{
    public static class AbsoluteUrl
    {
        private static bool _initializedAlready = false;
        private static readonly object Initlock = new object();

        public static string HomeIndex;

        /// <summary>
        /// Initialize only on the first request
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(System.Web.HttpContext context)
        {
            if (_initializedAlready)
            {
                return;
            }

            lock (Initlock)
            {
                if (_initializedAlready)
                {
                    return;
                }

                var uri = HttpContext.Current.Request.Url;

                // AbsoluteUrl without parameters
                HomeIndex = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

                _initializedAlready = true;
            }
        }
    }
}