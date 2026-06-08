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

using ExamClock.Core.Enums;
using ExamClock.Models;
using ExamClock.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using ExamClock.Mvvm;
using System.Windows.Threading;
using ExamClock.Core.Constants;

namespace ExamClock.ViewModels
{
    class ClockViewModel : ViewModelBase, IDisposable
    {
        public ClockViewModel()
        {
            SetNoticeTimes(Configuration.TimeTable,
                Configuration.NoticeBeforeEnding,
                Configuration.ExamBeginningNotice == SoundType.ExamBeginning,
                Configuration.ExamEndingNotice == SoundType.ExamEnding);

            RefreshClock();
            SetEventNameAndTime();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.4)
            };
            _timer.Tick += (s, e) =>
            {
                try
                {
                    RefreshClock();         // 刷新时间显示
                    SetEventNameAndTime();  // 刷新考试项目和考试时间显示

                    var now = DateTime.Now;
                    for (int i = 0; i < NoticeTimes.Count; i++)
                    {
                        NoticeItem notice = NoticeTimes[i];
                        if (now >= notice.NoticeTime && now < notice.NoticeTime.AddSeconds(2))
                        {
                            // 处于提醒的时间内，播放提醒
                            ExamVoiceReminder.Instance.Play(notice.NoticeType);
                            NoticeTimes.RemoveAt(i);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Timer Tick 出现异常：{ex}");
                }
            };
            _timer.Start();
        }

        /// <summary>
        /// 定时器
        /// </summary>
        private DispatcherTimer _timer;

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
        /// 当前的显示模式
        /// </summary>
        public ClockDisplayMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertyChanged(nameof(ClockGridVisibility));
                    OnPropertyChanged(nameof(EnteringGridVisibility));
                }
            }
        }
        private ClockDisplayMode _mode;

        /// <summary>
        /// 时钟页面的可见性（模式切换时使用）
        /// </summary>
        public Visibility ClockGridVisibility => Mode == ClockDisplayMode.Clock ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 进场页面的可见性（模式切换时使用）
        /// </summary>
        public Visibility EnteringGridVisibility => Mode == ClockDisplayMode.EnteringRoom ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 考试项目名称
        /// </summary>
        public string EventNameText
        {
            get => _eventNameText;
            private set
            {
                if (value == null) value = "";
                SetProperty(ref _eventNameText, value);
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
                SetProperty(ref _eventTimeText, value);
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

        /// <summary>
        /// 考场号文本
        /// </summary>
        public string RoomNumberText
        {
            get
            {
                var number = Configuration.RoomNumber;
                if (number == 0)
                {
                    return "未知考场";
                }
                return number.ToString("0000");
            }
        }

        /// <summary>
        /// 距离考试开始的时间文本，仅在进场页面时显示
        /// </summary>
        public string TimeToBeginningText
        {
            get => _timeToBeginningText;
            set
            {
                SetProperty(ref _timeToBeginningText, value);
            }
        }
        private string _timeToBeginningText = "";

        private void SetEventNameAndTime()
        {
            var current = Configuration.GetCurrentItem();
            var next = Configuration.GetNextItem();
            if (current != null)
            {
                if (current.Subject == SpecialSubject.EnteringRoom)
                {
                    Mode = ClockDisplayMode.EnteringRoom;
                    EventNameText = next?.Subject ?? "--";
                    EventTimeText = "请考生有序进入考场";
                    if (next == null)
                    {
                        TimeToBeginningText = "--:--";
                        return;
                    }
                    TimeToBeginningText = (next.BeginTime - DateTime.Now).ToString(@"mm\:ss");
                    return;
                }
                EventNameText = current.Subject;
                EventTimeText = current.GetTimeString();
            }
            else if (next != null)
            {
                if (next.BeginTime.Date == DateTime.Now.Date)
                {
                    var minutes = (int)(next.BeginTime - DateTime.Now).TotalMinutes;
                    if (next.Subject == SpecialSubject.EnteringRoom)
                    {
                        EventNameText = "--";
                        EventTimeText = $"距离进场：{(minutes > 0 ? minutes.ToString() : "<1")}分钟";
                        return;
                    }
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
            Mode = ClockDisplayMode.Clock;
        }

        private void RefreshClock()
        {
            OnPropertyChanged(nameof(HourAngle));
            OnPropertyChanged(nameof(MinuteAngle));
            OnPropertyChanged(nameof(SecondAngle));
        }

        /// <summary>
        /// 初始化语音提醒列表
        /// </summary>
        /// <param name="sortedTimeTable">按时间升序排序且无重叠的考试时间表</param>
        /// <param name="noticeBeforeEnding">考试结束前提醒的类型</param>
        /// <param name="beginningNotice">是否启用开考铃</param>
        /// <param name="endingNotice">是否启用结束铃</param>
        /// <returns>设置成功，返回<see langword="true"/>；否则返回<see langword="false"/></returns>
        private bool SetNoticeTimes(List<ExamItem> sortedTimeTable, SoundType noticeBeforeEnding, bool beginningNotice, bool endingNotice)
        {
            _noticeTimes.Clear();
            if (sortedTimeTable == null)
            {
                return false;
            }
            if (sortedTimeTable.Count == 0)
            {
                return true;
            }

            // 设置收卷前提醒的时间
            TimeSpan beforeEnding;
            switch (noticeBeforeEnding)
            {
                case SoundType._15MinBeforeEnding:
                    beforeEnding = TimeSpan.FromMinutes(15);
                    break;
                case SoundType._10MinBeforeEnding:
                    beforeEnding = TimeSpan.FromMinutes(10);
                    break;
                case SoundType.None:
                    beforeEnding = TimeSpan.Zero;
                    break;
                default:
                    // 如果不是期望的值，直接返回
                    return false;
            }

            foreach (var item in sortedTimeTable)
            {
                // 考生入场铃
                if (item.Subject == SpecialSubject.EnteringRoom)
                {
                    if (item.BeginTime < DateTime.Now)
                    {
                        continue;
                    }
                    _noticeTimes.Add(new NoticeItem(item.BeginTime, SoundType.EnteringRoom));
                    continue;
                }

                // 开考铃
                if (item.BeginTime > DateTime.Now && beginningNotice)
                {
                    _noticeTimes.Add(new NoticeItem(item.BeginTime, SoundType.ExamBeginning));
                }

                // 收卷前铃
                DateTime noticeTime = item.EndTime - beforeEnding;
                if (beforeEnding != TimeSpan.Zero && noticeTime > DateTime.Now && item.Duration > beforeEnding)
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

        public void Dispose()
        {
            _timer?.Stop();
            _timer = null;
        }
    }
}