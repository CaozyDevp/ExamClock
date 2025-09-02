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

namespace TimeSync
{
    public class TimeSyncMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 消息中包含的时间信息
        /// </summary>
        public List<DateTime> DateTimes { get; set; } = new List<DateTime>();

        /// <summary>
        /// 考场号
        /// </summary>
        public ushort HostNumber { get; set; }

        public TimeSyncMessage(MessageType type, List<DateTime> dateTimes, ushort hostNumber)
        {
            Type = type;
            DateTimes = dateTimes;
            HostNumber = hostNumber;
        }

        public TimeSyncMessage()
        {

        }


        public static TimeSyncMessage Parse(byte[] message)
        {
            // 最少有16个字节
            if (message == null || message.Length < 16)
            {
                throw new Exception("Length invalid!");
            }

            // 通过校验前4个字节，判断是不是“考试时钟时间校准信息”
            if (message[0] != 0x45 &&
                message[1] != 0x43 &&
                message[2] != 0x54 &&
                message[3] != 0x43)
            {
                throw new Exception("Is not Exam Clock Time Calibration Message!");
            }

            // 校验第4个字节的低2位，再次判断是不是“考试时钟时间校准信息”
            if ((message[4] & 0b_0000_0011) != 0b_0000_0011)
            {
                throw new Exception("Is not Exam Clock Time Calibration Message!");
            }

            // 第4字节的的第2位：0=>请求信息，1=>响应信息
            bool isResponse = (message[4] & 0b_0000_0100) == 0b_0000_0100;

            // 第4字节的的第4、5位：校验位
            byte verify = (byte)((message[4] & 0b_0011_0000) / 16);

            // 第5、6字节：考场号
            ushort number = (ushort)(message[6] * 256 + message[5]);

            List<DateTime> points;

            if (isResponse)
            {
                // 响应消息至少为34个字节
                if (message.Length < 34) throw new Exception("Length invalid!");

                // 比对校验码
                if (!Verify(message, 5, 9 * 3 + 2, verify)) throw new Exception("Validation failed!");

                var t1 = ConvertToDateTime(message, 7 + 9 * 0);
                var t2 = ConvertToDateTime(message, 7 + 9 * 1);
                var t3 = ConvertToDateTime(message, 7 + 9 * 2);

                points = new List<DateTime> { t1, t2, t3 };
            }
            else
            {
                // 比对校验码
                if (!Verify(message, 5, 9 * 1 + 2, verify)) throw new Exception("Validation failed!");

                var t1 = ConvertToDateTime(message, 7 + 9 * 0);

                points = new List<DateTime> { t1 };
            }

            return new TimeSyncMessage()
            {
                Type = isResponse ? MessageType.Response : MessageType.Request,
                DateTimes = points,
                HostNumber = number
            };
        }

        public static bool TryParse(out TimeSyncMessage result, byte[] message)
        {
            try
            {
                result = TimeSyncMessage.Parse(message);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// 转换为时刻信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex">从这个下标开始往后共9个字节</param>
        private static DateTime ConvertToDateTime(byte[] data, int startIndex)
        {
            if (data.Length < startIndex + 9)
            {
                throw new Exception("Length invalid!");
            }
            var year = data[startIndex + 1] * 256 + data[startIndex + 0];
            var month = data[startIndex + 2];
            var day = data[startIndex + 3];
            var hour = data[startIndex + 4];
            var minute = data[startIndex + 5];
            var second = data[startIndex + 6];
            var millisecond = data[startIndex + 8] * 256 + data[startIndex + 7];

            return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        }

        private static byte[] ConvertDateTimeToBytes(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            List<byte> result = new List<byte>();

            ushort year = (ushort)dateTime.Year;
            result.AddRange(new byte[] { (byte)(year % 256), (byte)(year / 256) });

            byte month = (byte)dateTime.Month;
            result.Add(month);

            byte day = (byte)dateTime.Day;
            result.Add(day);

            byte hour = (byte)dateTime.Hour;
            result.Add(hour);

            byte minute = (byte)dateTime.Minute;
            result.Add(minute);

            byte second = (byte)dateTime.Second;
            result.Add(second);

            ushort millisecond = (ushort)dateTime.Millisecond;
            result.AddRange(new byte[2] { (byte)(millisecond % 256), (byte)(millisecond / 256) });

            return result.ToArray();
        }

        public byte[] ToBytes()
        {
            // 0~3字节固定，第4个字节使用0x00占位
            List<byte> result = new List<byte>() { 0x45, 0x43, 0x54, 0x43, 0x00 };

            ushort number = HostNumber;
            result.AddRange(new byte[] { (byte)(HostNumber % 256), (byte)(HostNumber / 256) });

            foreach (var time in DateTimes)
            {
                result.AddRange(ConvertDateTimeToBytes(time));
            }

            // 处理标识字节
            byte flags = 0b_0000_0011;
            bool isResponse = Type == MessageType.Response;
            if (isResponse)
            {
                flags |= 0b_0000_0100;
            }
            bool isUtcTime = DateTimes[0].Kind == DateTimeKind.Utc;
            if (isUtcTime)
            {
                flags |= 0b_0000_1000;
            }
            byte verify = GetVerificationCode(result.ToArray(), 5, result.Count - 5);
            flags |= (byte)(verify * 16);

            result[4] = flags;

            return result.ToArray();
        }

        private static bool Verify(byte[] data, int startIndex, int length, byte verifyCode)
        {
            if (GetVerificationCode(data, startIndex, length) == verifyCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static byte GetVerificationCode(byte[] data, int startIndex, int length)
        {
            int sum = 0;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                sum += data[i];
            }
            return (byte)(sum % 4);
        }
    }
}