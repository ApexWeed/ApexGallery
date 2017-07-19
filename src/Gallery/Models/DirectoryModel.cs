using System.Collections.Generic;

namespace Gallery.Models
{
    public class DirectoryModel
    {
        public bool Success => true;
        public List<GalleryDirectory> Directories { get; set; }

        public DirectoryModel(List<GalleryDirectory> directories)
        {
            Directories = directories;
        }
    }
}
