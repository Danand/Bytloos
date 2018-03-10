using System.Text.RegularExpressions;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV data cell.
    /// </summary>
    public class Cell
    {
        internal const char DEFAULT_DELIMITER = ';';
        internal const char DEFAULT_QUOTE = '\"';
        internal const char ALTERNATIVE_QUOTE = '\'';

        private readonly CSVOptions options;

        /// <summary>
        /// Creates Cell object.
        /// </summary>
        /// <param name="data">Text.</param>
        /// <param name="dataParsing">Data parsing condition.</param>
        /// <param name="options"></param>
        private Cell(string data, bool dataParsing, CSVOptions options)
        {
            this.options = options;
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
            get; set;
        }

        /// <summary>
        /// Vertical position.
        /// </summary>
        internal int Y
        {
            get; set;
        }

        private string EscapedQuote
        {
            get { return $"{options.QuoteChar}{options.QuoteChar}"; }
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
        /// <param name="options"></param>
        /// <returns></returns>
        public static Cell Parse(string cellString, CSVOptions options)
        {
            return new Cell(cellString, true, options);
        }

        private string ParseData(string cellString)
        {
            if (string.IsNullOrEmpty(cellString))
                return cellString;

            return cellString.First() == options.QuoteChar && cellString.Last() == options.QuoteChar
                ? cellString.Trim(options.QuoteChar).Replace(EscapedQuote, options.QuoteChar.ToString())
                : cellString;
        }

        private string EscapeQuotes(string input)
        {
            if (options.SwapQuotes)
            {
                var newQuote = options.QuoteChar == ALTERNATIVE_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;
                var oldQuote = options.QuoteChar == DEFAULT_QUOTE ? DEFAULT_QUOTE : ALTERNATIVE_QUOTE;

                if (newQuote != oldQuote)
                    return input.Replace(oldQuote.ToString(), newQuote.ToString());
            }

            return input.Replace(options.QuoteChar.ToString(), EscapedQuote);
        }

        private string ChooseQuotes(string input)
        {
            if (input.Contains(options.Delimiter.ToString()) ||
                input.Contains(options.QuoteChar.ToString()) ||
                input.Contains(DEFAULT_QUOTE.ToString()))
            {
                return options.QuoteChar.ToString();
            }

            return string.Empty;
        }
    }
}
