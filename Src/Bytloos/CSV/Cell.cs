using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV data cell.
    /// </summary>
    public class Cell : ICloneable
    {
        internal const char QUOTE = '\"';

        private const string ESCAPED_QUOTE = "\"\"";

        private readonly int commonX;
        private readonly int commonY;
        private readonly char delimiter;
        private readonly CSVDocument parentDoc;

        /// <summary>
        /// Creates Cell object.
        /// </summary>
        /// <param name="parentDoc">Reference to a document that contains cell.</param>
        /// <param name="xPosition">Horizontal position.</param>
        /// <param name="yPosition">Vertical position.</param>
        /// <param name="data">Text.</param>
        /// <param name="dataParsing">Data parsing condition.</param>
        /// <param name="delimiter">Delimiter.</param>
        public Cell(
            CSVDocument parentDoc,
            int         xPosition,
            int         yPosition,
            string      data,
            bool        dataParsing = false,
            char        delimiter   = ';')
        {
            this.parentDoc = parentDoc;
            this.delimiter = delimiter;

            Data = dataParsing ? Parse(data) : data;

            this.commonX = this.X = xPosition;
            this.commonY = this.Y = yPosition;
        }

        /// <summary>
        /// Horizontal position.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Vertical position.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Text.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// First cell of column.
        /// </summary>
        public Cell ColumnKey { get { return this.parentDoc.Columns[this.commonX][0]; } }

        /// <summary>
        /// First cell of row.
        /// </summary>
        public Cell RowKey { get { return this.parentDoc.Rows[this.commonY][0]; } }

        internal string EscapedData
        {
            get
            {
                return string.Format(
                    format: "{1}{0}{1}",
                    arg0:   Regex.Replace(Data, @"[\r\n]", string.Empty)
                                .Replace(QUOTE.ToString(), ESCAPED_QUOTE),
                    arg1:   Data.Contains(this.delimiter.ToString()) ||
                            Data.Contains(QUOTE.ToString())
                                ? QUOTE.ToString()
                                : string.Empty);
            }
        }

        /// <summary>
        /// Gets memberwise clone.
        /// </summary>
        /// <returns>Memberwise clone.</returns>
        public object Clone() { return this.MemberwiseClone(); }

        /// <summary>
        /// Gets escaped string representation of cell.
        /// </summary>
        /// <returns>Escaped string representation of cell.</returns>
        public override string ToString() { return this.EscapedData; }

        internal void MovePosition(int xPosition, int yPosition)
        {
            X = xPosition;
            Y = yPosition;
        }

        private static string Parse(string cellString)
        {
            if (string.IsNullOrEmpty(cellString))
                return cellString;

            return cellString.First() == QUOTE && cellString.Last() == QUOTE
                ? cellString.Trim(QUOTE).Replace(ESCAPED_QUOTE, QUOTE.ToString())
                : cellString;
        }
    }
}
