using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    /// <summary>
    /// CSV data cell.
    /// </summary>
    public class Cell : ICloneable
    {
        internal const char DEFAULT_DELIMITER = ';';
        internal const char DEFAULT_QUOTE = '\"';
        internal const char ALTERNATIVE_QUOTE = '\'';

        private readonly bool swapQuotes;
        private readonly char delimiter;
        private readonly char quote;

        /// <summary>
        /// Creates Cell object.
        /// </summary>
        /// <param name="data">Text.</param>
        /// <param name="dataParsing">Data parsing condition.</param>
        /// <param name="swapQuotes">>Swap quotes between " and ' if cell contains ones.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <param name="quote">Quote char.</param>
        private Cell(
            string      data,
            bool        dataParsing = false,
            bool        swapQuotes = false,
            char        delimiter   = DEFAULT_DELIMITER,
            char        quote       = DEFAULT_QUOTE)
        {
            this.delimiter = delimiter;
            this.quote = quote;
            this.swapQuotes = swapQuotes;

            Data = dataParsing ? ParseData(data) : data;
        }

        /// <summary>
        /// Text.
        /// </summary>
        public string Data
        {
            get; set;
        }

        /// <summary>
        /// Horizontal position.
        /// </summary>
        internal int X
        {
            get; private set;
        }

        /// <summary>
        /// Vertical position.
        /// </summary>
        internal int Y
        {
            get; private set;
        }

        private string EscapedQuote
        {
            get { return $"{quote}{quote}"; }
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

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Gets escaped string representation of cell.
        /// </summary>
        /// <returns>Escaped string representation of cell.</returns>
        public override string ToString()
        {
            return EscapedData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cellString"></param>
        /// <param name="swapQuotes"></param>
        /// <param name="delimiter"></param>
        /// <param name="quote"></param>
        /// <returns></returns>
        public static Cell Parse(string cellString, bool swapQuotes, char delimiter, char quote)
        {
            return new Cell(cellString, true, swapQuotes, delimiter, quote);
        }

        internal void MovePosition(int xPosition, int yPosition)
        {
            X = xPosition;
            Y = yPosition;
        }

        private string ParseData(string cellString)
        {
            if (string.IsNullOrEmpty(cellString))
                return cellString;

            return cellString.First() == quote && cellString.Last() == quote
                ? cellString.Trim(quote).Replace(EscapedQuote, quote.ToString())
                : cellString;
        }

        private string EscapeQuotes(string input)
        {
            if (swapQuotes)
            {
                var newQuote = quote == ALTERNATIVE_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;
                var oldQuote = quote == DEFAULT_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;

                if (newQuote != oldQuote)
                    return input.Replace(oldQuote.ToString(), newQuote.ToString());
            }

            return input.Replace(quote.ToString(), EscapedQuote);
        }

        private string ChooseQuotes(string input)
        {
            if (input.Contains(delimiter.ToString()) ||
                input.Contains(quote.ToString()) ||
                input.Contains(DEFAULT_QUOTE.ToString()))
            {
                return quote.ToString();
            }

            return string.Empty;
        }
    }
}
