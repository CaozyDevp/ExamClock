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

using ExamClock.Enums;
using System.Net;

namespace ECGP
{
    /// <summary>
    /// 考场信息
    /// </summary>
    public class RoomInfo
    {
        /// <summary>
        /// 考场号
        /// </summary>
        public ushort RoomNumber { get; set; }

        /// <summary>
        /// 考场IP地址
        /// </summary>
        public IPAddress IP { get; set; }

        /// <summary>
        /// 日程配置的哈希值
        /// </summary>
        public byte[] ScheduleHash { get; set; }

        /// <summary>
        /// 考场当前状态
        /// </summary>
        public ClientStatus Status { get; set; }

        /// <summary>
        /// 考场音量大小（0~100）
        /// </summary>
        public byte Volume { get; set; }

        /// <summary>
        /// 是否开启了开考铃声
        /// </summary>
        public bool IsExamBeginNoticeEnabled { get; set; }

        /// <summary>
        /// 是否开启了结束铃声
        /// </summary>
        public bool IsExamEndNoticeEnabled { get; set; }

        /// <summary>
        /// 结束前铃声的类型（0 = 10分钟，1 = 15分钟）
        /// </summary>
        public SoundType NoticeBeforeEndingType { get; set; }
    }
}
