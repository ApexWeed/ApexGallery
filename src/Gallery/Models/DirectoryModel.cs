using System.Collections.Generic;

namespace Gallery.Models
{
    public class DirectoryModel
    {
        public bool Success { get { return true; } }
        public List<GalleryDirectory> Directories { get; set; }

        public DirectoryModel(List<GalleryDirectory> Directories)
        {
            this.Directories = Directories;
        }
    }
}
