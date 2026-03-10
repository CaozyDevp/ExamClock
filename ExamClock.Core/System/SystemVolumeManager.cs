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
    public class SystemVolumeManager
    {
        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        /// <summary>
        /// 获取系统音量
        /// </summary>
        /// <returns>系统音量值，范围0~100</returns>
        public static int GetVolume()
        {
            waveOutGetVolume(IntPtr.Zero, out uint volume);
            ushort leftChannel = (ushort)(volume & 0xFFFF);
            ushort rightChannel = (ushort)((volume >> 16) & 0xFFFF);
            return (leftChannel + rightChannel) / 2 * 100 / 65536;
        }

        /// <summary>
        /// 设置系统音量
        /// </summary>
        /// <param name="volume">音量值，范围0~100</param>
        public static void SetVolume(int volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            uint actualVolume = (uint)(volume * 65536 / 100);
            uint combinedVolume = (actualVolume << 16) | actualVolume;
            waveOutSetVolume(IntPtr.Zero, combinedVolume);
        }
    }
}
