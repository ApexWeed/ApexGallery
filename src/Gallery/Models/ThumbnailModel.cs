namespace Gallery.Models
{
    public class ThumbnailModel
    {
        public bool Success { get { return true; } }
        public int Generated { get; set; }
        public int Count { get; set; }

        public ThumbnailModel(int Generated, int Count)
        {
            this.Generated = Generated;
            this.Count = Count;
        }
    }
}
