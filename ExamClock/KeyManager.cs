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
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace ExamClock
{
    public static class KeyManager
    {
        #region Properties

        /// <summary>
        /// 密钥文件的路径
        /// </summary>
        private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.xml");

        /// <summary>
        /// RSA公钥的xml
        /// </summary>
        public static string RsaPublicKeyXml { get; private set; }

        /// <summary>
        /// 是否已经加载了密钥
        /// </summary>
        public static bool Loaded => (!string.IsNullOrEmpty(RsaPublicKeyXml)) && IsValidRsaKeyXml(RsaPublicKeyXml);

        #endregion

        static KeyManager()
        {
            if (!LoadKeyFromDefaultFile())
            {
                MessageBox.Show("密钥文件加载失败！");
            }
        }

        /// <summary>
        /// 从文件中加载密钥
        /// </summary>
        /// <param name="path">密钥文件的路径</param>
        /// <returns>是否加载成功</returns>
        public static bool LoadKeyFromFile(string path)
        {
            if (!File.Exists(path)) return false;

            try
            {
                var text = File.ReadAllText(path);
                if (!IsValidRsaKeyXml(text))
                {
                    return false;
                }
                RsaPublicKeyXml = text;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从默认配置文件加载密钥
        /// </summary>
        /// <returns>是否加载成功</returns>
        public static bool LoadKeyFromDefaultFile()
        {
            return LoadKeyFromFile(ConfigPath);
        }

        /// <summary>
        /// 将密钥信息写入文件
        /// </summary>
        /// <param name="path">密钥文件的路径</param>
        /// <returns>是否写入成功</returns>
        public static bool SaveKeyToFile(string path)
        {
            try
            {
                File.WriteAllText(path, RsaPublicKeyXml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将密钥信息写入默认密钥文件
        /// </summary>
        /// <returns>是否写入成功</returns>
        public static bool SaveKeyToDefaultFile()
        {
            return SaveKeyToFile(ConfigPath);
        }

        /// <summary>
        /// 判断rsa密钥是否合法
        /// </summary>
        private static bool IsValidRsaKeyXml(string keyXml)
        {
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(keyXml);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
