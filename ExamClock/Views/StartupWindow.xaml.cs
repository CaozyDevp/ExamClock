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
using System.Windows;

namespace ExamClock.Views
{
    /// <summary>
    /// StartupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();

            var app = App.Current as App;

            // 要求用户输入考场号
            while (Configuration.RoomNumber == 0)
            {
                var dialog = new UserInputWindow(Configuration.JudgeRoomNumberInput, "请设置考场号", "");
                if (dialog.ShowDialog() == false)
                {
                    Environment.Exit(0);
                }
                if (dialog.IsValid)
                {
                    Configuration.RoomNumber = Convert.ToUInt16(dialog.InputText);
                    try
                    {
                        Configuration.SaveConfig();
                    }
                    catch
                    {
                        MessageBox.Show("考场号保存失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            // 初始化时间同步响应器
            app.IsTimeResponderEnabled = true;

            // 如果公钥信息存在，再去初始化状态响应器和主响应器
            if (!string.IsNullOrEmpty(KeyManager.RsaPublicKeyXml))
            {
                // 如果不允许集控，则只启用状态响应器
                app.IsStatusResponderEnabled = true;
                if (Configuration.AllowControl)
                {
                    app.IsClientResponderEnabled = true;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
