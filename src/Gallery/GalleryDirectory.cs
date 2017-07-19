namespace Gallery
{
    public class GalleryDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public GalleryDirectory(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
