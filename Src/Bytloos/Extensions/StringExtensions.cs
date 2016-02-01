using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bytloos.Extensions
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly char[] SENTENCE_SPLITTING_CHARACTERS = { ' ', '.', '?', '—', ':', '–', ';', ',' };

        /// <summary>
        /// Counts entries of some string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="pattern">Pattern string.</param>
        /// <returns>Amount of entries.</returns>
        public static int CountEntries(this string source, string pattern)
        {
            return (source.Length - source.Replace(pattern, string.Empty).Length) / pattern.Length;
        }

        /// <summary>
        /// Counts number of words in string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Amount of words in string.</returns>
        public static int CountWords(this string source)
        {
            return source.Split(
                separator:  SENTENCE_SPLITTING_CHARACTERS,
                options:    StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Replaces entries of some substring by key and value.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="dictionary">Dictionary of patterns.</param>
        /// <returns>Edited string.</returns>
        public static string Replace(this string source, Dictionary<string, string> dictionary)
        {
            foreach (var keyValue in dictionary)
                source = source.Replace(keyValue.Key, keyValue.Value);

            return source;
        }

        /// <summary>
        /// Replaces entries of some character by key and value.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="dictionary">Dictionary of patterns.</param>
        /// <returns>Edited string.</returns>
        public static string Replace(this string source, Dictionary<char, char> dictionary)
        {
            foreach (var keyValue in dictionary)
                source = source.Replace(keyValue.Key, keyValue.Value);

            return source;
        }

        /// <summary>
        /// Removes entries of some string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="patterns">String params.</param>
        /// <returns></returns>
        public static string Remove(this string source, params string[] patterns)
        {
            foreach (var pattern in patterns)
                source = source.Replace(pattern, string.Empty);

            return source;
        }

        /// <summary>
        /// Removes entries of some character.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="patterns">Character params.</param>
        /// <returns></returns>
        public static string Remove(this string source, params char[] patterns)
        {
            foreach (var pattern in patterns)
                source = source.Replace(pattern.ToString(), string.Empty);

            return source;
        }

        /// <summary>
        /// Checks string for entries.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="patterns">String params.</param>
        /// <returns>True if string matches all patterns.</returns>
        public static bool Contains(this string source, params string[] patterns)
        {
            return patterns.All(source.Contains);
        }

        /// <summary>
        /// Checks string for entries.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="patterns">Character params.</param>
        /// <returns>True if string matches all patterns.</returns>
        public static bool Contains(this string source, params char[] patterns)
        {
            return patterns.All(source.Contains);
        }

        /// <summary>
        /// Gets null if string is null, empty or whitespace.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Null or non-empty and non-whitespace string.</returns>
        public static string NullSpace(this string source)
        {
            return source.IsNullOrEmpty() || source.IsNullOrWhiteSpace() ? null : source;
        }

        /// <summary>
        /// Extension method for string.IsNullOrEmpty().
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>True if string is null or empty.</returns>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
        
        /// <summary>
        /// Extension method for string.IsNullOrWhiteSpace().
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>True if string is null or whitespace.</returns>
        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Gets reversed string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Reversed string.</returns>
        public static string Reversed(this string source)
        {
            var charArray = source.ToCharArray();

            Array.Reverse(charArray);

            return new string(charArray);
        }

        /// <summary>
        /// Gets substring trimmed to last word.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="length">Length of substring.</param>
        /// <returns>Substring trimmed to last word.</returns>
        public static string SubWords(this string source, int length)
        {
            if (source.Length <= length)
                return source;

            source = source.Substring(0, length).Trim();

            var words = source.Split(' ').ToList();

            if (words.Count > 1)
                words.Remove(words.Last());

            source = string.Join(" ", words).Trim();

            return source;
        }

        /// <summary>
        /// Gets URL slug representation of string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="length">Max length of string.</param>
        /// <returns>URL slug representation of string.</returns>
        public static string Slugify(this string source, int length = 45)
        {
            source = source.ToLower().Trim();
            source = Regex.Replace(source, @"[^a-z0-9\s\-]", string.Empty);
            source = Regex.Replace(source, @"[\s\-]+", " ").Trim();
            source = source.SubWords(length);
            source = Regex.Replace(source, @"\s", "-"); 
 
            return source;
        }

        /// <summary>
        /// Fixes typoghraphics (spaces, commas, dashes).
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Typographically correct string.</returns>
        public static string ToTypographic(this string source)
        {
            var punctsMt = Regex.Matches(source, @"\s*([\),;:!\.\?])\s*", RegexOptions.Singleline);

            foreach (Match punctMatch in punctsMt)
                source = source.Replace(punctMatch.Groups[0].ToString(), punctMatch.Groups[1] + " ");

            source = Regex.Replace(source, @"\.\s*,", ".,");

            punctsMt = Regex.Matches(source, @"(\()\s", RegexOptions.Singleline);

            foreach (Match punctMatch in punctsMt)
                source = source.Replace(punctMatch.Groups[0].ToString(), punctMatch.Groups[1].ToString());

            punctsMt = Regex.Matches(source, @"(\S)([\u2012\u2013\u2014])(\S)", RegexOptions.Singleline);

            foreach (Match punctMatch in punctsMt)
                source = source.Replace(punctMatch.Groups[0].ToString(), punctMatch.Groups[1] + " " + punctMatch.Groups[2] + " " + punctMatch.Groups[3]);

            punctsMt
                = Regex.Matches(
                    input:      source,
                    pattern:    @"(['""\u00AB\u2039\u201F\u2018\u201B\u201E\u201A])\s*"
                                    + @"([^'""\u00AB\u2039\u201F\u2018\u201B\u201E\u201A\u00BB\u203A\u201C\u2018\u201D\u2019]*)\s*"
                                    + @"(['""\u00BB\u203A\u201C\u2018\u201D\u2019\u201E\u201A])",
                    options:    RegexOptions.Singleline);
            
            foreach (Match punctMatch in punctsMt)
                source
                    = source.Replace(
                        oldValue:   punctMatch.Groups[0].ToString(),
                        newValue:   punctMatch.Groups[1] + punctMatch.Groups[2].ToString().Trim() + punctMatch.Groups[3]);

            return source;
        }
    }
}
