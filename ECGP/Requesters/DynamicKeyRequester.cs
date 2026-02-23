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

using ECGP.Exceptions;
using ECGP.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace ECGP.Requesters
{
    public class DynamicKeyRequester : IDisposable
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

        public DynamicKeyRequester(int port, string rsaPrivateKeyXml)
        {
            _udpClient = new UdpClient();
            _port = port;
            _rsaPrivateKeyXml = rsaPrivateKeyXml;
        }

        /// <summary>
        /// 向指定的地址和端口发送请求
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="address">IP地址，只能是单播地址</param>
        /// <returns>发出消息的特征码</returns>
        private uint SendRequest(int port, IPAddress address)
        {
            var message = new DynamicKeyRequestMessage();
            var messageBytes = message.ToBytes();

            _udpClient.EnableBroadcast = false;

            var target = new IPEndPoint(address, port);
            _udpClient.Send(messageBytes, messageBytes.Length, target);

            return message.Number;
        }

        /// <summary>
        /// 接收客户端的响应，如果没有收到，返回<see cref="null"/>
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="numberSent">发出的请求消息中包含的特征码</param>
        private async Task<byte[]> ReceiveResponse(int timeout, uint numberSent)
        {
            var receiveTask = _udpClient.ReceiveAsync();
            var delayTask = Task.Delay(timeout);

            // 已经完成的任务，用于实现超时检测
            var completedTask = await Task.WhenAny(receiveTask, delayTask);
            if (completedTask == delayTask)
            {
                return null;
            }

            var result = await receiveTask;
            var data = result.Buffer;
            if (data == null || !DynamicKeyResponseMessage.TryParse(data, _rsaPrivateKeyXml, out var message))
            {
                return null;
            }

            // 未通过校验，说明是无效信息
            if (message.NumberReceived != numberSent)
            {
                return null;
            }
            return message.DynamicKey;
        }

        /// <summary>
        /// 发送请求，并接收客户端的响应
        /// </summary>
        /// <param name="address">客户端的IP地址</param>
        /// <returns>客户端返回的动态密钥。如果没有收到或者响应无效，返回null</returns>
        public async Task<byte[]> SendAndGetDynamicKeyAsync(IPAddress address)
        {
            const int timeout = 1000;
            try
            {
                var num = SendRequest(Port, address);
                var key = await ReceiveResponse(timeout, num);
                return key;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}