using System.Collections.Generic;

namespace Gallery
{
    public class ApiSettings
    {
        /// <summary>
        /// Base path on the server for images.
        /// </summary>
        public string BasePath { get; set; } = string.Empty;
        /// <summary>
        /// Base URL for public access to images.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
        /// <summary>
        /// Base path on the server for thumbnails.
        /// </summary>
        public string ThumbPath { get; set; } = string.Empty;
        /// <summary>
        /// Base path for public access to thumbnails.
        /// </summary>
        public string ThumbUrl { get; set; } = string.Empty;
        /// <summary>
        /// Size of thumbnail images.
        /// </summary>
        public int ThumbSize { get; set; } = 0;
        /// <summary>
        /// Extension to put on the thumbnails.
        /// </summary>
        public string ThumbExtension { get; set; } = ".jpg";
        /// <summary>
        /// Command to use to thumbnail images.
        /// </summary>
        public string ThumbCommand { get; set; } = string.Empty;
        /// <summary>
        /// Arguments to pass to thumbnail command.
        /// </summary>
        public string ThumbArgs { get; set; } = string.Empty;
        /// <summary>
        /// List of extensions to consider images.
        /// </summary>
        public List<string> ImageFormats { get; set; } = new List<string>();
        /// <summary>
        /// Dictionary of language codes mapping to dictionaries of translatable folder names.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Folders { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// Dictionary of language codes and names.
        /// </summary>
        public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// List of regex patterns to exclude directory names by.
        /// </summary>
        public List<string> ExcludedFolders { get; set; } = new List<string>();
        /// <summary>
        /// List of regex patterns to exclude files by.
        /// </summary>
        public List<string> ExcludedFiles { get; set; } = new List<string>();
        /// <summary>
        /// List of regex patterns to exclude paths by.
        /// </summary>
        public List<string> ExcludedPaths { get; set; } = new List<string>();
    }
}
