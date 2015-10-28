using System.IO;

namespace Bytloos
{
    /// <summary>
    /// Static class to work with paths.
    /// </summary>
    public static class FilePath
    {
        private const int MAX_PATH_LENGTH = 260;

        /// <summary>
        /// Checks directory for existance.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        /// <param name="create">Create directory if it doesn't exist.</param>
        /// <returns>True if directory exists.</returns>
        public static bool CheckDir(string path, bool create = false)
        {
            if (Directory.Exists(path))
                return true;

            if(create)
                Directory.CreateDirectory(path);
            else
                return false;

            return true;
        }

        /// <summary>
        /// Removes invalid chars from filename.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Cleaned up filename string.</returns>
        public static string RemoveInvalidFileNameChars(string source)
        {
            foreach (var character in Path.GetInvalidFileNameChars())
                source = source.Replace(character.ToString(), string.Empty);

            source = source.Length < MAX_PATH_LENGTH ? source : source.Substring(0, MAX_PATH_LENGTH);

            return source;
        }

        /// <summary>
        /// Removes invalid chars from path.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>Cleaned up path string.</returns>
        public static string RemoveInvalidPathChars(string source)
        {
            foreach (var character in Path.GetInvalidPathChars())
                source = source.Replace(character.ToString(), string.Empty);

            source = source.Length < MAX_PATH_LENGTH ? source : source.Substring(0, MAX_PATH_LENGTH);

            return source;
        }
    }
}
