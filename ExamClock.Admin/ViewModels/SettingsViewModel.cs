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

using ExamClock.Core;
using ExamClock.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ExamClock.Admin.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// 考试日程安排的原始文本，用于绑定到UI的文本框
        /// </summary>
        public string TimeTableText
        {
            get => _timeTableText;
            set
            {
                SetProperty(ref _timeTableText, value);
                IsSaved = false;
            }
        }
        private string _timeTableText = "";

        /// <summary>
        /// 考试项目
        /// </summary>
        public List<List<ExamItem>> ExamItems
        {
            get => _examItems;
            set => SetProperty(ref _examItems, value);
        }
        private List<List<ExamItem>> _examItems = new List<List<ExamItem>>();

        /// <summary>
        /// 尝试解析文本为<see cref="ExamItem"/>对象
        /// </summary>
        /// <returns>如果解析成功，返回null；否则返回错误信息</returns>
        private string TryParse(out List<ExamItem> timeTable)
        {
            // 尝试解析时间表文本
            var lines = TimeTableText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            List<ExamItem> parsedItems = new List<ExamItem>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (ExamItem.TryParse(line, out ExamItem parsedItem))
                {
                    parsedItems.Add(parsedItem);
                }
                else
                {
                    timeTable = null;
                    return $"第 {(i + 1)} 行格式不正确";
                }
            }

            timeTable = parsedItems;
            return null;
        }

        /// <summary>
        /// 检查是否存在重叠的考试时间，如果没有重叠，则返回false
        /// </summary>
        public bool CheckIfOverlapping(List<ExamItem> items)
        {
            if (items == null || items.Count == 0) return false;
            // 检查时间表中是否有重叠的考试时间
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (items[i].BeginTime < items[j].BeginTime + items[j].Duration &&
                        items[j].BeginTime < items[i].BeginTime + items[i].Duration)
                    {
                        return true; // 有重叠
                    }
                }
            }

            return false; // 无重叠
        }

        /// <summary>
        /// 是否已经保存修改
        /// </summary>
        public bool IsSaved { get; set; } = true;

        public ICommand SaveTimeTableCommand => new RelayCommand((_) => SaveTimeTable());

        private void SaveTimeTable()
        {
            string errorMessage = TryParse(out List<ExamItem> timeTable);
            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CheckIfOverlapping(timeTable))
            {
                MessageBox.Show("考试时间表中存在重叠的考试时间，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Configuration.Schedule.Import(timeTable);
                if (!Configuration.SaveConfig())
                {
                    throw new Exception();
                }
                MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                IsSaved = true;
            }
            catch
            {
                MessageBox.Show("保存失败，请检查配置文件是否可写", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
