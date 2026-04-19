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

namespace ExamClockInstaller
{
    public static class Configuration
    {
        /// <summary>
        /// 程序安装的目标路径，所需文件将被复制到该目录下
        /// </summary>
        public static string TargetPath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// 是否自动安装，即不需要用户确认
        /// </summary>
        public static bool AutoInstall { get; } = true;

        /// <summary>
        /// 在安装完成后是否自动关闭
        /// </summary>
        public static bool AutoExit { get; } = true;

        /// <summary>
        /// 安装完成后是否自动运行
        /// 如果此项为true，将运行<see cref="ExeRelativePath"/>指定的程序
        /// </summary>
        public static bool AutoRun { get; } = true;

        /// <summary>
        /// 是否创建桌面快捷方式
        /// </summary>
        public static bool CreateDesktopLnk { get; } = true;

        /// <summary>
        /// 安装后需要运行的可执行文件的相对路径
        /// 仅当<see cref="AutoRun"/>为true时有效
        /// </summary>
        public static string ExeRelativePath { get; } = "ExamClock.exe";
    }
}
