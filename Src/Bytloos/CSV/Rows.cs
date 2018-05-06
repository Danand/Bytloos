using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    public class Rows : IEnumerable<Row>
    {
        protected readonly List<Cell> cells;

        private Dictionary<int, List<Cell>> cellsDict; // Prefomance experiment.

        internal Rows(List<Cell> cells)
        {
            this.cells = cells;
            UpdateCellsDict();
        }

        /// <summary>
        /// Rows count.
        /// </summary>
        public int Count
        {
            get
            {
                if (!cells.Any())
                    return 0;

                return cells.Max(cell => cell.Y) + 1;
            }
        }

        /// <summary>
        /// Returns row by index.
        /// </summary>
        /// <param name="index">Row index.</param>
        public Row this[int index]
        {
            get { return new Row(cellsDict[index]); }
        }

        /// <summary>
        /// Returns row by key.
        /// </summary>
        /// <param name="key">Row key by first row cell.</param>
        public Row this[string key]
        {
            get
            {
                var keyCells = GetKeyCells();
                var keyCell = keyCells.FirstOrDefault(cell => cell.Data == key);

                if (keyCell == default(Cell))
                    throw new ArgumentOutOfRangeException(nameof(key));

                return new Row(GetLine(keyCell));
            }
        }

        /// <inheritdoc />
        public IEnumerator<Row> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <summary>
        /// Check row has given key.
        /// </summary>
        /// <param name="key">Row key by first row cell.</param>
        public bool HasKey(string key)
        {
            var keyCells = GetKeyCells();
            return keyCells.Any(cell => cell.Data == key);
        }

        /// <summary>
        /// Gets the row associated with the specified key.
        /// </summary>
        /// <returns>Row by given key.</returns>
        public bool TryGetRow(string key, out Row row)
        {
            row = null;

            var keyCells = GetKeyCells();
            var keyCell = keyCells.FirstOrDefault(cell => cell.Data == key);

            if (keyCell == default(Cell))
                return false;

            row = new Row(GetLine(keyCell));

            return true;
        }

        internal IEnumerable<Cell> GetKeyCells()
        {
            return cells.Where(cell => cell.X == 0);
        }

        internal void Append(IEnumerable<Cell> newCells)
        {
            var rowNumber = Count;
            var columnNumber = 0;

            foreach (var newCell in newCells)
            {
                newCell.Y = rowNumber;
                newCell.X = columnNumber;

                columnNumber++;

                cells.Add(newCell);
            }

            UpdateCellsDict();
        }

        protected List<Cell> GetLine(Cell keyCell)
        {
            return cells.Where(cell => cell.Y == keyCell.Y).ToList();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        private void UpdateCellsDict()
        {
            cellsDict = cells.GroupBy(cell => cell.Y).ToDictionary(group => group.Key, group => group.ToList());
        }
    }
}
