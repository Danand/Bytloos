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
                Assert.AreEqual(csvDocument.Rows.Count, expectedRowCount);

                if (!string.IsNullOrEmpty(text))
                    Assert.AreEqual(csvDocument.Rows.First().Count(), expectedCellCount);
            });
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 2, 3)]
        [TestCase("\"qw;e\";\"rt;y\";\"ui;o\"\r\n\"as;d\";\"fg;h\";", 2, 3)]
        public void LoadFromString_Multiline(string text, int expectedRowCount, int expectedCellCount)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            
            Assert.Multiple(() =>
            {
                Assert.AreEqual(csvDocument.Rows.Count, expectedRowCount);
                Assert.AreEqual(csvDocument.Rows.Last().Count(), expectedCellCount);
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
                Assert.AreEqual(csvDocument.Rows.Count, expectedRowCount);
                Assert.AreEqual(csvDocument.Rows.Last().Count(), expectedCellCount);
            });
        }

        [TestCase(50000, 10, 10, 35)]
        [TestCase(25000, 40, 15, 20)]
        public void LoadFromFile_BigFile(int rowsAmount, int columnsAmount, int minCellLength, int maxCellLength)
        {
            var path = TestFilePath;

            StringBuilder sbAllText = new StringBuilder();

            for (int i = 0; i < rowsAmount; i++)
            {
                StringBuilder sbRow = new StringBuilder();

                for (int j = 0; j < columnsAmount; j++)
                    sbRow.Append($"\"{GetRandomString(random.Next(minCellLength, maxCellLength))}\";");

                sbAllText.AppendLine(sbRow.ToString().TrimEnd(';'));
            }

            File.WriteAllText(path, sbAllText.ToString(), Encoding.Default);

            var csvDocument = CSVDocument.LoadFromFile(path);

            File.Delete(path);

            Assert.AreEqual(csvDocument.Rows.Count, rowsAmount);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 1, 1, "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", 0, 2, "789")]
        public void GetRowValue_ByRowIndex(string text, int rowIndex, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowIndex];
            var value = row[valueIndex].Data;

            Assert.AreEqual(value, expectedValue);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", "asd", 1, "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", "123", 2, "789")]
        public void GetRowValue_ByRowName(string text, string rowKey, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowKey];
            var value = row[valueIndex].Data;

            Assert.AreEqual(value, expectedValue);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", "asd", "456", "fgh")]
        [TestCase("123;456;789\r\nasd;fgh;jkl", "123", "456", "456")]
        public void GetRowValue_ByColumnName(string text, string rowKey, string columnKey, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowKey];
            var value = row[columnKey].Data;

            Assert.AreEqual(value, expectedValue);
        }

        [TestCase(1, 3, 1, "qwe", "rty", "uio")]
        [TestCase(2, 5, 2, "qwe", "rty", "uio", "zxc", "vbn")]
        public void AppendRow(int expectedRowCount, int expectedColumnCount, int iterations, params string[] values)
        {
            var csvDocument = CSVDocument.Create();

            for (int i = 0; i < iterations; i++)
                csvDocument.AppendRow(values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(csvDocument.Rows.Count, expectedRowCount);
                Assert.AreEqual(csvDocument.Rows.First().Count(), expectedColumnCount);
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

        private string GetRandomString(int length)
        {
            return new string(Enumerable.Repeat(AVAILABLE_RANDOM_CHARS, length)
                .Select(str => str[random.Next(str.Length)])
                .ToArray());
        }
    }
}