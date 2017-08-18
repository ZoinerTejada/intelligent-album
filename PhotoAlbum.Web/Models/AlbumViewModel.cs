using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoAlbum.DTO;

namespace PhotoAlbum.Web.Models
{
    public class AlbumViewModel : NavViewModel
    {
        public List<string> Tags { get; set; }
        public List<string> Names { get; set; }
        public List<Image> Images { get; set; }
        public string SelectedTag { get; set; }
        public string SelectedName { get; set; }
        public int? MaxNumberImages { get; set; }
    }
}