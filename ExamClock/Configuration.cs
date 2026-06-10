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
using ExamClock.Core.Enums;
using Spf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ExamClock
{
    public static class Configuration
    {
        static Configuration()
        {
            if (!LoadConfig())
            {
                // 如果从指定的配置文件加载失败，使用默认配置
                LoadDefaultConfig();
            }
        }

        private static SpfItem Config
        {
            get; set;
        }

        #region Properties
        /// <summary>
        /// 程序配置文件的路径
        /// </summary>
        private static string ConfigPath => AppDomain.CurrentDomain.BaseDirectory + "config.spf";

        /// <summary>
        /// 时间同步请求的端口：请求将被发送到这个端口；在这个端口接收请求
        /// </summary>
        public static int TimeSyncPort => _timeSyncPort;
        private const int _timeSyncPort = 25566;

        /// <summary>
        /// 软件的版本号（从资源中获取）
        /// </summary>
        public static string Version => (string)Application.Current.Resources["AppVersion"];

        /// <summary>
        /// 考试结束前提醒
        /// </summary>
        public static SoundType NoticeBeforeEnding
        {
            get
            {
                switch (Config.GetValue("SETTINGS.beforeEndingNotice"))
                {
                    case 10:
                        return SoundType._10MinBeforeEnding;
                    case 15:
                        return SoundType._15MinBeforeEnding;
                    default:
                        NoticeBeforeEnding = SoundType.None;
                        SaveConfig();
                        return SoundType.None;
                }
            }
            set
            {
                int beforeEnd;
                switch (value)
                {
                    case SoundType._15MinBeforeEnding:
                        beforeEnd = 15;
                        break;
                    case SoundType._10MinBeforeEnding:
                        beforeEnd = 10;
                        break;
                    case SoundType.None:
                        beforeEnd = 0;
                        break;
                    default:
                        throw new Exception("Invalid sound type!");
                }
                Config.SetValue("SETTINGS.beforeEndingNotice", beforeEnd);
            }
        }

        /// <summary>
        /// 考试开始提醒
        /// </summary>
        public static SoundType ExamBeginningNotice
        {
            get
            {
                switch (Config.GetValue("SETTINGS.beginNotice"))
                {
                    case true:
                        return SoundType.ExamBeginning;
                    case false:
                        return SoundType.None;
                    default:
                        throw new Exception("Internal error!");
                }
            }
            set
            {
                bool isEnabled;
                switch (value)
                {
                    case SoundType.ExamBeginning:
                        isEnabled = true;
                        break;
                    case SoundType.None:
                        isEnabled = false;
                        break;
                    default:
                        throw new Exception("Invalid sound type!");
                }
                Config.SetValue("SETTINGS.beginNotice", isEnabled);
            }
        }

        /// <summary>
        /// 考试结束提醒
        /// </summary>
        public static SoundType ExamEndingNotice
        {
            get
            {
                switch (Config.GetValue("SETTINGS.endingNotice"))
                {
                    case true:
                        return SoundType.ExamEnding;
                    case false:
                        return SoundType.None;
                    default:
                        throw new Exception("Internal error!");
                }
            }
            set
            {
                bool isEnabled;
                switch (value)
                {
                    case SoundType.ExamEnding:
                        isEnabled = true;
                        break;
                    case SoundType.None:
                        isEnabled = false;
                        break;
                    default:
                        throw new Exception("Invalid sound type!");
                }
                Config.SetValue("SETTINGS.endingNotice", isEnabled);
            }
        }

        /// <summary>
        /// 考场号
        /// </summary>
        public static ushort RoomNumber
        {
            get => (ushort)Config.GetValue("SETTINGS.roomNumber");
            set => Config.SetValue("SETTINGS.roomNumber", value);
        }

        /// <summary>
        /// 是否允许集控
        /// </summary>
        public static bool AllowControl
        {
            get => (bool)Config.GetValue("SETTINGS.allowControl");
            set => Config.SetValue("SETTINGS.allowControl", value);
        }

        /// <summary>
        /// 考试时间表
        /// </summary>
        public static ExamSchedule Schedule
        {
            get;
        } = new ExamSchedule();
        #endregion

        #region Methods

        /// <summary>
        /// 从指定的配置文件加载配置。如果没有找到配置文件，按照默认配置加载
        /// </summary>
        /// <returns>如果文件不存在，返回<see langword="false"/></returns>
        public static bool LoadConfig(string configFile)
        {
            // 如果配置文件不存在，则返回false
            if (configFile == null || !File.Exists(configFile))
            {
                return false;
            }

            var source = new SpfItem();

            try
            {
                source.LoadFile(configFile);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("无法找到配置文件，已重新创建");
                return false;
            }
            catch
            {
                MessageBox.Show("配置文件可能损坏，无法读取。\n注意：原有的所有配置将被覆盖！");
                return false;
            }

            MatchConfig(source, out var result);
            Config = result;

            return ImportSchedule();
        }

        /// <summary>
        /// 将配置文件中的考试时间表导入到<see cref="Schedule"/>对象中
        /// </summary>
        /// <returns></returns>
        private static bool ImportSchedule()
        {
            var tableObj = Config.GetValue("TIMETABLE.items");
            var type = tableObj.GetType();
            if (type != null && type != typeof(Table))
            {
                return false;
            }
            if (!(tableObj is Table table))
            {
                return false;
            }
            Schedule.Import(table);
            return true;
        }

        /// <summary>
        /// 加载默认的配置
        /// </summary>
        public static void LoadDefaultConfig()
        {
            MatchConfig(null, out var result);
            Config = result;
        }

        /// <summary>
        /// 将提供的源<see cref="SpfItem"/>对象的有意义的属性值赋值给新的对象，
        /// 没有指定的属性使用默认值
        /// </summary>
        /// <param name="source">如果为<see langword="null"/>，则使用默认值</param>
        /// <param name="target"></param>
        public static void MatchConfig(SpfItem source, out SpfItem target)
        {
            // VERSION
            object prgm = "", file = "";

            // SETTINGS
            object beginNotice = false,
                   beforeEndingNotice = 0,
                   endingNotice = false,
                   roomNumber = (ushort)0,
                   allowControl = false;

            // TIMETABLE
            object items = new Table();

            if (source != null)
            {
                source.TryGetValue($"VERSION.{nameof(prgm)}", ref prgm);
                source.TryGetValue($"VERSION.{nameof(file)}", ref file);

                source.TryGetValue($"SETTINGS.{nameof(beginNotice)}", ref beginNotice);
                source.TryGetValue($"SETTINGS.{nameof(beforeEndingNotice)}", ref beforeEndingNotice);
                source.TryGetValue($"SETTINGS.{nameof(endingNotice)}", ref endingNotice);
                source.TryGetValue($"SETTINGS.{nameof(roomNumber)}", ref roomNumber);
                source.TryGetValue($"SETTINGS.{nameof(allowControl)}", ref allowControl);

                source.TryGetValue($"TIMETABLE.{nameof(items)}", ref items);
            }

            target = new SpfItem()
            {
                Path = ConfigPath,
                Configuration = new List<ConfigurationItem>
                {
                    new ConfigurationItem
                    {
                        Key = "VERSION",
                        Properties = new List<PropertyItem>
                        {
                            new PropertyItem(nameof(prgm), prgm),
                            new PropertyItem(nameof(file), file),
                        }
                    },
                    new ConfigurationItem
                    {
                        Key = "SETTINGS",
                        Properties = new List<PropertyItem>
                        {
                            new PropertyItem(nameof(beginNotice), beginNotice),
                            new PropertyItem(nameof(beforeEndingNotice), beforeEndingNotice),
                            new PropertyItem(nameof(endingNotice), endingNotice),
                            new PropertyItem(nameof(roomNumber), roomNumber),
                            new PropertyItem(nameof(allowControl), allowControl),
                        }
                    },
                    new ConfigurationItem
                    {
                        Key = "TIMETABLE",
                        Properties = new List<PropertyItem>
                        {
                            new PropertyItem(nameof(items), items)
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 从默认路径加载配置文件（程序所在目录下的config.spf文件）
        /// </summary>
        public static bool LoadConfig()
        {
            return LoadConfig(ConfigPath);
        }

        /// <summary>
        /// 保存配置到指定的文件路径
        /// </summary>
        /// <returns>如果保存失败，返回<see langword="false"/></returns>
        private static bool SaveConfig(string configFile)
        {
            // 如果配置文件路径为空，则返回false
            if (configFile == null)
            {
                return false;
            }
            try
            {
                if (Config.Path != configFile)
                {
                    Config.Path = configFile;
                }
                Config.SetValue("TIMETABLE.items", Schedule.Export());
                Config.SaveFile();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从默认路径保存配置文件（程序所在目录下的config.spf文件）
        /// </summary>
        public static bool SaveConfig()
        {
            return SaveConfig(ConfigPath);
        }

        /// <summary>
        /// 判断用户输入的考场号是否正确
        /// </summary>
        /// <param name="input">用户输入的考场号</param>
        /// <returns>如果正确，返回<see langword="null"/>，否则返回警告信息</returns>
        public static string JudgeRoomNumberInput(string input)
        {
            bool succeed = ushort.TryParse(input, out var roomNumber);
            if (!succeed || roomNumber > 9999)
            {
                return "请输入0~9999的整数";
            }
            return null;
        }
        #endregion
    }
}