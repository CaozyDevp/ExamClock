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
using System.Net;
using System.Net.Sockets;

namespace TimeSync
{
    public class TimeSyncRequester
    {
        private void BroadcastTimeSyncRequest(int port, ushort hostNumber)
        {
            var now = DateTime.Now.ToLocalTime();
            var message = new TimeSyncMessage()
            {
                Type = MessageType.Request,
                DateTimes = new List<DateTime> { now },
                HostNumber = hostNumber
            };

            var data = message.ToBytes();

            var target = new IPEndPoint(IPAddress.Broadcast, port);

            using (var sender = new UdpClient())
            {
                sender.EnableBroadcast = true;
                sender.Send(data, data.Length, target);
            }
        }

        /// <summary>
        /// 接收指定端口的回复，如果没有收到，返回<see cref="null"/>
        /// </summary>
        private TimeKeeper ReceiveResponse(int port, int timeout)
        {
            var target = new IPEndPoint(IPAddress.Any, 0);
            byte[] data;

            using (var receiver = new UdpClient(port))
            {
                try
                {
                    receiver.Client.ReceiveTimeout = timeout;
                    data = receiver.Receive(ref target);
                }
                catch
                {
                    return null;
                }
            }
            var message = TimeSyncMessage.Parse(data);

            if (message.Type == MessageType.Response)
            {
                return GetTimeKeeper(message.DateTimes, message.HostNumber, DateTime.UtcNow, target.Address);
            }
            else
            {
                return null;
            }

        }

        private TimeKeeper GetTimeKeeper(List<DateTime> utcTimes, ushort hostNumber, DateTime utcNow, IPAddress ip)
        {
            var temp = (utcTimes[1] - utcTimes[0]) + (utcTimes[2] - utcNow);
            var offset = new TimeSpan(temp.Ticks / 2);
            return new TimeKeeper(offset)
            {
                Address = ip,
                HostNumber = hostNumber,
            };
        }

        /// <summary>
        /// 广播请求消息，并将接收到的消息转为<see cref="TimeKeeper"/>
        /// </summary>
        /// <param name="requestPort"></param>
        /// <param name="respondPort"></param>
        /// <param name="hostNumber"></param>
        /// <returns></returns>
        public List<TimeKeeper> BroadcastAndGetTimeKeepers(int requestPort, int respondPort, ushort hostNumber)
        {
            const int timeout = 1000;
            BroadcastTimeSyncRequest(requestPort, hostNumber);
            var timeKeepers = new List<TimeKeeper>();
            while (true)
            {
                var keeper = ReceiveResponse(respondPort, timeout);
                if (keeper == null)
                {
                    break;
                }
                timeKeepers.Add(keeper);
            }
            return timeKeepers;
        }
    }
}
