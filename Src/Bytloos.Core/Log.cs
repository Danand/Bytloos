using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Bytloos
{
    /// <summary>
    /// Simple logger solution.
    /// </summary>
    public class Log : IDisposable
    {
        private const string DEFAULT_DATE_FORMAT = "G";

        private readonly bool append;
        private readonly string path;
        private readonly string dateFormat;
        private readonly Encoding encoding;
        private readonly List<Tuple<DateTime, object>> lines;

        /// <summary>
        /// Creates log object.
        /// </summary>
        /// <param name="path">Path to save log.</param>
        /// <param name="append">Overrides file content if true.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="dateFormat">DateTime format.</param>
        public Log(
            string      path,
            bool        append      = true,
            Encoding    encoding    = null,
            string      dateFormat  = DEFAULT_DATE_FORMAT)
        {
            this.path = path;
            this.append = append;
            this.encoding = encoding;
            this.dateFormat = dateFormat;

            lines = new List<Tuple<DateTime, object>>();
        }

        /// <summary>
        /// Collection of string messages.
        /// </summary>
        public ReadOnlyCollection<string> Messages
        {
            get
            {
                return lines
                    .Select(tuple => tuple.Item2)
                    .OfType<string>()
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <summary>
        /// Collection of exceptions.
        /// </summary>
        public ReadOnlyCollection<Exception> Exceptions
        {
            get
            {
                return lines
                    .Select(tuple => tuple.Item2)
                    .OfType<Exception>()
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Saves changes to file.
        /// </summary>
        public void Dispose()
        {
            Save();
        }

        /// <summary>
        /// Appends new message.
        /// </summary>
        /// <param name="message">Message.</param>
        public void Append(string message)
        {
            lines.Add(new Tuple<DateTime, object>(DateTime.Now, message));
        }

        /// <summary>
        /// Appends new exception.
        /// </summary>
        /// <param name="exception">Exception.</param>
        public void Append(Exception exception)
        {
            lines.Add(new Tuple<DateTime, object>(DateTime.Now, exception));
        }

        /// <summary>
        /// Saves changes to file.
        /// </summary>
        /// <param name="saveAs">Specified path.</param>
        public void Save(string saveAs = null)
        {
            var resultPath = saveAs ?? path;
            var dir = Path.GetDirectoryName(resultPath);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(path, append, encoding ?? Encoding.Default))
            {
                foreach (var line in lines)
                {
                    sw.WriteLine(
                        line.Item2 is Exception exception
                            ? string.Format(
                                "{0}{1}:{0}{2}{0}{3}{0}",
                                Environment.NewLine,
                                line.Item1.ToString(dateFormat),
                                exception,
                                exception.StackTrace)
                            : string.Format(
                                "{0}: {1}",
                                line.Item1.ToString(dateFormat),
                                line.Item2));
                }
            }
        }
    }
}
