using System;
using System.Security.Cryptography;
using System.Text;

namespace Bytloos.Extensions
{
    /// <summary>
    /// String encryption extension methods.
    /// </summary>
    public static class CryptExtensions
    {
        /// <summary>
        /// Generates MD5 hash string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns>MD5 hash string.</returns>
        public static string MD5(this string source)
        {
            var md5CryptoServiceProvider = new MD5CryptoServiceProvider();

            var hashBytes = md5CryptoServiceProvider.ComputeHash(Encoding.Default.GetBytes(source));

            var stringBuilder = new StringBuilder();

            foreach (var hashByte in hashBytes)
                stringBuilder.Append(hashByte.ToString("x2"));

            return stringBuilder.ToString();
        }
    }
}
