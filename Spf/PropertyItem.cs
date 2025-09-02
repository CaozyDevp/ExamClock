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
    public class PropertyItem
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; set; }

        public PropertyItem(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name} = {ValueParser.GetValueString(Value, '"')}";
        }

        /// <summary>
        /// 将字符串解析为<see cref="PropertyItem"/>对象
        /// </summary>
        public static PropertyItem Parse(string str)
        {
            string propName = "";
            string propValue = "";

            bool inName = true;
            char? quotation = null;

            foreach (var c in str)
            {
                if (c == '"' || c == '\"')
                {
                    if (quotation == c)
                    {
                        quotation = null;
                    }
                    else if (quotation == null)
                    {
                        quotation = c;
                    }
                }

                // 如果字符为空白，且不在引号内，直接跳过
                if (char.IsWhiteSpace(c) && quotation == null) continue;

                if (inName)
                {
                    // 属性名
                    if (c == '=')
                    {
                        inName = false;
                        continue;
                    }

                    propName += c;
                }
                else
                {
                    // 属性值
                    if (c == '=')
                    {
                        // 如果出现两次赋值符号，直接抛出异常
                        throw new Exception("There is more than one equal character!");
                    }

                    propValue += c;
                }
            }

            return new PropertyItem(propName, ValueParser.ParseValue(propValue));
        }


        /// <summary>
        /// 尝试将字符串解析为<see cref="PropertyItem"/>对象
        /// </summary>
        public static bool TryParse(string line, ref PropertyItem result)
        {
            try
            {
                // 尝试解析
                Parse(line);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}