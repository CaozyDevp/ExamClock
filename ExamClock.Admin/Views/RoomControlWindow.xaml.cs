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
using ExamClock.Admin.ViewModels;
using System;
using System.Windows;

namespace ExamClock.Admin.Views
{
    public partial class RoomControlWindow : Window
    {
        public RoomControlWindow(RoomInfo info, TimeSpan timeOffset)
        {
            if (info == null || info.IP == null)
            {
                MessageBox.Show("考场信息错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            InitializeComponent();
            (DataContext as RoomControlViewModel)?.Init(info, timeOffset);
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            (DataContext as RoomControlViewModel)?.Dispose();
        }
    }
}
