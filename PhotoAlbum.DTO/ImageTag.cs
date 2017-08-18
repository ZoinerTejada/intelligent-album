using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoAlbum.DTO
{
    public class ImageTag
    {
        public Guid ImageId { get; set; }
        public string Tag { get; set; }
    }
}
