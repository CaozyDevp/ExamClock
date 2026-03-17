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

namespace ExamClock.Admin
{
    public static class Configuration
    {
        #region Properties
        /// <summary>
        /// 时间同步请求的端口：请求将被发送到这个端口；在这个端口接收请求
        /// </summary>
        public static int TimeSyncPort => _timeSyncPort;
        private const int _timeSyncPort = 25566;

        /// <summary>
        /// 存在探测端口：将存在探测请求发送到客户端的这个端口
        /// </summary>
        public static int DetectingPort => _detectingPort;
        private const int _detectingPort = 25585;

        /// <summary>
        /// 控制端口：将控制指令等报文发送到客户端的这个端口
        /// </summary>
        public static int ControllingPort => _controllingPort;
        private const int _controllingPort = 25584;

        #endregion
    }
}