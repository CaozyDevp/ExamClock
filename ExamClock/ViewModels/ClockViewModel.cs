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
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace ExamClock.ViewModels
{
    class ClockViewModel : ViewModelBase
    {
        public ClockViewModel()
        {
            SetNoticeTimes(Configuration.TimeTable,
                Configuration.NoticeBeforeEnding,
                Configuration.ExamBeginningNotice == SoundType.ExamBeginning,
                Configuration.ExamEndingNotice == SoundType.ExamEnding);

            RefreshClock();
            SetEventNameAndTime();

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.4)
            };
            timer.Tick += (s, e) =>
            {
                RefreshClock();
                SetEventNameAndTime();

                // 如果没有设置提醒，则不进行通知
                if (Configuration.NoticeBeforeEnding == SoundType.None) return;

                var now = DateTime.Now;
                foreach (var notice in NoticeTimes)
                {
                    if (now < notice.NoticeTime || now >= notice.NoticeTime.AddSeconds(2)) continue;
                    new ExamVoiceReminder(notice.NoticeType).Play();
                    NoticeTimes.Remove(notice);
                    break; // 上面的集合在循环中被修改，所以需要退出当前循环
                    // 否则会抛出异常
                }

            };
            timer.Start();
        }

        /// <summary>
        /// 当前的时间
        /// </summary>
        private static DateTime Time => DateTime.Now;

        /// <summary>
        /// 时钟的角度
        /// </summary>
        public double HourAngle => (Time.Hour % 12.0 * 30.0) + (Time.Minute / 60.0 * 30.0);

        /// <summary>
        /// 分针的角度
        /// </summary>
        public double MinuteAngle => (Time.Minute * 6.0) + (Time.Second / 60.0 * 6.0);

        /// <summary>
        /// 秒针的角度
        /// </summary>
        public double SecondAngle => Time.Second * 6.0;


        /// <summary>
        /// 考试项目名称
        /// </summary>
        public string EventNameText
        {
            get => _eventNameText;
            private set
            {
                if (value == _eventNameText || value == null) return;
                _eventNameText = value;
                OnPropertyChanged();
            }
        }
        private string _eventNameText = "";

        /// <summary>
        /// 显示的时间
        /// </summary>
        public string EventTimeText
        {
            get => _eventTimeText;
            private set
            {
                if (value == _eventTimeText || value == null) return;
                _eventTimeText = value;
                OnPropertyChanged();
            }
        }
        private string _eventTimeText = "";

        /// <summary>
        /// 需要播放提醒的时间
        /// </summary>
        private List<NoticeItem> NoticeTimes
        {
            get => _noticeTimes;
            set
            {
                if (value == null || value.Count == 0) return;
                _noticeTimes = value;
            }
        }
        private List<NoticeItem> _noticeTimes = new List<NoticeItem>();


        private void SetEventNameAndTime()
        {
            var current = Configuration.GetCurrentItem();
            var next = Configuration.GetNextItem();
            if (current != null)
            {
                EventNameText = current.Subject;
                EventTimeText = current.GetTimeString();
            }
            else if (next != null)
            {
                if (next.BeginTime.Date == DateTime.Now.Date)
                {
                    var minutes = (int)(next.BeginTime - DateTime.Now).TotalMinutes;
                    EventNameText = "下一场  " + next.Subject;
                    EventTimeText = $"距离开考：{(minutes > 0 ? minutes.ToString() : "<1")}分钟";
                }
                else
                {
                    EventNameText = "暂无考试";
                    EventTimeText = "今天没有考试";
                }
            }
            else
            {
                EventNameText = "暂无考试";
                EventTimeText = "所有考试已结束";
            }
        }

        private void RefreshClock()
        {
            OnPropertyChanged(nameof(HourAngle));
            OnPropertyChanged(nameof(MinuteAngle));
            OnPropertyChanged(nameof(SecondAngle));
        }

        private bool SetNoticeTimes(List<ExamItem> sortedTimeTable, SoundType noticeBeforeEnding, bool beginningNotice, bool endingNotice)
        {
            _noticeTimes.Clear();
            if (sortedTimeTable == null || sortedTimeTable.Count == 0)
            {
                return false;
            }

            // 设置考试前提醒的分钟数
            int noticeBeforeEndingMinutes;
            switch (noticeBeforeEnding)
            {
                case SoundType._15MinBeforeEnding:
                    noticeBeforeEndingMinutes = 15;
                    break;
                case SoundType._10MinBeforeEnding:
                    noticeBeforeEndingMinutes = 10;
                    break;
                case SoundType.None:
                default:
                    // 如果没有设置提醒，则不添加任何提醒
                    return false;
            }

            foreach (var item in sortedTimeTable)
            {
                // 开考铃
                if (item.BeginTime > DateTime.Now && beginningNotice)
                {
                    _noticeTimes.Add(new NoticeItem(item.BeginTime, SoundType.ExamBeginning));
                }

                // 收卷前铃
                DateTime noticeTime = item.EndTime - TimeSpan.FromMinutes(noticeBeforeEndingMinutes);
                if (noticeTime > DateTime.Now && item.Duration > TimeSpan.FromMinutes(noticeBeforeEndingMinutes))
                {
                    _noticeTimes.Add(new NoticeItem(noticeTime, noticeBeforeEnding));
                }

                // 结束铃
                if (item.EndTime > DateTime.Now && endingNotice)
                {
                    _noticeTimes.Add(new NoticeItem(item.EndTime, SoundType.ExamEnding));
                }
            }

            return true;
        }
    }
}