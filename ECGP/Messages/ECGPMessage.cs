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
using System.Text;

namespace ECGP.Messages
{
    public class ECGPMessage
    {
        #region Fields

        /// <summary>
        /// 报头的ASCII字符串
        /// </summary>
        private const string HeaderString = ".%ECGP%.";

        #endregion

        #region Properties

        /// <summary>
        /// 报头
        /// </summary>
        public ulong Head
        {
            get => BitConverter.ToUInt64(Encoding.ASCII.GetBytes(HeaderString), 0);
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public ushort Version { get; set; } = 0x01;

        /// <summary>
        /// 特征码
        /// </summary>
        public uint Number { get; private set; }

        /// <summary>
        /// 类型
        /// </summary>
        public ushort Type { get; set; }

        /// <summary>
        /// 加和校验
        /// </summary>
        public uint Sum
        {
            get
            {
                unchecked
                {
                    uint tempSum = 0;
                    foreach (char c in HeaderString)
                    {
                        tempSum += (byte)c;
                    }
                    tempSum += Version;
                    tempSum += Number;
                    tempSum += Type;
                    foreach (var b in Body)
                    {
                        tempSum += b;
                    }
                    return tempSum;
                }
            }
        }

        /// <summary>
        /// 主体
        /// </summary>
        public byte[] Body { get; set; } = new byte[0];

        #endregion

        #region Constructors

        /// <summary>
        /// 构造一个ECGP消息，版本号默认
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="body">消息主体</param>
        public ECGPMessage(ushort type, byte[] body)
        {
            Number = GetRandomNumber();
            Type = type;
            Body = body ?? (new byte[0]);
        }

        /// <summary>
        /// 构造一个ECGP消息
        /// </summary>
        /// <param name="ver">版本号</param>
        /// <param name="type">类型</param>
        /// <param name="body">消息主体</param>
        public ECGPMessage(ushort ver, ushort type, byte[] body)
        {
            Version = ver;
            Number = GetRandomNumber();
            Type = type;
            Body = body ?? (new byte[0]);
        }

        /// <summary>
        /// 构造一个填入指定特征码的ECGP消息
        /// </summary>
        /// <param name="ver">版本号</param>
        /// <param name="type">类型</param>
        /// <param name="body">消息主体</param>
        /// <param name="number">特征码</param>
        public ECGPMessage(ushort ver, uint number, ushort type, byte[] body)
        {
            Version = ver;
            Number = number;
            Type = type;
            Body = body ?? (new byte[0]);
        }

        /// <summary>
        /// 构造一个填入指定特征码的ECGP消息，版本号默认
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="body">消息主体</param>
        /// <param name="number">特征码</param>
        public ECGPMessage(uint number, ushort type, byte[] body)
        {
            Number = number;
            Type = type;
            Body = body ?? (new byte[0]);
        }

        #endregion

        /// <summary>
        /// 将ECGP消息转换为字节数组
        /// </summary>
        public virtual byte[] ToBytes()
        {
            var arrayList = new List<byte[]>()
            {
                BitConverter.GetBytes(Head),
                BitConverter.GetBytes(Version),
                BitConverter.GetBytes(Number),
                BitConverter.GetBytes(Type),
                BitConverter.GetBytes(Sum),
                Body ?? (new byte[0])
            };

            int totalLength = 0;
            foreach (var array in arrayList)
            {
                totalLength += array.Length;
            }
            var result = new byte[totalLength];

            // 拼合数组
            int offset = 0;
            foreach (var array in arrayList)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        /// <summary>
        /// 将字节数组解析为ECGP消息
        /// </summary>
        /// <exception cref="ECGPFormatException">不符合ECGP的定义，无法正常解析</exception>
        public static ECGPMessage Parse(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            // 至少20字节
            if (bytes.Length < 20)
            {
                throw new ECGPFormatException("Invalid ECGP message: insufficient length.");
            }

            // 校验报头
            var headBytes = Encoding.ASCII.GetBytes(HeaderString);
            for (int i = 0; i < 8; i++)
            {
                if (bytes[i] != headBytes[i])
                {
                    throw new ECGPFormatException("Invalid ECGP message: HEAD verification failed.");
                }
            }

            // 验证加和校验字段
            if (!VerifySum(bytes, 16, out _))
            {
                throw new ECGPFormatException("Invalid ECGP message: SUM verification failed.");
            }

            var ver = BitConverter.ToUInt16(bytes, 8);
            var num = BitConverter.ToUInt32(bytes, 10);
            var type = BitConverter.ToUInt16(bytes, 14);
            var body = new byte[bytes.Length - 20];
            Buffer.BlockCopy(bytes, 20, body, 0, body.Length);

            return new ECGPMessage(ver, num, type, body);
        }

        /// <summary>
        /// 尝试将字节数组解析为ECGP消息
        /// </summary>
        /// <param name="result">结果ECGP消息，如果解析失败，返回空引用</param>
        /// <param name="bytes">源字节数组</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(out ECGPMessage result, byte[] bytes)
        {
            try
            {
                result = Parse(bytes);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// 验证加和校验字段
        /// </summary>
        /// <param name="bytes">ECGP报文</param>
        /// <param name="sumOffset">加和校验字段起始字节的偏移量，从0开始</param>
        /// <param name="sum">加和校验字段的值（报文中给出的）</param>
        /// <returns>加和校验字段是否正确</returns>
        private static bool VerifySum(byte[] bytes, int sumOffset, out uint sum)
        {
            uint calculatedSum = 0;   // 期望的加和校验值（实际计算出的）
            unchecked
            {
                foreach (var b in bytes)
                {
                    calculatedSum += b;
                }
                for (var i = sumOffset; i < sumOffset + 4; i++)
                {
                    calculatedSum -= bytes[i];
                }
            }

            var sumBytes = new byte[4];
            Buffer.BlockCopy(bytes, sumOffset, sumBytes, 0, sumBytes.Length);
            sum = BitConverter.ToUInt32(sumBytes, 0);

            return sum == calculatedSum;
        }

        /// <summary>
        /// 获取一个密码学安全随机数
        /// </summary>
        protected static uint GetRandomNumber()
        {
            byte[] bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
