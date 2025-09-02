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
    public class TableItem : ICollection<object>
    {
        public TableItem()
        {
            Data = new List<object>();
        }

        public TableItem(List<object> items)
        {
            Data = items;
        }

        private List<object> Data
        { get; set; }

        public int Count => Data.Count;

        public object this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void Add(object value)
        {
            Data.Add(value);
        }

        public void RemoveAt(int index)
        {
            Data.RemoveAt(index);
        }

        public void Remove(object item)
        {
            Data.Remove(item);
        }

        public static TableItem Parse(string str)
        {
            // 例如：(string:'语文', DateTime:'2025/9/4 08:00:00.0', int:'120')
            str = str.Trim();
            if (str.Length == 0) return null;
            if (str[0] != '(' && str[str.Length - 1] != ')')
            {
                throw new Exception("Invalid format: is not TableItem!");
            }

            var result = new TableItem();

            // null: 没有在引号作用域；非空：进入了（单/双）引号的作用域
            char? quotation = null;

            var tempStr = "";
            for (int i = 1; i < str.Length - 1; i++)
            {
                if (char.IsWhiteSpace(str[i]) && quotation == null)
                {
                    continue;
                }
                else if (str[i] == '"' || str[i] == '\'')
                {
                    if (quotation == str[i])
                    {
                        quotation = null;   // 退出引号作用域
                    }
                    else if (quotation == null)
                    {
                        quotation = str[i]; // 进入引号作用域
                    }
                    // else ... 不是配对的引号被忽略
                }
                else if (str[i] == ',' && quotation == null)
                {
                    if (string.IsNullOrEmpty(tempStr))
                    {
                        throw new Exception("Table item have empty properties!");
                    }
                    result.Add(ValueParser.ParseValue(tempStr));
                    tempStr = "";
                    continue;
                }
                tempStr += str[i];
            }

            if (!string.IsNullOrEmpty(tempStr))
            {
                result.Add(ValueParser.ParseValue(tempStr));
            }

            if (quotation != null)
            {
                throw new Exception("Unclosed quote!");
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TableItem))
            {
                return false;
            }

            TableItem other = (TableItem)obj;

            if (other.Data.Count != Data.Count)
            {
                return false;
            }

            return Data.SequenceEqual(other.Data);
        }

        public override string ToString()
        {
            string result = "(";
            for (int i = 0; i < Data.Count; i++)
            {
                var typeStr = ValueParser.GetTypeString(Data[i].GetType());
                result += ValueParser.GetValueString(Data[i], '\'');
                if (i < Data.Count - 1)
                {
                    result += ",";
                }
            }
            result += ")";

            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
