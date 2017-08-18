using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoAlbum.Web.Models;

namespace PhotoAlbum.Web
{
    public class ViewHelper
    {
        public static void PopulateNavData(INavViewModel model, ViewContext context)
        {
            model.CurrentController = context.RouteData.Values["controller"] as string ?? "Home";
            model.CurrentAction = context.RouteData.Values["action"] as string ?? "Index";
            model.CurrentPage = (model.CurrentController + "-" + model.CurrentAction).ToLower();
        }
    }
}