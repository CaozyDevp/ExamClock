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
using System.Threading.Tasks;
using System.Windows;

namespace TimeSync
{
    public class TimeSyncRequester
    {
        private void BroadcastTimeSyncRequest(int port, ushort hostNumber)
        {
            var message = new TimeSyncMessage()
            {
                Type = MessageType.Request,
                DateTimes = new List<DateTime> { DateTime.UtcNow },
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
        private async Task<TimeKeeper> ReceiveResponse(int port, int timeout)
        {
            var target = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = null;

            using (var receiver = new UdpClient(port))
            {
                try
                {
                    var receiveTask = receiver.ReceiveAsync();
                    var delayTask = Task.Delay(timeout);

                    // 已经完成的任务，用于实现超时检测
                    var completedTask = await Task.WhenAny(receiveTask, delayTask);
                    if (completedTask == delayTask)
                    {
                        return null;
                    }

                    var result = await receiveTask;
                    data = result.Buffer;
                    target = result.RemoteEndPoint;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"接收时间同步响应消息时发生错误，错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            if (data == null || !TimeSyncMessage.TryParse(out var message, data))
            {
                return null;
            }

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
        /// 广播请求消息，并将接收到的消息转为<see cref="TimeKeeper"/>，最多接收50个响应
        /// </summary>
        /// <param name="requestPort"></param>
        /// <param name="respondPort"></param>
        /// <param name="hostNumber"></param>
        /// <returns></returns>
        public async Task<List<TimeKeeper>> BroadcastAndGetTimeKeepers(int requestPort, int respondPort, ushort hostNumber)
        {
            const int timeout = 1000;
            BroadcastTimeSyncRequest(requestPort, hostNumber);
            var timeKeepers = new List<TimeKeeper>();

            // 最多接收50个响应
            for (int i = 0; i < 50; i++)
            {
                var keeper = await ReceiveResponse(respondPort, timeout);
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
