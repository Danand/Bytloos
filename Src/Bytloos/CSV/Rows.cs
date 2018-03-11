using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    public class Rows : IEnumerable<Row>
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly List<Cell> cells;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cells"></param>
        internal Rows(List<Cell> cells)
        {
            this.cells = cells;
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        public Row this[int index]
        {
            get
            {
                return new Row(cells.Where(cell => cell.Y == index).ToList());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
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
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Cell> GetKeyCells()
        {
            return cells.Where(cell => cell.X == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyCell"></param>
        /// <returns></returns>
        protected List<Cell> GetLine(Cell keyCell)
        {
            return cells.Where(cell => cell.Y == keyCell.Y).ToList();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }
    }
}
