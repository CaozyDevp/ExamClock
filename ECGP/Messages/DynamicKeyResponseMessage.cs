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

using ECGP.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ECGP.Messages
{
    /// <summary>
    /// 动态密钥响应消息
    /// </summary>
    public class DynamicKeyResponseMessage : ECGPMessage
    {
        #region Properties

        /// <summary>
        /// 动态AES密钥
        /// </summary>
        public byte[] DynamicKey
        {
            get => _dynamicKey;
            set
            {
                if (value == null || value.Length != 16)
                {
                    throw new ArgumentException("Invalid value: dynamic key cannot be null and its length must be 16.", nameof(value));
                }
                _dynamicKey = value;

                Buffer.BlockCopy(value, 0, Body, 0, value.Length);
            }
        }
        private byte[] _dynamicKey = new byte[16];

        /// <summary>
        /// 接收到的特征码
        /// </summary>
        public uint NumberReceived
        {
            get => _numberReceived;
            set
            {
                _numberReceived = value;
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Body, 16, sizeof(uint));
            }
        }
        private uint _numberReceived;

        /// <summary>
        /// 用于加密的RSA公钥
        /// </summary>
        public string RSAPublicKeyXml { get; set; }

        #endregion

        public DynamicKeyResponseMessage(byte[] dynamicKey, uint numReceived, string rsaPublicKeyXml) : base(0x04, null)
        {
            Body = new byte[20];
            DynamicKey = dynamicKey;
            NumberReceived = numReceived;
            RSAPublicKeyXml = rsaPublicKeyXml;
        }

        /// <summary>
        /// 将动态密钥响应消息转换为字节数组
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            byte[] encryptedBody = CryptoHelper.RsaEncrypt(Body, RSAPublicKeyXml);

            var byteArrays = new List<byte[]>()
            {
                BitConverter.GetBytes(Head),
                BitConverter.GetBytes(Version),
                BitConverter.GetBytes(Number),
                BitConverter.GetBytes(Type),
                BitConverter.GetBytes(Sum),
                encryptedBody
            };

            int totalLength = 0;
            foreach (var array in byteArrays)
            {
                totalLength += array.Length;
            }
            var result = new byte[totalLength];

            // 拼合数组
            int offset = 0;
            foreach (var array in byteArrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        /// <summary>
        /// 将字节数组解析为动态密钥响应消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="rsaPrivateKeyXml">用于解密的RSA私钥</param>
        public static DynamicKeyResponseMessage Parse(byte[] bytes, string rsaPrivateKeyXml)
        {
            ECGPMessage rawMessage = Parse(bytes);
            string publicKeyXml;
            byte[] body = CryptoHelper.RsaDecrypt(rawMessage.Body, rsaPrivateKeyXml);
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(rsaPrivateKeyXml);
                publicKeyXml = rsa.ToXmlString(false);
            }

            if (body.Length != 20)
            {
                throw new ECGPFormatException("The length of decrypted body must be 20.");
            }

            var numReceived = BitConverter.ToUInt32(body, 16);
            var dynamicKey = new byte[16];
            Buffer.BlockCopy(body, 0, dynamicKey, 0, dynamicKey.Length);

            return new DynamicKeyResponseMessage(dynamicKey, numReceived, publicKeyXml);
        }

        /// <summary>
        /// 尝试将字节数组解析为动态密钥响应消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="rsaPrivateKeyXml">用于解密的RSA私钥</param>
        /// <param name="result">解析结果</param>
        /// <returns>解析是否成功。如果失败，<paramref name="result"/>设为null</returns>
        public static bool TryParse(byte[] bytes, string rsaPrivateKeyXml, out DynamicKeyResponseMessage result)
        {
            try
            {
                result = Parse(bytes, rsaPrivateKeyXml);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
