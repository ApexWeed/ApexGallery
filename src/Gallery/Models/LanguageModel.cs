using System.Collections.Generic;

namespace Gallery.Models
{
    public class LanguageModel
    {
        public bool Success { get { return true; } }
        public Dictionary<string, string> Languages { get; set; }
        
        public LanguageModel(Dictionary<string, string> Languages)
        {
            this.Languages = Languages;
        }
    }
}
