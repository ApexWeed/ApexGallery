namespace Gallery.Models
{
    public class CountModel
    {
        public bool Success { get { return true; } }
        public int Count { get; set; }

        public CountModel(int Count)
        {
            this.Count = Count;
        }
    }
}
