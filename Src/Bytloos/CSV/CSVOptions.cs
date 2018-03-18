using System.Text;

namespace Bytloos.CSV
{
    /// <summary>
    /// 
    /// </summary>
    public class CSVOptions
    {
        internal const bool DEFAULT_SWAP_QUOTES = false;
        internal const int DEFAULT_ROW_LIMIT = 0;

        /// <summary>
        /// 
        /// </summary>
        public static CSVOptions Default
        {
            get { return new CSVOptions(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SwapQuotes { get; set; } = DEFAULT_SWAP_QUOTES;

        /// <summary>
        /// 
        /// </summary>
        public int RowLimit { get; set; } = DEFAULT_ROW_LIMIT;

        /// <summary>
        /// 
        /// </summary>
        public char Delimiter { get; set; } = Cell.DEFAULT_DELIMITER;

        /// <summary>
        /// 
        /// </summary>
        public char QuoteChar { get; set; } = Cell.DEFAULT_QUOTE;

        /// <summary>
        /// 
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}