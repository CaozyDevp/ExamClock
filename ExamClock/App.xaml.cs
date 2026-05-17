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
using ECGP.Responders;
using ExamClock.Core.System;
using ExamClock.Core.Enums;
using ExamClock.Views;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using TimeSync;
using System.Text;

namespace ExamClock
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 禁止重复运行
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
            {
                MessageBox.Show("当前程序已在运行！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }
        }

        /// <summary>
        /// 客户端当前状态。通过此访问器，可以实现状态的切换
        /// </summary>
        public ClientStatus Status
        {
            get
            {
                bool matched = false;
                foreach (var winItem in Current.Windows)
                {
                    if (winItem is SettingsWindow setWin)
                    {
                        return ClientStatus.Editing;
                    }
                    else if (winItem is ClockWindow)
                    {
                        return ClientStatus.Examining;
                    }
                    else if (winItem is StartupWindow)
                    {
                        matched = true;
                    }
                }
                return matched ? ClientStatus.Exiting : ClientStatus.Unknown;
            }
            set
            {
                switch (value)
                {
                    case ClientStatus.Exiting:
                        foreach (var winItem in Current.Windows)
                        {
                            if (winItem is Window win)
                            {
                                if (!(win is StartupWindow))
                                {
                                    win.Close();
                                }
                            }
                        }
                        break;
                    case ClientStatus.Editing:
                        foreach (var winItem in Current.Windows)
                        {
                            if (winItem is SettingsWindow setWin)
                            {
                                setWin.Show();
                            }
                            else if (winItem is StartupWindow)
                            {
                                continue;
                            }
                            else if (winItem is Window win)
                            {
                                win.Close();
                            }
                        }
                        new SettingsWindow().ShowDialog();
                        break;
                    case ClientStatus.Examining:
                        foreach (var winItem in Current.Windows)
                        {
                            if (winItem is ClockWindow clockWin)
                            {
                                clockWin.Show();
                                return;
                            }
                        }
                        new ClockWindow().Show();
                        break;
                }
            }
        }

        private TimeSyncResponder TimeResponder { get; set; }
        private StatusResponder StatusResponder { get; set; }
        private ClientResponder ClientResponder { get; set; }

        public bool IsTimeResponderEnabled
        {
            get => TimeResponder != null && TimeResponder.IsRunning;
            set
            {
                if (IsTimeResponderEnabled == value)
                {
                    return;
                }

                if (value)
                {
                    InitTimeResponder();
                }
                else
                {
                    TimeResponder?.Stop();
                }
            }
        }
        public bool IsStatusResponderEnabled
        {
            get => StatusResponder != null && StatusResponder.IsRunning;
            set
            {
                if (IsStatusResponderEnabled == value)
                {
                    return;
                }
                if (value)
                {
                    InitStatusResponder();
                }
                else
                {
                    StatusResponder?.Stop();
                }
            }
        }
        public bool IsClientResponderEnabled
        {
            get => ClientResponder != null && ClientResponder.IsRunning;
            set
            {
                if (IsClientResponderEnabled == value)
                {
                    return;
                }
                if (value)
                {
                    InitClientResponder();
                }
                else
                {
                    ClientResponder?.Stop();
                }
            }
        }

        /// <summary>
        /// 初始化时间同步响应器
        /// </summary>
        private async void InitTimeResponder()
        {
            try
            {
                TimeResponder?.Stop();
                TimeResponder = null;

                TimeResponder = new TimeSyncResponder(() => { return Configuration.RoomNumber; }, Configuration.TimeSyncPort);
                await TimeResponder.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"时间同步响应器启动失败，您将无法响应其他主机的时间同步请求！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 初始化状态信息响应器。在公钥改变时需要重新初始化以更新公钥。
        /// </summary>
        private async void InitStatusResponder()
        {
            try
            {
                StatusResponder = new StatusResponder(
                    () => { return (byte)SystemVolumeManager.GetVolume(); },
                    () => { return Configuration.RoomNumber; },
                    () => { return Status; },
                    () => { return Configuration.GetScheduleHash(); },
                    () =>
                    {
                        return new NoticeConfig()
                        {
                            EnableBeginning = Configuration.ExamBeginningNotice == Core.Enums.SoundType.ExamBeginning,
                            EnableEnding = Configuration.ExamEndingNotice == Core.Enums.SoundType.ExamEnding,
                            BeforeEnding = Configuration.NoticeBeforeEnding
                        };
                    },
                    KeyManager.RsaPublicKeyXml
                );
                await StatusResponder.StartAsync();
            }
            catch (Exception ex)
            {
                StatusResponder?.Stop();
                StatusResponder = null;
                MessageBox.Show($"状态信息响应器启动失败，管理端将无法搜索到您！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 初始化客户端主响应器。在公钥改变时需要重新初始化以更新公钥。
        /// </summary>
        private async void InitClientResponder()
        {
            try
            {
                ClientResponder = new ClientResponder(KeyManager.RsaPublicKeyXml, ConductInstructionAsync);
                await ClientResponder.StartAsync();
            }
            catch (Exception ex)
            {
                ClientResponder?.Stop();
                ClientResponder = null;
                MessageBox.Show($"客户端主响应器启动失败，管理端将无法正常对您集控！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 执行管理端发来的指令的异步方法
        /// </summary>
        /// <param name="type">指令类型</param>
        /// <param name="paras">参数列表。根据指令的定义，参数及其含义有所不同。</param>
        /// <param name="source">管理端</param>
        /// <returns>执行结果的状态码</returns>
        private async Task<ReturnCode> ConductInstructionAsync(InstructionType type, byte[] paras, IPEndPoint source)
        {
            try
            {
                switch (type)
                {
                    case InstructionType.ModeSwitch:
                        if (paras[0] == 0)
                            Status = ClientStatus.Exiting;
                        else if (paras[0] == 1)
                            Status = ClientStatus.Examining;
                        else
                            return ReturnCode.InvalidInstruction;
                        break;

                    case InstructionType.SetSystemVolume:
                        byte volume = paras[0];
                        if (volume > 100)
                            return ReturnCode.InvalidInstruction;
                        SystemVolumeManager.SetVolume(volume);
                        break;

                    case InstructionType.SyncTimeWithController:
                        try
                        {
                            var requester = new TimeSyncRequester(Configuration.TimeSyncPort);
                            var keeper = await requester.UnicastAndGetTimeKeeperAsync(source.Address, Configuration.TimeSyncPort, Configuration.RoomNumber);
                            SystemTimeManager.SetSystemTime(keeper.CurrentTime.ToUniversalTime());
                            Reload();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"时间同步失败！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;

                    case InstructionType.SetRoomNumber:
                        Configuration.RoomNumber = BitConverter.ToUInt16(paras, 0);
                        Configuration.SaveConfig();
                        Reload();
                        break;

                    case InstructionType.SetNotice:
                        const byte examBeginningFlag = 0b_0000_0001;
                        const byte examEndingFlag = 0b_0000_0010;
                        const byte _10MinFlag = 0b_0000_0100;
                        const byte _15MinFlag = 0b_0000_1100;
                        var noticeFlags = paras[0];
                        Configuration.ExamBeginningNotice = ((noticeFlags & examBeginningFlag) == examBeginningFlag) ? SoundType.ExamBeginning : SoundType.None;
                        Configuration.ExamEndingNotice = ((noticeFlags & examEndingFlag) == examEndingFlag) ? SoundType.ExamEnding : SoundType.None;
                        switch (noticeFlags & _15MinFlag)
                        {
                            case _10MinFlag:
                                Configuration.NoticeBeforeEnding = SoundType._10MinBeforeEnding;
                                break;
                            case _15MinFlag:
                                Configuration.NoticeBeforeEnding = SoundType._15MinBeforeEnding;
                                break;
                            default:
                                Configuration.NoticeBeforeEnding = SoundType.None;
                                break;
                        }
                        Configuration.SaveConfig();
                        Reload();
                        break;

                    case InstructionType.PushSchedule:
                        string scheduleStr = Encoding.UTF8.GetString(paras);
                        var timetable = Spf.Table.Parse(scheduleStr);
                        if (Configuration.SetTimeTable(timetable))
                        {
                            Configuration.SaveConfig();
                        }
                        else
                        {
                            throw new Exception("Set schedule failed due to unknown reason.");
                        }
                        Reload();
                        break;

                    default:
                        return ReturnCode.InvalidInstruction;
                }
                return ReturnCode.Success;
            }
            catch
            {
                return ReturnCode.UnknownFailure;
            }
        }

        /// <summary>
        /// 用于在执行指令后重新加载界面
        /// </summary>
        private void Reload()
        {
            var currentStatus = Status;
            if (currentStatus == ClientStatus.Exiting)
            {
                return;
            }
            Status = ClientStatus.Exiting;
            Status = currentStatus;
        }
    }
}