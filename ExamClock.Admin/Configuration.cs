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
using System.IO;
using System.Windows;

namespace ExamClock.Admin
{
    public static class Configuration
    {
        #region Properties

        /// <summary>
        /// 密钥文件的路径
        /// </summary>
        private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key");

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
        /// 被加密的RSA私钥Xml，使用AES算法ECB模式加密，密钥为用户名和密码以下划线连缀拼合后ASCII的SHA-512值
        /// </summary>
        public static byte[] EncryptedKey { get; set; }

        #endregion

        static Configuration()
        {
            if (!LoadConfigFromDefaultFile())
            {
                MessageBox.Show("配置文件加载失败！");
            }
        }

        /// <summary>
        /// 从文件中加载配置
        /// </summary>
        /// <param name="path">配置文件的路径</param>
        /// <returns>是否加载成功</returns>
        public static bool LoadConfigFromFile(string path)
        {
            if (!File.Exists(path)) return false;

            try
            {
                var text = File.ReadAllBytes(path);
                EncryptedKey = text;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从默认配置文件加载配置
        /// </summary>
        /// <returns>是否加载成功</returns>
        public static bool LoadConfigFromDefaultFile()
        {
            return LoadConfigFromFile(ConfigPath);
        }

        /// <summary>
        /// 将配置信息写入文件
        /// </summary>
        /// <param name="path">配置文件的路径</param>
        /// <returns>是否写入成功</returns>
        public static bool SaveConfigToFile(string path)
        {
            try
            {
                File.WriteAllBytes(path, EncryptedKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将配置信息写入默认配置文件
        /// </summary>
        /// <returns>是否写入成功</returns>
        public static bool SaveConfigToDefaultFile()
        {
            return SaveConfigToFile(ConfigPath);
        }
    }
}