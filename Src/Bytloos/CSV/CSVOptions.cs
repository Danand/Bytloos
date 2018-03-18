using System.Text;

namespace Bytloos.CSV
{
    /// <summary>
    /// Options such as encoding, row limit, delimiter, quote char, etc.
    /// </summary>
    public class CSVOptions
    {
        internal const bool DEFAULT_SWAP_QUOTES = false;
        internal const int DEFAULT_ROW_LIMIT = 0;

        /// <summary>
        /// Default options.
        /// </summary>
        public static CSVOptions Default
        {
            get { return new CSVOptions(); }
        }

        /// <summary>
        /// Swaps default and escape quotes.
        /// </summary>
        public bool SwapQuotes { get; set; } = DEFAULT_SWAP_QUOTES;

        /// <summary>
        /// Row limit.
        /// </summary>
        public int RowLimit { get; set; } = DEFAULT_ROW_LIMIT;

        /// <summary>
        /// Row delimiter.
        /// </summary>
        public char Delimiter { get; set; } = Cell.DEFAULT_DELIMITER;

        /// <summary>
        /// Quote character.
        /// </summary>
        public char QuoteChar { get; set; } = Cell.DEFAULT_QUOTE;

        /// <summary>
        /// Encoding.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}