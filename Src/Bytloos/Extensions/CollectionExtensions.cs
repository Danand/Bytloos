using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Bytloos.Extensions
{
    /// <summary>
    /// IEnumerable, Array and List extension methods.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Repeats some object into new array.
        /// </summary>
        /// <typeparam name="T">Type of object to repeat.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="times">Number of repeating.</param>
        /// <returns>Array of repeated objects.</returns>
        public static T[] Repeat<T>(this T source, int times)
        {
            var list = new List<T>();

            for (var i = 0; i < times; i++)
                list.Add(source);

            return list.ToArray();
        }

        /// <summary>
        /// Reversive ElementAt().
        /// </summary>
        /// <typeparam name="T">Type of object to get.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="index">Index.</param>
        /// <returns>Object at index in reversed enumeration.</returns>
        public static T ElementAtReverse<T>(this IEnumerable<T> source, int index)
        {
            var array = source as T[] ?? source.ToArray();
            return array.ElementAt((array.Count() - 1) - index);
        }

        /// <summary>
        /// Reversive ElementAtOrDefault();
        /// </summary>
        /// <typeparam name="T">Type of object to get.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="index">Index.</param>
        /// <returns>Object at index in reversed enumeration or default value.</returns>
        public static T ElementAtReverseOrDefault<T>(this IEnumerable<T> source, int index)
        {
            var array = source as T[] ?? source.ToArray();
            return array.ElementAtOrDefault((array.Count() - 1) - index);
        }

        /// <summary>
        /// Batches enumeration into some parts.
        /// </summary>
        /// <typeparam name="T">Type of objects in enumeration.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="size">Size of each part.</param>
        /// <returns>Enumerations splitted by size.</returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            return source
                .Select((item, index) => new { item, index })
                .GroupBy(item => item.index / size)
                .Select(group => group.Select(x => x.item));
        }

        /// <summary>
        /// Finds element in enumeration of arrays by first value of array.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="key">Key to search.</param>
        /// <param name="keyPosition">Index of key in array.</param>
        /// <returns>Element in enumeration of arrays by first value of array</returns>
        public static IEnumerable<T> FindElementByKey<T>(this IEnumerable<T[]> source, T key, int keyPosition = 0)
        {
            return source.FirstOrDefault(element => element[keyPosition < element.Count() ? keyPosition : element.Count() - 1].Equals(key));
        }

        /// <summary>
        /// Wrapper of All().
        /// </summary>
        /// <param name="source">Source enumeration of strings..</param>
        /// <param name="patterns">String pattern.</param>
        /// <returns>True if pattern matches some element.</returns>
        public static bool Contains(this IEnumerable<string> source, params string[] patterns)
        {
            return patterns.All(pattern => source.Any(element => element == pattern));
        }

        /// <summary>
        /// Converts dictionary to NameValueCollection.
        /// </summary>
        /// <param name="source">Source dictionary of strings.</param>
        /// <returns>NameValueCollection.</returns>
        public static NameValueCollection ToNameValueCollection(this Dictionary<string, string> source)
        {
            var nameValueCollection = new NameValueCollection();

            source.ToList().ForEach(element => nameValueCollection.Add(element.Key, element.Value));

            return nameValueCollection;
        }
    }
}
