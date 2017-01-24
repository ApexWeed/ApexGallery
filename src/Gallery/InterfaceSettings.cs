using System.Collections.Generic;

namespace Gallery
{
    public class InterfaceSettings
    {
        /// <summary>
        /// Dictionary of language codes mapping to dictionaries of translatable interface strings.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Strings { get; set; }
    }
}
