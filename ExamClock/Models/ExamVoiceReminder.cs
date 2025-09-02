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

using ExamClock.Enums;
using ExamClock.Interfaces;
using ExamClock.ViewModels;
using System;
using System.Media;

namespace ExamClock.Models
{
    public class ExamVoiceReminder : IAudioPlayer
    {
        /// <summary>
        /// 结束前15分钟提醒：（叮咚）距考试结束还有15分钟，请检查答案是否按规定填涂在答题卡上。
        /// </summary>
        private const string _15MinBeforeEndingPath = "ExamClock.Assets.15MinBeforeEnding.wav";

        /// <summary>
        /// 结束前15分钟提醒：（叮咚）距考试结束还有10分钟，请检查答案是否按规定填涂在答题卡上。
        /// </summary>
        private const string _10MinBeforeEndingPath = "ExamClock.Assets.10MinBeforeEnding.wav";

        /// <summary>
        /// 开考铃声：（电铃10秒）考生可以开始答题。
        /// </summary>
        private const string ExamBeginningPath = "ExamClock.Assets.ExamBeginning.wav";

        /// <summary>
        /// 开考铃声：（电铃15秒）请考生立即停笔并停止答题，请考生立即停笔并停止答题。
        /// </summary>
        private const string ExamEndingPath = "ExamClock.Assets.ExamEnding.wav";

        /// <summary>
        /// 声音类型
        /// </summary>
        private SoundType AudioType { get; set; }


        public ExamVoiceReminder(SoundType audioType)
        {
            AudioType = audioType;
        }


        /// <summary>
        /// 获取音频文件的路径
        /// </summary>
        /// <returns>音频文件的路径</returns>
        private string GetAudioPath(SoundType type)
        {
            switch (type)
            {
                case SoundType._15MinBeforeEnding:
                    return _15MinBeforeEndingPath;
                case SoundType._10MinBeforeEnding:
                    return _10MinBeforeEndingPath;
                case SoundType.ExamBeginning:
                    return ExamBeginningPath;
                case SoundType.ExamEnding:
                    return ExamEndingPath;
                case SoundType.None:
                    return null;
                default:
                    throw new Exception("Sound type not defined! Cannot get the path.");
            }
        }


        public void Play()
        {
            string path = GetAudioPath(AudioType);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var assembly = typeof(ClockViewModel).Assembly;
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Play();
                }
            }
        }


        public bool CanPlay()
        {
            try
            {
                if (string.IsNullOrEmpty(GetAudioPath(AudioType)))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
