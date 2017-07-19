namespace Gallery.Models
{
    public class CountModel
    {
        public bool Success => true;
        public int Count { get; set; }

        public CountModel(int count)
        {
            Count = count;
        }
    }
}
