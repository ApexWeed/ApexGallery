namespace Gallery.Models
{
    public class ErrorModel
    {
        public bool Success { get { return false; } }
        public int Code { get; set; }
        public string Error { get; set; }

        public ErrorModel(int Code, string Error)
        {
            this.Code = Code;
            this.Error = Error;
        }
    }
}
