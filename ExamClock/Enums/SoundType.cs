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

using System.ComponentModel;

namespace ExamClock.Enums
{
    public enum SoundType
    {
        [Description("不提醒")]
        None = 0,
        [Description("结束前15分钟")]
        _15MinBeforeEnding,
        [Description("结束前10分钟")]
        _10MinBeforeEnding,
        [Description("考试开始")]
        ExamBeginning,
        [Description("考试结束")]
        ExamEnding,
    }
}
