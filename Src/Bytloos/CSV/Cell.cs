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
        internal const char DEFAULT_DELIMITER = ';';
        internal const char DEFAULT_QUOTE = '\"';
        internal const char ALTERNATIVE_QUOTE = '\'';

        private readonly bool swapQuotes;
        private readonly int commonX;
        private readonly int commonY;
        private readonly char delimiter;
        private readonly char quote;
        private readonly CSVDocument parentDoc;

        /// <summary>
        /// Creates Cell object.
        /// </summary>
        /// <param name="parentDoc">Reference to a document that contains cell.</param>
        /// <param name="xPosition">Horizontal position.</param>
        /// <param name="yPosition">Vertical position.</param>
        /// <param name="data">Text.</param>
        /// <param name="dataParsing">Data parsing condition.</param>
        /// <param name="swapQuotes">>Swap quotes between " and ' if cell contains ones.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <param name="quote">Quote char.</param>
        public Cell(
            CSVDocument parentDoc,
            int         xPosition,
            int         yPosition,
            string      data,
            bool        dataParsing = false,
            bool        swapQuotes = false,
            char        delimiter   = DEFAULT_DELIMITER,
            char        quote       = DEFAULT_QUOTE)
        {
            this.parentDoc = parentDoc;
            this.delimiter = delimiter;
            this.quote = quote;
            this.swapQuotes = swapQuotes;

            Data = dataParsing ? Parse(data) : data;

            this.commonX = X = xPosition;
            this.commonY = Y = yPosition;
        }

        /// <summary>
        /// Horizontal position.
        /// </summary>
        public int X
        {
            get; private set;
        }

        /// <summary>
        /// Vertical position.
        /// </summary>
        public int Y
        {
            get; private set;
        }

        /// <summary>
        /// Text.
        /// </summary>
        public string Data
        {
            get; set;
        }

        /// <summary>
        /// First cell of column.
        /// </summary>
        public Cell ColumnKey
        {
            get { return this.parentDoc.Columns[this.commonX].First(); }
        }

        /// <summary>
        /// First cell of row.
        /// </summary>
        public Cell RowKey
        {
            get { return this.parentDoc.Rows[this.commonY].First(); }
        }

        private string EscapedQuote
        {
            get { return string.Format("{0}{1}", this.quote, this.quote); }
        }

        private string EscapedData
        {
            get
            {
                var data = Regex.Replace(Data, @"[\r\n]", string.Empty);
                var chosenQuote = ChooseQuotes(data);
                var escapedData = EscapeQuotes(data);

                return $"{chosenQuote}{escapedData}{chosenQuote}";
            }
        }

        /// <summary>
        /// Gets memberwise clone.
        /// </summary>
        /// <returns>Memberwise clone.</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Gets escaped string representation of cell.
        /// </summary>
        /// <returns>Escaped string representation of cell.</returns>
        public override string ToString()
        {
            return this.EscapedData;
        }

        internal void MovePosition(int xPosition, int yPosition)
        {
            X = xPosition;
            Y = yPosition;
        }

        private string Parse(string cellString)
        {
            if (string.IsNullOrEmpty(cellString))
                return cellString;

            return cellString.First() == this.quote && cellString.Last() == this.quote
                ? cellString.Trim(this.quote).Replace(EscapedQuote, this.quote.ToString())
                : cellString;
        }

        private string EscapeQuotes(string input)
        {
            if (this.swapQuotes)
            {
                var newQuote = this.quote == ALTERNATIVE_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;
                var oldQuote = this.quote == DEFAULT_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;

                if (newQuote != oldQuote)
                    return input.Replace(oldQuote.ToString(), newQuote.ToString());
            }

            return input.Replace(this.quote.ToString(), EscapedQuote);
        }

        private string ChooseQuotes(string input)
        {
            if (input.Contains(this.delimiter.ToString()) ||
                input.Contains(this.quote.ToString()) ||
                input.Contains(DEFAULT_QUOTE.ToString()))
            {
                return quote.ToString();
            }

            return string.Empty;
        }
    }
}
