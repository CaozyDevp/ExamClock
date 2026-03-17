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
using System.Text;
using System.Windows;

namespace ExamClock.Admin
{
    public class KeyManager
    {
        #region Properties

        /// <summary>
        /// 密钥文件的路径
        /// </summary>
        private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key");

        /// <summary>
        /// 被加密的RSA私钥Xml，使用AES算法ECB模式加密，密钥为用户名和密码以下划线连缀拼合后ASCII的SHA256值
        /// </summary>
        public static byte[] EncryptedKey { get; private set; }

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
                var text = File.ReadAllBytes(path);
                EncryptedKey = text;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从默认文件加载密钥
        /// </summary>
        /// <returns>是否加载成功</returns>
        public static bool LoadKeyFromDefaultFile()
        {
            return LoadKeyFromFile(ConfigPath);
        }

        /// <summary>
        /// 将密钥写入文件
        /// </summary>
        /// <param name="path">密钥文件的路径</param>
        /// <returns>是否写入成功</returns>
        public static bool SaveKeyToFile(string path)
        {
            try
            {
                File.WriteAllBytes(path, EncryptedKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将密钥信息写入默认配置文件
        /// </summary>
        /// <returns>是否写入成功</returns>
        public static bool SaveKeyToDefaultFile()
        {
            return SaveKeyToFile(ConfigPath);
        }

        /// <summary>
        /// 获取RSA私钥Xml字符串。使用AES算法ECB模式解密，密钥为用户名和密码以下划线连缀拼合后ASCII的SHA256值
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>返回原始的Xml格式RSA私钥。如果验证失败或密钥不存在，返回null</returns>
        public static string GetKeyXml(string username, string password)
        {
            if (EncryptedKey == null) return null;
            try
            {
                string token = username + "_" + password;
                byte[] aesKey;
                string plainKey;

                using (SHA256 sha = SHA256.Create())
                {
                    aesKey = sha.ComputeHash(Encoding.ASCII.GetBytes(token));
                }

                // 使用AES算法ECB模式解密
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Key = aesKey;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] plainText = decryptor.TransformFinalBlock(EncryptedKey, 0, EncryptedKey.Length);
                        plainKey = Encoding.UTF8.GetString(plainText);
                    }
                }

                // 检验解密后的字符串是否为合法的RSA私钥Xml格式
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(plainKey);
                }

                return plainKey;
            }
            catch
            {
                return null;
            }
        }
    }
}
