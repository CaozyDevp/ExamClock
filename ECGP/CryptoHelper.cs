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


using System.IO;
using System.Security.Cryptography;

namespace ECGP
{
    public static class CryptoHelper
    {
        /// <summary>
        /// 使用RSA算法对数据加密
        /// </summary>
        /// <param name="data">需要加密的原始数据</param>
        /// <param name="publicKeyXml">RSA公钥的Xml</param>
        /// <returns>加密后的数据</returns>
        public static byte[] RsaEncrypt(byte[] data, string publicKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKeyXml);
                return rsa.Encrypt(data, true);
            }
        }

        /// <summary>
        /// 使用RSA算法对数据解密
        /// </summary>
        /// <param name="data">需要被解密的数据</param>
        /// <param name="privateKeyXml">RSA私钥的Xml</param>
        /// <returns>解密后的数据</returns>
        public static byte[] RsaDecrypt(byte[] data, string privateKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKeyXml);
                return rsa.Decrypt(data, true);
            }
        }

        /// <summary>
        /// 使用AES算法的CBC模式（PKCS7填充）对数据加密
        /// </summary>
        /// <param name="data">需要加密的原始数据</param>
        /// <param name="key">AES密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>加密后的数据</returns>
        public static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 使用AES算法的CBC模式（PKCS7填充）对数据解密
        /// </summary>
        /// <param name="data">需要解密的数据</param>
        /// <param name="key">AES密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>解密后的数据</returns>
        public static byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
