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
        /// <summary>
        /// 主机号（考场号）
        /// </summary>
        public ushort HostNumber { get => GetHostNumber.Invoke(); }

        /// <summary>
        /// 接收请求的端口
        /// </summary>
        public int RequestPort { get; }

        /// <summary>
        /// 对方接收响应的端口
        /// </summary>
        public int RespondPort { get; }

        /// <summary>
        /// 是否已经启动
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 获取主机号（考场号）的委托
        /// </summary>
        private Func<ushort> GetHostNumber { get; }

        /// <summary>
        /// 用于接收请求信息的UdpClient
        /// </summary>
        private UdpClient Receiver { get; set; }

        public TimeSyncResponder(Func<ushort> getHostNumber, int requestPort, int respondPort)
        {
            GetHostNumber = getHostNumber ?? throw new ArgumentNullException(nameof(getHostNumber));
            RequestPort = requestPort;
            RespondPort = respondPort;
        }

        public async Task StartAsync()
        {
            if (IsRunning) return;
            IsRunning = true;

            Receiver = new UdpClient(RequestPort);

            while (IsRunning)
            {
                var result = await Receiver.ReceiveAsync();
                var data = result.Buffer;
                var endPoint = result.RemoteEndPoint;

                // 如果是本机发来的请求，则忽略
                if (IsLocalAddress(endPoint.Address)) continue;

                ReplyTimeSyncMessage(TimeSyncMessage.Parse(data), DateTime.UtcNow, endPoint.Address, HostNumber, RespondPort);
            }
        }
        private bool IsLocalAddress(IPAddress address)
        {
            var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var addr in hostAddresses)
            {
                if (addr.Equals(address))
                    return true;
            }
            return IPAddress.IsLoopback(address);
        }

        public void Stop()
        {
            IsRunning = false;
            Receiver = null;
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
            List<DateTime> dateTimes = new List<DateTime>
            {
                request.DateTimes[0],
                arriveTime,
                DateTime.UtcNow
            };
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
    }
}