using System;
using System.Collections.Generic;
using System.Linq;

namespace Gallery
{
    public static class LanguageManager
    {
        public static string SelectLanguage(Dictionary<string, string> InterfaceLanguages, string AcceptLanguages, string DefaultLanguage)
        {
            if (AcceptLanguages.IndexOf(' ') > -1)
            {
                AcceptLanguages = AcceptLanguages.Remove(' ');
            }
            var splitLanguages = AcceptLanguages.Split(',');
            var userLanguages = splitLanguages.Select((x) => new Language(x)).ToList();
            userLanguages.Sort();

            // Find exact matches for the user's accept language.
            foreach (var language in userLanguages)
            {
                if (InterfaceLanguages.ContainsKey(language.Code))
                {
                    return language.Code;
                }
            }

            // Be more permissive and match en for en-AU etc.
            foreach (var language in userLanguages)
            {
                if (language.Code.IndexOf('-') > -1)
                {
                    var shortCode = language.Code.Split('-')[0];
                    if (InterfaceLanguages.ContainsKey(shortCode))
                    {
                        return shortCode;
                    }
                }
            }

            // User's language wasn't found.
            return DefaultLanguage;
        }

        private class Language : IComparable<Language>
        {
            public string Code { get; set; }
            public float Quality { get; set; }

            public Language(string Input)
            {
                var parts = Input.Split(';');
                if (parts.Length == 1)
                {
                    // en
                    Code = Input;
                    Quality = 1.0f;
                }
                else if (parts.Length == 2)
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
                        throw new ArgumentException($"Invalid language \"{Input}\"", nameof(Input));
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid language \"{Input}\"", nameof(Input));
                }
            }

            public override string ToString()
            {
                return $"{Code};{Quality}";
            }

            public int CompareTo(Language other)
            {
                return other.Quality.CompareTo(Quality);
            }
        }
    }
}
