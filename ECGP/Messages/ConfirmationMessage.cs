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
using ECGP.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ECGP.Messages
{
    /// <summary>
    /// 控制指令确认消息
    /// </summary>
    public class ConfirmationMessage : ECGPMessage
    {
        #region Properties

        /// <summary>
        /// 接收到的控制指令的特征码
        /// </summary>
        public uint NumberReceived
        {
            get => _numberReceived;
            set
            {
                _numberReceived = value;
                var bytes = BitConverter.GetBytes(_numberReceived);
                Buffer.BlockCopy(bytes, 0, Body, 0, bytes.Length);
            }
        }
        private uint _numberReceived;

        /// <summary>
        /// 返回值
        /// </summary>
        public ReturnCode ReturnCode
        {
            get => _returnCode;
            set
            {
                _returnCode = value;
                Body[4] = (byte)value;
            }
        }
        private ReturnCode _returnCode;

        /// <summary>
        /// 用于加密的RSA公钥
        /// </summary>
        public string RSAPublicKeyXml { get; set; }

        #endregion

        /// <summary>
        /// 构造一个控制指令确认消息
        /// </summary>
        /// <param name="numReceived">接收到的控制指令的特征码</param>
        /// <param name="ret">返回值</param>
        /// <param name="rsaPublicKeyXml">用于加密的RSA公钥</param>
        public ConfirmationMessage(uint numReceived, ReturnCode ret, string rsaPublicKeyXml) : base(0x06, null)
        {
            Body = new byte[5];
            NumberReceived = numReceived;
            ReturnCode = ret;
            RSAPublicKeyXml = rsaPublicKeyXml;
        }

        /// <summary>
        /// 将控制指令确认消息转换为字节数组
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            byte[] encryptedBody;
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(RSAPublicKeyXml);
                encryptedBody = rsa.EncryptValue(Body);
            }

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
        /// 将字节数组解析为控制指令确认消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="rsaPrivateKeyXml">用于解密的RSA私钥</param>
        public static ConfirmationMessage Parse(byte[] bytes, string rsaPrivateKeyXml)
        {
            ECGPMessage rawMessage = ECGPMessage.Parse(bytes);
            string publicKeyXml;
            byte[] body;
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(rsaPrivateKeyXml);
                body = rsa.DecryptValue(rawMessage.Body);
                publicKeyXml = rsa.ToXmlString(false);
            }

            if (body.Length != 5)
            {
                throw new ECGPFormatException("The length of decrypted body must be 5.");
            }

            var numReceived = BitConverter.ToUInt32(body, 0);
            var retCode = (ReturnCode)body[4];

            return new ConfirmationMessage(numReceived, retCode, publicKeyXml);
        }

        /// <summary>
        /// 尝试将字节数组解析为控制指令确认消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="rsaPrivateKeyXml">用于解密的RSA私钥</param>
        /// <param name="result">解析结果</param>
        /// <returns>解析是否成功。如果失败，将<paramref name="result"/>设为null</returns>
        public static bool TryParse(byte[] bytes, string rsaPrivateKeyXml, out ConfirmationMessage result)
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
