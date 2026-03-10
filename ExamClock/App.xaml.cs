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
using System.Diagnostics;
using System.Windows;
using TimeSync;

namespace ExamClock
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
            {
                MessageBox.Show("当前程序已在运行！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }
            InitTimeResponder();
        }

        private TimeSyncResponder TimeResponder { get; set; }

        private async void InitTimeResponder()
        {
            try
            {
                TimeResponder?.Stop();
                TimeResponder = null;

                TimeResponder = new TimeSyncResponder(() => { return Configuration.RoomNumber; }, Configuration.TimeSyncPort);
                await TimeResponder.StartAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"时间同步响应器启动失败，您将无法响应其他主机的时间同步请求！\n错误信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
