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

using ExamClock.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TimeSync;

namespace ExamClock.Views.UserControls
{
    /// <summary>
    /// HostTimeBox.xaml 的交互逻辑
    /// </summary>
    public partial class HostTimeBox : UserControl, INotifyPropertyChanged
    {
        public HostTimeBox()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            timer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(DisplayedTime));
            };
            timer.Start();

            Unloaded += (s, e) =>
            {
                timer.Stop();
                timer = null;
            };
        }

        /// <summary>
        /// 显示的时间
        /// </summary>
        public TimeKeeper TimeKeeper
        {
            get => (TimeKeeper)GetValue(TimeKeeperProperty);
            set
            {
                SetValue(TimeKeeperProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(DisplayedTime);
            }
        }
        public static readonly DependencyProperty TimeKeeperProperty =
            DependencyProperty.Register(nameof(TimeKeeper), typeof(TimeKeeper), typeof(HostTimeBox));

        /// <summary>
        /// 控件中显示的时间字符串
        /// </summary>
        public string DisplayedTime
        {
            get
            {
                if (TimeKeeper == null)
                    return "未知时间";
                return TimeKeeper.CurrentTime.ToString("yyyy/MM/dd\nHH:mm:ss");
            }
        }

        /// <summary>
        /// 主机号（考场号）
        /// </summary>
        public string HostName
        {
            get => (string)GetValue(HostNameProperty);
            set
            {
                SetValue(HostNameProperty, value);
                OnPropertyChanged();
            }
        }
        public static readonly DependencyProperty HostNameProperty =
            DependencyProperty.Register(nameof(HostName), typeof(string), typeof(HostTimeBox));

        /// <summary>
        /// 来源的IP地址
        /// </summary>
        public IPAddress Source
        {
            get => (IPAddress)GetValue(SourceProperty);
            set
            {
                SetValue(SourceProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IpString));
            }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(IPAddress), typeof(HostTimeBox), new PropertyMetadata(IPAddress.Any));

        /// <summary>
        /// 来源IP地址的字符串
        /// </summary>
        public string IpString
        {
            get => Source.ToString();
        }

        private bool SetSystemTime(TimeKeeper keeper)
        {
            try
            {
                var dateStr = TimeKeeper.CurrentTime.ToString("yyyy-MM-dd");
                var timeStr = TimeKeeper.CurrentTime.ToString("HH:mm:ss");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C date {dateStr} && time {timeStr}",
                        Verb = "runas", // 请求管理员权限
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public ICommand SetSystemTimeCommand => new RelayCommand(execute =>
        {
            var result = MessageBox.Show($"确定与 [{HostName}] 的时间同步吗？", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if (SetSystemTime(TimeKeeper))
            {
                MessageBox.Show("设置成功！");
            }
            else
            {
                MessageBox.Show("设置失败！请尝试以管理员身份运行或手动设置");
            }
        });

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

