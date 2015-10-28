using System.Collections.Generic;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV data cell extension methods.
    /// </summary>
    public static class CellExtensions
    {
        /// <summary>
        /// Sorts in order provided by list of key cells.
        /// </summary>
        /// <param name="source">Source list of cells.</param>
        /// <param name="keyCells">List of key cells.</param>
        /// <returns>List of cells sorted in order provided by list of key cells.</returns>
        public static List<Cell> ArrangeBy(this List<Cell> source, List<Cell> keyCells)
        {
            foreach (var keyCell in keyCells)
            {
                foreach (var cell in source)
                {
                    var columnKeyMatches = keyCell.Data == cell.ColumnKey.Data;
                    var rowKeyMatches = keyCell.Data == cell.RowKey.Data;

                    if (!rowKeyMatches && !columnKeyMatches)
                        continue;

                    cell.MovePosition(
                        xPosition:  columnKeyMatches ? keyCell.X : cell.X,
                        yPosition:  rowKeyMatches ? keyCell.Y : cell.Y);

                    break;
                }
            }

            return source;
        }

        /// <summary>
        /// Converts row list of cells to a dictionary.
        /// </summary>
        /// <param name="source">Source list of cells.</param>
        /// <param name="suppressConflicts">Ignore content conflicts.</param>
        /// <returns>Dictionary of cells with row key as key.</returns>
        public static Dictionary<string, Cell> RowToDictionary(this List<Cell> source, bool suppressConflicts = false)
        {
            return source.ToDictionary(suppressConflicts);
        }

        /// <summary>
        /// Converts column list of cells to a dictionary.
        /// </summary>
        /// <param name="source">Source list of cells.</param>
        /// <param name="suppressConflicts">Ignore content conflicts.</param>
        /// <returns>Dictionary of cells with column key as key.</returns>
        public static Dictionary<string, Cell> ColumnToDictionary(this List<Cell> source, bool suppressConflicts = false)
        {
            return source.ToDictionary(isColumn: true, suppressConflicts: suppressConflicts);
        }

        private static Dictionary<string, Cell> ToDictionary(this List<Cell> source, bool isColumn = false, bool suppressConflicts = false)
        {
            var dict = new Dictionary<string, Cell>();

            foreach (var cell in source)
                if(suppressConflicts && !dict.ContainsKey((isColumn ? cell.RowKey : cell.ColumnKey).Data))
                    dict.Add((isColumn ? cell.RowKey : cell.ColumnKey).Data, cell);

            return dict;
        }
    }
}
