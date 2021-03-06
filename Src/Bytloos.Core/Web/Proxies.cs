﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Bytloos.Web
{
    /// <summary>
    /// Proxy tools.
    /// </summary>
    public static class Proxies
    {
        /// <summary>
        /// Parses proxy list.
        /// </summary>
        /// <param name="proxyListPath">Path to file that contains proxy list.</param>
        /// <returns>List of proxy objects.</returns>
        public static List<WebProxy> LoadProxyList(string proxyListPath)
        {
            if (string.IsNullOrEmpty(proxyListPath))
                return null;

            if (!File.Exists(proxyListPath))
                throw new FileNotFoundException();

            var list = new List<WebProxy>();

            using (var sr = new StreamReader(proxyListPath, Encoding.Default))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var lineSplitted = line.Split(';', ',', '\t');

                    if (lineSplitted.Length == 1)
                        list.Add(new WebProxy(line.Trim()));

                    if (lineSplitted.Length == 3)
                    {
                        list.Add(new WebProxy(lineSplitted[0].Trim())
                        {
                            Credentials = new NetworkCredential(lineSplitted[1].Trim(), lineSplitted[2].Trim())
                        });
                    }
                }
            }

            return list;
        }
    }
}
