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

using TimeSync;

namespace TimeSyncTests
{
    [TestClass()]
    public class TimeSyncMessageTests
    {
        [TestMethod()]
        public void TimeSyncMessage_Test()
        {
            var message = new TimeSyncMessage
                (MessageType.Request, new List<DateTime> { new DateTime(2025, 8, 26, 10, 0, 0, DateTimeKind.Utc) }, 152);
            Assert.AreEqual(MessageType.Request, message.Type);
            Assert.AreEqual((ushort)152, message.HostNumber);
            Assert.ReferenceEquals(new List<DateTime> { new DateTime(2025, 8, 26, 10, 0, 0, DateTimeKind.Utc) }, message.DateTimes);
        }

        [TestMethod()]
        public void Parse_Test()
        {
            // 请求消息 考场号:1234 T1:2025/9/1/14:00:00.000
            byte verifyByte = 0b_0010_0011;
            var data = new byte[] { 0x45, 0x43, 0x54, 0x43, verifyByte, 0xd2, 0x04,
            0xe9, 0x07, 0x09, 0x01, 0x0e, 0x00, 0x00, 0x00, 0x00}; ;
            var message = TimeSyncMessage.Parse(data);

            Assert.AreEqual((ushort)1234, message.HostNumber);
            Assert.AreEqual(MessageType.Request, message.Type);
            Assert.ReferenceEquals(new List<DateTime> { new DateTime(2025, 9, 1, 14, 0, 0, DateTimeKind.Utc) }, message.DateTimes);
        }

        [TestMethod()]
        public void Parse_InvalidVarifyCode_Test()
        {
            // 请求消息 考场号:1234 T1:2025/9/1/14:00:00.000
            byte verifyByte = 0b_0011_0011;
            var data = new byte[] { 0x45, 0x43, 0x54, 0x43, verifyByte, 0xd2, 0x04,
            0xe9, 0x07, 0x09, 0x01, 0x0e, 0x00, 0x00, 0x00, 0x00};
            try
            {
                var message = TimeSyncMessage.Parse(data);
                Assert.Fail();
            }
            catch
            {
                Assert.IsTrue(true);    // 抛出异常，通过测试
            }


        }

        [TestMethod()]
        public void ToBytes_Test()
        {
            var message = new TimeSyncMessage()
            {
                Type = MessageType.Request,
                HostNumber = (ushort)1234,
                DateTimes = new List<DateTime> { new DateTime(2025, 9, 1, 14, 0, 0, DateTimeKind.Utc) }
            };

            // 请求消息 考场号:1234 T1:2025/9/1/14:00:00.000
            byte flag = 0b_0010_0011;
            var data = new byte[] { 0x45, 0x43, 0x54, 0x43, flag, 0xd2, 0x04,
            0xe9, 0x07, 0x09, 0x01, 0x0e, 0x00, 0x00, 0x00, 0x00}; ;

            Assert.ReferenceEquals(data, message.ToBytes());
        }
    }
}