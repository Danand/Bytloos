using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV document.
    /// </summary>
    public class CSVDocument : IDisposable
    {
        private const char DEFAULT_SOURCE_SEPARATOR = ';';

        private readonly int columnsAmountFilter;
        private readonly bool swapQuotes;
        private readonly char delimiter;
        private readonly char quoteChar;
        private readonly string path;
        private readonly string[] readingLineFilters;
        private readonly Encoding encoding;
        private readonly List<Cell> cells;

        // TODO: move parameters to settings class.
        /// <summary>
        /// Creates CSV document object.
        /// </summary>
        /// <param name="path">Path to CSV file.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <param name="quoteChar">Quote char.</param>
        /// <param name="swapQuotes">Swap quotes between " and ' if cell contains ones.</param>
        /// <param name="columnsAmountFilter">Number of columns limiting to.</param>
        /// <param name="readingLineFilters">Array of string using as filter when parsing CSV.</param>
        public CSVDocument(
            string      path,
            Encoding    encoding            = null,
            char        delimiter           = Cell.DEFAULT_DELIMITER,
            char        quoteChar           = Cell.DEFAULT_QUOTE,
            bool        swapQuotes          = false,
            int         columnsAmountFilter = 0,
            string[]    readingLineFilters  = null)
        {
            this.path = path;
            this.encoding = encoding ?? Encoding.Default;
            this.delimiter = delimiter;
            this.quoteChar = quoteChar;
            this.swapQuotes = swapQuotes;
            this.columnsAmountFilter = columnsAmountFilter;
            this.readingLineFilters = readingLineFilters;

            cells = new List<Cell>();

            Rows = new Rows(cells);

            if (File.Exists(this.path))
                Load(this.path, this.encoding, this.delimiter);
        }

        /// <summary>
        /// Rows of cells.
        /// </summary>
        public Rows Rows { get; }

        /// <inheritdoc />
        /// <summary>
        /// Disposes and saves changes to file.
        /// </summary>
        public void Dispose()
        {
            Save();
        }

        /// <summary>
        /// Loads CSV document from file.
        /// </summary>
        /// <param name="sourcePath">Path to CSV file.</param>
        /// <param name="sourceEncoding">Encoding.</param>
        /// <param name="sourceSeparator">Delimiter.</param>
        public void Load(string sourcePath, Encoding sourceEncoding = null, char sourceSeparator = DEFAULT_SOURCE_SEPARATOR)
        {
            using (var streamReader = new StreamReader(sourcePath, sourceEncoding ?? Encoding.Default))
            {
                while (streamReader.Peek() >= 0)
                {
                    var line = streamReader.ReadLine();

                    if (line == null || (readingLineFilters != null && readingLineFilters.Any(line.Contains)))
                        continue;

                    var cellStrings = new List<string>();
                    var cellStringsDraft = line.Split(sourceSeparator);
                    var waitingForComplete = false;
                    var cellStringBuffer = string.Empty;

                    foreach (var cellStringDraft in cellStringsDraft)
                    {
                        waitingForComplete
                            = (cellStringDraft.FirstOrDefault() == quoteChar && cellStringDraft.LastOrDefault() != quoteChar) ||
                              (waitingForComplete && cellStringDraft.LastOrDefault() != quoteChar);

                        if (!waitingForComplete)
                        {
                            cellStrings.Add(
                                cellStringBuffer != null
                                    ? cellStringBuffer + cellStringDraft
                                    : cellStringDraft);

                            cellStringBuffer = null;
                        }
                        else
                        {
                            cellStringBuffer += (cellStringDraft + sourceSeparator);
                        }
                    }

                    if (columnsAmountFilter != 0 && cellStrings.Count != columnsAmountFilter)
                        continue;

                    foreach (var cellString in cellStrings)
                        cells.Add(Cell.Parse(cellString, swapQuotes, delimiter, quoteChar));
                }
            }
        }

        /// <summary>
        /// Saves CSV document to file.
        /// </summary>
        /// <param name="saveAs">Save in another directory.</param>
        public void Save(string saveAs = null)
        {
            var resultPath = saveAs ?? path;
            var dir = Path.GetDirectoryName(resultPath);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(resultPath, false, encoding))
            {
                sw.WriteLine(
                    string.Join(
                        separator:  Environment.NewLine,
                        values:     Rows.Select(
                                        row => string.Join(
                                                separator:  delimiter.ToString(CultureInfo.InvariantCulture),
                                                values:     row))));
            }
        }

        /// <summary>
        /// Clears cells.
        /// </summary>
        public void Clear()
        {
            cells.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void AppendRow(params string[] items)
        {
            Rows.Append(items.Select(item => Cell.Parse(item, swapQuotes, delimiter, quoteChar)));
        }
    }
}