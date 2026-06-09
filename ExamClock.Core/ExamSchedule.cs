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

using Spf;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ExamClock.Core
{
    public class ExamSchedule
    {
        /// <summary>
        /// 考试项目列表
        /// </summary>
        public List<ExamItem> ExamItems => _examItems;
        private List<ExamItem> _examItems = new List<ExamItem>();

        /// <summary>
        /// 获取当前正在进行的考试项目，如果没有则返回null
        /// </summary>
        public ExamItem Current
        {
            get
            {
                var index = CurrentIndex;
                if (index < 0)
                {
                    return null;
                }
                return _examItems[index];
            }
        }

        /// <summary>
        /// 获取当前正在进行考试项目的索引，如果没有则返回-1
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                if (ExamItems == null || ExamItems.Count == 0)
                {
                    return -1;
                }
                for (int i = 0; i < ExamItems.Count; i++)
                {
                    ExamItem item = ExamItems[i];
                    var endTime = item.BeginTime + item.Duration;
                    var now = DateTime.Now;
                    if (now >= item.BeginTime && now <= endTime)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// 获取下一个即将开始的考试项目，如果没有则返回null
        /// </summary>
        public ExamItem Next
        {
            get
            {
                if (ExamItems == null || ExamItems.Count == 0)
                {
                    return null;
                }
                foreach (var item in ExamItems)
                {
                    if (DateTime.Now < item.BeginTime)
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 向考试时间表中添加一个考试项目
        /// </summary>
        /// <param name="item"></param>
        public void AddExamItem(ExamItem item)
        {
            _examItems.Add(item);
            SortExamItems(ref _examItems);
        }

        /// <summary>
        /// 删除考试时间表中指定索引处的考试项目
        /// </summary>
        /// <param name="index">元素索引</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void RemoveExamItemAt(int index)
        {
            if (index < 0 || index >= _examItems.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            _examItems.RemoveAt(index);
        }

        /// <summary>
        /// 使用新的考试项目列表替换原有的
        /// </summary>
        /// <param name="items">考试项目列表</param>
        public void Import(List<ExamItem> items)
        {
            _examItems = new List<ExamItem>(items);
            SortExamItems(ref _examItems);
        }

        /// <summary>
        /// 导入考试项目列表
        /// </summary>
        /// <param name="timeTable">由SPF解析的时间表</param>
        /// <returns></returns>
        public bool Import(Table timeTable)
        {
            var list = new List<ExamItem>();
            try
            {
                for (int i = 0; i < timeTable.Count; i++)
                {
                    var subjectObj = timeTable[i][0];
                    var beginTimeObj = timeTable[i][1];
                    var durationObj = timeTable[i][2];

                    if (!(subjectObj is string &&
                        beginTimeObj is DateTime &&
                        durationObj is int))
                    {
                        throw new Exception("Invalid type!");
                    }

                    list.Add(new ExamItem((string)subjectObj,
                        (DateTime)beginTimeObj,
                        TimeSpan.FromMinutes((int)durationObj)));
                }
                SortExamItems(ref list);
                _examItems = list;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将考试项目列表导出为SPF表
        /// </summary>
        /// <returns></returns>
        public Table Export()
        {
            var table = new Table();
            foreach (var item in ExamItems)
            {
                var tableItem = new TableItem(new List<object>
                    {
                        item.Subject,
                        item.BeginTime,
                        (int)item.Duration.TotalMinutes,
                    });
                table.Add(tableItem);
            }
            return table;
        }

        /// <summary>
        /// 对<see cref="ExamItem"/>列表按照时间由早到晚进行冒泡排序
        /// </summary>
        private static void SortExamItems(ref List<ExamItem> timeTable)
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
        /// 获取日程表的MD5哈希值
        /// </summary>
        /// <returns></returns>
        public byte[] GetScheduleHash()
        {
            using (var md5 = MD5.Create())
            {
                var table = Export();
                var bytes = Encoding.UTF8.GetBytes((table as Table).ToString());
                return md5.ComputeHash(bytes);
            }
        }
    }
}
