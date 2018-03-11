using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    public class Row : IEnumerable<Cell>
    {
        private readonly List<Cell> cells;

        internal Row(List<Cell> cells)
        {
            this.cells = cells;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public Cell this[int index]
        {
            get
            {
                var result = cells.FirstOrDefault(cell => cell.X == index);

                if (result == default(Cell))
                    throw new IndexOutOfRangeException(nameof(index));

                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public Cell this[string key]
        {
            get
            {
                // TODO: Crutchy. Replace ParentDoc.GetColumnKeyCells() with ParentDoc.Columns.GetKeyCells().
                var keyCells = cells.First().ParentDoc.GetColumnKeyCells();

                var keyCell = keyCells.FirstOrDefault(cell => cell.Data == key);

                if (keyCell == default(Cell))
                    throw new ArgumentOutOfRangeException(nameof(key));

                return cells.First(cell => cell.X == keyCell.X);
            }
        }

        /// <inheritdoc />
        public IEnumerator<Cell> GetEnumerator()
        {
            return cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cells.GetEnumerator();
        }
    }
}
