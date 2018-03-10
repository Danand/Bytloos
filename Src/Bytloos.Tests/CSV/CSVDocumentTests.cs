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
        [TestCase("", 0, 0)]
        [TestCase(" ", 1, 1)]
        [TestCase("123;456;789", 1, 3)]
        [TestCase("qwe;rty;uio", 1, 3)]
        [TestCase("'qwe';'rty';'uio'", 1, 3)]
        [TestCase("\"qwe\";\"rty\";\"uio\"", 1, 3)]
        [TestCase("\"qw;e\";\"rt;y\";\"ui;o\"", 1, 3)]
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
            var directory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            var path = directory + "\\test.csv";

            File.WriteAllText(path, text, Encoding.Default);

            var csvDocument = CSVDocument.LoadFromFile(path);

            File.Delete(path);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(csvDocument.Rows.Count, expectedRowCount);
                Assert.AreEqual(csvDocument.Rows.Last().Count(), expectedCellCount);
            });
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", 1, 1, "fgh")]
        public void GetRowValue_ByIndex(string text, int rowIndex, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowIndex];
            var value = row[valueIndex].Data;

            Assert.AreEqual(value, expectedValue);
        }

        [TestCase("123;456;789\r\nasd;fgh;jkl", "asd", 1, "fgh")]
        public void GetRowValue_ByName(string text, string rowKey, int valueIndex, string expectedValue)
        {
            var csvDocument = CSVDocument.LoadFromString(text);
            var row = csvDocument.Rows[rowKey];
            var value = row[valueIndex].Data;

            Assert.AreEqual(value, expectedValue);
        }
    }
}