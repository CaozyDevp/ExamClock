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
using System.IO;

namespace Spf
{
    public class SpfItem
    {
        public List<ConfigurationItem> Configuration
        { get; set; }

        public string Path
        { get; set; }

        public void LoadFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            Path = filePath;
            Configuration = Parse(new string[] { content });
        }

        public static List<ConfigurationItem> Parse(string[] lines)
        {
            string content = "";
            foreach (var line in lines)
            {
                content += line.Trim();
            }

            var configItems = new List<ConfigurationItem>();
            var temp = "";
            var inItem = false;
            foreach (var c in content)
            {
                temp += c;
                if (c == '{')
                {
                    // 在"{"之前理应有配置项名，并且一定没有进入配置项
                    if (temp.Trim().Length == 1 || inItem)
                    {
                        throw new Exception("Invalid format: unclosed '{'!");
                    }
                    inItem = true;
                }
                else if (c == '}')
                {
                    // 在"}"之前理应有内容，并且一定在配置项内
                    if (temp.Trim().Length < 2 || !inItem)
                    {
                        throw new Exception("Invalid format: unclosed '{'!");
                    }
                    inItem = false;
                    configItems.Add(ConfigurationItem.Parse(temp));
                    temp = "";
                }
            }

            return configItems;
        }

        public static List<ConfigurationItem> Parse(List<string> lines)
        {
            return Parse(lines.ToArray());
        }


        public object GetValue(string path)
        {
            var token = path.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (token.Length == 0 || token.Length > 2)
            {
                throw new Exception("The number of the tokens is invalid!");
            }

            foreach (var config in Configuration)
            {
                if (token[0] != config.Key) continue;

                if (token.Length == 1)
                {
                    return config;
                }

                foreach (var prop in config.Properties)
                {
                    if (prop.Name != token[1]) continue;

                    return prop.Value;
                }

            }

            throw new Exception("Value not found!");
        }

        public bool TryGetValue(string path, ref object value)
        {
            try
            {
                value = GetValue(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CheckIfExists(string path)
        {
            var token = path.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (token.Length == 0 || token.Length > 2)
            {
                return false;
            }

            foreach (var config in Configuration)
            {
                if (token[0] != config.Key) continue;

                if (token.Length == 1)
                {
                    return true;
                }

                foreach (var prop in config.Properties)
                {
                    if (prop.Name != token[1]) continue;

                    return true;
                }

            }
            return false;
        }

        public void SetValue(string path, object value)
        {
            var token = path.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (token.Length == 0 || token.Length > 2)
            {
                throw new Exception("The number of the tokens is invalid!");
            }

            for (int i = 0; i < Configuration.Count; i++)
            {
                if (token[0] != Configuration[i].Key) continue;

                if (token.Length == 1)
                {
                    if (value.GetType() != typeof(ConfigurationItem))
                    {
                        throw new Exception("Invalid type!");
                    }
                    Configuration[i] = (ConfigurationItem)value;
                    return;
                }

                for (int j = 0; j < Configuration[i].Properties.Count; j++)
                {
                    if (Configuration[i].Properties[j].Name != token[1]) continue;

                    Configuration[i].Properties[j].Value = value;
                    return;
                }

            }

            throw new Exception("Value not found!");
        }

        public string GetString()
        {
            var result = "";
            foreach (var item in Configuration)
            {
                result += item.ToString();
            }
            return result;
        }

        public void SaveFile()
        {
            var str = GetString();
            File.WriteAllText(Path, str);
        }
    }
}
