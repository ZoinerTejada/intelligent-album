using System;

namespace PhotoAlbum.DTO
{
    public class ImagePerson
    {
        public Guid ImageId { get; set; }
        public Guid PersonId { get; set; }
        public string Name { get; set; }
    }
}
