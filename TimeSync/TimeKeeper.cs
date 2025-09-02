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
using System.Diagnostics;
using System.Net;

namespace TimeSync
{
    /// <summary>
    /// 这个类用于保存一个时间信息，会随时间的推移而增加
    /// </summary>
    public class TimeKeeper
    {
        private DateTime _baseTime;
        private Stopwatch _stopwatch = new Stopwatch();

        public bool IsRunning => _stopwatch.IsRunning;

        public TimeKeeper(TimeSpan offset)
        {
            SetTime(DateTime.Now.ToLocalTime() + offset);
        }

        public TimeKeeper(DateTime time)
        {
            SetTime(time);
        }

        public void SetTime(DateTime baseTime)
        {
            _baseTime = baseTime;

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }
            else
            {
                _stopwatch.Restart();
            }
        }

        /// <summary>
        /// 来源 IP 地址
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// 当前的时间
        /// </summary>
        public DateTime CurrentTime => _baseTime + _stopwatch.Elapsed;

        /// <summary>
        /// 考场号
        /// </summary>
        public ushort HostNumber { get; set; }
    }
}
