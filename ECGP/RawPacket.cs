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

using System.Net;

namespace ECGP
{
    public struct RawPacket
    {
        /// <summary>
        /// 接收到的数据
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// 数据来源
        /// </summary>
        IPEndPoint Source { get; set; }

        public RawPacket(byte[] data, IPEndPoint source)
        {
            Data = data;
            Source = source;
        }
    }
}
