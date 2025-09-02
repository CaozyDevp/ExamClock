/*
 * Copyright(C) 2025 Zachary Cao (CaozyDevp)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Spf;

namespace SpfTests
{
    [TestClass]
    public class ValueParserTests
    {
        [TestMethod]
        [DataRow("string:'a,b'", "a,b")]
        [DataRow("string:\"char.c\"", "char.c")]
        [DataRow("int:'1234'", 1234)]
        [DataRow("int:\"1234\"", 1234)]
        [DataRow("ushort:'555'", (ushort)555)]
        [DataRow("ushort:\"555\"", (ushort)555)]
        [DataRow("double:'3.14'", 3.14)]
        [DataRow("double:\"3.14\"", 3.14)]
        [DataRow("bool:'True'", true)]
        [DataRow("bool:\"True\"", true)]
        public void ParseValue_BasicType_Test(string str, object expected)
        {
            var result = ValueParser.ParseValue(str);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseValue_DateTime_Test()
        {
            var result = ValueParser.ParseValue("DateTime:\"2025/8/31 12:00\"");
            var expected = new DateTime(2025, 8, 31, 12, 0, 0);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseValue_TableItem_Test()
        {
            var result = ValueParser.ParseValue("(string:'语文', DateTime:'2025/9/4 08:00:00.0', int:'120')");
            var expected = new TableItem(new List<object>
            {
                "语文",
                new DateTime(2025,9,4,8,0,0),
                120
            });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseValue_Table_Test()
        {
            var result = ValueParser.ParseValue("table:\"[" +
                "(string:'语文', DateTime:'2025/9/4 08:00:00.0', int:'100')," +
                "(string:'数学', DateTime:'2025/9/4 10:00:00.0', int:'100')" +
                "]\"");
            var expected = new Table(new List<TableItem>()
            {
                new TableItem(new List<object>() {"语文", new DateTime(2025,9,4,8,0,0), 100}),
                new TableItem(new List<object>() {"数学", new DateTime(2025,9,4,10,0,0), 100}),
            });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("string:'a,b'", "a,b", '\'')]
        [DataRow("string:\"char.c\"", "char.c", '"')]
        [DataRow("int:'1234'", 1234, '\'')]
        [DataRow("int:\"1234\"", 1234, '"')]
        [DataRow("ushort:'555'", (ushort)555, '\'')]
        [DataRow("ushort:\"555\"", (ushort)555, '"')]
        [DataRow("double:'3.14'", 3.14, '\'')]
        [DataRow("double:\"3.14\"", 3.14, '"')]
        [DataRow("bool:'True'", true, '\'')]
        [DataRow("bool:\"True\"", true, '"')]
        public void GetValueString_Test(string expected, object obj, char quote)
        {
            Assert.AreEqual(expected, ValueParser.GetValueString(obj, quote));
        }

    }
}
