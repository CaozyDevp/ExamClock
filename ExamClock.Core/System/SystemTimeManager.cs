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
using System.Runtime.InteropServices;

namespace ExamClock.Core.System
{
    /// <summary>
    /// 使用协调世界时（UTC）设置系统时间
    /// </summary>
    public class SystemTimeManager
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime utcTime);

        public static bool SetSystemTime(DateTime utcTime)
        {
            var time = new SystemTime()
            {
                wYear = (ushort)utcTime.Year,
                wMonth = (ushort)utcTime.Month,
                wDay = (ushort)utcTime.Day,
                wDayOfWeek = (ushort)utcTime.DayOfWeek,
                wHour = (ushort)utcTime.Hour,
                wMinute = (ushort)utcTime.Minute,
                wSecond = (ushort)utcTime.Second,
                wMilliseconds = (ushort)utcTime.Millisecond
            };

            return SetSystemTime(ref time);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }
}
