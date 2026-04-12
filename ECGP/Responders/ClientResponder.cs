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
using ECGP.Enums;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ECGP.Responders
{
    /// <summary>
    /// 客户端的主响应器，用于回复动态密钥、控制指令确认报文等信息
    /// </summary>
    public class ClientResponder : IDisposable
    {
        private UdpClient _udpClient;

        /// <summary>
        /// 接收请求的端口号
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
        private int _port = 25584;

        /// <summary>
        /// RSA公钥xml
        /// </summary>
        public string RsaPublicKeyXml
        {
            get => _rsaPublicKeyXml;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _rsaPublicKeyXml = value;
            }
        }
        private string _rsaPublicKeyXml;

        private DynamicKeyManager _keyManager = new DynamicKeyManager();

        /// <summary>
        /// 执行指定命令的委托
        /// </summary>
        public Func<InstructionType, byte[], IPEndPoint, Task<ReturnCode>> ExecuteInstruction
        {
            get => _executeInstruction;
            set
            {
                _executeInstruction = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private Func<InstructionType, byte[], IPEndPoint, Task<ReturnCode>> _executeInstruction;

        /// <summary>
        /// 显示执行结果的委托
        /// </summary>
        public Action<ReturnCode> ShowExecutionResult
        {
            get => _showExecutionResult;
            set
            {
                _showExecutionResult = value;
            }
        }
        private Action<ReturnCode> _showExecutionResult;

        /// <summary>
        /// 是否已经启动
        /// </summary>
        public bool IsRunning { get; private set; }

        public ClientResponder(string rsaPublicKeyXml, Func<InstructionType, byte[], IPEndPoint, Task<ReturnCode>> executeInstruction)
        {
            RsaPublicKeyXml = rsaPublicKeyXml;
            ExecuteInstruction = executeInstruction;
        }

        public ClientResponder(string rsaPublicKeyXml, Func<InstructionType, byte[], IPEndPoint, Task<ReturnCode>> executeInstruction, int port)
        {
            RsaPublicKeyXml = rsaPublicKeyXml;
            ExecuteInstruction = executeInstruction;
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
                    if (message.Type == 0x03)       // 动态密钥请求报文
                    {
                        if (message is DynamicKeyRequestMessage)
                        {
                            uint number = message.Number;
                            ReplyDynamicKey(endPoint, number);
                        }
                    }
                    else if (message.Type == 0x05)  // 控制指令报文
                    {
                        if (message is InstructionMessage msg)
                        {
                            var ret = await ExecuteInstruction(msg.CommandCode, msg.Parameters, endPoint);
                            ShowExecutionResult?.Invoke(ret);
                            ReplyConfirmation(endPoint, ret, msg.Number);
                        }
                    }
                }
                catch
                {
                    // [TODO] 这里可以log一下，暂且忽略
                }
            }
        }

        public void Stop()
        {
            _udpClient?.Close();
            IsRunning = false;
        }

        /// <summary>
        /// 回复动态密钥响应报文
        /// </summary>
        /// <param name="target">目标主机</param>
        /// <param name="numReceived">接收到的请求报文的特征码</param>
        private void ReplyDynamicKey(IPEndPoint target, uint numReceived)
        {
            var dynamicKey = _keyManager.LatestKey;
            var message = new DynamicKeyResponseMessage(dynamicKey, numReceived, RsaPublicKeyXml);
            var bytes = message.ToBytes();

            using (UdpClient sender = new UdpClient())
            {
                sender.Send(bytes, bytes.Length, target);
            }
        }

        /// <summary>
        /// 回复控制指令确认报文
        /// </summary>
        /// <param name="target">目标主机</param>
        /// <param name="returnCode">返回值</param>
        private void ReplyConfirmation(IPEndPoint target, ReturnCode returnCode, uint numReceived)
        {
            var message = new ConfirmationMessage(numReceived, returnCode, RsaPublicKeyXml);
            var bytes = message.ToBytes();

            using (UdpClient sender = new UdpClient())
            {
                sender.Send(bytes, bytes.Length, target);
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

        public void Dispose()
        {
            IsRunning = false;
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
