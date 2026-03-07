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

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ECGP
{
    /// <summary>
    /// 动态密钥管理类
    /// </summary>
    public class DynamicKeyManager : IDisposable
    {
        /// <summary>
        /// 保存5个可用密钥，每个密钥的失效时间间隔1s
        /// </summary>
        private byte[][] _keys = new byte[5][];

        /// <summary>
        /// 指向最新的密钥
        /// </summary>
        private int _pointer = 0;

        private readonly Timer _timer;

        /// <summary>
        /// 最新的密钥
        /// </summary>
        public byte[] LatestKey => _keys[_pointer];

        private object _updateLock = new object();

        /// <summary>
        /// 更新密钥
        /// </summary>
        private void Update()
        {
            lock (_updateLock)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.GenerateKey();
                    int next = (_pointer + 1) % _keys.Length;
                    _keys[next] = aes.Key;
                    _pointer = next;
                }
            }
        }

        /// <summary>
        /// 检查密钥是否有效
        /// </summary>
        /// <param name="key">需要检查的密钥</param>
        /// <returns>是否有效</returns>
        public bool CheckIfValid(byte[] key)
        {
            foreach (var k in _keys)
            {
                if (k != null && key != null && k.SequenceEqual(key)) return true;
            }
            return false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public DynamicKeyManager()
        {
            // 装填初始密钥
            for (int i = 0; i < _keys.Length; i++)
            {
                Update();
            }

            //初始化每秒更新机制
            _timer = new Timer((e) =>
            {
                Update();
            }, null, 1000, 1000);
        }
    }
}
