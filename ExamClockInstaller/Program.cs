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

using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExamClockInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowWelcome();
            if (!Configuration.AutoInstall)
            {
                ShowInstallationInfo();
                Console.WriteLine("\n是否继续安装？（Y/N）");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Console.WriteLine("\n安装已取消");
                    return;
                }
            }

            if (CopyFiles())
            {
                Console.WriteLine("\n安装成功");
            }
            else
            {
                Console.WriteLine("\n安装失败");
            }

            if (Configuration.CreateDesktopLnk)
            {
                if (CreateDesktopShortcut())
                {
                    Console.WriteLine("\n桌面快捷方式创建成功");
                }
                else
                {
                    Console.WriteLine("\n桌面快捷方式创建失败");
                }
            }
        }

        /// <summary>
        /// 显示欢迎信息
        /// </summary>
        static void ShowWelcome()
        {
            Console.WriteLine("\t================\t");
            Console.WriteLine("\t考试时钟安装程序\t");
            Console.WriteLine("\t================\t");
        }

        /// <summary>
        /// 显示安装程序的配置信息
        /// </summary>
        static void ShowInstallationInfo()
        {
            Console.WriteLine("\n-----程序安装信息-----");
            Console.WriteLine($"目标版本号：\t{Configuration.AppVersion}");
            Console.WriteLine($"程序安装路径：\t{Configuration.TargetPath}");
            Console.WriteLine($"桌面快捷方式：\t{(Configuration.CreateDesktopLnk ? "创建" : "不创建")}");
            Console.WriteLine($"安装后运行：\t{(Configuration.AutoRun ? "是" : "否")}");
            Console.WriteLine($"安装后退出：\t{(Configuration.AutoExit ? "是" : "否")}");
        }

        /// <summary>
        /// 将文件复制到指定目录
        /// </summary>
        /// <returns>是否成功复制</returns>
        static bool CopyFiles()
        {
            const string prefix = "ExamClockInstaller.Resources.";
            if (string.IsNullOrEmpty(Configuration.TargetPath)) return false;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            if (resourceNames.Length == 0)
            {
                Console.Error.WriteLine("\n没有需要安装的内容");
                return false;
            }

            try
            {
                foreach (var name in resourceNames)
                {
                    if (!name.StartsWith(prefix)) continue;

                    if (!Directory.Exists(Configuration.TargetPath))
                    {
                        Directory.CreateDirectory(Configuration.TargetPath);
                    }

                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
                        if (stream == null) continue;

                        string fileName = name.Substring(prefix.Length);
                        string filePath = Path.Combine(Configuration.TargetPath, fileName);
                        using (var destination = System.IO.File.Create(filePath))
                        {
                            stream.CopyTo(destination);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\n安装过程中发生错误：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建桌面快捷方式
        /// </summary>
        /// <returns></returns>
        static bool CreateDesktopShortcut()
        {
            // 源程序的路径
            var source = Path.Combine(Configuration.TargetPath, Configuration.ExeRelativePath);
            
            // 快捷方式的路径
            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{Configuration.LnkName}.lnk");
            
            if (!System.IO.File.Exists(source))
            {
                return false;
            }

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(target);
            try
            {
                shortcut.TargetPath = source;
                shortcut.Save();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\n创建桌面快捷方式时发生错误：{ex.Message}");
                return false;
            }
            finally
            {
                // 手动释放资源
                if (shortcut != null)
                {
                    try { Marshal.FinalReleaseComObject(shortcut); } catch { }
                    shortcut = null;
                }
                if (shell != null)
                {
                    try { Marshal.FinalReleaseComObject(shell); } catch { }
                    shell = null;
                }
            }
        }
    }
}