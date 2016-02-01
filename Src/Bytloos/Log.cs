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
        private readonly bool isAppending;
        private readonly string path;
        private readonly string dateFormat;
        private readonly Encoding encoding;
        private readonly List<Tuple<DateTime, object>> lines;

        /// <summary>
        /// Creates log object.
        /// </summary>
        /// <param name="path">Path to save log.</param>
        /// <param name="appendMode">Overrides file content if true.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="dateFormat">DateTime format.</param>
        public Log(string path, bool appendMode = true, Encoding encoding = null, string dateFormat = "G")
        {
            this.path = path;
            this.isAppending = appendMode;
            this.encoding = encoding;
            this.dateFormat = dateFormat;

            this.lines = new List<Tuple<DateTime, object>>();
        }

        /// <summary>
        /// Collection of string messages.
        /// </summary>
        public ReadOnlyCollection<string> Messages
        {
            get
            {
                return this.lines
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
                return this.lines
                    .Select(tuple => tuple.Item2)
                    .OfType<Exception>()
                    .ToList()
                    .AsReadOnly();
            }
        }

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
        /// <param name="message"></param>
        public void Append(string message)
        {
            this.lines.Add(new Tuple<DateTime, object>(DateTime.Now, message));
        }

        /// <summary>
        /// Appends new exception.
        /// </summary>
        /// <param name="exception"></param>
        public void Append(Exception exception)
        {
            this.lines.Add(new Tuple<DateTime, object>(DateTime.Now, exception));
        }

        /// <summary>
        /// Saves changes to file.
        /// </summary>
        /// <param name="saveAs"></param>
        public void Save(string saveAs = null)
        {
            var resultPath = saveAs ?? this.path;

            var dir = Path.GetDirectoryName(resultPath);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(this.path, this.isAppending, this.encoding ?? Encoding.Default))
                foreach (var line in this.lines)
                    sw.WriteLine(
                        line.Item2 is Exception
                            ? string.Format(
                                "{0}{1}:{0}{2}{0}{3}{0}",
                                Environment.NewLine,
                                line.Item1.ToString(this.dateFormat),
                                ((Exception)line.Item2),
                                ((Exception)line.Item2).StackTrace)
                            : string.Format(
                                "{0}: {1}",
                                line.Item1.ToString(this.dateFormat),
                                line.Item2));
        }
    }
}
