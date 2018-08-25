using System.Net;

namespace Bytloos.Web
{
    /// <summary>
    /// HTTP request properties such as auto redirection, timeout, proxy and except continue.
    /// </summary>
    public class HTTPOptions
    {
        /// <summary>
        /// Enable auto redirect.
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// Enable expect 100 continue.
        /// </summary>
        public bool Expect100Continue { get; set; }

        /// <summary>
        /// Response timeout.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Proxy.
        /// </summary>
        public WebProxy Proxy { get; set; }
    }
}
