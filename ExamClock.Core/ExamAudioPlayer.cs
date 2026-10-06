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

using System.Windows.Media;
using System.IO;
using System;

namespace ExamClock.Core
{
    /// <summary>
    /// 用于播放听力考试音频的类，
    /// 仅提供基本的播放功能，定时播放需要在外部实现，
    /// 底层使用 MediaPlayer 进行音频播放
    /// </summary>
    public class ExamAudioPlayer : IDisposable
    {
        /// <summary>
        /// 音频文件的路径
        /// </summary>
        public string AudioPath
        {
            get => _audioPath;
            set
            {
                _player?.Close();
                if (string.IsNullOrEmpty(value))
                {
                    _audioPath = string.Empty;
                }
                else
                {
                    _audioPath = value;
                    if (AudioFileExists)
                    {
                        _player.Open(new Uri(AudioPath, UriKind.Absolute));
                    }
                }
            }
        }
        private string _audioPath = string.Empty;

        /// <summary>
        /// 音频文件是否存在
        /// </summary>
        public bool AudioFileExists => !string.IsNullOrEmpty(AudioPath) && File.Exists(AudioPath);

        private readonly MediaPlayer _player = new MediaPlayer()
        {
            Volume = 1.0
        };

        public ExamAudioPlayer(string path)
        {
            AudioPath = path;
        }

        public void Play()
        {
            if (_player.HasAudio || _player.HasVideo)
            {
                _player.Play();
            }
        }

        public void Pause()
        {
            if (_player.HasAudio || _player.HasVideo)
            {
                _player.Pause();
            }
        }

        public void Stop()
        {
            if (_player.HasAudio || _player.HasVideo)
            {
                _player.Stop();
            }
        }

        public void Dispose()
        {
            _player?.Close();
        }
    }
}
