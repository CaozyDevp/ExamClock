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

namespace ECGP.Requesters
{
    public class InstructionSender : IDisposable
    {
        private UdpClient _udpClient;

        /// <summary>
        /// 客户端接收消息的端口
        /// </summary>
        public int Port => _port;
        private readonly int _port;

        /// <summary>
        /// 从客户端获取到的动态密钥
        /// </summary>
        public byte[] DynamicKey => _dynamicKey;
        private readonly byte[] _dynamicKey;

        /// <summary>
        /// RSA私钥的XML字符串（包含私钥参数），用于解密动态密钥响应消息中的动态密钥
        /// </summary>
        public string RsaPrivateKeyXml => _rsaPrivateKeyXml;
        private readonly string _rsaPrivateKeyXml;

        public InstructionSender(int port, byte[] dynamicKey, string rsaPrivateKeyXml)
        {
            _udpClient = new UdpClient();
            _port = port;
            _dynamicKey = dynamicKey;
            _rsaPrivateKeyXml = rsaPrivateKeyXml;
        }

        /// <summary>
        /// 向指定的客户端发送指令
        /// </summary>
        /// <param name="port">客户端接收指令的端口</param>
        /// <param name="address">客户端的IP地址</param>
        /// <param name="type">指令类型</param>
        /// <param name="paras">指令的参数列表。如果没有，传入空数组</param>
        /// <returns>发出的消息的特征码</returns>
        private uint SendInstruction(int port, IPAddress address, InstructionType type, byte[] paras)
        {
            paras = paras ?? new byte[0];
            var message = new InstructionMessage(type, DynamicKey, paras);
            var messageBytes = message.ToBytes();

            var target = new IPEndPoint(address, port);

            _udpClient.Send(messageBytes, messageBytes.Length, target);

            return message.Number;
        }

        /// <summary>
        /// 接收客户端返回的确认信息
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>来自客户端的返回值</returns>
        private async Task<ReturnCode> ReceiveResponse(int timeout, uint numberSent)
        {
            var receiveTask = _udpClient.ReceiveAsync();
            var delayTask = Task.Delay(timeout);

            // 已经完成的任务，用于实现超时检测
            var completedTask = await Task.WhenAny(receiveTask, delayTask);
            if (completedTask == delayTask)
            {
                return ReturnCode.UnknownFailure;   // 客户端响应超时
            }

            var result = await receiveTask;
            var data = result.Buffer;
            if (data == null || !ConfirmationMessage.TryParse(data, _rsaPrivateKeyXml, out var message))
            {
                return ReturnCode.UnknownFailure;   // 无法解析消息，说明是无效信息
            }

            // 未通过校验，说明是无效信息
            if (message.NumberReceived != numberSent)
            {
                return ReturnCode.UnknownFailure;
            }

            return message.ReturnCode;
        }

        /// <summary>
        /// 发送控制指令，并接收客户端返回的确认信息。若发生任何异常，返回<see cref="ReturnCode.UnknownFailure"/>
        /// </summary>
        /// <param name="address">客户端的IP地址</param>
        /// <param name="type">控制指令的类型</param>
        /// <param name="paras">参数列表（如果没有参数，传入空数组）</param>
        /// <returns>返回值</returns>
        public async Task<ReturnCode> SendInstructionAndReceiveAsync(IPAddress address, InstructionType type, byte[] paras)
        {
            const int timeout = 1000;
            try
            {
                var num = SendInstruction(Port, address, type, paras);
                var ret = await ReceiveResponse(timeout, num);
                return ret;
            }
            catch
            {
                return ReturnCode.UnknownFailure;
            }
        }

        public void Dispose()
        {
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
