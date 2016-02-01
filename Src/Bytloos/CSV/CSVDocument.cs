using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Bytloos.CSV
{
    /// <summary>
    /// CSV document.
    /// </summary>
    public class CSVDocument : IDisposable
    {
        private readonly int columnsAmountFilter;
        private readonly bool isSilent;
        private readonly char delimiter;
        private readonly string path;
        private readonly string[] readingLineFilters;
        private readonly Encoding encoding;

        private List<Cell> cells;
        private List<List<Cell>> cachedRows;
        private List<List<Cell>> cachedColumns;

        /// <summary>
        /// Creates CSV document object.
        /// </summary>
        /// <param name="path">Path to CSV file.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="delimiter">Delimiter.</param>
        /// <param name="silentMode">Suppress exceptions.</param>
        /// <param name="columnsAmountFilter">Number of columns limiting to.</param>
        /// <param name="readingLineFilters">Array of string using as filter when parsing CSV.</param>
        public CSVDocument(
            string      path,
            Encoding    encoding            = null,
            char        delimiter           = Cell.DEFAULT_DELIMITER,
            bool        silentMode          = false,
            int         columnsAmountFilter = 0,
            string[]    readingLineFilters  = null)
        {
            this.path = path;
            this.encoding = encoding ?? Encoding.Default;
            this.delimiter = delimiter;
            this.isSilent = silentMode;
            this.columnsAmountFilter = columnsAmountFilter;
            this.readingLineFilters = readingLineFilters;
            this.cells = new List<Cell>();

            if (File.Exists(this.path))
                Load(this.path, this.encoding, this.delimiter);
        }

        /// <summary>
        /// Row of cells.
        /// </summary>
        public List<List<Cell>> Rows
        {
            get { return cachedRows ?? (cachedRows = GetLines()); }
        }

        /// <summary>
        /// Column of cells.
        /// </summary>
        public List<List<Cell>> Columns
        {
            get { return cachedColumns ?? (cachedColumns = GetLines(vertical: true)); }
        }

        /// <summary>
        /// Serializes object fields as CSV table.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="document">CSV document for writing fields in.</param>
        public static void Serialize(object target, CSVDocument document)
        {
            foreach (var fieldInfo in target.GetType().GetFields())
                if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(ISerializable)))
                    document.AppendRow(
                        fieldInfo.Name,
                        ((ISerializable)fieldInfo.GetValue(target)).GetStringValue());

            foreach (var propertyInfo in target.GetType().GetProperties())
                if (propertyInfo.PropertyType.GetInterfaces().Contains(typeof(ISerializable)))
                    document.AppendRow(
                        propertyInfo.Name,
                        ((ISerializable)propertyInfo.GetValue(obj: target, index: null)).GetStringValue());

            document.Save();
        }

        /// <summary>
        /// Deserializes object fields from CSV table.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="document">CSV document with stored object.</param>
        /// <returns>Deserialized object.</returns>
        public static T Deserialize<T>(CSVDocument document)
            where T : new()
        {
            var result = new T();

            foreach (var row in document.Rows)
                foreach (var fieldInfo in typeof(T).GetFields())
                    if (fieldInfo.Name == row.First().Data &&
                        fieldInfo.FieldType.GetInterfaces().Contains(typeof(ISerializable)))
                    {
                        fieldInfo.SetValue(
                            obj:    result,
                            value:  Activator.CreateInstance(fieldInfo.FieldType));

                        ((ISerializable)fieldInfo.GetValue(result)).SetValueFromString(row.Last().Data);
                    }

            foreach (var row in document.Rows)
                foreach (var propertyInfo in typeof(T).GetProperties())
                    if (propertyInfo.Name == row.First().Data &&
                        propertyInfo.PropertyType.GetInterfaces().Contains(typeof(ISerializable)))
                    {
                        propertyInfo.SetValue(
                            obj:    result,
                            value:  Activator.CreateInstance(propertyInfo.PropertyType),
                            index:  null);

                        ((ISerializable)propertyInfo.GetValue(obj: result, index: null)).SetValueFromString(row.Last().Data);
                    }

            return result;
        }

        /// <summary>
        /// Disposes and saves changes to file.
        /// </summary>
        public void Dispose()
        {
            Save();
        }

        /// <summary>
        /// Loads CSV document from file.
        /// </summary>
        /// <param name="sourcePath">Path to CSV file.</param>
        /// <param name="sourceEncoding">Encoding.</param>
        /// <param name="sourceSeparator">Delimiter.</param>
        public void Load(string sourcePath, Encoding sourceEncoding = null, char sourceSeparator = ';')
        {
            using (var streamReader = new StreamReader(sourcePath, sourceEncoding ?? Encoding.Default))
            {
                var y = 0;

                while (streamReader.Peek() >= 0)
                {
                    var x = 0;
                    var line = streamReader.ReadLine();

                    if (line == null || (this.readingLineFilters != null && readingLineFilters.Any(line.Contains)))
                        continue;

                    var cellStrings = new List<String>();
                    var cellStringsDraft = line.Split(sourceSeparator);
                    var waitingForComplete = false;
                    var cellStringBuffer = string.Empty;

                    foreach (var cellStringDraft in cellStringsDraft)
                    {
                        waitingForComplete
                            = (cellStringDraft.FirstOrDefault() == Cell.QUOTE && cellStringDraft.LastOrDefault() != Cell.QUOTE) ||
                              (waitingForComplete && cellStringDraft.LastOrDefault() != Cell.QUOTE);

                        if (!waitingForComplete)
                        {
                            cellStrings.Add(
                                cellStringBuffer != null
                                    ? cellStringBuffer + cellStringDraft
                                    : cellStringDraft);

                            cellStringBuffer = null;
                        }
                        else
                        {
                            cellStringBuffer += (cellStringDraft + sourceSeparator);
                        }
                    }

                    if (this.columnsAmountFilter != 0 && cellStrings.Count != this.columnsAmountFilter)
                        continue;

                    foreach (var cellString in cellStrings)
                        this.cells.Add(
                            new Cell(
                                parentDoc:      this,
                                xPosition:      x++,
                                yPosition:      y,
                                data:           cellString,
                                dataParsing:    true,
                                delimiter:      this.delimiter));

                    y++;
                }
            }
        }

        /// <summary>
        /// Saves CSV document to file.
        /// </summary>
        /// <param name="saveAs">Save in another directory.</param>
        public void Save(string saveAs = null)
        {
            var resultPath = saveAs ?? this.path;

            var dir = Path.GetDirectoryName(resultPath);

            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var sw = new StreamWriter(resultPath, false, this.encoding))
                sw.WriteLine(
                    string.Join(
                        separator:  Environment.NewLine,
                        values:     Rows.Select(
                                    row => string.Join(
                                        separator:  this.delimiter.ToString(CultureInfo.InvariantCulture),
                                        values:     row))));
        }

        /// <summary>
        /// Gets row by string key.
        /// </summary>
        /// <param name="key">String representation of row's first cell.</param>
        /// <returns>Row of cells.</returns>
        public List<Cell> GetRow(string key)
        {
            return GetLine(key);
        }

        /// <summary>
        /// Gets column by string key.
        /// </summary>
        /// <param name="key">String representation of column's first cell.</param>
        /// <returns>Column of cells.</returns>
        public List<Cell> GetColumn(string key)
        {
            return GetLine(key, isColumn: true);
        }

        /// <summary>
        /// Clears cells.
        /// </summary>
        public void Clear()
        {
            this.cells = new List<Cell>();
        }

        #region Append and Insert overloads

        /// <summary>
        /// Appends cells to last row.
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        public void AppendCellsHorizontal(List<Cell> cellList) { AppendLine(cellList, isNewLine: false); }

        /// <summary>
        /// Appends cells to last last row.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        public void AppendCellsHorizontal(object[] cellObjects) { AppendLine(cellObjects, isNewLine: false); }

        /// <summary>
        /// Appends cells to last last row.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        public void AppendCellsHorizontal(params string[] cellStrings) { AppendLine(cellStrings.Select(cell => (object)cell).ToArray(), isNewLine: false); }

        /// <summary>
        /// Appends cells to last column.
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        public void AppendCellsVertical(List<Cell> cellList) { AppendLine(cellList, isColumn: true, isNewLine: false); }

        /// <summary>
        /// Appends cells to last column.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        public void AppendCellsVertical(object[] cellObjects) { AppendLine(cellObjects, isColumn: true, isNewLine: false); }

        /// <summary>
        /// Appends cells to last column.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        public void AppendCellsVertical(params string[] cellStrings) { AppendLine(cellStrings.Select(cell => (object)cell).ToArray(), isColumn: true, isNewLine: false); }

        /// <summary>
        /// Appends cells as last row.
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        public void AppendRow(List<Cell> cellList) { AppendLine(cellList); }

        /// <summary>
        /// Appends cells as last column.
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        public void AppendColumn(List<Cell> cellList) { AppendLine(cellList, isColumn: true); }

        /// <summary>
        /// Appends cells as last last row.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        public void AppendRow(object[] cellObjects) { AppendLine(cellObjects); }

        /// <summary>
        /// Appends cells as last column.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        public void AppendColumn(object[] cellObjects) { AppendLine(cellObjects, isColumn: true); }

        /// <summary>
        /// Appends cells as last last row.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        public void AppendRow(params string[] cellStrings) { AppendLine(cellStrings.Select(cell => (object)cell).ToArray()); }

        /// <summary>
        /// Appends cells as last column.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        public void AppendColumn(params string[] cellStrings) { AppendLine(cellStrings.Select(cell => (object)cell).ToArray(), isColumn: true); }

        /// <summary>
        /// Inserts cells as row at index.
        /// </summary>
        /// <param name="cellObjects">List of cell objects.</param>
        /// <param name="position">Index.</param>
        public void InsertRow(List<Cell> cellObjects, int position) { InsertLine(cellObjects, position); }

        /// <summary>
        /// Inserts cells as column at index.
        /// </summary>
        /// <param name="cellObjects">List of cell objects.</param>
        /// <param name="position">Index.</param>
        public void InsertColumn(List<Cell> cellObjects, int position) { InsertLine(cellObjects, position, isColumn: true); }

        /// <summary>
        /// Inserts cells as row at index.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        /// <param name="position">Index.</param>
        public void InsertRow(object[] cellObjects, int position) { InsertLine(cellObjects, position); }

        /// <summary>
        /// Inserts cells as row at index.
        /// </summary>
        /// <param name="cellObjects">Array of objects.</param>
        /// <param name="position">Index.</param>
        public void InsertColumn(object[] cellObjects, int position) { InsertLine(cellObjects, position, isColumn: true); }

        /// <summary>
        /// Inserts cells as row at index.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        /// <param name="position">Index.</param>
        public void InsertRow(string[] cellStrings, int position) { InsertLine(cellStrings.Select(cell => (object)cell).ToArray(), position); }

        /// <summary>
        /// Inserts cells as column at index.
        /// </summary>
        /// <param name="cellStrings">String params.</param>
        /// <param name="position">Index.</param>
        public void InsertColumn(string[] cellStrings, int position) { InsertLine(cellStrings.Select(cell => (object)cell).ToArray(), position, isColumn: true); }

        #endregion

        private void AppendLine<TObject>(
            IEnumerable<TObject>    cellObjects,
            bool                    isColumn    = false,
            int                     inset       = -1,
            bool                    isNewLine   = true)
        {
            var x
                = ComputePosition(
                    isVertical: false,
                    forColumn:  isColumn,
                    forNewLine: isNewLine,
                    inset:      inset);

            var y
                = ComputePosition(
                    isVertical: true,
                    forColumn:  isColumn,
                    forNewLine: isNewLine,
                    inset:      inset);

            if (cellObjects is List<Cell>)
            {
                foreach (var clonedCell in ((List<Cell>)cellObjects).Select(cellObject => (Cell)cellObject.Clone()))
                {
                    clonedCell.MovePosition(
                        xPosition:  isColumn ? x : x++,
                        yPosition:  !isColumn ? y : y++);

                    this.cells.Add(clonedCell);
                }
            }
            else
            {
                foreach (var cellObject in cellObjects)
                    this.cells.Add(
                        new Cell(
                            parentDoc:  this,
                            xPosition:  isColumn ? x : x++,
                            yPosition:  !isColumn ? y : y++,
                            data:       cellObject != null ? cellObject.ToString() : string.Empty));
            }

            if (inset > -1)
                CleanUpCells();

            cachedRows = null;
            cachedColumns = null;
        }

        private void InsertLine(List<Cell> cellObjects, int position, bool isColumn = false)
        {
            AppendLine(cellObjects, isColumn, position);
        }

        private void InsertLine(object[] cellObjects, int position, bool isColumn = false)
        {
            AppendLine(cellObjects, isColumn, position);
        }

        private void CleanUpCells()
        {
            this.cells
                = this.cells
                    .GroupBy(cell => new { cell.X, cell.Y })
                    .Select(group => group.Last())
                    .ToList();
        }

        private int ComputePosition(
            bool    isVertical  = false,
            bool    forColumn   = false,
            bool    forNewLine  = true,
            int     inset       = -1)
        {
            var point
                = isVertical
                    ? !forColumn ? Rows.Count : 0
                    : forColumn ? Columns.Count : 0;

            if (!forNewLine)
                point
                    = isVertical
                        ? !forColumn
                            ? Rows.Any() ? Rows.Count - 1 : 0
                            : Columns.Any() ? Columns.Last().Count : 0
                        : !forColumn
                            ? Rows.Any() ? Rows.Last().Count : 0
                            : Columns.Any() ? Columns.Count - 1 : 0;

            return isVertical
                ? inset > -1 && !forColumn ? inset : point
                : inset > -1 && forColumn ? inset : point;
        }

        private List<Cell> GetLine(string key, bool isColumn = false)
        {
            var result = (isColumn ? Columns : Rows).Find(line => line[0].Data == key);

            if (result == null && !this.isSilent)
                throw new InvalidOperationException();

            return result;
        }

        private List<List<Cell>> GetLines(bool vertical = false)
        {
            var lines = new List<List<Cell>>();

            foreach (var cell in this.cells)
            {
                while ((vertical ? cell.X : cell.Y) >= lines.Count)
                    lines.Add(new List<Cell>());

                while ((vertical ? cell.Y : cell.X) > lines[vertical ? cell.X : cell.Y].Count)
                    lines[vertical ? cell.X : cell.Y].Add(
                        new Cell(
                            parentDoc:  this,
                            xPosition:  cell.X,
                            yPosition:  cell.Y,
                            data:       string.Empty));

                lines[vertical ? cell.X : cell.Y].Insert(vertical ? cell.Y : cell.X, cell);
            }

            return lines;
        }
    }
}