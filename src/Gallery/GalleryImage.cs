namespace Gallery
{
    public class GalleryImage
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Thumb { get; set; }
        public bool Animated { get; set; }

        public GalleryImage(string Name, string Path, string Thumb, bool Animated)
        {
            this.Name = Name;
            this.Path = Path;
            this.Thumb = Thumb;
            this.Animated = Animated;
        }
    }
}
