using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    public class Rows : IEnumerable<IEnumerable<Cell>>
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
        public List<Cell> this[int index]
        {
            get
            {
                return cells.Where(cell => cell.Y == index).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public List<Cell> this[string key]
        {
            get
            {
                var keyCells = GetKeyCells();

                var keyCell = keyCells.FirstOrDefault(cell => cell.Data == key);

                if (keyCell == default(Cell))
                    throw new ArgumentOutOfRangeException(nameof(key));

                return GetLine(keyCell);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <inheritdoc />
        public IEnumerator<IEnumerable<Cell>> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Cell> GetKeyCells()
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
    }
}
