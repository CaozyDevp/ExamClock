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

namespace Spf
{
    public static class ValueParser
    {
        /// <summary>
        /// 将属性值解析为相应类型的对象，例如：string:"str" => string str = "str";
        /// </summary>
        public static object ParseValue(string valueStr)
        {
            valueStr = valueStr.Trim();
            if (string.IsNullOrEmpty(valueStr))
            {
                // 如果字符串为空，抛出异常
                throw new Exception("Value is null or empty!");
            }

            if (valueStr[0] == '(' && valueStr[valueStr.Length - 1] == ')')
            {
                return TableItem.Parse(valueStr);
            }

            string dataType = "";
            string content = "";
            char? quotation = null;
            int count = 0;

            // 解析类型
            for (; count < valueStr.Length; count++)
            {
                if (char.IsWhiteSpace(valueStr[count]))
                {
                    continue;
                }
                else if (valueStr[count] == ':')
                {
                    count++;
                    break;
                }
                else
                {
                    dataType += valueStr[count];
                }
            }

            // 解析值
            for (; count < valueStr.Length; count++)
            {
                if (char.IsWhiteSpace(valueStr[count]) && quotation == null)
                {
                    continue;
                }
                else if (valueStr[count] == '"' || valueStr[count] == '\'')
                {
                    if (quotation != null)
                    {
                        if (quotation == valueStr[count])
                        {
                            quotation = '~';    // 代表已经结束
                            break;
                        }
                        content += valueStr[count];
                    }
                    else
                    {
                        quotation = valueStr[count];
                    }
                }
                else
                {
                    content += valueStr[count];
                }
            }

            // 检查引号是否成对
            if (quotation == null)
            {
                throw new Exception("Quote not found!");
            }
            else if (quotation != '~')
            {
                throw new Exception("Unclosed quote!");
            }

            dataType = dataType.Trim();

            // 匹配类型
            switch (dataType)
            {
                case "int":
                    return int.Parse(content);
                case "ushort":
                    return ushort.Parse(content);
                case "double":
                    return double.Parse(content);
                case "string":
                    return content;
                case "bool":
                    return bool.Parse(content);
                case "DateTime":
                    return DateTime.Parse(content);
                case "table":
                    return Table.Parse(content);
                default:
                    // 未定义类型
                    throw new Exception($"Type \"{dataType}\" is not defined!");
            }

        }

        /// <summary>
        /// 获取一个对象的字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="quote">引号的类型，单引号或双引号</param>
        /// <returns>例如，string:"TEST"</returns>
        public static object GetValueString(object obj, char quote)
        {
            if (quote != '"' && quote != '\'')
            {
                throw new Exception("Invalid quote!");
            }

            var typeStr = GetTypeString(obj.GetType());

            if (typeStr == "DateTime")
            {
                var value = (DateTime)obj;
                var valueStr = $"{value.Year}/{value.Month}/{value.Day} {value.Hour}:{value.Minute}:{value.Second}.{value.Millisecond}";
                return $"DateTime:{quote}{valueStr}{quote}";
            }
            return $"{typeStr}:{quote}{obj}{quote}";
        }

        public static string GetTypeString(Type type)
        {
            // 因为 switch 语句不接受 typeof，所以使用if-else
            if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(ushort))
            {
                return "ushort";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(DateTime))
            {
                return "DateTime";
            }
            else if (type == typeof(TableItem))
            {
                return "item";
            }
            else if (type == typeof(Table))
            {
                return "table";
            }
            else
            {
                return null;
            }
        }
    }
}
