namespace Gallery.Models
{
    public class ErrorModel
    {
        public bool Success => false;
        public int Code { get; set; }
        public string Error { get; set; }

        public ErrorModel(int code, string error)
        {
            Code = code;
            Error = error;
        }
    }
}
