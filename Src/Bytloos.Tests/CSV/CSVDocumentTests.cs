using System;
using System.IO;
using System.Linq;
using System.Text;
using Bytloos.CSV;
using NUnit.Framework;

namespace Bytloos.Tests.CSV
{
    [TestFixture]
    public class CSVDocumentTests
    {
        private const string AVAILABLE_RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789;-+'\"";

        private readonly Random random = new Random();

        private string TestFilePath
        {
            get
            {
                var directory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
                return $"{directory}\\{GetType().Name}.csv";
            }
        }

        [TestCase("", 0, 0)]
        [TestCase(" ", 1, 1)]
        [TestCase("123;456;789", 1, 3)]
        [TestCase("qwe;rty;uio", 1, 3)]
        [TestCase("'qwe';'rty';'uio'", 1, 3)]
        [TestCase("\"qwe\";\"rty\";\"uio\"", 1, 3)]
        [TestCase("\"qw;e\";\"rt;y\";\"ui;o\"", 1, 3)]
        [TestCase("\"q\"we\";\"r\"ty\";\"u\"io\"", 1, 3)]
        public void LoadFromString_Singleline(string text, int expectedRowCount, int expectedCellCount)
        {
            var csvDocument = CSVDocument.LoadFromString(text);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedRowCount, csvDocument.Rows.Count);

                if (!string.IsNullOrEmpty(text))
                    Assert.AreEqual(expectedCellCount, csvDocument.Rows.First().Count());
            });
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 2, 3)]
        [TestCase("\"qw;e\";\"rt;y\";\"ui;o\"\r\n\"as;d\";\"fg;h\";", 2, 3)]
        public void LoadFromString_Multiline(string text, int expectedRowCount, int expectedCellCount)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedRowCount, csvDocument.Rows.Count);
                Assert.AreEqual(expectedCellCount, csvDocument.Rows.Last().Count());
            });
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 2, 3)]
        [TestCase("\"qw;e\";\"rt;y\";\"ui;o\"\r\n\"as;d\";\"fg;h\";", 2, 3)]
        public void LoadFromFile(string text, int expectedRowCount, int expectedCellCount)
        {
            var path = TestFilePath;

            File.WriteAllText(path, text, Encoding.Default);

            var csvDocument = CSVDocument.LoadFromFile(path);

            File.Delete(path);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedRowCount, csvDocument.Rows.Count);
                Assert.AreEqual(expectedCellCount, csvDocument.Rows.Last().Count());
            });
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 2, 2)]
        [TestCase("123;456;789\r\nasd;fgh;jkl\r\nzxc;vbn;mqw", 2, 2)]
        public void LoadFromFile_LimitRows(string text, int expectedRowCount, int rowLimit)
        {
            var path = TestFilePath;

            File.WriteAllText(path, text, Encoding.Default);

            var options = new CSVOptions { RowLimit = rowLimit };
            var csvDocument = CSVDocument.LoadFromFile(path, options);

            File.Delete(path);

            Assert.AreEqual(expectedRowCount, csvDocument.Rows.Count);
        }

        [TestCase(50000, 10, 10, 35)]
        [TestCase(25000, 40, 15, 20)]
        public void LoadFromFile_BigFile(int expectedRowsAmount, int columnsAmount, int minCellLength, int maxCellLength)
        {
            var path = TestFilePath;

            var sbAllText = new StringBuilder();

            for (var i = 0; i < expectedRowsAmount; i++)
            {
                var sbRow = new StringBuilder();

                for (var j = 0; j < columnsAmount; j++)
                    sbRow.Append($"\"{GetRandomString(random.Next(minCellLength, maxCellLength))}\";");

                sbAllText.AppendLine(sbRow.ToString().TrimEnd(';'));
            }

            File.WriteAllText(path, sbAllText.ToString(), Encoding.Default);

            var csvDocument = CSVDocument.LoadFromFile(path);

            File.Delete(path);

            Assert.AreEqual(expectedRowsAmount, csvDocument.Rows.Count);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 1, 1, "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", 0, 2, "789")]
        public void GetRowValue_ByRowIndex(string text, int rowIndex, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowIndex];
            var value = row[valueIndex].Data;

            Assert.AreEqual(expectedValue, value);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", "asd", 1, "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", "123", 2, "789")]
        public void GetRowValue_ByRowName(string text, string rowKey, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowKey];
            var value = row[valueIndex].Data;

            Assert.AreEqual(expectedValue, value);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", "asd", "456", "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", "123", "456", "456")]
        public void GetRowValue_ByColumnName(string text, string rowKey, string columnKey, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowKey];
            var value = row[columnKey].Data;

            Assert.AreEqual(expectedValue, value);
        }

        [TestCase(1, 3, 1, "qwe", "rty", "uio")]
        [TestCase(2, 5, 2, "qwe", "rty", "uio", "zxc", "vbn")]
        public void AppendRow(int expectedRowCount, int expectedColumnCount, int iterations, params string[] values)
        {
            var csvDocument = CSVDocument.Create();

            for (var i = 0; i < iterations; i++)
                csvDocument.AppendRow(values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedRowCount, csvDocument.Rows.Count);
                Assert.AreEqual(expectedColumnCount, csvDocument.Rows.First().Count());
            });
        }

        [TestCase("qwe;rty;uio", "qwe", "rty", "uio")]
        [TestCase("qwe;\"r;ty\";uio", "qwe", "r;ty", "uio")]
        public void SaveToFile(string expectedFileContent, params string[] values)
        {
            var path = TestFilePath;
            var csvDocument = CSVDocument.Create();

            csvDocument.AppendRow(values);
            csvDocument.SaveToFile(path);

            var actualFileContent = File.ReadAllText(path);

            File.Delete(path);

            Assert.AreEqual(expectedFileContent, actualFileContent);
        }

        [TestCase("qwe;rty;uio\r\nasd;fgh\r\njkl;zxc;vbn", 1, 2, "vbn")]
        [TestCase("qwe;rty;uio\r\nasd;fgh\r\njkl;zxc;vbn", 1, 1, "zxc")]
        public void CleanBrokenRows(string text, int expectedRowIndex, int expectedColumnIndex, string expectedCellData)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            csvDocument.CleanBrokenRows();
            Assert.AreEqual(expectedCellData, csvDocument.Rows[expectedRowIndex][expectedColumnIndex].Data);
        }

        private string GetRandomString(int length)
        {
            return new string(Enumerable.Repeat(AVAILABLE_RANDOM_CHARS, length)
                .Select(str => str[random.Next(str.Length)])
                .ToArray());
        }
    }
}