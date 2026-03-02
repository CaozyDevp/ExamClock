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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ECGP.Responders
{
    public class StatusResponder : IDisposable
    {
        private UdpClient _udpClient;

        /// <summary>
        /// 接收探测请求的端口号
        /// </summary>
        public int Port
        {
            get => _port;
            set
            {
                if (value >= 1 && value <= 65535)
                {
                    _port = value;
                    return;
                }
                throw new ArgumentException("Invalid port number!", nameof(value));
            }
        }
        private int _port = 25585;

        /// <summary>
        /// 获取系统音量的委托，返回1~100
        /// </summary>
        public Func<byte> GetSystemVolumeFunc
        {
            get => _getSystemVolumeFunc;
            set
            {
                _getSystemVolumeFunc = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<byte> _getSystemVolumeFunc;

        /// <summary>
        /// 获取考场号的委托
        /// </summary>
        public Func<ushort> GetRoomNumberFunc
        {
            get => _getRoomNumberFunc;
            set
            {
                _getRoomNumberFunc = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<ushort> _getRoomNumberFunc;

        /// <summary>
        /// 获取当前状态的委托
        /// </summary>
        public Func<ClientStatus> GetStatusFunc
        {
            get => _getStatusFunc;
            set
            {
                _getStatusFunc = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<ClientStatus> _getStatusFunc;

        /// <summary>
        /// 获取日程配置哈希值的委托（16字节MD5值）
        /// </summary>
        public Func<byte[]> GetScheduleHashFunc
        {
            get => _getScheduleHashFunc;
            set
            {
                _getScheduleHashFunc = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<byte[]> _getScheduleHashFunc;

        /// <summary>
        /// 获取提醒（播报）配置的委托
        /// </summary>
        public Func<NoticeConfig> GetNoticeConfigFunc
        {
            get => _getNoticeConfigFunc;
            set
            {
                _getNoticeConfigFunc = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<NoticeConfig> _getNoticeConfigFunc;

        /// <summary>
        /// RSA私钥xml
        /// </summary>
        public string RsaPrivateKeyXml
        {
            private get => _rsaPrivateKeyXml;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _rsaPrivateKeyXml = value;
            }
        }
        private string _rsaPrivateKeyXml;

        /// <summary>
        /// 是否已经启动
        /// </summary>
        public bool IsRunning { get; private set; }

        public StatusResponder(Func<byte> getSystemVolumeFunc, Func<ushort> getRoomNumberFunc, Func<ClientStatus> getStatusFunc, 
            Func<byte[]> getScheduleHashFunc, Func<NoticeConfig> getNoticeConfigFunc, string rsaPrivateKeyXml)
        {
            GetSystemVolumeFunc = getSystemVolumeFunc;
            GetRoomNumberFunc = getRoomNumberFunc;
            GetStatusFunc = getStatusFunc;
            GetScheduleHashFunc = getScheduleHashFunc;
            GetNoticeConfigFunc = getNoticeConfigFunc;
            RsaPrivateKeyXml = rsaPrivateKeyXml;
        }

        public StatusResponder(Func<byte> getSystemVolumeFunc, Func<ushort> getRoomNumberFunc, Func<ClientStatus> getStatusFunc, 
            Func<byte[]> getScheduleHashFunc, Func<NoticeConfig> getNoticeConfigFunc, int port, string rsaPrivateKeyXml)
            : this(getSystemVolumeFunc, getRoomNumberFunc, getStatusFunc, getScheduleHashFunc, getNoticeConfigFunc, rsaPrivateKeyXml)
        {
            Port = port;
        }

        public async Task StartAsync()
        {
            if (IsRunning) return;
            IsRunning = true;

            _udpClient?.Close();
            _udpClient = new UdpClient(Port);

            while (IsRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var data = result.Buffer;
                    var endPoint = result.RemoteEndPoint;

                    // 如果是本机发来的请求，则忽略
                    if (IsLocalAddress(endPoint.Address)) continue;

                    var message = ECGPMessage.Parse(data);
                    if (message.Type == 0x01)
                    {
                        ReplyStatusMessage(message.Number, endPoint.Address);
                    }
                }
                catch
                {
                    // [TODO] 这里可以log一下，暂且忽略
                }
            }
        }

        /// <summary>
        /// 回复状态响应信息
        /// </summary>
        /// <param name="numReceived">接收到的请求信息中包含的特征码</param>
        /// <param name="target">目标IP地址</param>
        private void ReplyStatusMessage(uint numReceived, IPAddress target)
        {
            var roomNumber = GetRoomNumberFunc();
            var status = GetStatusFunc();
            var hash = GetScheduleHashFunc();
            var volume = GetSystemVolumeFunc();
            var notice = GetNoticeConfigFunc();

            var message = new StatusResponseMessage(numReceived, roomNumber, status, hash, notice.EnableBeginning,
                notice.EnableEnding, notice.BeforeEnding, volume, RsaPrivateKeyXml);
            var msgBytes = message.ToBytes();

            _udpClient.Send(msgBytes, msgBytes.Length, new IPEndPoint(target, Port));
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

        public void Dispose()
        {
            IsRunning = false;
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
