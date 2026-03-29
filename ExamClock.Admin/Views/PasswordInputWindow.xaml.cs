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

using System.Windows;
using System.Windows.Media;

namespace ExamClock.Admin.Views
{
    public partial class PasswordInputWindow : Window
    {
        public PasswordInputWindow(string username)
        {
            InitializeComponent();
            UsernameString = username;
            UsernameInputBox.Text = UsernameString;
        }

        /// <summary>
        /// 用户输入的密码字符串
        /// </summary>
        public string PasswordString { get; set; }

        /// <summary>
        /// 用户输入的用户名字符串
        /// </summary>
        public string UsernameString { get; set; }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordString = PasswordInputBox.Password;
            UsernameString = UsernameInputBox.Text;

            if (string.IsNullOrEmpty(UsernameString))
            {
                UsernameInputBox.BorderBrush = Brushes.Red;
                UsernameInputBox.BorderThickness = new Thickness(2);
                return;
            }
            UsernameInputBox.BorderBrush = Brushes.Gray;
            UsernameInputBox.BorderThickness = new Thickness(1);

            if (string.IsNullOrEmpty(PasswordString))
            {
                PasswordInputBox.BorderBrush = Brushes.Red;
                PasswordInputBox.BorderThickness = new Thickness(2);
                return;
            }
            PasswordInputBox.BorderBrush = Brushes.Gray;
            PasswordInputBox.BorderThickness = new Thickness(1);

            DialogResult = true;
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
