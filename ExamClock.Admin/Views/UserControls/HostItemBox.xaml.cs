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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ECGP;
using ExamClock.Core.Enums;
using TimeSync;

namespace ExamClock.Admin.Views.UserControls
{
    /// <summary>
    /// HostItemBox.xaml 的交互逻辑
    /// </summary>
    public partial class HostItemBox : UserControl, INotifyPropertyChanged, IDisposable
    {
        public HostItemBox()
        {
            InitializeComponent();
            _timer.Tick += UpdateTimeDisplay;
            _timer.Start();
        }

        #region Interface Implementations
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= UpdateTimeDisplay;
        }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// 考场信息
        /// </summary>
        public RoomInfo RoomInfo
        {
            get => (RoomInfo)GetValue(RoomInfoProperty);
            set => SetValue(RoomInfoProperty, value);
        }
        private static readonly DependencyProperty RoomInfoProperty =
            DependencyProperty.Register(nameof(RoomInfo), typeof(RoomInfo), typeof(HostItemBox),
                new PropertyMetadata(OnRoomInfoChanged));

        /// <summary>
        /// 考场时间
        /// </summary>
        public TimeKeeper RoomTime
        {
            get => (TimeKeeper)GetValue(RoomTimeProperty);
            set => SetValue(RoomTimeProperty, value);
        }
        private static readonly DependencyProperty RoomTimeProperty =
            DependencyProperty.Register(nameof(RoomTime), typeof(TimeKeeper), typeof(HostItemBox));

        /// <summary>
        /// 配置是否正确
        /// </summary>
        public bool IsCorrect
        {
            get => (bool)GetValue(IsCorrectProperty);
            set => SetValue(IsCorrectProperty, value);
        }
        private static readonly DependencyProperty IsCorrectProperty =
            DependencyProperty.Register(nameof(IsCorrect), typeof(bool), typeof(HostItemBox), new PropertyMetadata(false, OnIsCorrectChanged));
        #endregion

        #region Properties
        /// <summary>
        /// 考场号字符串
        /// </summary>
        public string RoomNumberString
        {
            get => $"# {RoomInfo?.RoomNumber.ToString("0000")}";
        }

        /// <summary>
        /// IP地址字符串
        /// </summary>
        public string IpString
        {
            get => RoomInfo?.IP?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 音量百分比字符串
        /// </summary>
        public string VolumeString
        {
            get => $"{RoomInfo?.Volume}%";
        }

        /// <summary>
        /// 状态信息字符串
        /// </summary>
        public string StatusString
        {
            get
            {
                if (RoomInfo == null)
                {
                    return "未知状态";
                }
                switch (RoomInfo.Status)
                {
                    case ClientStatus.Examining:
                        return "考试中";
                    case ClientStatus.Exiting:
                        return "已退出";
                    case ClientStatus.Editing:
                        return "正在修改";
                    case ClientStatus.Unknown:
                    default:
                        return "未知状态";
                }
            }
        }

        /// <summary>
        /// 状态文本的颜色
        /// </summary>
        public SolidColorBrush StatusBrush
        {
            get
            {
                if (RoomInfo == null)
                {
                    return (SolidColorBrush)Resources["InactiveRed"];
                }
                switch (RoomInfo.Status)
                {
                    case ClientStatus.Examining:
                        return (SolidColorBrush)Resources["ActiveGreen"];
                    case ClientStatus.Exiting:
                    case ClientStatus.Editing:
                    case ClientStatus.Unknown:
                    default:
                        return (SolidColorBrush)Resources["InactiveRed"];
                }
            }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public SolidColorBrush BackgroundBrush
        {
            get
            {
                if (IsCorrect)
                {
                    return (SolidColorBrush)Resources["BlueBackground"];
                }
                else
                {
                    return (SolidColorBrush)Resources["RedBackground"];
                }
            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        public SolidColorBrush BorderColorBrush
        {
            get
            {
                if (IsCorrect)
                {
                    return (SolidColorBrush)Resources["BlueBorder"];
                }
                else
                {
                    return (SolidColorBrush)Resources["RedBorder"];
                }
            }
        }

        /// <summary>
        /// 考场时间字符串
        /// </summary>
        public string TimeString
        {
            get
            {
                if (RoomTime == null)
                {
                    return "未知时间";
                }
                return RoomTime.CurrentTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 语音提醒开启状态的字符串
        /// </summary>
        public string NoticeString
        {
            get
            {
                if (RoomInfo == null)
                {
                    return "未知状态";
                }
                string noticeSettings = "";
                if (RoomInfo.IsExamBeginNoticeEnabled) noticeSettings += "开考 ";
                if (RoomInfo.NoticeBeforeEndingType == SoundType._10MinBeforeEnding) noticeSettings += "10' ";
                else if (RoomInfo.NoticeBeforeEndingType == SoundType._15MinBeforeEnding) noticeSettings += "15' ";
                if (RoomInfo.IsExamEndNoticeEnabled) noticeSettings += "收卷";
                return noticeSettings;
            }
        }
        #endregion

        /// <summary>
        /// 用于更新时间显示
        /// </summary>
        private DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        private void UpdateTimeDisplay(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TimeString));
        }

        private static void OnRoomInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is HostItemBox box)) return;

            box.OnPropertyChanged(nameof(RoomNumberString));
            box.OnPropertyChanged(nameof(IpString));
            box.OnPropertyChanged(nameof(VolumeString));
            box.OnPropertyChanged(nameof(StatusString));
            box.OnPropertyChanged(nameof(StatusBrush));
            box.OnPropertyChanged(nameof(NoticeString));
        }

        private static void OnIsCorrectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is HostItemBox box)) return;

            box.OnPropertyChanged(nameof(BackgroundBrush));
            box.OnPropertyChanged(nameof(BorderColorBrush));
        }

    }
}
