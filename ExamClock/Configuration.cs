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
using ExamClock.Models;
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
        /// 时间同步请求的端口：请求将被发送到这个端口；在这个端口接收请求
        /// </summary>
        public static int RequestPort => 25566;

        /// <summary>
        /// 时间同步响应的端口：响应将被发送到这个端口；在这个端口接收响应
        /// </summary>
        public static int ResponsePort => 25567;

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
        /// 考试时间表，在调用set访问器时会自动进行排序
        /// </summary>
        public static List<ExamItem> TimeTable
        {
            get
            {
                var list = new List<ExamItem>();
                var dataObj = Config.GetValue("TIMETABLE.items");
                if (dataObj.GetType() != typeof(Table))
                {
                    throw new Exception("Invalid type!");
                }
                var data = dataObj as Table;

                for (int i = 0; i < data.Count; i++)
                {
                    var subjectObj = data[i][0];
                    var beginTimeObj = data[i][1];
                    var durationObj = data[i][2];

                    if (subjectObj.GetType() != typeof(string) ||
                        beginTimeObj.GetType() != typeof(DateTime) ||
                        durationObj.GetType() != typeof(int))
                    {
                        throw new Exception("Invalid type!");
                    }

                    list.Add(new ExamItem((string)subjectObj,
                        (DateTime)beginTimeObj,
                        TimeSpan.FromMinutes((int)durationObj)));
                }

                return list;
            }
            set
            {
                var table = new Table();
                SortTimeTable(ref value);   // 排序
                foreach (var item in value)
                {
                    var tableItem = new TableItem(new List<object>
                    {
                        item.Subject,
                        item.BeginTime,
                        (int)item.Duration.TotalMinutes,
                    });
                    table.Add(tableItem);
                }
                Config.SetValue("TIMETABLE.items", table);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// 获取当前考试项，如果没有则返回<see langword="null"/>
        /// </summary>
        public static ExamItem GetCurrentItem()
        {
            if (TimeTable == null || TimeTable.Count == 0)
            {
                return null;
            }
            foreach (var item in TimeTable)
            {
                DateTime endTime = item.BeginTime + item.Duration;
                if (DateTime.Now >= item.BeginTime && DateTime.Now <= endTime)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取下一个考试项，如果没有则返回<see langword="null"/>
        /// </summary>
        public static ExamItem GetNextItem()
        {
            if (TimeTable == null || TimeTable.Count == 0)
            {
                return null;
            }
            foreach (var item in TimeTable)
            {
                if (DateTime.Now < item.BeginTime)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 对<see cref="ExamItem"/>列表按照时间由早到晚进行冒泡排序
        /// </summary>
        private static void SortTimeTable(ref List<ExamItem> timeTable)
        {
            if (timeTable == null)
            {
                return;
            }

            // 冒泡排序，将开始时间从小到大排列
            for (var i = 0; i < timeTable.Count - 1; i++)
            {
                for (var j = 0; j < timeTable.Count - i - 1; j++)
                {
                    if (timeTable[j].BeginTime <= timeTable[j + 1].BeginTime) continue;
                    ExamItem temp = timeTable[j];
                    timeTable[j] = timeTable[j + 1];
                    timeTable[j + 1] = temp;
                }
            }
        }

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
            catch
            {
                MessageBox.Show("配置文件加载失败，请检查文件是否“只读”！");
                return false;
            }

            MatchConfig(source, out var result);

            Config = result;

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
            object beginNotice = false, beforeEndingNotice = 0,
                   endingNotice = false, roomNumber = (ushort)0;

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

                source.TryGetValue($"TIMETABLE.{nameof(items)}", ref items);
            }

            target = new SpfItem()
            {
                Path = AppDomain.CurrentDomain.BaseDirectory + "config.spf",
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
            return LoadConfig(AppDomain.CurrentDomain.BaseDirectory + "config.spf");
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
            return SaveConfig(AppDomain.CurrentDomain.BaseDirectory + "config.spf");
        }
        #endregion
    }
}