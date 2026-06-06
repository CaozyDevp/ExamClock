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
using ECGP.Enums;
using ExamClock.Mvvm;
using ExamClock.Admin.Models;
using ExamClock.Admin.Views;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ExamClock.Admin.ViewModels
{
    /// <remarks>
    /// 使用前需要调用Init方法进行初始化
    /// </remarks>
    public class RoomControlViewModel : ViewModelBase, IDisposable
    {
        private readonly SolidColorBrush _backgroundGreen = new SolidColorBrush(Color.FromRgb(233, 255, 241));
        private readonly SolidColorBrush _backgroundRed = new SolidColorBrush(Color.FromRgb(255, 233, 233));

        private readonly SolidColorBrush _foregroundGreen = new SolidColorBrush(Color.FromRgb(0, 140, 52));
        private readonly SolidColorBrush _foregroundRed = new SolidColorBrush(Color.FromRgb(140, 0, 0));

        private Timer _timer;
        public RoomControlViewModel()
        {

        }

        /// <summary>
        /// 更新时间显示
        /// </summary>
        private void UpdateTime()
        {
            OnPropertyChanged(nameof(TimeText));
            OnPropertyChanged(nameof(LocalTimeText));
        }

        /// <summary>
        /// 客户端与本机的时间差
        /// </summary>
        public TimeSpan TimeOffset { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// 客户端的状态
        /// </summary>
        public ClientStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusSwitchButtonBackground));
                    OnPropertyChanged(nameof(StatusSwitchButtonForeground));
                    OnPropertyChanged(nameof(StatusSwitchButtonText));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }
        private ClientStatus _status;

        /// <summary>
        /// 考场号
        /// </summary>
        public ushort RoomNumber
        {
            get => _roomNumber;
            set
            {
                if (SetProperty(ref _roomNumber, value))
                {
                    OnPropertyChanged(nameof(RoomNumberText));
                }
            }
        }
        private ushort _roomNumber;

        /// <summary>
        /// 显示的考场号字符串
        /// </summary>
        public string RoomNumberText
        {
            get => $"# {RoomNumber:0000}";
        }

        /// <summary>
        /// 考场状态字符串
        /// </summary>
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case ClientStatus.Exiting:
                        return "[已退出]";
                    case ClientStatus.Examining:
                        return "[考试中]";
                    case ClientStatus.Editing:
                        return "[编辑中]";
                    default:
                        return "[未知状态]";
                }
            }
        }

        /// <summary>
        /// 显示的IP地址字符串
        /// </summary>
        public IPAddress Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }
        private IPAddress _ip;

        /// <summary>
        /// 状态切换按钮的文本
        /// </summary>
        public string StatusSwitchButtonText
        {
            get => Status == ClientStatus.Examining ? "结束考试" : "开始考试";
        }

        /// <summary>
        /// 当前时间的字符串
        /// </summary>
        public string TimeText
        {
            get
            {
                string offsetStr = string.Empty;
                double offset = TimeOffset.TotalSeconds;
                if (offset >= 0)
                {
                    offsetStr += "+";
                }

                if (Math.Abs(offset) > 100)
                {
                    offsetStr += "偏差过大";
                }
                else
                {
                    offsetStr += $"{offset:0.0}s";
                }
                return $"{(DateTime.Now + TimeOffset):yyyy/MM/dd HH:mm:ss}（{offsetStr}）";
            }
        }

        /// <summary>
        /// 语音提醒配置字符串
        /// </summary>
        public string NoticeConfigText
        {
            get => _noticeConfigText;
            set => SetProperty(ref _noticeConfigText, value);
        }
        private string _noticeConfigText;

        /// <summary>
        /// 音量字符串
        /// </summary>
        public string VolumeText
        {
            get => _volumeText;
            set => SetProperty(ref _volumeText, value);
        }
        private string _volumeText;

        /// <summary>
        /// 本机本地时间字符串
        /// </summary>
        public string LocalTimeText
        {
            get
            {
                return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            }
        }

        /// <summary>
        /// 音量控制条的值，范围0-100
        /// </summary>
        public int VolumeBarValue
        {
            get => _volumeBarValue;
            set => SetProperty(ref _volumeBarValue, value);
        }
        private int _volumeBarValue;

        /// <summary>
        /// 用户输入的考场号字符串
        /// </summary>
        public string UserInputRoomNumberText
        {
            get => _userInputRoomNumberText;
            set => SetProperty(ref _userInputRoomNumberText, value);
        }
        private string _userInputRoomNumberText;

        /// <summary>
        /// 状态切换按钮的前景色
        /// </summary>
        public SolidColorBrush StatusSwitchButtonForeground
        {
            get => Status == ClientStatus.Examining ? _foregroundRed : _foregroundGreen;
        }

        /// <summary>
        /// 状态切换按钮的背景色
        /// </summary>
        public SolidColorBrush StatusSwitchButtonBackground
        {
            get => Status == ClientStatus.Examining ? _backgroundRed : _backgroundGreen;
        }

        public string[] BeginNoticeItems => _beginNoticeItems;
        private readonly string[] _beginNoticeItems = { "无", "开考铃" };
        public string[] EndNoticeItems => _endNoticeItems;
        private readonly string[] _endNoticeItems = { "无", "结束铃" };
        public string[] BeforeEndingNoticeItems => _beforeEndingNoticeItems;
        private readonly string[] _beforeEndingNoticeItems = { "无提醒", "收卷前10分钟", "收卷前15分钟" };

        /// <summary>
        /// 选中的开考铃下标
        /// </summary>
        public int BeginNoticeIndex
        {
            get => _beginNoticeIndex;
            set => SetProperty(ref _beginNoticeIndex, value);
        }
        private int _beginNoticeIndex = 0;

        /// <summary>
        /// 选中的结束铃下标
        /// </summary>
        public int EndNoticeIndex
        {
            get => _endNoticeIndex;
            set => SetProperty(ref _endNoticeIndex, value);
        }
        private int _endNoticeIndex = 0;

        /// <summary>
        /// 选中的结束前提醒下标
        /// </summary>
        public int BeforeEndingNoticeIndex
        {
            get => _beforeEndingNoticeIndex;
            set => SetProperty(ref _beforeEndingNoticeIndex, value);
        }
        private int _beforeEndingNoticeIndex = 0;

        /// <summary>
        /// 切换状态命令
        /// </summary>
        public ICommand SwitchStatusCommand => new RelayCommand(async (_) =>
        {
            try
            {
                var privateKey = GetPrivateKeyXmlAndUpdateUsername();

                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                byte code = Status == ClientStatus.Examining ? (byte)0 : (byte)1;
                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.ModeSwitch, new byte[] { code });

                if (result == ReturnCode.Success)
                {
                    MessageBox.Show("状态切换成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    Status = Status == ClientStatus.Examining ? ClientStatus.Exiting : ClientStatus.Examining;
                }
                else
                {
                    MessageBox.Show("状态切换失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("状态切换过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 推送日程配置命令
        /// </summary>
        public ICommand PushScheduleCommand => new RelayCommand(async (_) =>
        {
            try
            {
                var privateKey = GetPrivateKeyXmlAndUpdateUsername();
                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string scheduleStr = Configuration.GetScheduleString(); // 日程配置的SPF字符串
                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.PushSchedule, Encoding.UTF8.GetBytes(scheduleStr));

                if (result == ReturnCode.Success)
                {
                    MessageBox.Show("日程配置推送成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("日程配置推送失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("日程配置推送过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 同步时间命令
        /// </summary>
        public ICommand SyncTimeCommand => new RelayCommand(async (_) =>
        {
            try
            {
                var privateKey = GetPrivateKeyXmlAndUpdateUsername();

                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.SyncTimeWithController, new byte[] { });

                if (result == ReturnCode.Success)
                {
                    MessageBox.Show("时间同步成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    TimeOffset = TimeSpan.Zero;
                }
                else
                {
                    MessageBox.Show("时间同步失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("时间同步过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 设置考场号命令
        /// </summary>
        public ICommand SetRoomNumberCommand => new RelayCommand(async (_) =>
        {
            try
            {
                if (!ushort.TryParse(UserInputRoomNumberText, out ushort roomNumber) || roomNumber > 9999)
                {
                    MessageBox.Show("考场号输入有误！必须在0-9999之间", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var privateKey = GetPrivateKeyXmlAndUpdateUsername();

                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.SetRoomNumber, BitConverter.GetBytes(roomNumber));

                if (result == ReturnCode.Success)
                {
                    MessageBox.Show("考场号设置成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    RoomNumber = roomNumber;
                }
                else
                {
                    MessageBox.Show("考场号设置失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("考场号设置过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 设置音量命令
        /// </summary>
        public ICommand SetVolumeCommand => new RelayCommand(async (_) =>
        {
            try
            {
                var privateKey = GetPrivateKeyXmlAndUpdateUsername();

                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.SetSystemVolume, new byte[] { (byte)VolumeBarValue });

                if (result == ReturnCode.Success)
                {
                    MessageBox.Show("音量设置成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    VolumeText = $"{VolumeBarValue}%";
                }
                else
                {
                    MessageBox.Show("音量设置失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("音量设置过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 设置提醒命令
        /// </summary>
        public ICommand SetNoticeCommand => new RelayCommand(async (_) =>
        {
            // data字节：0b_0000_DCBA
            // A置1=开启开考铃；B置1=开启收卷铃；C置1=开启结束前提醒；D置1=15分钟提醒，0=10分钟提醒
            byte data = 0;
            if (BeginNoticeIndex != 0)
            {
                data |= 0b_0000_0001;
            }
            if (EndNoticeIndex != 0)
            {
                data |= 0b_0000_0010;
            }
            switch (BeforeEndingNoticeIndex)
            {
                case 1:
                    data |= 0b_0000_0100;
                    break;
                case 2:
                    data |= 0b_0000_1100;
                    break;
            }

            try
            {
                var privateKey = GetPrivateKeyXmlAndUpdateUsername();

                if (privateKey == null)
                {
                    MessageBox.Show("身份验证失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var instructor = new InstructionClient(privateKey);
                var result = await instructor.SendInstructionAsync(new IPEndPoint(Ip, Configuration.ControllingPort), InstructionType.SetNotice, new byte[] { data });

                if (result == ReturnCode.Success)
                {
                    // 语音提醒配置
                    NoticeConfigText = string.Empty;
                    if (BeginNoticeIndex != 0)
                    {
                        NoticeConfigText += "开考 ";
                    }
                    switch (BeforeEndingNoticeIndex)
                    {
                        case 1:
                            NoticeConfigText += "10' ";
                            break;
                        case 2:
                            NoticeConfigText += "15' ";
                            break;
                    }
                    if (EndNoticeIndex != 0)
                    {
                        NoticeConfigText += "结束";
                    }

                    MessageBox.Show("提醒设置成功", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("提醒设置失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("提醒设置过程中发生错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// 初始化ViewModel，设置显示信息并启动更新时间的定时器
        /// </summary>
        /// <param name="info">获取到的考场信息</param>
        /// <param name="timeOffset">与本机的时间偏差</param>
        public void Init(RoomInfo info, TimeSpan timeOffset)
        {
            InitDisplay(info, timeOffset);
            _timer = new Timer(_ => UpdateTime(), null, 0, 1000);
        }

        private void InitDisplay(RoomInfo info, TimeSpan timeOffset)
        {
            TimeOffset = timeOffset;
            Status = info.Status;
            RoomNumber = info.RoomNumber;
            Ip = info.IP;
            VolumeText = $"{info.Volume}%";
            VolumeBarValue = info.Volume;
            UserInputRoomNumberText = info.RoomNumber.ToString("0000");

            // 语音提醒配置
            NoticeConfigText = string.Empty;
            if (info.IsExamBeginNoticeEnabled)
            {
                NoticeConfigText += "开考 ";
                BeginNoticeIndex = 1;
            }
            switch (info.NoticeBeforeEndingType)
            {
                case Core.Enums.SoundType._10MinBeforeEnding:
                    NoticeConfigText += "10' ";
                    BeforeEndingNoticeIndex = 1;
                    break;
                case Core.Enums.SoundType._15MinBeforeEnding:
                    NoticeConfigText += "15' ";
                    BeforeEndingNoticeIndex = 2;
                    break;
            }
            if (info.IsExamEndNoticeEnabled)
            {
                NoticeConfigText += "收卷";
                EndNoticeIndex = 1;
            }
        }

        /// <summary>
        /// 通过用户输入的用户名和密码，获取私钥xml字符串，并将用户名保存在配置类中
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>私钥xml字符串。如果获取失败，返回null</returns>
        private string GetPrivateKeyXmlAndUpdateUsername()
        {
            var username = Configuration.Username;
            if (!GetPasswordInput(ref username, out string password))
            {
                return null;
            }
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                return null;
            }
            Configuration.Username = username;

            var privateKey = KeyManager.GetKeyXml(Configuration.Username, password);
            return privateKey;
        }

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
            if (dialog.ShowDialog() == true)
            {
                username = dialog.UsernameString;
                password = dialog.PasswordString;
                return true;
            }

            password = string.Empty;
            return false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
