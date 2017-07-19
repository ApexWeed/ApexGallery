using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallery
{
    public static class LanguageManager
    {
        /// <summary>
        /// Gets the most appropriate language based on available languages and the user's accept languages.
        /// </summary>
        /// <param name="interfaceLanguages">Languages available in the system.</param>
        /// <param name="acceptLanguages">Languages the user wants.</param>
        /// <param name="defaultLanguage">Language to use if no appropriate one is found.</param>
        /// <returns></returns>
        public static string SelectLanguage(Dictionary<string, string> interfaceLanguages, string acceptLanguages, string defaultLanguage)
        {
            if (acceptLanguages.IndexOf(' ') > -1)
            {
                acceptLanguages = acceptLanguages.Remove(' ');
            }
            var splitLanguages = acceptLanguages.Split(',');
            var userLanguages = splitLanguages.Select(x => new Language(x)).ToList();
            userLanguages.Sort();

            // Find exact matches for the user's accept language.
            foreach (var language in userLanguages)
            {
                if (interfaceLanguages.ContainsKey(language.Code))
                {
                    return language.Code;
                }
            }

            // Be more permissive and match en for en-AU etc.
            foreach (var language in userLanguages)
            {
                if (language.Code.IndexOf('-') <= -1)
                    continue;

                var shortCode = language.Code.Split('-')[0];
                if (interfaceLanguages.ContainsKey(shortCode))
                {
                    return shortCode;
                }
            }

            // User's language wasn't found.
            return defaultLanguage;
        }

        private class Language : IComparable<Language>
        {
            public string Code { get; set; }
            public float Quality { get; set; }

            public Language(string input)
            {
                var parts = input.Split(';');
                switch (parts.Length)
                {
                    case 1:
                    {
                        // en
                        Code = input;
                        Quality = 1.0f;
                        break;
                    }
                    case 2:
                    {
                        Code = parts[0];
                        parts = parts[1].Split('=');
                        if (parts[0] == "q")
                        {
                            // en;q=0.8
                            Quality = float.Parse(parts[1]);
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid language \"{input}\"", nameof(input));
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException($"Invalid language \"{input}\"", nameof(input));
                    }
                }
            }

            public override string ToString() => $"{Code};{Quality}";

            public int CompareTo(Language other) => other.Quality.CompareTo(Quality);
        }
    }
}
