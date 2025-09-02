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

namespace Spf
{
    public class ConfigurationItem
    {
        /// <summary>
        /// 配置项名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 属性列表
        /// </summary>
        public List<PropertyItem> Properties { get; set; }

        public static ConfigurationItem Parse(string str)
        {
            string configItemName = "";

            str = str.Trim();

            int count;

            // 解析配置项名称
            for (count = 0; count < str.Length; count++)
            {
                if (str[count] == '{')
                {
                    count++;
                    break;
                }
                configItemName += str[count];
            }
            if (configItemName[0] != '#')
            {
                throw new Exception("Format invalid!");
            }
            configItemName = configItemName.Substring(1).Trim();

            // 解析属性列表
            var propStrs = new List<string>();
            var propItems = new List<PropertyItem>();
            char? quotation = null;
            string temp = "";
            for (; count < str.Length; count++)
            {
                if (str[count] == ',' && !string.IsNullOrEmpty(temp) && quotation == null)
                {
                    propStrs.Add(temp);
                    temp = "";
                }
                else if (str[count] == '}')
                {
                    if (!string.IsNullOrEmpty(temp))
                    {
                        propStrs.Add(temp);
                    }
                    break;
                }
                else if (str[count] == '"' || str[count] == '\'')
                {
                    if (quotation == str[count])
                    {
                        quotation = null;
                    }
                    else if (quotation == null)
                    {
                        quotation = str[count];
                    }
                    temp += str[count];
                }
                else
                {
                    temp += str[count];
                }
            }


            foreach (var propStr in propStrs)
            {
                propItems.Add(PropertyItem.Parse(propStr));
            }

            return new ConfigurationItem()
            {
                Key = configItemName,
                Properties = propItems
            };
        }

        public override string ToString()
        {
            string result = "#" + Key + "{";
            for (int i = 0; i < Properties.Count; i++)
            {
                result += Properties[i].ToString();
                if (i != Properties.Count - 1)
                {
                    result += ",";
                }
            }
            result += "}";
            return result;
        }
    }
}