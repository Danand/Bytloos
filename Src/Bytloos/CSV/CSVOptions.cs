using System.Text;

namespace Bytloos.CSV
{
    /// <summary>
    /// 
    /// </summary>
    public class CSVOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public static CSVOptions Default
        {
            get
            {
                return new CSVOptions
                {
                    SwapQuotes = false,
                    Delimiter = Cell.DEFAULT_DELIMITER,
                    QuoteChar = Cell.DEFAULT_QUOTE,
                    Encoding = Encoding.UTF8
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SwapQuotes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public char QuoteChar { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Encoding Encoding { get; set; }
    }
}