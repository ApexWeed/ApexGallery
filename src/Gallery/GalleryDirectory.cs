namespace Gallery
{
    public class GalleryDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public GalleryDirectory(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }
    }
}
