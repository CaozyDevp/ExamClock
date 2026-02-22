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
using System.Net;
using System.Net.Sockets;

namespace ECGP.Requesters
{
    /// <summary>
    /// 存在探测消息发送器
    /// </summary>
    public class DetectionRequester
    {
        private UdpClient _udpClient;

        /// <summary>
        /// 客户端接收消息的端口
        /// </summary>
        public int Port => _port;
        private readonly int _port;

        public DetectionRequester(int port)
        {
            _udpClient = new UdpClient();
            _port = port;
        }

        /// <summary>
        /// 向指定端口广播存在探测消息
        /// </summary>
        /// <param name="port"></param>
        private void BroadcastDetectionRequest(int port)
        {
            var messageBytes = new DetectionMessage().ToBytes();

            var target = new IPEndPoint(IPAddress.Broadcast, port);

            _udpClient.EnableBroadcast = true;
            _udpClient.Send(messageBytes, messageBytes.Length, target);
        }

    }
}
