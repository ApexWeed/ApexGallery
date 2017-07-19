namespace Gallery.Models
{
    public class ThumbnailModel
    {
        public bool Success => true;
        public int Generated { get; set; }
        public int Count { get; set; }

        public ThumbnailModel(int generated, int count)
        {
            Generated = generated;
            Count = count;
        }
    }
}
