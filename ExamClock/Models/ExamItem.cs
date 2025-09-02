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

namespace ExamClock.Models
{
    public class ExamItem
    {
        /// <summary>
        /// 初始化<see cref="ExamItem"/>对象
        /// </summary>
        /// <param name="subject">科目名称</param>
        /// <param name="beginTime">考试开始时间</param>
        /// <param name="duration">考试持续时长</param>
        /// <exception cref="Exception">科目名称不允许为空白</exception>
        public ExamItem(string subject, DateTime beginTime, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new Exception("科目名称不允许为空");
            }

            Subject = subject;
            BeginTime = beginTime;
            Duration = duration;
        }

        /// <summary>
        /// 科目名称
        /// </summary>
        private string _subject = "未知";
        public string Subject
        {
            get => _subject;
            set
            {
                if (value != null && !string.IsNullOrEmpty(value))
                {
                    _subject = value;
                }
            }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; } = new DateTime();

        /// <summary>
        /// 持续时长
        /// </summary>
        private TimeSpan _duration = TimeSpan.Zero;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value != TimeSpan.Zero && value != _duration)
                {
                    _duration = value;
                }
            }
        }

        public DateTime EndTime => BeginTime + Duration;

        /// <summary>
        /// 返回“HH:mm - HH:mm”格式的字符串，表示考试的开始时间和结束时间
        /// </summary>
        public string GetTimeString()
        {
            return $"{BeginTime:HH:mm} - {BeginTime + Duration:HH:mm}";
        }

        /// <summary>
        /// 尝试解析文本为<see cref="ExamItem"/>对象
        /// </summary>
        public static bool TryParse(string text, out ExamItem timeTable)
        {
            try
            {
                var parts = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4)
                {
                    timeTable = null;
                    return false;
                }
                var subject = parts[0];
                var beginTime = DateTime.Parse($"{parts[1]} {parts[2]}");
                var durationMinutes = int.Parse(parts[3]);
                timeTable = new ExamItem(subject, beginTime, TimeSpan.FromMinutes(durationMinutes));
                return true;
            }
            catch
            {
                timeTable = null;
                return false;
            }
        }
    }
}
