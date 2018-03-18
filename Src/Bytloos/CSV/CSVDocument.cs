﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV document.
    /// </summary>
    public class CSVDocument
    {
        private readonly CSVOptions options;
        private readonly List<Cell> cells;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        private CSVDocument(CSVOptions options)
        {
            this.options = options;
            cells = new List<Cell>();
            Rows = new Rows(cells);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="options"></param>
        private CSVDocument(string text, CSVOptions options)
        {
            this.options = options;
            cells = ParseCells(text);
            Rows = new Rows(cells);
        }

        /// <summary>
        /// Rows of cells.
        /// </summary>
        public Rows Rows { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CSVDocument Create()
        {
            return new CSVDocument(CSVOptions.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CSVDocument Create(CSVOptions options)
        {
            return new CSVDocument(options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static CSVDocument LoadFromString(string text)
        {
            return new CSVDocument(text, CSVOptions.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CSVDocument LoadFromString(string text, CSVOptions options)
        {
            return new CSVDocument(text, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CSVDocument LoadFromFile(string path)
        {
            var options = CSVOptions.Default;
            var text = File.ReadAllText(path, options.Encoding);
            return new CSVDocument(text, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CSVDocument LoadFromFile(string path, CSVOptions options)
        {
            var text = File.ReadAllText(path, options.Encoding);
            return new CSVDocument(text, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void AppendRow(params string[] items)
        {
            Rows.Append(items.Select(item => Cell.Parse(item, options)));
        }

        /// <summary>
        /// Saves CSV document to file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveToFile(string path)
        {
            var dir = Path.GetDirectoryName(path);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(path, false, options.Encoding))
            {
                sw.Write(
                    string.Join(
                        separator:  Environment.NewLine,
                        values:     Rows.Select(row =>
                                        string.Join(
                                            separator:  options.Delimiter.ToString(),
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
        /// Cleans rows with lost and redundant cells. May be slow.
        /// </summary>
        public void CleanBrokenRows()
        {
            var cleanRows
                = Rows
                    .GroupBy(row => row.Count())
                    .OrderByDescending(group => group.Count())
                    .First();

            var rowNumber = 0;

            foreach (var row in cleanRows)
            {
                foreach (var cell in row)
                    cell.Y = rowNumber;

                rowNumber++;
            }

            var cleanCells = cleanRows.SelectMany(row => row);

            cells.Clear();
            cells.AddRange(cleanCells);

            Rows = new Rows(cells);
        }

        internal IEnumerable<Cell> GetColumnKeyCells()
        {
            return cells.Where(cell => cell.Y == 0);
        }

        private List<Cell> ParseCells(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var result = new List<Cell>();

            var textBytes = options.Encoding.GetBytes(text);
            var memoryStream = new MemoryStream(textBytes);

            using (var streamReader = new StreamReader(memoryStream))
            {
                var rowNumber = 0;

                while (streamReader.Peek() >= 0 && CheckRowLimit(rowNumber))
                {
                    var columnNumber = 0;

                    var line = streamReader.ReadLine();

                    if (line == null)
                        continue;

                    var cellStrings = new List<string>();
                    var cellStringsDraft = line.Split(options.Delimiter);
                    var waitingForComplete = false;
                    var cellStringBuffer = string.Empty;

                    foreach (var cellStringDraft in cellStringsDraft)
                    {
                        waitingForComplete
                            = (cellStringDraft.FirstOrDefault() == options.QuoteChar && cellStringDraft.LastOrDefault() != options.QuoteChar) ||
                              (waitingForComplete && cellStringDraft.LastOrDefault() != options.QuoteChar);

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
                            cellStringBuffer += cellStringDraft + options.Delimiter;
                        }
                    }

                    foreach (var cellString in cellStrings)
                    {
                        var cell = Cell.Parse(cellString, options);

                        cell.X = columnNumber;
                        cell.Y = rowNumber;
                        cell.ParentDoc = this;

                        result.Add(cell);

                        columnNumber++;
                    }

                    rowNumber++;
                }
            }

            return result;
        }

        private bool CheckRowLimit(int rowNumber)
        {
            var limit = options.RowLimit;

            if (limit == CSVOptions.DEFAULT_ROW_LIMIT)
                return true;

            return rowNumber < limit;
        }
    }
}