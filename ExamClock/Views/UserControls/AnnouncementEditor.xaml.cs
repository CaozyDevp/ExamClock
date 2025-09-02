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

using ExamClock.Commands;
using ExamClock.Enums;
using ExamClock.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ExamClock.Views.UserControls
{
    /// <summary>
    /// AnnouncementEditor.xaml 的交互逻辑
    /// </summary>
    public partial class AnnouncementEditor : UserControl, INotifyPropertyChanged
    {
        public AnnouncementEditor()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region Dependency Properties
        public static readonly DependencyProperty AnnouncementNameProperty =
            DependencyProperty.Register(nameof(AnnouncementName), typeof(string), typeof(AnnouncementEditor), new PropertyMetadata(""));
        public string AnnouncementName
        {
            get => (string)GetValue(AnnouncementNameProperty);
            set => SetValue(AnnouncementNameProperty, value);
        }

        public static readonly DependencyProperty AnnouncementOptionsProperty =
            DependencyProperty.Register(nameof(AnnouncementOptions), typeof(List<string>), typeof(AnnouncementEditor), new PropertyMetadata(new List<string>()));
        public List<string> AnnouncementOptions
        {
            get => (List<string>)GetValue(AnnouncementOptionsProperty);
            set => SetValue(AnnouncementOptionsProperty, value);
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(AnnouncementEditor), new PropertyMetadata(0));
        /// <summary>
        /// 选中的下标，下标0代表“关闭”
        /// </summary>
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set
            {
                SetValue(SelectedIndexProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty AudioTypeProperty =
            DependencyProperty.Register(nameof(AudioType), typeof(SoundType), typeof(AnnouncementEditor), new PropertyMetadata(SoundType.None));
        public SoundType AudioType
        {
            get => (SoundType)GetValue(AudioTypeProperty);
            set { SetValue(AudioTypeProperty, value); }
        }
        #endregion


        #region Properties

        public bool IsAudioPlaying
        {
            get => isAudioPlaying;
            set
            {
                isAudioPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAudioPlaying));
                OnPropertyChanged(nameof(StoppedIconVisibility));
            }
        }
        private bool isAudioPlaying = false;

        public bool IsNotAudioPlaying => !isAudioPlaying;

        public Visibility StoppedIconVisibility
        {
            get => isAudioPlaying ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion


        #region Methods
        private void PlayAudio()
        {
            if (AudioType == SoundType.None)
            {
                return;
            }
            new ExamVoiceReminder(AudioType).Play();
        }
        #endregion


        #region Commands
        public ICommand PlayTestAudioCommand => new RelayCommand(execute =>
        {
            if (SelectedIndex == 0)
            {
                // 将0定义为“关闭”选项，所以这种状态不做处理
                return;
            }
            if (!IsAudioPlaying)
            {
                PlayAudio();
                IsAudioPlaying = true;
                DispatcherTimer timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(10)
                };
                timer.Start();
                timer.Tick += (object sender, EventArgs e) =>
                {
                    IsAudioPlaying = false;
                    timer.Stop();
                };
            }
        });
        #endregion
    }
}
