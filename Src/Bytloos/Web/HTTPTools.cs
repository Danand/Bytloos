using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace Bytloos.Web
{
    /// <summary>
    /// Simple HTTP client.
    /// </summary>
    public class HTTPTools : ICloneable
    {
        public delegate void OnStreamReading(long currentBytes, long totalBytes);

        private const int DEFAULT_BUFFER_SIZE = 2048;
        private const string DEFAULT_URL = "http://localhost";
        private const string DEFAULT_POST_CONTENT_TYPE = "application/x-www-form-urlencoded";

        private const string DEFAULT_USER_AGENT
            = "Mozilla/5.0 (Windows NT 6.1; WOW64) "
                + "AppleWebKit/537.36 (KHTML, like Gecko) "
                + "Chrome/39.0.2171.95 "
                + "Safari/537.36 "
                + "OPR/26.0.1656.60";

        private const string DEFAULT_ACCEPT
            = "text/html, "
                + "application/xml;q=0.9, "
                + "application/xhtml+xml, "
                + "image/png, "
                + "image/webp, "
                + "image/jpeg, "
                + "image/gif, "
                + "image/x-xbitmap, */*;q=0.1";

        private readonly bool isSilent;
        private readonly bool isProxyAutoswitching;
        private readonly Stopwatch stopWatch;
        private readonly HttpWebRequest defaultRequest;
        private readonly List<Exception> exceptions;

        private WebProxy currentProxy;

        /// <summary>
        /// Creates HTTP tools object.
        /// </summary>
        /// <param name="silentMode">Suppress exceptions.</param>
        /// <param name="proxies">Proxy list.</param>
        /// <param name="autoSwitchProxies">Switch proxy when connection error occured.</param>
        public HTTPTools(bool silentMode = true, List<WebProxy> proxies = null, bool autoSwitchProxies = false)
        {
            SSLValidator.OverrideValidation();

            this.isSilent = silentMode;
            this.isProxyAutoswitching = autoSwitchProxies;
            Proxies = proxies ?? new List<WebProxy>();

            if (Proxies.Any())
                this.currentProxy = Proxies[CurrentProxyIndex];

            this.stopWatch = new Stopwatch();
            this.exceptions = new List<Exception>();

            this.defaultRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(DEFAULT_URL));

            #region Default request initializing

            this.defaultRequest.UserAgent = DEFAULT_USER_AGENT;
            this.defaultRequest.Accept = DEFAULT_ACCEPT;

            this.defaultRequest.Headers.Add(
                HttpRequestHeader.AcceptLanguage,
                string.Format(
                    format: "{0},{1};q=0.9,en;q=0.8",
                    arg0:   CultureInfo.CurrentCulture.Name,
                    arg1:   CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
            
            #endregion

            Cookies = new CookieContainer();
        }

        /// <summary>
        /// Index of current proxy.
        /// </summary>
        public int CurrentProxyIndex { get; private set; }

        /// <summary>
        /// Delay between requests.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Duration of last request and response.
        /// </summary>
        public long Duration { get { return this.stopWatch.ElapsedMilliseconds; } }

        /// <summary>
        /// Current referer.
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// Cookie container.
        /// </summary>
        public CookieContainer Cookies { get; set; }

        /// <summary>
        /// Proxy list.
        /// </summary>
        public List<WebProxy> Proxies { get; set; }

        /// <summary>
        /// List of occured exceptions.
        /// </summary>
        public List<Exception> Exceptions { get { return this.exceptions; } }

        /// <summary>
        /// Current response stream reading progress.
        /// </summary>
        public event OnStreamReading StreamReading;

        /// <summary>
        /// Makes memberwise clone of object.
        /// </summary>
        /// <returns>Memberwise clone of object</returns>
        public object Clone() { return MemberwiseClone(); }

        /// <summary>
        /// Switch proxy to next.
        /// </summary>
        public void SwitchProxy()
        {
            if (Proxies == null || Proxies.Count <= 1) return;

            CurrentProxyIndex = Proxies.Count > CurrentProxyIndex ? CurrentProxyIndex + 1 : 0;

            if (!this.isSilent && CurrentProxyIndex == 0)
                throw new IndexOutOfRangeException();

            this.currentProxy = Proxies[CurrentProxyIndex];
        }

        /// <summary>
        /// GET request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Get(
            string                      url,
            Dictionary<string, string>  parameters = null,
            CookieContainer             cookies = null,
            Dictionary<string, string>  headers = null,
            Encoding                    encoding = null,
            int                         tryTimes = 0)
        {
            try
            {
                this.stopWatch.Reset();
                this.stopWatch.Start();

                if (Delay > 0)
                    Thread.Sleep(Delay);

                var queryString = ParseQuery(url);
                var possibleQuery = BuildQuery(parameters);

                url = url.Split('?')[0];

                var request
                    = BuildRequest(
                        string.Format(
                            "{0}{1}{2}{3}{4}",
                            url,
                            string.IsNullOrEmpty(queryString) && string.IsNullOrEmpty(possibleQuery)
                                ? string.Empty
                                : "?",
                            queryString,
                            !string.IsNullOrEmpty(queryString) && !string.IsNullOrEmpty(possibleQuery)
                                ? "&"
                                : string.Empty,
                            possibleQuery),
                        headers);

                request.Method = "GET";
                request.CookieContainer = cookies ?? Cookies;

                Cookies = cookies ?? Cookies;

                var response = (HttpWebResponse)request.GetResponse();

                if (encoding == null && !string.IsNullOrEmpty(response.ContentType))
                    encoding
                        = response.ContentType.Contains("=")
                            ? Encoding.GetEncoding(response.ContentType.Split('=')[1])
                            : Encoding.Default;

                var responseStream = response.GetResponseStream();

                responseStream
                    = response.ContentEncoding == "gzip" && responseStream != null
                        ? new GZipStream(responseStream, CompressionMode.Decompress)
                        : responseStream;

                if (responseStream == null)
                    return null;

                var memoryStream = new MemoryStream();

                int bytesSize;
                var bytesBuffer = new byte[DEFAULT_BUFFER_SIZE];

                while ((bytesSize = responseStream.Read(bytesBuffer, 0, bytesBuffer.Length)) > 0)
                {
                    if(memoryStream.Length == 0 && StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);
                    
                    memoryStream.Write(bytesBuffer, 0, bytesSize);

                    if (StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);
                }

                memoryStream.Position = 0;

                var streamReader = new StreamReader(memoryStream, encoding ?? Encoding.Default);

                var content = streamReader.ReadToEnd();

                streamReader.Close();
                memoryStream.Close();
                response.Close();

                this.stopWatch.Stop();

                Referer = url;

                return content;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);

                if(this.isProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Get(url, parameters, cookies, headers, encoding, --tryTimes);

                if (!this.isSilent)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// POST request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="rawData">Raw data before building query string.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Post(
            string                      url,
            Dictionary<string, string>  parameters = null,
            string                      rawData = null,
            CookieContainer             cookies = null,
            Dictionary<string, string>  headers = null,
            Encoding                    encoding = null,
            int                         tryTimes = 0)
        {
            try
            {
                this.stopWatch.Reset();
                this.stopWatch.Start();

                if (Delay > 0)
                    Thread.Sleep(Delay);

                var postData = parameters != null ? BuildQuery(parameters) : rawData;

                var request = BuildRequest(url, headers);

                request.Method = "POST";
                request.ContentType = request.ContentType ?? DEFAULT_POST_CONTENT_TYPE;
                request.CookieContainer = cookies ?? Cookies;

                Cookies = cookies ?? Cookies;

                var bytes = (encoding ?? Encoding.Default).GetBytes(postData ?? string.Empty);

                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);

                var response = (HttpWebResponse)request.GetResponse();

                if (encoding == null && !string.IsNullOrEmpty(response.ContentType))
                    encoding
                        = response.ContentType.Contains("=")
                            ? Encoding.GetEncoding(response.ContentType.Split('=')[1])
                            : Encoding.Default;

                var responseStream = response.GetResponseStream();

                responseStream
                    = response.ContentEncoding == "gzip" && responseStream != null
                        ? new GZipStream(responseStream, CompressionMode.Decompress)
                        : responseStream;

                if (responseStream == null)
                    return null;

                var memoryStream = new MemoryStream();

                int bytesSize;
                var bytesBuffer = new byte[DEFAULT_BUFFER_SIZE];

                while ((bytesSize = responseStream.Read(bytesBuffer, 0, bytesBuffer.Length)) > 0)
                {
                    if (memoryStream.Length == 0 && StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);

                    memoryStream.Write(bytesBuffer, 0, bytesSize);

                    if (StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);
                }

                memoryStream.Position = 0;

                var streamReader = new StreamReader(memoryStream, encoding ?? Encoding.Default);

                var content = streamReader.ReadToEnd();

                streamReader.Close();
                memoryStream.Close();
                response.Close();

                this.stopWatch.Stop();

                Referer = url;

                return content;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);

                if (this.isProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Post(url, parameters, rawData, cookies, headers, encoding, --tryTimes);

                if (!this.isSilent)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// Multipart POST request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="rawData">Raw data before building query string.</param>
        /// <param name="files">Dictionary of files where key is name and value is path.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Multipart(
            string                      url,
            Dictionary<string, string>  parameters = null,
            string                      rawData = null,
            Dictionary<string, string>  files = null,
            CookieContainer             cookies = null,
            Dictionary<string, string>  headers = null,
            Encoding                    encoding = null,
            int                         tryTimes = 0)
        {
            try
            {
                this.stopWatch.Reset();
                this.stopWatch.Start();

                if (Delay > 0)
                    Thread.Sleep(Delay);

                var boundary = new string('-', 28) + DateTime.Now.Ticks.ToString("x");

                var postData = parameters != null ? BuildMultipartQuery(parameters, boundary, files) : rawData;

                var request = BuildRequest(url, headers);

                request.Method = "POST";
                request.CookieContainer = cookies ?? Cookies;
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                Cookies = cookies ?? Cookies;

                var bytes = (encoding ?? Encoding.Default).GetBytes(postData ?? string.Empty);

                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);

                var response = (HttpWebResponse)request.GetResponse();

                if (encoding == null && !string.IsNullOrEmpty(response.ContentType))
                    encoding
                        = response.ContentType.Contains("=")
                            ? Encoding.GetEncoding(response.ContentType.Split('=')[1])
                            : Encoding.Default;

                var responseStream = response.GetResponseStream();

                responseStream
                    = response.ContentEncoding == "gzip" && responseStream != null
                        ? new GZipStream(responseStream, CompressionMode.Decompress)
                        : responseStream;

                if (responseStream == null)
                    return null;

                var memoryStream = new MemoryStream();

                int bytesSize;
                var bytesBuffer = new byte[DEFAULT_BUFFER_SIZE];

                while ((bytesSize = responseStream.Read(bytesBuffer, 0, bytesBuffer.Length)) > 0)
                {
                    if (memoryStream.Length == 0 && StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);

                    memoryStream.Write(bytesBuffer, 0, bytesSize);

                    if (StreamReading != null)
                        StreamReading(memoryStream.Length, response.ContentLength);
                }

                memoryStream.Position = 0;

                var streamReader = new StreamReader(memoryStream, encoding ?? Encoding.Default);

                var content = streamReader.ReadToEnd();

                streamReader.Close();
                memoryStream.Close();
                response.Close();

                this.stopWatch.Stop();

                Referer = url;

                return content;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);

                if (this.isProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Multipart(url, parameters, rawData, files, cookies, headers, encoding, --tryTimes);

                if (!this.isSilent)
                    throw;

                return null;
            }
        }

        private static string ParseQuery(string url)
        {
            var urlSlices = url.Split('?');

            if (urlSlices.Length != 2) return string.Empty;

            var queryString = urlSlices[1];

            var parameters = new Dictionary<string, string>();

            foreach (var keyVal in queryString.Split('&'))
                parameters.Add(
                    keyVal.Split('=')[0].Trim(),
                    keyVal.Split('=')[1].Trim());

            return BuildQuery(parameters);
        }

        private static string BuildQuery(Dictionary<string, string> parameters)
        {
            return parameters != null
                ? parameters.Aggregate(
                    string.Empty,
                    (current, parameter) =>
                        current + (parameter.Key + "=" + HttpUtility.UrlEncode(parameter.Value) + "&")).TrimEnd('&')
                : string.Empty;
        }

        private static string BuildMultipartQuery(Dictionary<string, string> parameters, string boundary, Dictionary<string, string> files = null)
        {
            var data = "\r\n";

            foreach (var parameter in parameters)
                data += string.Format(
                    "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                    boundary,
                    parameter.Key,
                    parameter.Value);

            if (files != null)
            {
                foreach (var fieldPathPair in files)
                {
                    var content = string.Empty;

                    if (File.Exists(fieldPathPair.Value))
                        using (var sr = new StreamReader(fieldPathPair.Value, Encoding.Default))
                            content = sr.ReadToEnd();

                    data
                        += string.Format(
                            "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n{4}\r\n",
                            boundary,
                            fieldPathPair.Key,
                            Path.GetFileName(fieldPathPair.Value),
                            GetMultipartFileContentType(fieldPathPair.Value),
                            content);
                }
            }

            data += (boundary + "--\r\n\r\n");

            return data;
        }

        private static string GetMultipartFileContentType(string file)
        {
            var extension = file.Split('.').LastOrDefault() ?? "*";

            #region Initializing content types 

            var typesDict = new Dictionary<string, string>
            {
                { "*", "application/octet-stream" },
                { "323", "text/h323" },
                { "acx", "application/internet-property-stream" },
                { "ai", "application/postscript" },
                { "aif", "audio/x-aiff" },
                { "aifc", "audio/x-aiff" },
                { "aiff", "audio/x-aiff" },
                { "asf", "video/x-ms-asf" },
                { "asr", "video/x-ms-asf" },
                { "asx", "video/x-ms-asf" },
                { "au", "audio/basic" },
                { "avi", "video/x-msvideo" },
                { "axs", "application/olescript" },
                { "bas", "text/plain" },
                { "bcpio", "application/x-bcpio" },
                { "bin", "application/octet-stream" },
                { "bmp", "image/bmp" },
                { "c", "text/plain" },
                { "cat", "application/vnd.ms-pkiseccat" },
                { "cdf", "application/x-cdf" },
                { "cer", "application/x-x509-ca-cert" },
                { "class", "application/octet-stream" },
                { "clp", "application/x-msclip" },
                { "cmx", "image/x-cmx" },
                { "cod", "image/cis-cod" },
                { "cpio", "application/x-cpio" },
                { "crd", "application/x-mscardfile" },
                { "crl", "application/pkix-crl" },
                { "crt", "application/x-x509-ca-cert" },
                { "csh", "application/x-csh" },
                { "css", "text/css" },
                { "dcr", "application/x-director" },
                { "der", "application/x-x509-ca-cert" },
                { "dir", "application/x-director" },
                { "dll", "application/x-msdownload" },
                { "dms", "application/octet-stream" },
                { "doc", "application/msword" },
                { "dot", "application/msword" },
                { "dvi", "application/x-dvi" },
                { "dxr", "application/x-director" },
                { "eps", "application/postscript" },
                { "etx", "text/x-setext" },
                { "evy", "application/envoy" },
                { "exe", "application/octet-stream" },
                { "fif", "application/fractals" },
                { "flr", "x-world/x-vrml" },
                { "gif", "image/gif" },
                { "gtar", "application/x-gtar" },
                { "gz", "application/x-gzip" },
                { "h", "text/plain" },
                { "hdf", "application/x-hdf" },
                { "hlp", "application/winhlp" },
                { "hqx", "application/mac-binhex40" },
                { "hta", "application/hta" },
                { "htc", "text/x-component" },
                { "htm", "text/html" },
                { "html", "text/html" },
                { "htt", "text/webviewhtml" },
                { "ico", "image/x-icon" },
                { "ief", "image/ief" },
                { "iii", "application/x-iphone" },
                { "ins", "application/x-internet-signup" },
                { "isp", "application/x-internet-signup" },
                { "jfif", "image/pipeg" },
                { "jpe", "image/jpeg" },
                { "jpeg", "image/jpeg" },
                { "jpg", "image/jpeg" },
                { "js", "application/x-javascript" },
                { "latex", "application/x-latex" },
                { "lha", "application/octet-stream" },
                { "lsf", "video/x-la-asf" },
                { "lsx", "video/x-la-asf" },
                { "lzh", "application/octet-stream" },
                { "m13", "application/x-msmediaview" },
                { "m14", "application/x-msmediaview" },
                { "m3u", "audio/x-mpegurl" },
                { "man", "application/x-troff-man" },
                { "mdb", "application/x-msaccess" },
                { "me", "application/x-troff-me" },
                { "mht", "message/rfc822" },
                { "mhtml", "message/rfc822" },
                { "mid", "audio/mid" },
                { "mny", "application/x-msmoney" },
                { "mov", "video/quicktime" },
                { "movie", "video/x-sgi-movie" },
                { "mp2", "video/mpeg" },
                { "mp3", "audio/mpeg" },
                { "mpa", "video/mpeg" },
                { "mpe", "video/mpeg" },
                { "mpeg", "video/mpeg" },
                { "mpg", "video/mpeg" },
                { "mpp", "application/vnd.ms-project" },
                { "mpv2", "video/mpeg" },
                { "ms", "application/x-troff-ms" },
                { "msg", "application/vnd.ms-outlook" },
                { "mvb", "application/x-msmediaview" },
                { "nc", "application/x-netcdf" },
                { "nws", "message/rfc822" },
                { "oda", "application/oda" },
                { "p10", "application/pkcs10" },
                { "p12", "application/x-pkcs12" },
                { "p7b", "application/x-pkcs7-certificates" },
                { "p7c", "application/x-pkcs7-mime" },
                { "p7m", "application/x-pkcs7-mime" },
                { "p7r", "application/x-pkcs7-certreqresp" },
                { "p7s", "application/x-pkcs7-signature" },
                { "pbm", "image/x-portable-bitmap" },
                { "pdf", "application/pdf" },
                { "pfx", "application/x-pkcs12" },
                { "pgm", "image/x-portable-graymap" },
                { "pko", "application/ynd.ms-pkipko" },
                { "pma", "application/x-perfmon" },
                { "pmc", "application/x-perfmon" },
                { "pml", "application/x-perfmon" },
                { "pmr", "application/x-perfmon" },
                { "pmw", "application/x-perfmon" },
                { "pnm", "image/x-portable-anymap" },
                { "pot", "application/vnd.ms-powerpoint" },
                { "ppm", "image/x-portable-pixmap" },
                { "pps", "application/vnd.ms-powerpoint" },
                { "ppt", "application/vnd.ms-powerpoint" },
                { "prf", "application/pics-rules" },
                { "ps", "application/postscript" },
                { "pub", "application/x-mspublisher" },
                { "qt", "video/quicktime" },
                { "ra", "audio/x-pn-realaudio" },
                { "ram", "audio/x-pn-realaudio" },
                { "ras", "image/x-cmu-raster" },
                { "rgb", "image/x-rgb" },
                { "rmi", "audio/mid" },
                { "roff", "application/x-troff" },
                { "rtf", "application/rtf" },
                { "rtx", "text/richtext" },
                { "scd", "application/x-msschedule" },
                { "sct", "text/scriptlet" },
                { "setpay", "application/set-payment-initiation" },
                { "setreg", "application/set-registration-initiation" },
                { "sh", "application/x-sh" },
                { "shar", "application/x-shar" },
                { "sit", "application/x-stuffit" },
                { "snd", "audio/basic" },
                { "spc", "application/x-pkcs7-certificates" },
                { "spl", "application/futuresplash" },
                { "src", "application/x-wais-source" },
                { "sst", "application/vnd.ms-pkicertstore" },
                { "stl", "application/vnd.ms-pkistl" },
                { "stm", "text/html" },
                { "sv4cpio", "application/x-sv4cpio" },
                { "sv4crc", "application/x-sv4crc" },
                { "svg", "image/svg+xml" },
                { "swf", "application/x-shockwave-flash" },
                { "t", "application/x-troff" },
                { "tar", "application/x-tar" },
                { "tcl", "application/x-tcl" },
                { "tex", "application/x-tex" },
                { "texi", "application/x-texinfo" },
                { "texinfo", "application/x-texinfo" },
                { "tgz", "application/x-compressed" },
                { "tif", "image/tiff" },
                { "tiff", "image/tiff" },
                { "tr", "application/x-troff" },
                { "trm", "application/x-msterminal" },
                { "tsv", "text/tab-separated-values" },
                { "txt", "text/plain" },
                { "uls", "text/iuls" },
                { "ustar", "application/x-ustar" },
                { "vcf", "text/x-vcard" },
                { "vrml", "x-world/x-vrml" },
                { "wav", "audio/x-wav" },
                { "wcm", "application/vnd.ms-works" },
                { "wdb", "application/vnd.ms-works" },
                { "wks", "application/vnd.ms-works" },
                { "wmf", "application/x-msmetafile" },
                { "wps", "application/vnd.ms-works" },
                { "wri", "application/x-mswrite" },
                { "wrl", "x-world/x-vrml" },
                { "wrz", "x-world/x-vrml" },
                { "xaf", "x-world/x-vrml" },
                { "xbm", "image/x-xbitmap" },
                { "xla", "application/vnd.ms-excel" },
                { "xlc", "application/vnd.ms-excel" },
                { "xlm", "application/vnd.ms-excel" },
                { "xls", "application/vnd.ms-excel" },
                { "xlt", "application/vnd.ms-excel" },
                { "xlw", "application/vnd.ms-excel" },
                { "xof", "x-world/x-vrml" },
                { "xpm", "image/x-xpixmap" },
                { "xwd", "image/x-xwindowdump" },
                { "z", "application/x-compress" },
                { "zip", "application/zip" }
            };

            #endregion

            string result;

            typesDict.TryGetValue(extension.ToLower(), out result);

            return result ?? "application/octet-stream";
        }

        private HttpWebRequest BuildRequest(string url, Dictionary<string, string> headers = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Proxy = this.currentProxy ?? request.Proxy;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    switch (header.Key)
                    {
                        case "user-agent":          request.UserAgent = header.Value;
                            break;
                        case "accept":              request.Accept = header.Value;
                            break;
                        case "host":                request.Host = header.Value;
                            break;
                        case "referer":             request.Referer = header.Value;
                            break;
                        case "content-type":        request.ContentType = header.Value;
                            break;
                        case "content-length":      request.ContentLength = long.Parse(header.Value);
                            break;
                        case "connection":

                            if (header.Value != "keep-alive" && header.Value != "close")
                                request.Connection = header.Value;
                            else
                                request.KeepAlive = header.Value == "keep-alive";

                            break;
                        #region Pseudoheaders (custom directives)
                        case "allowautoredirect":   request.AllowAutoRedirect = bool.Parse(header.Value);
                            break;
                        case "expect":              request.ServicePoint.Expect100Continue = header.Value == "100-Continue";
                            break;
                        case "timeout":             request.Timeout = int.Parse(header.Value);
                            break;
                        case "proxy":               request.Proxy = new WebProxy(header.Value);
                            break;
                        #endregion
                        default:                    request.Headers.Add(header.Key, header.Value);
                            break;
                    }
                }
            }

            request.Accept = request.Accept ?? this.defaultRequest.Accept;

            request.UserAgent = request.UserAgent ?? this.defaultRequest.UserAgent;

            if (string.IsNullOrEmpty(request.Headers[HttpRequestHeader.AcceptLanguage]))
                request.Headers.Add(
                    HttpRequestHeader.AcceptLanguage,
                    this.defaultRequest.Headers[HttpRequestHeader.AcceptLanguage]);

            request.Referer = request.Referer ?? Referer;

            return request;
        }
    }
}
