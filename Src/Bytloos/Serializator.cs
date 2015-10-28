using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Bytloos
{
    /// <summary>
    /// Binary serialization wrapper.
    /// </summary>
    public static class Serializator
    {
        /// <summary>
        /// Serializes object to file.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="pathToBinaryData">Path to file.</param>
        public static void SaveObject(object target, string pathToBinaryData)
        {
            var targetDirectory = Path.GetDirectoryName(pathToBinaryData);

            if (targetDirectory != null && !Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            using (var fs = new FileStream(pathToBinaryData, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                new BinaryFormatter().Serialize(fs, target);
        }

        /// <summary>
        /// Deserializes object from file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="pathToBinaryData">Path to file that contains object.</param>
        /// <returns>Object of type passed as type argument.</returns>
        public static T GetObjectOfType<T>(string pathToBinaryData)
        {
            if (!File.Exists(pathToBinaryData))
                throw new FileNotFoundException();

            using (var fs = new FileStream(pathToBinaryData, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (T) new BinaryFormatter().Deserialize(fs);
        }

        /// <summary>
        /// Makes deep clone of object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="source">Source object.</param>
        /// <returns>Deep clone of object.</returns>
        public static T DeepClone<T>(this T source)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, source);

                ms.Position = 0;

                return (T) bf.Deserialize(ms);
            }
        }
    }
}
