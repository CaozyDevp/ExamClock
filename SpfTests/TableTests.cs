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
    [TestClass()]
    public class TableTests
    {
        [TestMethod]
        public void Parse_Test()
        {
            var result = Table.Parse("[(string:'str',int:'12'),(ushort:'20',bool:'True')]");
            var expected = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            Assert.ReferenceEquals(result, expected);
        }

        [TestMethod]
        public void Add_Test()
        {
            var table = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            table.Add(new TableItem(new List<object> { 1.1, false }));

            var expected = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true}),
                new TableItem(new List<object>{1.1,false})
            });
        }

        [TestMethod]
        public void RemoveAt_Test()
        {
            var table = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            table.RemoveAt(0);

            var expected = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{(ushort)20,true})
            });
        }

        [TestMethod]
        public void Remove_Test()
        {
            var table = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            table.Remove(new TableItem(new List<object> { "str", 12 }));

            var expected = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{(ushort)20,true})
            });

            Assert.ReferenceEquals(expected, table);
        }

        [TestMethod]
        public void Equals_Test()
        {
            var table1 = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            var table2 = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            var table3 = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",10}),
                new TableItem(new List<object>{(ushort)20,true})
            });
            var table4 = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{1,12}),
                new TableItem(new List<object>{(ushort)20,true})
            });

            Assert.IsTrue(table1.Equals(table2));
            Assert.IsFalse(table1.Equals(table3));
            Assert.IsFalse(table1.Equals(table4));
        }

        [TestMethod]
        public void ToString_Test()
        {
            var expected = "table:\"[(string:'str',int:'12'),(ushort:'20',bool:'True')]\"";
            var result = new Table(new List<TableItem>
            {
                new TableItem(new List<object>{"str",12}),
                new TableItem(new List<object>{(ushort)20,true})
            }).ToString();
            Assert.AreEqual(expected, result);
        }
    }
}
