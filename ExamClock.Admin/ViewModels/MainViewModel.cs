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

using ECGP;
using ECGP.Requesters;
using ExamClock.Admin.Commands;
using ExamClock.Admin.Views;
using ExamClock.Admin.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TimeSync;

namespace ExamClock.Admin.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// 应有的总考场数（由配置文件定义）
        /// </summary>
        public ushort ExpectedTotalRooms
        {
            get => _expectedTotalRooms;
            set
            {
                if (_expectedTotalRooms == value) return;
                _expectedTotalRooms = value;
                OnPropertyChanged(nameof(ExpectedTotalRooms));
            }
        }
        private ushort _expectedTotalRooms;

        /// <summary>
        /// 实际的总考场数（在线考场数）
        /// </summary>
        public ushort ActualTotalRooms
        {
            get => _actualTotalRooms;
            set
            {
                if (_actualTotalRooms != value) return;
                _actualTotalRooms = value;
                OnPropertyChanged(nameof(ActualTotalRooms));
            }
        }
        private ushort _actualTotalRooms;

        /// <summary>
        /// 本机时间字符串（"本机时间：yyyy/MM/dd HH:mm:ss"）
        /// </summary>
        public string LocalTimeString
        {
            get => DateTime.Now.ToString("本机时间：yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 正确配置的考场数
        /// </summary>
        public ushort RightConfiguredRooms
        {
            get => _rightConfiguredRooms;
            set
            {
                if (_rightConfiguredRooms == value) return;
                _rightConfiguredRooms = value;
                OnPropertyChanged(nameof(RightConfiguredRooms));
            }
        }
        private ushort _rightConfiguredRooms;

        /// <summary>
        /// 错误配置的考场数
        /// </summary>
        public ushort WrongConfiguredRooms
        {
            get => _wrongConfiguredRooms;
            set
            {
                if (_wrongConfiguredRooms == value) return;
                _wrongConfiguredRooms = value;
                OnPropertyChanged(nameof(WrongConfiguredRooms));
            }
        }
        private ushort _wrongConfiguredRooms;

        /// <summary>
        /// 时间与本机同步（±1s）的考场数
        /// </summary>
        public ushort TimeSyncedRooms
        {
            get => _timeSyncedRooms;
            set
            {
                if (_timeSyncedRooms == value) return;
                _timeSyncedRooms = value;
                OnPropertyChanged(nameof(TimeSyncedRooms));
            }
        }
        private ushort _timeSyncedRooms;

        /// <summary>
        /// 时间偏离本机（1~10s）的考场数
        /// </summary>
        public ushort TimeWanderedRooms
        {
            get => _timeWanderedRooms;
            set
            {
                if (_timeWanderedRooms == value) return;
                _timeWanderedRooms = value;
                OnPropertyChanged(nameof(TimeWanderedRooms));
            }
        }
        private ushort _timeWanderedRooms;

        /// <summary>
        /// 时间与本机明显偏离（>10s）的考场数
        /// </summary>
        public ushort TimeWrongRooms
        {
            get => _timeWrongRooms;
            set
            {
                if (_timeWrongRooms == value) return;
                _timeWrongRooms = value;
                OnPropertyChanged(nameof(TimeWrongRooms));
            }
        }
        private ushort _timeWrongRooms;

        /// <summary>
        /// 事件标题（正在进行/即将进行）
        /// </summary>
        public string EventTitle
        {
            get => _isEventContinuing ? "正在进行" : "下一场";
        }
        private bool _isEventContinuing = false;

        /// <summary>
        /// 正在进行/即将进行的考试名称
        /// </summary>
        public string EventName
        {
            get => _eventName;
            set
            {
                if (_eventName == value) return;
                _eventName = value;
                OnPropertyChanged(nameof(EventName));
            }
        }
        private string _eventName = "--";

        /// <summary>
        /// 事件时间字符串（格式：HH:mm）
        /// </summary>
        public string EventTimeString
        {
            get => _eventTimeString;
            set
            {
                if (_eventTimeString == value) return;
                _eventTimeString = value;
                OnPropertyChanged(nameof(EventTimeString));
            }
        }
        private string _eventTimeString = "--:--";

        /// <summary>
        /// 管理权限字符串
        /// </summary>
        public string PermissionString
        {
            get => _permissionString;
            set
            {
                if (_permissionString == value) return;
                _permissionString = value;
                OnPropertyChanged(nameof(PermissionString));
            }
        }
        private string _permissionString = "--";

        /// <summary>
        /// 自动登出倒计时字符串（格式：HH:mm）
        /// </summary>
        public string LogoutTimeString
        {
            get => _logoutTimeString;
            set
            {
                if (_logoutTimeString == value) return;
                _logoutTimeString = value;
                OnPropertyChanged(nameof(LogoutTimeString));
            }
        }
        private string _logoutTimeString = "--:--";

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => Configuration.Username;
            set
            {
                Configuration.Username = value ?? string.Empty;
            }
        }

        /// <summary>
        /// 在主窗体上显示的考场相关控件
        /// </summary>
        public ObservableCollection<UIElement> HostElements
        {
            get => _hostElements;
            private set
            {
                if (value == _hostElements) return;
                _hostElements = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged();
            }
        }
        private ObservableCollection<UIElement> _hostElements = new ObservableCollection<UIElement>();

        private DispatcherTimer _timer;

        public MainViewModel()
        {
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(LocalTimeString));
        }

        /// <summary>
        /// 刷新命令（重新获取考场信息）
        /// </summary>
        public ICommand RefreshCommand => new RelayCommand(async (execute) =>
        {
            try
            {
                string username = Username;
                if (!GetPasswordInput(ref username, out string password))
                {
                    return;
                }
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
                {
                    return;
                }
                Username = username;

                var privateKey = KeyManager.GetKeyXml(Username, password);
                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 获取考场信息
                List<RoomInfo> rooms;
                using (var requester = new DetectionRequester(Configuration.DetectingPort, privateKey))
                {
                    rooms = await requester.BroadcastAndGetRoomInfos();
                }

                // 获取时间信息
                List<TimeKeeper> times;
                using (var requester = new TimeSyncRequester(Configuration.TimeSyncPort))
                {
                    times = await requester.BroadcastAndGetTimeKeepersAsync(Configuration.TimeSyncPort, 0);
                }

                // 将时间与考场信息按IP进行匹配
                List<TimeKeeper> sortedTimes = new List<TimeKeeper>();
                for (int i = 0; i < rooms.Count; i++)
                {
                    foreach (var time in times)
                    {
                        if (time.Address == rooms[i].IP)
                        {
                            sortedTimes.Add(time);
                            break;
                        }
                    }
                    if (sortedTimes.Count < i + 1)
                    {
                        sortedTimes.Add(null);
                    }
                }

                // 在UI上显示考场信息
                ShowHosts(rooms, sortedTimes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新考场信息出现错误！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 推送命令
        /// </summary>
        public ICommand PushCommand => new RelayCommand(excute =>
        {

        });

        /// <summary>
        /// 打开设置窗口命令
        /// </summary>
        public ICommand SettingsCommand => new RelayCommand(excute =>
        {

        });

        /// <summary>
        /// 显示关于信息命令
        /// </summary>
        public ICommand ShowInfoCommand => new RelayCommand(excute =>
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

        /// <summary>
        /// 弹窗获取用户输入的用户名和密码
        /// </summary>
        /// <param name="username">
        /// 以 ref 方式传入的用户名参数，其初始值将显示给用户，并在用户提交后被更新为对话框中输入的用户名。
        /// </param>
        /// <param name="password">用户输入的密码</param>
        /// <returns>是否成功获取。如果为false，说明用户未提交，直接关闭了窗口</returns>
        private bool GetPasswordInput(ref string username, out string password)
        {
            var dialog = new PasswordInputWindow(string.IsNullOrEmpty(username) ? string.Empty : username);
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    username = dialog.UsernameString;
                    password = dialog.PasswordString;
                    return true;
                }

                password = string.Empty;
                return false;
            }
            finally
            {
                dialog.Close();
            }
        }

        /// <summary>
        /// 在UI上显示考场信息
        /// </summary>
        /// <param name="rooms">考场列表</param>
        /// <param name="times">考场时间，要求与考场按顺序一一对应</param>
        private void ShowHosts(List<RoomInfo> rooms, List<TimeKeeper> times)
        {
            if (rooms.Count != times.Count)
            {
                throw new ArgumentException("The count of rooms must equal to that of times.");
            }

            HostElements.Clear();

            for (int i = 0; i < rooms.Count; i++)
            {
                var element = new HostItemBox()
                {
                    RoomInfo = rooms[i],
                    RoomTime = times[i],
                    Margin = new Thickness(4),
                    IsCorrect = false,  // [TODO] 这里日后再实现判断逻辑
                };
                HostElements.Add(element);
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer?.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }
        }
    }
}
