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

using ExamClock.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace ExamClock.Models
{
    public class ExamVoiceReminder : IDisposable
    {
        /// <summary>
        /// 可在外部访问的实例
        /// </summary>
        public static ExamVoiceReminder Instance => _instance;
        private static readonly ExamVoiceReminder _instance = new ExamVoiceReminder();

        /// <summary>
        /// 用于播放考试提示音的<see cref="MediaPlayer"/>对象
        /// </summary>
        private readonly MediaPlayer _player = new MediaPlayer();

        /// <summary>
        /// 考试提示音的资源路径
        /// </summary>
        private readonly Dictionary<SoundType, string> _audioResourcePaths = new Dictionary<SoundType, string>()
        {
            { SoundType.ExamBeginning, "ExamClock.Assets.ExamBeginning.mp3" },
            { SoundType.ExamEnding, "ExamClock.Assets.ExamEnding.mp3" },
            { SoundType._10MinBeforeEnding, "ExamClock.Assets.10MinBeforeEnding.mp3" },
            { SoundType._15MinBeforeEnding, "ExamClock.Assets.15MinBeforeEnding.mp3" },
            { SoundType.EnteringRoom, "ExamClock.Assets.EnteringRoom.mp3" },
        };

        /// <summary>
        /// 各考试提示音的总秒数
        /// </summary>
        private readonly Dictionary<SoundType, int> _audioTotalSeconds = new Dictionary<SoundType, int>()
        {
            { SoundType.ExamBeginning, 13 },
            { SoundType.ExamEnding, 23 },
            { SoundType._10MinBeforeEnding, 10 },
            { SoundType._15MinBeforeEnding, 10 },
            { SoundType.EnteringRoom, 18 },
            { SoundType.None, 0 },
        };

        /// <summary>
        /// 将指定类型的音频保存为临时文件，如果存在则覆盖它
        /// </summary>
        /// <param name="type">音频类型</param>
        /// <returns>如果成功，返回临时文件的路径；否则返回<see langword="null"/></returns>
        private string GetTempFile(SoundType type)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(_audioResourcePaths[type]))
                {
                    if (stream == null) return null;

                    // 在系统临时目录下创建一个专门的文件夹
                    string tempDir = Path.Combine(Path.GetTempPath(), "ExamClock_Res");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }

                    // 保存临时文件
                    string tempFilePath = Path.Combine(tempDir, $"{type}.mp3");
                    using (var destination = File.Create(tempFilePath))
                    {
                        stream.CopyTo(destination);
                    }

                    return tempFilePath;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 播放指定类型的音频
        /// </summary>
        public void Play(SoundType type)
        {
            if (type == SoundType.None) return;

            string tempFilePath = GetTempFile(type);
            if (tempFilePath == null) return;

            // 打开并播放指定音频（在构造函数中已经添加了MediaOpened事件处理器，在打开之后会自动播放，无需再调用Play()方法）
            _player.Open(new Uri(tempFilePath, UriKind.Absolute));
        }

        /// <summary>
        /// 获取指定类型音频的总秒数
        /// </summary>
        /// <param name="type">音频类型</param>
        /// <returns>该类型音频的总秒数。如果不存在，返回0</returns>
        public int GetTotalSeconds(SoundType type)
        {
            if (_audioTotalSeconds.TryGetValue(type, out var seconds))
            {
                return seconds;
            }
            return 0;
        }

        public ExamVoiceReminder()
        {
            _player.Volume = 1.0;
            _player.MediaOpened += MediaOpened;
            _player.MediaEnded += MediaEnded;
        }

        private void MediaOpened(object sender, EventArgs e)
        {
            _player.Play();
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            _player.Close();
        }

        public void Dispose()
        {
            _player.Close();
        }
    }
}
