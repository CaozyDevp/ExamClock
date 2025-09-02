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
            if (currentItem != null)
            {
                var endTime = currentItem.BeginTime + currentItem.Duration;
                ExamDateText = $"当前考试：{currentItem.Subject}，还有{(int)(endTime - DateTime.Now).TotalMinutes}分钟结束";
            }
            else if (nextItem != null)
            {
                ExamDateText = nextItem.BeginTime.Date == DateTime.Now.Date ?
                    $"下一场：{nextItem.Subject}，还有{(int)(nextItem.BeginTime - DateTime.Now).TotalMinutes}分钟开始" :
                    $"考试将在 {nextItem.BeginTime.Date.ToShortDateString()} 开始";
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
            new ClockWindow().Show();
        });
    }
}
