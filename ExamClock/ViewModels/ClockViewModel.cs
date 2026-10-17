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
using System.Windows.Media;
using System.IO;
using ExamClock.Core.System;

namespace ExamClock.ViewModels
{
    class ClockViewModel : ViewModelBase, IDisposable
    {
        public ClockViewModel()
        {
            // 加载听力音频
            string path = "";
            try
            {
                path = Configuration.AudioFileSource;
                if (File.Exists(path))
                {
                    AudioPlayer = new MediaPlayer();
                    AudioPlayer.MediaOpened += (s, e) =>
                    {
                        OnPropertyChanged(nameof(HasAudio));
                        OnPropertyChanged(nameof(AudioPositionText));
                        OnPropertyChanged(nameof(AudioButtonVisibility));
                    };
                    AudioPlayer.MediaEnded += (s, e) =>
                    {
                        IsAudioPlaying = false;
                        AudioPlayer.Position = TimeSpan.Zero;
                        AudioPlayer.Pause();
                        OnPropertyChanged(nameof(AudioPositionText));
                    };
                    AudioPlayer.Open(new Uri(path, UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
            }

            SetNoticeTimes(Configuration.Schedule.ExamItems,
                Configuration.NoticeBeforeEnding,
                Configuration.ExamBeginningNotice == SoundType.ExamBeginning,
                Configuration.ExamEndingNotice == SoundType.ExamEnding);

            RefreshClock();
            SetEventNameAndTime();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.4)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                RefreshClock();         // 刷新时间显示
                SetEventNameAndTime();  // 刷新考试项目和考试时间显示

                var now = DateTime.Now;
                if (HasAudio)
                {
                    if (now < Configuration.AudioStartTime - TimeSpan.FromMinutes(20) ||
                        now > Configuration.AudioStartTime + AudioPlayer.NaturalDuration.TimeSpan + TimeSpan.FromMinutes(30))
                    {
                        AudioButtonVisibility = Visibility.Collapsed;
                        AudioStatusText = "";
                    }
                    else
                    {
                        AudioButtonVisibility = Visibility.Visible;
                        if (Configuration.AudioStartTime > DateTime.Now && !IsAudioPlaying)
                        {
                            AudioStatusText = $"将在{Configuration.AudioStartTime:hh:mm}播放听力";
                        }
                    }
                    if (now >= Configuration.AudioStartTime && now < Configuration.AudioStartTime.AddSeconds(2))
                    {
                        // 处于指定的听力播放时间内，播放听力
                        if (!IsAudioPlaying)
                        {
                            AudioPlayer.Play();
                            IsAudioPlaying = true;
                        }
                        OnPropertyChanged(nameof(AudioButtonVisibility));
                        return;
                    }
                    if (IsAudioPlaying)
                    {
                        OnPropertyChanged(nameof(AudioPositionText));
                    }
                }
                for (int i = 0; i < NoticeTimes.Count; i++)
                {
                    NoticeItem notice = NoticeTimes[i];
                    if (now >= notice.NoticeTime && now < notice.NoticeTime.AddSeconds(2))
                    {
                        // 如果听力正在播放，则不播放提醒
                        if (IsAudioPlaying)
                        {
                            NoticeTimes.RemoveAt(i);
                            return;
                        }
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
                _timer.Stop();
            }
        }


        #region Clock & Notice

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
            var current = Configuration.Schedule.Current;
            var next = Configuration.Schedule.Next;
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
                    var offset = next.BeginTime - DateTime.Now;
                    if (offset > TimeSpan.Zero)
                    {
                        TimeToBeginningText = $"{(int)offset.TotalMinutes:00}:{offset.Seconds:00}";
                    }
                    else
                    {
                        TimeToBeginningText = "00:00";
                    }
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
                if (beforeEnding != TimeSpan.Zero)
                {
                    DateTime noticeTime = item.EndTime - beforeEnding;
                    if (noticeTime > DateTime.Now && item.Duration > beforeEnding)
                    {
                        _noticeTimes.Add(new NoticeItem(noticeTime, noticeBeforeEnding));
                    }
                }

                // 结束铃
                if (item.EndTime > DateTime.Now && endingNotice)
                {
                    _noticeTimes.Add(new NoticeItem(item.EndTime, SoundType.ExamEnding));
                }
            }

            return true;
        }

        #endregion

        #region Mode

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

        #endregion

        #region Audio

        /// <summary>
        /// 音频播放器
        /// </summary>
        public MediaPlayer AudioPlayer { get; }

        /// <summary>
        /// 音频是否正在播放
        /// </summary>
        public bool IsAudioPlaying
        {
            get => _isAudioPlaying;
            set
            {
                if (value == _isAudioPlaying) return;

                _isAudioPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PauseVisibility));
                OnPropertyChanged(nameof(PlayVisibility));
                if (value)
                {
                    AudioStatusText = "正在播放听力";
                }
                else
                {
                    AudioStatusText = "";
                }
            }
        }
        private bool _isAudioPlaying = false;

        /// <summary>
        /// 是否有音频文件
        /// </summary>
        public bool HasAudio => AudioPlayer != null && AudioPlayer.Source != null && AudioPlayer.NaturalDuration.HasTimeSpan;

        /// <summary>
        /// 听力控制按钮的可见性
        /// </summary>
        public Visibility AudioButtonVisibility
        {
            get => _audioButtonVisibility;
            set
            {
                if (value == _audioButtonVisibility)
                    return;

                _audioButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        private Visibility _audioButtonVisibility = Visibility.Collapsed;

        public Visibility PauseVisibility => IsAudioPlaying ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PlayVisibility => IsAudioPlaying ? Visibility.Collapsed : Visibility.Visible;

        public int SystemVolume
        {
            get => SystemVolumeManager.GetVolume();
            set
            {
                SystemVolumeManager.SetVolume(value);
                OnPropertyChanged(nameof(SystemVolume));
            }
        }

        public string AudioPositionText
        {
            get
            {
                if (!HasAudio)
                {
                    return "00:00 / 00:00";
                }
                return $"{AudioPlayer.Position:mm\\:ss} / {AudioPlayer.NaturalDuration.TimeSpan:mm\\:ss}";
            }
        }

        public RelayCommand PlayAudioCommand => new RelayCommand((_) =>
        {
            if (!HasAudio)
            {
                return;
            }
            if (IsAudioPlaying)
            {
                AudioPlayer.Pause();
                IsAudioPlaying = false;
            }
            else
            {
                AudioPlayer.Play();
                IsAudioPlaying = true;
            }
        });

        public RelayCommand AudioForwardCommand => new RelayCommand((_) =>
        {
            if (!HasAudio || !IsAudioPlaying)
            {
                return;
            }
            var total = AudioPlayer.NaturalDuration.TimeSpan;
            var position = AudioPlayer.Position;
            var newposition = position + TimeSpan.FromSeconds(5);
            if (newposition > total)
            {
                newposition = total;
            }
            AudioPlayer.Position = newposition;
        });

        public RelayCommand AudioBackwardCommand => new RelayCommand((_) =>
        {
            if (!HasAudio || !IsAudioPlaying)
            {
                return;
            }
            var position = AudioPlayer.Position;
            var newposition = position - TimeSpan.FromSeconds(5);
            if (newposition < TimeSpan.Zero)
            {
                newposition = TimeSpan.Zero;
            }
            AudioPlayer.Position = newposition;
        });

        #endregion

        #region Info

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

        /// <summary>
        /// 听力考试音频状态的文本
        /// </summary>
        public string AudioStatusText
        {
            get => _audioStatusText;
            set
            {
                SetProperty(ref _audioStatusText, value);
            }
        }
        private string _audioStatusText = "";

        #endregion

        public void Dispose()
        {
            _timer?.Stop();
            _timer = null;

            AudioPlayer?.Stop();
            AudioPlayer?.Close();
        }
    }
}