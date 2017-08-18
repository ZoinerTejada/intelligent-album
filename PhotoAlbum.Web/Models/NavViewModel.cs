using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoAlbum.Web.Models
{
    public interface INavViewModel
    {
        string CurrentController { get; set; }
        string CurrentAction { get; set; }
        string CurrentPage { get; set; }
    }

    public class NavViewModel : INavViewModel
    {
        public string CurrentController { get; set; }
        public string CurrentAction { get; set; }
        public string CurrentPage { get; set; }
    }
}