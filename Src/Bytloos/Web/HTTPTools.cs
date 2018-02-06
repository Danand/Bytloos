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
using System.Threading.Tasks;
using System.Web;
using Bytloos.Extensions;

using static Bytloos.Web.HTTPContentTypes;

namespace Bytloos.Web
{
    

    /// <summary>
    /// Simple HTTP client.
    /// </summary>
    public class HTTPTools : ICloneable
    {
        public delegate void OnStreamReading(long currentBytes, long totalBytes);

        /// <summary>
        /// Current response stream reading progress.
        /// </summary>
        public event OnStreamReading StreamReading = delegate {};

        private const int       DEFAULT_BUFFER_SIZE             = 2048;
        private const int       MULTIPART_BOUNDARY_LINE_LENGTH  = 28;
        private const string    DEFAULT_URL                     = "http://localhost";
        private const string    DEFAULT_POST_CONTENT_TYPE       = "application/x-www-form-urlencoded";

        private const string DEFAULT_USER_AGENT
            = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                + "AppleWebKit/537.36 (KHTML, like Gecko) "
                + "Chrome/61.0.3163.100 "
                + "Safari/537.36 "
                + "OPR/48.0.2685.52";

        private const string DEFAULT_ACCEPT
            = "text/html, "
                + "application/xml;q=0.9, "
                + "application/xhtml+xml, "
                + "image/png, "
                + "image/webp, "
                + "image/jpeg, "
                + "image/gif, "
                + "image/x-xbitmap, */*;q=0.1";

        private readonly Stopwatch stopWatch;
        private readonly HttpWebRequest defaultRequest;

        private WebProxy currentProxy;

        /// <summary>
        /// Creates HTTP tools object.
        /// </summary>
        public HTTPTools()
        {
            SSLValidator.OverrideValidation();

            stopWatch = new Stopwatch();
            Exceptions = new List<Exception>();

            defaultRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(DEFAULT_URL));

            defaultRequest.UserAgent = DEFAULT_USER_AGENT;
            defaultRequest.Accept = DEFAULT_ACCEPT;

            defaultRequest.Headers.Add(
                HttpRequestHeader.AcceptLanguage,
                string.Format(
                    "{0},{1};q=0.9,en;q=0.8",
                    CultureInfo.CurrentCulture.Name,
                    CultureInfo.CurrentCulture.TwoLetterISOLanguageName));

            Cookies = new CookieContainer();
        }

        /// <summary>
        /// Suppress HTTP request exceptions.
        /// </summary>
        public bool SilentMode { get; set; }

        /// <summary>
        /// Switch proxies automatically.
        /// </summary>
        public bool ProxyAutoswitching { get; set; }

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
        public long Duration { get { return stopWatch.ElapsedMilliseconds; } }

        /// <summary>
        /// Current referer.
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// Cookie container.
        /// </summary>
        public CookieContainer Cookies { get; set; }

        /// <summary>
        /// HTTP request options.
        /// </summary>
        public HTTPOptions Options { get; set; }

        /// <summary>
        /// HTTP headers.
        /// </summary>
        public List<HTTPHeader> Headers { get; set; } 

        /// <summary>
        /// Proxy list.
        /// </summary>
        public List<WebProxy> Proxies { get; set; }

        /// <summary>
        /// List of occured exceptions.
        /// </summary>
        public List<Exception> Exceptions { get; }

        /// <inheritdoc />
        /// <summary>
        /// Makes memberwise clone of object.
        /// </summary>
        /// <returns>Memberwise clone of object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Switch proxy to next.
        /// </summary>
        public void SwitchProxy()
        {
            if (Proxies == null || Proxies.Count <= 1)
                return;

            CurrentProxyIndex = Proxies.Count > CurrentProxyIndex ? CurrentProxyIndex + 1 : 0;

            if (!SilentMode && CurrentProxyIndex == 0)
                throw new IndexOutOfRangeException();

            currentProxy = Proxies[CurrentProxyIndex];
        }

        /// <summary>
        /// GET request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Get(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            try
            {
                BeginWatching();

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
                        headers,
                        options);

                request.Method = "GET";
                request.CookieContainer = cookies ?? Cookies;

                Cookies = cookies ?? Cookies;

                return GetResponseString(request, encoding);
            }
            catch (Exception exception)
            {
                Exceptions.Add(exception);

                if (TryHandleWebException(exception, encoding, out var content))
                    return content;

                if (ProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Get(url, parameters, cookies, headers, options, encoding, --tryTimes);

                if (!SilentMode)
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
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Post(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            string                      rawData     = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            try
            {
                BeginWatching();

                var postData = parameters != null ? BuildQuery(parameters) : rawData;

                var request = BuildRequest(url, headers, options);

                request.Method = "POST";
                request.ContentType = request.ContentType ?? DEFAULT_POST_CONTENT_TYPE;
                request.CookieContainer = cookies ?? Cookies;

                Cookies = cookies ?? Cookies;

                var bytes = (encoding ?? Encoding.Default).GetBytes(postData ?? string.Empty);

                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);

                return GetResponseString(request, encoding);
            }
            catch (Exception exception)
            {
                Exceptions.Add(exception);

                if (TryHandleWebException(exception, encoding, out var content))
                    return content;

                if (ProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Post(url, parameters, rawData, cookies, headers, options, encoding, --tryTimes);

                if (!SilentMode)
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
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response string.</returns>
        public string Multipart(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            string                      rawData     = null,
            Dictionary<string, string>  files       = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            try
            {
                BeginWatching();

                var boundary = new string('-', MULTIPART_BOUNDARY_LINE_LENGTH) + DateTime.Now.Ticks.ToString("x");

                var postData = parameters != null ? BuildMultipartQuery(parameters, boundary, files) : rawData;

                var request = BuildRequest(url, headers, options);

                request.Method = "POST";
                request.CookieContainer = cookies ?? Cookies;
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                Cookies = cookies ?? Cookies;

                var bytes = (encoding ?? Encoding.Default).GetBytes(postData ?? string.Empty);

                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);

                return GetResponseString(request, encoding);
            }
            catch (Exception exception)
            {
                Exceptions.Add(exception);

                if (TryHandleWebException(exception, encoding, out var content))
                    return content;

                if (ProxyAutoswitching)
                    SwitchProxy();

                if (tryTimes > 0)
                    return Multipart(url, parameters, rawData, files, cookies, headers, options, encoding, --tryTimes);

                if (!SilentMode)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// Async GET request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response task.</returns>
        public Task<string> GetAsync(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            return Task.Factory.StartNew(() => Get(url, parameters, cookies, headers, options, encoding, tryTimes));
        }

        /// <summary>
        /// Async POST request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="rawData">Raw data before building query string.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response task.</returns>
        public Task<string> PostAsync(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            string                      rawData     = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            return Task.Factory.StartNew(() => Post(url, parameters, rawData, cookies, headers, options, encoding, tryTimes));
        }

        /// <summary>
        /// Async multipart POST request.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="parameters">Query.</param>
        /// <param name="rawData">Raw data before building query string.</param>
        /// <param name="files">Dictionary of files where key is name and value is path.</param>
        /// <param name="cookies">Cookies.</param>
        /// <param name="headers">HTTP request headers.</param>
        /// <param name="options">HTTP request options (auto redirect, timeout, etc.).</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="tryTimes">Number of attempts when getting response has failed.</param>
        /// <returns>Response task.</returns>
        public Task<string> MultipartAsync(
            string                      url,
            Dictionary<string, string>  parameters  = null,
            string                      rawData     = null,
            Dictionary<string, string>  files       = null,
            CookieContainer             cookies     = null,
            List<HTTPHeader>            headers     = null,
            HTTPOptions                 options     = null,
            Encoding                    encoding    = null,
            int                         tryTimes    = 0)
        {
            return Task.Factory.StartNew(() => Multipart(url, parameters, rawData, files, cookies, headers, options, encoding, tryTimes));
        }

        private static string ParseQuery(string url)
        {
            var urlSlices = url.Split('?');

            if (urlSlices.Length != 2)
                return string.Empty;

            var queryString = urlSlices[1];

            var parameters = new Dictionary<string, string>();

            foreach (var keyVal in queryString.Split('&'))
            {
                parameters.Add(
                    keyVal.Split('=')[0].Trim(),
                    keyVal.Split('=')[1].Trim());
            }

            return BuildQuery(parameters);
        }

        private static string BuildQuery(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return string.Empty;

            return parameters.Aggregate(
                    string.Empty,
                    (current, parameter) => current + parameter.Key + "=" + HttpUtility.UrlEncode(parameter.Value) + "&")
                .TrimEnd('&');
        }

        private static string BuildMultipartQuery(Dictionary<string, string> parameters, string boundary, Dictionary<string, string> files = null)
        {
            var data = "\r\n";

            foreach (var parameter in parameters)
                data += $"--{boundary}\r\nContent-Disposition: form-data; " +
                        $"name=\"{parameter.Key}\"\r\n\r\n{parameter.Value}\r\n";

            if (files != null)
            {
                foreach (var fieldPathPair in files)
                {
                    var content = string.Empty;

                    if (File.Exists(fieldPathPair.Value))
                        using (var sr = new StreamReader(fieldPathPair.Value, Encoding.Default))
                            content = sr.ReadToEnd();

                    data += $"--{boundary}\r\nContent-Disposition: form-data; name=\"{fieldPathPair.Key}\"; " +
                            $"filename=\"{Path.GetFileName(fieldPathPair.Value)}\"\r\n" +
                            $"Content-Type: {GetMultipartFileContentType(fieldPathPair.Value)}\r\n\r\n{content}\r\n";
                }
            }

            data += $"{boundary}--\r\n\r\n";

            return data;
        }

        private static string GetMultipartFileContentType(string file)
        {
            var extension = file.Split('.').LastOrDefault() ?? "*";
            contentTypes.TryGetValue(extension.ToLower(), out var result);
            return result ?? "application/octet-stream";
        }

        private void BeginWatching()
        {
            stopWatch.Reset();
            stopWatch.Start();

            if (Delay > 0)
                Thread.Sleep(Delay);
        }

        private HttpWebRequest BuildRequest(string url, IEnumerable<HTTPHeader> headers, HTTPOptions options)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Proxy = currentProxy ?? request.Proxy;

            headers = headers ?? Headers;
            options = options ?? Options;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    switch (header.PresetKey)
                    {
                        default:                            request.Headers.Add(header.CustomKey, header.Value);
                            break;
                        case HTTPHeaderKey.UserAgent:       request.UserAgent = header.Value;
                            break;
                        case HTTPHeaderKey.Accept:          request.Accept = header.Value;
                            break;
                        case HTTPHeaderKey.Host:            request.Host = header.Value;
                            break;
                        case HTTPHeaderKey.Referer:         request.Referer = header.Value;
                            break;
                        case HTTPHeaderKey.ContentType:     request.ContentType = header.Value;
                            break;
                        case HTTPHeaderKey.ContentLength:   request.ContentLength = long.Parse(header.Value);
                            break;
                        case HTTPHeaderKey.Connection:
                            if (header.Value != "keep-alive" && header.Value != "close")
                                request.Connection = header.Value;
                            else
                                request.KeepAlive = header.Value == "keep-alive";
                            break;
                    }
                }
            }

            if (options != null)
            {
                request.AllowAutoRedirect = options.AllowAutoRedirect;
                request.ServicePoint.Expect100Continue = options.Expect100Continue;
                request.Timeout = options.Timeout;
                request.Proxy = options.Proxy;
            }

            request.Accept = request.Accept ?? defaultRequest.Accept;
            request.UserAgent = request.UserAgent ?? defaultRequest.UserAgent;

            if (string.IsNullOrEmpty(request.Headers[HttpRequestHeader.AcceptLanguage]))
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, defaultRequest.Headers[HttpRequestHeader.AcceptLanguage]);

            request.Referer = request.Referer ?? Referer;

            return request;
        }

        private string GetResponseString(HttpWebRequest request, Encoding encoding)
        {
            var response = request.GetResponse(quietly: SilentMode);

            encoding = DetectEncoding(encoding, response);

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
                if (memoryStream.Length == 0)
                    StreamReading(memoryStream.Length, response.ContentLength);

                memoryStream.Write(bytesBuffer, 0, bytesSize);

                StreamReading(memoryStream.Length, response.ContentLength);
            }

            memoryStream.Position = 0;

            var streamReader = new StreamReader(memoryStream, encoding);

            var content = streamReader.ReadToEnd();

            streamReader.Close();
            memoryStream.Close();
            response.Close();

            stopWatch.Stop();

            Referer = request.RequestUri.AbsoluteUri;

            return content;
        }

        private Encoding DetectEncoding(Encoding encoding, HttpWebResponse response)
        {
            if (encoding == null && !string.IsNullOrEmpty(response.ContentType))
                encoding = response.ContentType.Contains("=") ? Encoding.GetEncoding(response.ContentType.Split('=')[1]) : null;

            return encoding ?? Encoding.Default;
        }

        private bool TryHandleWebException(Exception exception, Encoding encoding, out string content)
        {
            content = null;

            if (!SilentMode)
                return false;

            if (exception is WebException webException)
            {
                var response = webException.Response;
                var responseStream = response.GetResponseStream() ?? throw new InvalidOperationException();

                encoding = DetectEncoding(encoding, (HttpWebResponse)response);

                using (var sr = new StreamReader(responseStream, encoding))
                    content = sr.ReadToEnd();

                return true;
            }

            return false;
        }
    }
}
