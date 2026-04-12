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
using ExamClock.Core.Enums;
using System;
using ECGP.Enums;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ECGP.Messages
{
    /// <summary>
    /// 状态响应消息：对<see cref="DetectionMessage"/>的回复
    /// </summary>
    public class StatusResponseMessage : ECGPMessage
    {
        public StatusResponseMessage(uint numberReceived, ushort roomNumber, ClientStatus status, byte[] scheduleMD5, bool isBeginningNoticeEnabled,
            bool isEndingNoticeEnabled, SoundType noticeBeforeEndingType, byte systemVolume, string rsaPublicKeyXml) : base(0x02, null)
        {
            Body = new byte[25];
            NumberReceived = numberReceived;
            RoomNumber = roomNumber;
            Status = status;
            ScheduleMD5 = scheduleMD5;

            IsBeginningNoticeEnabled = isBeginningNoticeEnabled;
            IsEndingNoticeEnabled = isEndingNoticeEnabled;
            NoticeBeforeEndingType = noticeBeforeEndingType;

            SystemVolume = systemVolume;

            RSAPublicKeyXml = rsaPublicKeyXml;
        }

        /// <summary>
        /// 将状态响应消息转换为字节数组
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            byte[] encryptedBody;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(RSAPublicKeyXml);
                encryptedBody = rsa.Encrypt(Body, false);
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
        /// 将字节数组解析为状态响应消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="RSAPrivateKeyXml">用于解密的RSA私钥xml字符串</param>
        /// <returns></returns>
        /// <exception cref="ECGPFormatException"></exception>
        public static StatusResponseMessage Parse(byte[] bytes, string RSAPrivateKeyXml)
        {
            ECGPMessage rawMessage = Parse(bytes);
            string publicKeyXml;
            byte[] body;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(RSAPrivateKeyXml);
                body = rsa.Decrypt(rawMessage.Body, false);
                publicKeyXml = rsa.ToXmlString(false);
            }

            if (body.Length != 25)
            {
                throw new ECGPFormatException("The length of decrypted body must be 25.");
            }

            var numReceived = BitConverter.ToUInt32(body, 0);
            var roomNum = BitConverter.ToUInt16(body, 4);
            var scheduleMD5 = new byte[16];
            Buffer.BlockCopy(body, 7, scheduleMD5, 0, scheduleMD5.Length);

            // 状态信息
            var statusByte = body[6];
            if (statusByte > 3)
            {
                throw new ECGPFormatException("Status invalid. It can only be 0~3.");
            }
            ClientStatus status = (ClientStatus)statusByte;

            // 播报配置
            var notice = body[23];
            bool beginning = false;
            bool ending = false;
            SoundType beforeEnding = SoundType.None;
            if ((notice & 0b0000_0001) != 0)
            {
                beginning = true;
            }
            if ((notice & 0b0000_0010) != 0)
            {
                ending = true;
            }
            if ((notice & 0b0000_0100) != 0)
            {
                if ((notice & 0b0000_1000) == 0)
                {
                    beforeEnding = SoundType._10MinBeforeEnding;
                }
                else
                {
                    beforeEnding = SoundType._15MinBeforeEnding;
                }
            }

            // 系统音量
            var volume = body[24];

            return new StatusResponseMessage(numReceived, roomNum, status, scheduleMD5, beginning, ending, beforeEnding, volume, publicKeyXml);
        }

        /// <summary>
        /// 尝试将字节数组解析为状态响应消息
        /// </summary>
        /// <param name="bytes">源字节数组</param>
        /// <param name="RSAPrivateKeyXml">用于解密的RSA私钥xml字符串</param>
        /// <param name="result">解析结果，如果失败则为null</param>
        /// <returns>如果解析成功则为true，否则为false。</returns>
        public static bool TryParse(byte[] bytes, string RSAPrivateKeyXml, out StatusResponseMessage result)
        {
            try
            {
                result = Parse(bytes, RSAPrivateKeyXml);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        #region Properties

        /// <summary>
        /// 接收到的特征码
        /// </summary>
        public uint NumberReceived
        {
            get => _numberReceived;
            set
            {
                _numberReceived = value;
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Body, 0, sizeof(uint));
            }
        }
        private uint _numberReceived;

        /// <summary>
        /// 考场号
        /// </summary>
        public ushort RoomNumber
        {
            get => _roomNumber;
            set
            {
                _roomNumber = value;
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Body, 4, sizeof(ushort));
            }
        }
        private ushort _roomNumber;

        /// <summary>
        /// 状态信息
        /// </summary>
        public ClientStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                Body[6] = (byte)value;
            }
        }
        private ClientStatus _status;

        /// <summary>
        /// 日程配置哈希值
        /// </summary>
        public byte[] ScheduleMD5
        {
            get => _scheduleMD5;
            set
            {
                if (value == null || value.Length != 16)
                {
                    throw new ArgumentException("MD5 hash cannot be null and must have a length of 16 bytes.", nameof(value));
                }
                _scheduleMD5 = value;
                Buffer.BlockCopy(value, 0, Body, 7, value.Length);
            }
        }
        private byte[] _scheduleMD5 = new byte[16];

        /// <summary>
        /// 开考铃是否开启
        /// </summary>
        public bool IsBeginningNoticeEnabled
        {
            get => _isBeginningNoticeEnabled;
            set
            {
                _isBeginningNoticeEnabled = value;
                Body[23] &= 0b1111_1110;                    // 先置0
                Body[23] |= value ? (byte)0b0001 : (byte)0; // 然后根据实际情况置1
            }
        }
        private bool _isBeginningNoticeEnabled;

        /// <summary>
        /// 结束铃是否开启
        /// </summary>
        public bool IsEndingNoticeEnabled
        {
            get => _isEndingNoticeEnabled;
            set
            {
                _isEndingNoticeEnabled = value;
                Body[23] &= 0b1111_1101;                    // 先置0
                Body[23] |= value ? (byte)0b0010 : (byte)0; // 然后根据实际情况置1
            }
        }
        private bool _isEndingNoticeEnabled;

        /// <summary>
        /// 结束前提醒的类型
        /// </summary>
        public SoundType NoticeBeforeEndingType
        {
            get => _noticeBeforeEndingType;
            set
            {
                switch (value)
                {
                    case SoundType.None:
                        Body[23] &= (byte)0b_1111_0011; // 第2、3位：置0
                        break;
                    case SoundType._10MinBeforeEnding:
                        Body[23] &= (byte)0b_1111_0011; // 第2、3位：置0
                        Body[23] |= (byte)0b_0000_0100; // 第2位：置1
                        break;
                    case SoundType._15MinBeforeEnding:
                        Body[23] &= (byte)0b_1111_0011; // 第2、3位：置0
                        Body[23] |= (byte)0b_0000_1100; // 第2、3位：置1
                        break;
                    default:
                        throw new ArgumentException(
                            $"Invalid sound type. Only {SoundType.None}, {SoundType._10MinBeforeEnding}, and {SoundType._15MinBeforeEnding} are supported.",
                            nameof(value));
                }
                _noticeBeforeEndingType = value;
            }
        }
        private SoundType _noticeBeforeEndingType;

        /// <summary>
        /// 系统音量（范围0~100）
        /// </summary>
        public byte SystemVolume
        {
            get => _systemVolume;
            set
            {
                if (value > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "System volume must be between 0 and 100.");
                }
                _systemVolume = value;
                Body[24] = value;
            }
        }
        private byte _systemVolume;

        /// <summary>
        /// 用于加密的RSA公钥
        /// </summary>
        public string RSAPublicKeyXml { get; set; }

        #endregion
    }
}