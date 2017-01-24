using System.Collections.Generic;

namespace Gallery.Models
{
    public class HomeViewModel
    {
        public Dictionary<string, string> Strings { get; set; }
        public string Language { get; set; }
        public Dictionary<string, string> Languages {get; set; }
    }
}
