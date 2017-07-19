using System.Collections.Generic;

namespace Gallery
{
    public class ApiSettings
    {
        /// <summary>
        /// Base path on the server for images.
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        /// Base URL for public access to images.
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Base path on the server for thumbnails.
        /// </summary>
        public string ThumbPath { get; set; }
        /// <summary>
        /// Base path for public access to thumbnails.
        /// </summary>
        public string ThumbUrl { get; set; }
        /// <summary>
        /// Size of thumbnail images.
        /// </summary>
        public int ThumbSize { get; set; }
        /// <summary>
        /// Extension to put on the thumbnails.
        /// </summary>
        public string ThumbExtension { get; set; }
        /// <summary>
        /// Command to use to thumbnail images.
        /// </summary>
        public string ThumbCommand { get; set; }
        /// <summary>
        /// Arguments to pass to thumbnail command.
        /// </summary>
        public string ThumbArgs { get; set; }
        /// <summary>
        /// List of extensions to consider images.
        /// </summary>
        public List<string> ImageFormats { get; set; }
        /// <summary>
        /// Dictionary of language codes mapping to dictionaries of translatable folder names.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Folders { get; set; }
        /// <summary>
        /// Dictionary of language codes and names.
        /// </summary>
        public Dictionary<string, string> Languages { get; set; }
    }
}
