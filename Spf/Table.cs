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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Spf
{
    public class Table : ICollection<TableItem>
    {
        public Table(List<TableItem> items)
        {
            Data = items;
        }

        public Table()
        {
            Data = new List<TableItem>();
        }


        public static Table Parse(string str)
        {
            var tempStr = "";
            char? quotation = null;
            var inTableItem = false;

            var result = new Table();

            str = str.Trim();
            if (str[0] != '[' && str[str.Length - 1] != ']')
            {
                throw new Exception("Invalid format: is not Table type!");
            }

            for (int i = 1; i < str.Length - 1; i++)
            {
                if (char.IsWhiteSpace(str[i]) && quotation == null)
                {
                    continue;
                }
                else if (str[i] == '"' || str[i] == '\'')
                {
                    if (!inTableItem)
                    {
                        throw new Exception("Invalid format: there is ':' outside TableItem!"); // 在表项外部出现引号
                    }
                    if (quotation == str[i])
                    {
                        quotation = null;   // 退出引号作用域
                    }
                    else if (quotation != null)
                    {
                        continue;           // 不是配对的引号被忽略
                    }
                    else
                    {
                        quotation = str[i]; // 进入引号作用域
                    }
                }
                else if (str[i] == '(' && quotation == null)
                {
                    if (inTableItem)
                    {
                        throw new Exception("Invalid format: unclosed '('!");
                    }
                    inTableItem = true;
                }
                else if (str[i] == ')' && quotation == null)
                {
                    if (!inTableItem)
                    {
                        throw new Exception("Invalid format: can not find '(' in pair!");
                    }
                    inTableItem = false;
                }
                else if (str[i] == ',' && quotation == null && !inTableItem)
                {
                    if (string.IsNullOrEmpty(tempStr))
                    {
                        throw new Exception("Table item have empty properties!");
                    }
                    result.Add(TableItem.Parse(tempStr));
                    tempStr = "";
                    continue;
                }
                tempStr += str[i];
            }

            if (!string.IsNullOrEmpty(tempStr))
            {
                result.Add(TableItem.Parse(tempStr));
            }

            return result;
        }

        public TableItem this[int index]
        {
            get
            {
                return Data[index];
            }
            set
            {
                Data[index] = value;
            }
        }

        private List<TableItem> Data { get; set; }

        public int Count => Data.Count;

        public void Add(TableItem value)
        {
            Data.Add(value);
        }

        public void RemoveAt(int index)
        {
            Data.RemoveAt(index);
        }

        public void Remove(TableItem item)
        {
            Data.Remove(item);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Table)) return false;

            return Data.SequenceEqual((obj as Table).Data);
        }

        public override string ToString()
        {
            var result = "table:\"[";
            for (int i = 0; i < Data.Count; i++)
            {
                result += Data[i].ToString();
                if (i < Data.Count - 1)
                {
                    result += ",";
                }
            }
            result += "]\"";
            return result;
        }
    }
}
