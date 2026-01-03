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
using ExamClock.Views;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ExamClock.ViewModels
{
    internal class StartupViewModel : ViewModelBase
    {
        public StartupViewModel()
        {
            RefreshExamInfo();


            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                RefreshExamInfo();
            };
            timer.Start();
        }


        private void RefreshExamInfo()
        {
            var currentItem = Configuration.GetCurrentItem();
            var nextItem = Configuration.GetNextItem();

            var nowTime = DateTime.Now;

            if (currentItem != null)
            {
                var endTime = currentItem.BeginTime + currentItem.Duration;
                ExamDateText = $"当前考试：{currentItem.Subject}，还有{(int)(endTime - nowTime).TotalMinutes}分钟结束";
            }
            else if (nextItem != null)
            {
                // 如果下一场考试在今天
                if (nextItem.BeginTime.Date == nowTime.Date)
                {
                    // 距离下一场考试的分钟数
                    int minutes = (int)(nextItem.BeginTime - nowTime).TotalMinutes;

                    if (minutes < 1)
                    {
                        ExamDateText = $"下一场：{nextItem.Subject}，即将开始";
                    }
                    else
                    {
                        ExamDateText = $"下一场：{nextItem.Subject}，还有{minutes}分钟开始";
                    }
                }
                // 如果下一场考试不在今天
                else
                {
                    ExamDateText = $"考试将在 {nextItem.BeginTime.Date.ToShortDateString()} 开始";
                }
            }
            else
            {
                ExamDateText = "没有考试安排";
            }
            OnPropertyChanged(nameof(ExamDateText));
        }

        private string _examDateText;
        public string ExamDateText
        {
            get => _examDateText;
            set
            {
                _examDateText = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowInfoCommand => new RelayCommand(execute =>
            {
                var version = (string)Application.Current.Resources["AppVersion"];
                var publish = (string)Application.Current.Resources["PublishDate"];
                var author = (string)Application.Current.Resources["Author"];
                var account = (string)Application.Current.Resources["Account"];

                // 显示关于软件的信息
                MessageBox.Show($"版本：{version}\n发行：{publish}\n开发：{author}({account})\n" +
                    "开源：本软件基于 GNU GPL v3.0 协议开源",
                    "关于软件", MessageBoxButton.OK, MessageBoxImage.Information);
            });

        public ICommand ShowSettingsWindowCommand => new RelayCommand(execute =>
        { new SettingsWindow().ShowDialog(); });

        public ICommand StartExamCommand => new RelayCommand(execute =>
        {
            var clockWindow = new ClockWindow();
            var startupWindow = Application.Current.MainWindow as StartupWindow;

            // 关闭时钟窗口的事件处理器
            void ClosedHandler(object sender, EventArgs e)
            {
                clockWindow.Closed -= ClosedHandler;    // 取消订阅，避免内存泄漏
                startupWindow?.Show();                  // 显示启动窗口
            }

            // 现在：隐藏启动窗口，并显示时钟窗口
            startupWindow?.Hide();
            clockWindow.Show();

            clockWindow.Closed += ClosedHandler;
        });
    }
}
