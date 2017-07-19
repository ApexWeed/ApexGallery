using System.Collections.Generic;

namespace Gallery.Models
{
    public class ImagesModel
    {
        public bool Success { get { return true; } }
        public int Start { get; set; }
        public int End { get { return Start + Count; } }
        public int Count { get { return Images.Count; } }
        public List<GalleryImage> Images { get; set; }

        public ImagesModel(int start, List<GalleryImage> images)
        {
            Start = start;
            Images = images;
        }
    }
}
