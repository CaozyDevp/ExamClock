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

using ExamClock.Core;
using Spf;
using System;
using System.IO;
using System.Windows;

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

        /// <summary>
        /// 用户名
        /// </summary>
        public static string Username
        {
            get; set;
        }

        /// <summary>
        /// 考试日程
        /// </summary>
        public static ExamSchedule Schedule
        {
            get; private set;
        } = new ExamSchedule();
        #endregion

        /// <summary>
        /// 从文件加载时间表配置文件
        /// </summary>
        /// <returns>如果文件不存在，返回<see langword="false"/></returns>
        public static bool LoadTimeTable(string configFile)
        {
            // 如果配置文件不存在，则返回false
            if (configFile == null || !File.Exists(configFile))
            {
                return false;
            }

            try
            {
                string rawText = File.ReadAllText(configFile);
                Schedule.Import(Table.Parse(rawText));
            }
            catch
            {
                MessageBox.Show("配置文件可能损坏，无法读取。\n注意：原有的所有配置将被覆盖！");
                Schedule = new ExamSchedule();
                return false;
            }

            return true;
        }

        static Configuration()
        {
            const string defaultConfigFile = "timeTable.spf";
            if (!LoadTimeTable(Path.Combine(Environment.CurrentDirectory, defaultConfigFile)))
            {
                MessageBox.Show("无法找到配置文件");
            }
        }
    }
}