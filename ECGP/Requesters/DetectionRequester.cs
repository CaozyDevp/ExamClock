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

using ECGP.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ECGP.Requesters
{
    /// <summary>
    /// 存在探测消息发送器
    /// </summary>
    public class DetectionRequester : IDisposable
    {
        private UdpClient _udpClient;

        /// <summary>
        /// 客户端接收消息的端口
        /// </summary>
        public int Port => _port;
        private readonly int _port;

        /// <summary>
        /// RSA私钥的XML字符串（包含私钥参数），用于解密动态密钥响应消息中的动态密钥
        /// </summary>
        public string RsaPrivateKeyXml => _rsaPrivateKeyXml;
        private readonly string _rsaPrivateKeyXml;

        public DetectionRequester(int port, string rsaPrivateKeyXml)
        {
            if (rsaPrivateKeyXml == null || string.IsNullOrEmpty(rsaPrivateKeyXml))
            {
                throw new ArgumentNullException(nameof(rsaPrivateKeyXml));
            }
            _udpClient = new UdpClient(0);
            _port = port;
            _rsaPrivateKeyXml = rsaPrivateKeyXml;
        }

        /// <summary>
        /// 向指定端口广播存在探测消息
        /// </summary>
        private void BroadcastDetectionRequest()
        {
            var messageBytes = new DetectionMessage().ToBytes();

            var target = new IPEndPoint(IPAddress.Broadcast, Port);

            _udpClient.EnableBroadcast = true;
            _udpClient.Send(messageBytes, messageBytes.Length, target);
        }

        /// <summary>
        /// 接收单个原始UDP数据包。如果在超时时间内没有接收到，返回null
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        private async Task<RawPacket?> ReceiveRawPacket(int timeout)
        {
            try
            {
                var receiveTask = _udpClient.ReceiveAsync();
                var delayTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(receiveTask, delayTask);
                if (completedTask == delayTask)
                    return null;

                var result = await receiveTask;
                return new RawPacket(result.Buffer, result.RemoteEndPoint);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将接收到的数据包解析为<see cref="RoomInfo"/>
        /// </summary>
        /// <param name="packets">接收到的原始数据包</param>
        /// <returns>解析结果</returns>
        private List<RoomInfo> ParsePackets(List<RawPacket> packets)
        {
            var rooms = new List<RoomInfo>();
            foreach (var packet in packets)
            {
                // 数据包为空
                if (packet.Data == null || packet.Data.Length == 0)
                    continue;

                // 解析失败
                if (!StatusResponseMessage.TryParse(packet.Data, RsaPrivateKeyXml, out var message))
                    continue;

                var room = new RoomInfo()
                {
                    RoomNumber = message.RoomNumber,
                    IP = packet.Source.Address,
                    ScheduleHash = message.ScheduleMD5,
                    Status = message.Status,
                    Volume = message.SystemVolume,
                    NoticeBeforeEndingType = message.NoticeBeforeEndingType,
                    IsExamBeginNoticeEnabled = message.IsBeginningNoticeEnabled,
                    IsExamEndNoticeEnabled = message.IsEndingNoticeEnabled,
                };
                rooms.Add(room);
            }

            return rooms;
        }

        /// <summary>
        /// 广播存在探测消息，并将接收到的消息转为<see cref="RoomInfo"/>，最多接收64个响应
        /// </summary>
        /// <returns>接收到的响应信息</returns>
        public async Task<List<RoomInfo>> BroadcastAndGetRoomInfos()
        {
            const int totalTimeout = 1000;

            BroadcastDetectionRequest();

            var endTime = DateTime.UtcNow.AddMilliseconds(totalTimeout);
            var rawPackets = new List<RawPacket>();
            while (DateTime.UtcNow < endTime)
            {
                var remaining = (int)(endTime - DateTime.UtcNow).TotalMilliseconds;
                if (remaining <= 0) break;

                var packet = await ReceiveRawPacket(remaining);
                if (packet != null)
                {
                    rawPackets.Add((RawPacket)packet);
                }
            }

            return ParsePackets(rawPackets);
        }

        public void Dispose()
        {
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
