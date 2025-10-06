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
using System;
using System.Media;
using System.Reflection;

namespace ExamClock.Models
{
    public class ExamVoiceReminder
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
        private SoundType AudioType { get; }

        /// <summary>
        /// 使用<see cref="SoundType"/>枚举值初始化<see cref="ExamVoiceReminder"/>对象
        /// </summary>
        /// <param name="audioType">指定的声音类型</param>
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

        /// <summary>
        /// 播放音频
        /// </summary>
        public void Play()
        {
            // 如果音频不能播放，直接返回
            if (!CanPlay()) return;

            string path = GetAudioPath(AudioType);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // 获取包含当前执行的代码的程序集
            var assembly = Assembly.GetExecutingAssembly();

            // 播放路径为path的资源，即指定的音频
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    var player = new SoundPlayer(stream);
                    player.Play();
                }
            }
        }

        /// <summary>
        /// 检测当前的音频是否可以播放
        /// </summary>
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
