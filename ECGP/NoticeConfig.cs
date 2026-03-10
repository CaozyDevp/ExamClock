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

namespace ECGP
{
    public struct NoticeConfig
    {
        /// <summary>
        /// 开启开考提醒
        /// </summary>
        public bool EnableBeginning { get; set; }

        /// <summary>
        /// 开启收卷提醒
        /// </summary>
        public bool EnableEnding { get; set; }

        /// <summary>
        /// 结束前提醒的类型，如果传入了不合法的值，则会被设置为None
        /// </summary>
        public SoundType BeforeEnding
        {
            get => _beforeEnding;
            set
            {
                if(value == SoundType.None || value == SoundType._10MinBeforeEnding || value == SoundType._15MinBeforeEnding)
                {
                    _beforeEnding = value;
                    return;
                }
                _beforeEnding = SoundType.None;
            }
        }
        private SoundType _beforeEnding;
    }
}
