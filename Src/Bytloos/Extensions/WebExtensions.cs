using System.Net;

namespace Bytloos.Extensions
{
    /// <summary>
    /// HttpWebRequest extension methods.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Gets HttpWebResponse.
        /// </summary>
        /// <param name="source">Source request.</param>
        /// <param name="quietly">Don't throw exceptions because of some status codes.</param>
        /// <returns>Response.</returns>
        public static HttpWebResponse GetResponse(this HttpWebRequest source, bool quietly)
        {
            return quietly ? GetResponseQuietly(source) : (HttpWebResponse)source.GetResponse();
        }

        /// <summary>
        /// Gets HttpWebResponse ignoring some exceptions caused by some status codes.
        /// </summary>
        /// <param name="source">Source request.</param>
        /// <returns>Response.</returns>
        public static HttpWebResponse GetResponseQuietly(this HttpWebRequest source)
        {
            try
            {
                return (HttpWebResponse) source.GetResponse();
            }
            catch (WebException exception)
            {
                var response = exception.Response as HttpWebResponse;

                if (response == null)
                    throw;

                return response;
            }
        }
    }
}
