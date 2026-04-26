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

using ECGP.Enums;
using ECGP.Requesters;
using System.Net;
using System.Threading.Tasks;

namespace ExamClock.Admin.Models
{
    public class InstructionClient
    {
        // 指令发送流程
        // 1-- 向客户端单播动态密钥请求消息（IP地址已预先通过状态响应消息获得）
        // 2-- 接收到客户端响应的动态密钥，有效期5s
        // 3-- 将使用动态密钥加密的控制消息单播给客户端
        // 4-- 接收客户端返回的确认消息

        /// <summary>
        /// RSA私钥的XML字符串
        /// </summary>
        public string RsaPrivateKeyXml { get; set; }

        /// <summary>
        /// 动态密钥响应、指令确认消息响应的超时时间
        /// </summary>
        public const int timeout = 2000;

        /// <summary>
        /// 向指定的客户端发送控制指令，并接收返回的确认消息
        /// </summary>
        /// <param name="target">目标客户端主机</param>
        /// <param name="type">控制指令的类型</param>
        /// <param name="paras">控制指令附带的参数列表</param>
        /// <returns>客户端返回的值。如果没有收到或任何一个环节失败，返回null</returns>
        public async Task<ReturnCode?> SendInstructionAsync(IPEndPoint target, InstructionType type, byte[] paras)
        {
            try
            {
                var requestKeyTask = RequestDynamicKeyAsync(target);
                var timeoutTask = Task.Delay(timeout);
                var completedTask = await Task.WhenAny(requestKeyTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    return null;
                }
                var key = await requestKeyTask;

                using (var sender = new InstructionSender(target.Port, key, RsaPrivateKeyXml))
                {
                    var instructTask = sender.SendInstructionAndReceiveAsync(target.Address, type, paras);
                    timeoutTask = Task.Delay(timeout);
                    completedTask = await Task.WhenAny(instructTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        return null;
                    }
                    return await instructTask;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 向客户端请求动态密钥
        /// </summary>
        /// <param name="target">目标客户端主机</param>
        /// <returns>请求到的动态密钥。如果失败，返回null</returns>
        private async Task<byte[]> RequestDynamicKeyAsync(IPEndPoint target)
        {
            using (var requester = new DynamicKeyRequester(target.Port, RsaPrivateKeyXml))
            {
                var response = await requester.SendAndGetDynamicKeyAsync(target.Address);
                if (response != null && response.Length != 0)
                {
                    return response;
                }
                else
                {
                    return null;
                }
            }
        }

        public InstructionClient(string rsaPrivateKeyXml)
        {
            RsaPrivateKeyXml = rsaPrivateKeyXml;
        }
    }
}
