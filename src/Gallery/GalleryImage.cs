namespace Gallery
{
    public class GalleryImage
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Thumb { get; set; }
        public bool Animated { get; set; }

        public GalleryImage(string name, string path, string thumb, bool animated)
        {
            Name = name;
            Path = path;
            Thumb = thumb;
            Animated = animated;
        }
    }
}
