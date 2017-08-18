using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoAlbum.DTO
{
    public class Image
    {
        public Guid Id { get; set; }
        public string ImageName { get; set; }
        public string Description { get; set; }
        public List<ImagePerson> People { get; set; }
        public List<ImageTag> Tags { get; set; }
    }
}
