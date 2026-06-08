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
using ExamClock.Mvvm;
using ExamClock.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TimeSync;

namespace ExamClock.ViewModels
{
    internal class ManualTimeSyncViewModel : ViewModelBase
    {
        /// <summary>
        /// 显示时间信息的控件列表
        /// </summary>
        public ObservableCollection<UIElement> SyncItemElements { get; set; } = new ObservableCollection<UIElement>()
        {
            new TextBlock()
            {
                Text = "点击“重新搜索”以搜索主机",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
            }
        };

        /// <summary>
        /// 是否可以查找主机（即广播请求报文）
        /// </summary>
        public bool CanFind
        {
            get => _canFind;
            set
            {
                _canFind = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FindButtonText));

                if (_canFind == true)
                {
                    return;
                }
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1.2)
                };
                timer.Tick += (s, e) =>
                {
                    CanFind = true;
                    timer.Stop();
                    timer = null;
                };
                timer.Start();
            }
        }
        private bool _canFind = true;

        public string FindButtonText => CanFind ? "重新搜索" : "搜索中...";

        /// <summary>
        /// 查找局域网中的可用主机，并显示
        /// </summary>
        /// <param name="timeSyncPort">用于时间同步的端口</param>
        /// <param name="roomNumber">本机的考场号</param>
        private async Task FindHostAndShow(int timeSyncPort, ushort roomNumber)
        {
            if (SyncItemElements == null)
            {
                SyncItemElements = new ObservableCollection<UIElement>();
            }
            SyncItemElements.Clear();

            List<TimeKeeper> keepers = new List<TimeKeeper>();

            try
            {
                var requester = new TimeSyncRequester(timeSyncPort);
                keepers = await requester.BroadcastAndGetTimeKeepersAsync(timeSyncPort, roomNumber);
            }
            catch
            {
                MessageBox.Show($"发送请求失败！");
            }

            foreach (var keeper in keepers)
            {
                var textbox = new HostTimeBox
                {
                    TimeKeeper = keeper,
                    Source = keeper.Address,
                    HostName = $"{keeper.HostNumber.ToString("0000")}考场",
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                SyncItemElements.Add(textbox);
            }

            if (keepers.Count == 0)
            {
                SyncItemElements.Add(new TextBlock()
                {
                    Text = "未找到可用主机\n点击“重新搜索”以重试",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                });
            }

            OnPropertyChanged(nameof(SyncItemElements));
        }

        public ICommand FindHostsCommand
        {
            get => new RelayCommand(async execute =>
            {
                CanFind = false;
                await FindHostAndShow(Configuration.TimeSyncPort, Configuration.RoomNumber);
            }, canExecute =>
            {
                return CanFind;
            });
        }
    }
}
