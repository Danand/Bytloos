﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <inheritdoc />
    public class Rows : IEnumerable<Row>
    {
        private readonly List<Cell> cells;
        private readonly Cached<int> cachedCount = new Cached<int>();
        private readonly Cached<Dictionary<int, List<Cell>>> cachedCellsDict = new Cached<Dictionary<int, List<Cell>>>();
        private readonly Cached<Dictionary<string, Cell>> cachedKeyCells = new Cached<Dictionary<string, Cell>>();

        internal Rows(List<Cell> cells)
        {
            this.cells = cells;
            GetCellsDict();
        }

        /// <summary>
        /// Rows count.
        /// </summary>
        public int Count
        {
            get { return cachedCount.PassValue(CalcCount); }
        }

        /// <summary>
        /// Returns row by index.
        /// </summary>
        /// <param name="index">Row index.</param>
        public Row this[int index]
        {
            get { return new Row(CellsDict[index]); }
        }

        private Dictionary<int, List<Cell>> CellsDict
        {
            get { return cachedCellsDict.PassValue(GetCellsDict); }
        }

        internal Dictionary<string, Cell> KeyCells
        {
            get { return cachedKeyCells.PassValue(GetKeyCells); }
        }

        /// <summary>
        /// Returns row by key.
        /// </summary>
        /// <param name="key">Row key by first row cell.</param>
        public Row this[string key]
        {
            get
            {
                if (!KeyCells.TryGetValue(key, out var keyCell))
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
            return KeyCells.ContainsKey(key);
        }

        /// <summary>
        /// Gets the row associated with the specified key.
        /// </summary>
        /// <returns>Row by given key.</returns>
        public bool TryGetRow(string key, out Row row)
        {
            row = null;

            if (!KeyCells.TryGetValue(key, out var keyCell))
                return false;

            row = new Row(GetLine(keyCell));

            return true;
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

            cachedCount.MarkNeedsUpdate();
            cachedCellsDict.MarkNeedsUpdate();
            cachedKeyCells.MarkNeedsUpdate();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        private Dictionary<string, Cell> GetKeyCells()
        {
            return cells
                .Where(cell => cell.X == 0)
                .GroupBy(cell => cell.Data)
                .Select(group => group.First())
                .ToDictionary(cell => cell.Data, cell => cell);
        }

        private List<Cell> GetLine(Cell keyCell)
        {
            return cells.Where(cell => cell.Y == keyCell.Y).ToList();
        }

        private int CalcCount()
        {
            if (!cells.Any())
                return 0;

            return cells.Max(cell => cell.Y) + 1;
        }

        private Dictionary<int, List<Cell>> GetCellsDict()
        {
            return cells.GroupBy(cell => cell.Y).ToDictionary(group => group.Key, group => group.ToList());
        }
    }
}