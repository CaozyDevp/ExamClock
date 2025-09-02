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
using System.Windows.Input;

namespace ExamClock.Views
{
    /// <summary>
    /// UserInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserInputWindow : Window
    {
        public UserInputWindow(Func<string, string> isValid, string prompt, string defaultValue)
        {
            InitializeComponent();
            IsValid = isValid;
            PromptLabel.Content = prompt;
            InputTextBox.Text = defaultValue;
        }

        /// <summary>
        /// 用户输入的字符串
        /// </summary>
        public string InputText
        {
            get; set;
        }

        /// <summary>
        /// 传入文本内容。如果不通过，返回警告消息；否则返回null
        /// </summary>
        public Func<string, string> IsValid { get; set; }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid != null)
            {
                var warning = IsValid.Invoke(InputTextBox.Text);
                if (warning != null)
                {
                    WarningLabel.Content = warning;
                    return;
                }
            }

            InputText = InputTextBox.Text;
            DialogResult = true;
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 拖动窗体
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
