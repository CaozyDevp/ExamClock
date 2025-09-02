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
    public class TableItemTests
    {
        [TestMethod]
        public void Add_Test()
        {
            var item = new TableItem();
            item.Add("str");
            item.Add(123);
            item.Add(true);
            item.Add(null);

            Assert.IsTrue((string)item[0] == "str" &&
                          (int)item[1] == 123 &&
                          (bool)item[2] == true &&
                          item[3] == null &&
                          item.Count == 4);
        }

        [TestMethod]
        public void RemoveAt_Test()
        {
            var item = new TableItem();
            item.Add("str");
            item.Add(123);
            item.Add(true);
            item.Add(null);

            item.RemoveAt(0);

            Assert.IsTrue((int)item[0] == 123 &&
                          (bool)item[1] == true &&
                          item[2] == null &&
                          item.Count == 3);
        }

        [TestMethod]
        public void Remove_Test()
        {
            var item = new TableItem();
            item.Add("str");
            item.Add(123);
            item.Add(true);
            item.Add(null);

            item.Remove(true);

            Assert.IsTrue((string)item[0] == "str" &&
                          (int)item[1] == 123 &&
                          item[2] == null &&
                          item.Count == 3);
        }

        [TestMethod]
        public void Parse_Test()
        {
            var result = TableItem.Parse("(string:'某某某', bool:'True', ushort:'20')");
            var expected = new TableItem(new List<object> { "某某某", true, (ushort)20 });
            Assert.ReferenceEquals(result, expected);
        }

        [TestMethod]
        public void Equals_Test()
        {
            var item1 = new TableItem(new List<object> { "123", false, 123, 0.1 });
            var item2 = new TableItem(new List<object> { "123", false, 123, 0.1 });
            var item3 = new TableItem(new List<object> { "123", false, 123, 0 });
            var item4 = new TableItem(new List<object> { "123", true, 123, 0.1 });

            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item4));
        }

        [TestMethod]
        public void ToString_Test()
        {
            var expected = "(int:'1',ushort:'123',bool:'True',string:'str')";
            var result = new TableItem(new List<object> { 1, (ushort)123, true, "str" }).ToString();
            Assert.AreEqual(expected, result);
        }
    }
}
