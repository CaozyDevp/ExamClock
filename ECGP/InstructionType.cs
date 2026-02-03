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

namespace ECGP
{
    /// <summary>
    /// 控制指令的类型
    /// </summary>
    public enum InstructionType
    {
        ModeSwitch = 0x01,
        SetSystemVolume = 0x02,
        SyncTimeWithController = 0x03,
        SetRoomNumber = 0x04
    }
}
