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

using ExamClock.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ExamClock.Views.UserControls
{
    /// <summary>
    /// ExamItemBox.xaml 的交互逻辑
    /// </summary>
    public partial class ExamItemBox : UserControl, INotifyPropertyChanged
    {
        public ExamItemBox()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExamItem Item
        {
            get => (ExamItem)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(ExamItem), typeof(ExamItemBox),
                new PropertyMetadata(OnItemChanged));


        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as ExamItemBox;
            if (box == null) return;

            if (e.NewValue is ExamItem item)
            {
                box.Item = item;
            }

            box.OnPropertyChanged(nameof(SubjectText));
            box.OnPropertyChanged(nameof(TimeText));
            box.OnPropertyChanged(nameof(DurationText));
        }

        public string SubjectText
        {
            get
            {
                if (Item == null || string.IsNullOrEmpty(Item.Subject))
                {
                    return "未知科目";
                }

                return Item?.Subject;
            }
        }

        public string TimeText => Item == null ? "未知时间" : Item?.GetTimeString();

        public string DurationText => Item == null ? "未知时长" : (int)Item.Duration.TotalMinutes + " min";
    }
}