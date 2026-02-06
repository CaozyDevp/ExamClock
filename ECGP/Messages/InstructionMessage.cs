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
using System.Security.Cryptography;

namespace ECGP.Messages
{
    /// <summary>
    /// 控制指令消息
    /// </summary>
    public class InstructionMessage : ECGPMessage
    {
        #region Properties

        /// <summary>
        /// 指令代码
        /// </summary>
        public InstructionType CommandCode
        {
            get => _commandCode;
            set
            {
                _commandCode = value;
                var cmdBytes = BitConverter.GetBytes((ushort)value);
                Buffer.BlockCopy(cmdBytes, 0, Body, 16, cmdBytes.Length);
            }
        }
        private InstructionType _commandCode;

        /// <summary>
        /// 可选参数
        /// </summary>
        public byte[] Parameters
        {
            get => _parameters;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Invalid value: parameters can be empty but cannot be null!");
                }
                _parameters = (byte[])value.Clone();

                var temp = Body;
                Body = new byte[18 + value.Length];
                Buffer.BlockCopy(temp, 0, Body, 0, 18);
                Buffer.BlockCopy(value, 0, Body, 18, value.Length);
            }
        }
        private byte[] _parameters = new byte[0];

        /// <summary>
        /// 动态AES密钥，用于加解密
        /// </summary>
        public byte[] DynamicKey
        {
            get => _dynamicKey;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value.Length != 16)
                {
                    throw new ArgumentException("Invalid value: the length of dynamic key must be 16.", nameof(value));
                }
                _dynamicKey = (byte[])value.Clone();
            }
        }
        private byte[] _dynamicKey = new byte[0];

        /// <summary>
        /// AES的初始化向量
        /// </summary>
        public byte[] AesIV
        {
            get => _aesIV;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value.Length != 16)
                {
                    throw new ArgumentException("Invalid value: the length of aes IV must be 16.");
                }

                _aesIV = (byte[])value.Clone();
                Buffer.BlockCopy(value, 0, Body, 0, value.Length);
            }
        }
        private byte[] _aesIV = new byte[16];

        #endregion

        /// <summary>
        /// 构造一个控制指令消息
        /// </summary>
        /// <param name="commandCode">指令代码</param>
        /// <param name="dynamicKey">动态AES密钥</param>
        /// <param name="parameters">参数列表（如果没有参数，传入空数组）</param>
        public InstructionMessage(InstructionType commandCode, byte[] dynamicKey, byte[] parameters) : base(0x05, null)
        {
            Body = new byte[18];
            CommandCode = commandCode;
            Parameters = parameters ?? new byte[0];
            DynamicKey = dynamicKey;

            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();   // 生成一个随机的初始化向量
                AesIV = aes.IV;
            }
        }

        /// <summary>
        /// 构造一个控制指令消息
        /// </summary>
        /// <param name="commandCode">指令代码</param>
        /// <param name="dynamicKey">动态AES密钥</param>
        /// <param name="parameters">参数列表（如果没有参数，传入空数组）</param>
        /// <param name="aesIV">AES初始化向量</param>
        public InstructionMessage(InstructionType commandCode, byte[] dynamicKey, byte[] parameters, byte[] aesIV) : base(0x05, null)
        {
            Body = new byte[18];
            CommandCode = commandCode;
            Parameters = parameters;
            DynamicKey = dynamicKey;
            AesIV = aesIV;
        }

        /// <summary>
        /// 将控制指令消息转换为字节数组
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = DynamicKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = AesIV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(
                        ms,
                        aes.CreateEncryptor(aes.Key, aes.IV),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(Body, 16, Body.Length - 16);
                    }
                    encrypted = ms.ToArray();
                }
            }

            var byteArrays = new List<byte[]>()
            {
                BitConverter.GetBytes(Head),
                BitConverter.GetBytes(Version),
                BitConverter.GetBytes(Number),
                BitConverter.GetBytes(Type),
                BitConverter.GetBytes(Sum),
                AesIV,
                encrypted
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
        /// 将字节数组解析为控制指令消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="dynamicKey">动态AES密钥</param>
        /// <returns></returns>
        public static InstructionMessage Parse(byte[] bytes, byte[] dynamicKey)
        {
            ECGPMessage rawMessage = Parse(bytes);

            byte[] aesIV = new byte[16];
            Buffer.BlockCopy(rawMessage.Body, 0, aesIV, 0, aesIV.Length);

            byte[] encrypted = new byte[rawMessage.Body.Length - 16];
            Buffer.BlockCopy(rawMessage.Body, 16, encrypted, 0, rawMessage.Body.Length - 16);

            byte[] decrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = dynamicKey;
                aes.Mode = CipherMode.CBC;
                aes.IV = aesIV;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(
                        ms,
                        aes.CreateDecryptor(),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(encrypted, 0, encrypted.Length);
                        cs.FlushFinalBlock();
                        decrypted = ms.ToArray();
                    }
                }
            }

            var cmdCode = (InstructionType)BitConverter.ToUInt16(decrypted, 0);
            var paras = new byte[decrypted.Length - 2];
            Buffer.BlockCopy(decrypted, 2, paras, 0, paras.Length);

            return new InstructionMessage(cmdCode, dynamicKey, paras, aesIV);
        }

        /// <summary>
        /// 尝试将字节数组解析为控制指令消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="dynamicKey">动态AES密钥</param>
        /// <param name="result">解析结果</param>
        /// <returns></returns>
        public static bool TryParse(byte[] bytes, byte[] dynamicKey, out InstructionMessage result)
        {
            try
            {
                result = Parse(bytes, dynamicKey);
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