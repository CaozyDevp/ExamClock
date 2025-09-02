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

namespace TimeSync
{
    /// <summary>
    /// 这个类用于接收时间同步请求，并做出回复
    /// </summary>
    public class TimeSyncResponder
    {
        public ushort HostNumber { get => GetHostNumber.Invoke(); }
        public int RequestPort { get; set; }
        public int RespondPort { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                if (value) _ = StartAsync();
                else Stop();
            }
        }
        private bool _isEnabled = false;

        public TimeSyncResponder(Func<ushort> getHostNumber, int requestPort, int respondPort)
        {
            GetHostNumber = getHostNumber;
            RequestPort = requestPort;
            RespondPort = respondPort;
        }

        public async Task StartAsync()
        {
            _isEnabled = true;
            while (_isEnabled)
            {
                await ListenAndReplyAsync(RequestPort, RespondPort, HostNumber);
            }
        }

        public Func<ushort> GetHostNumber { get; private set; }

        public void Stop()
        {
            _isEnabled = false;
        }

        /// <summary>
        /// 对时间同步请求作出回应
        /// </summary>
        /// <param name="request">接收到的请求消息</param>
        /// <param name="arriveTime">接到请求消息时，本机的时间（UTC）</param>
        /// <param name="target">对方主机的IP地址。响应消息将发送到这个IP地址。</param>
        /// <param name="hostNumber">本机的考场号（主机号）</param>
        /// <param name="port">对方主机的端口号。响应消息将发送到这个端口。</param>
        private void ReplyTimeSyncMessage(TimeSyncMessage request, DateTime arriveTime, IPAddress target, ushort hostNumber, int port)
        {
            List<DateTime> dateTimes = new List<DateTime>();
            dateTimes.Add(request.DateTimes[0]);
            dateTimes.Add(arriveTime);
            dateTimes.Add(DateTime.UtcNow);
            TimeSyncMessage response = new TimeSyncMessage()
            {
                Type = MessageType.Response,
                HostNumber = hostNumber,
                DateTimes = dateTimes
            };

            var data = response.ToBytes();

            using (var sender = new UdpClient())
            {
                sender.Send(data, data.Length, new IPEndPoint(target, port));
            }
        }

        /// <summary>
        /// 接收时间同步请求，并立即作出回应
        /// </summary>
        /// <param name="requestPort">本机被监听的端口，对方的请求发送到本机的这个端口</param>
        /// <param name="respondPort">对方主机接收响应消息的端口</param>
        /// <param name="hostNumber">本机的考场号（主机号）</param>
        /// <returns></returns>
        private async Task ListenAndReplyAsync(int requestPort, int respondPort, ushort hostNumber)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] data;

            using (var receiver = new UdpClient(requestPort))
            {
                var result = await receiver.ReceiveAsync();
                data = result.Buffer;
                endPoint = result.RemoteEndPoint;
            }

            ReplyTimeSyncMessage(TimeSyncMessage.Parse(data), DateTime.UtcNow, endPoint.Address, hostNumber, respondPort);
        }

    }
}