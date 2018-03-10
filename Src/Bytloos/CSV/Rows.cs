using System;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// </summary>
    public class Rows
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
            get { return cells.Max(cell => cell.Y); }
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
                var keyCell = keyCells.FirstOrDefault();

                if (keyCell == default(Cell))
                    throw new ArgumentOutOfRangeException(nameof(key));

                return GetLine(keyCell);
            }
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
        protected Row GetLine(Cell keyCell)
        {
            return (Row)cells.Where(cell => cell.Y == keyCell.Y).ToList();
        }
    }
}
