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

using ExamClock.Core;
using ExamClock.Core.Enums;
using ExamClock.Views;
using ExamClock.Views.UserControls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExamClock.Mvvm;

namespace ExamClock.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            InitArrangementText();
            InitElements();
        }

        public Action CloseWindow { get; set; }
        public ICommand CloseWindowCommand
        {
            get => new RelayCommand(execute =>
            {
                var result = MessageBoxResult.Yes;
                if (!IsSaved)
                {
                    result = MessageBox.Show("确定不保存设置并关闭吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        CloseWindow?.Invoke();
                        break;
                    case MessageBoxResult.No:
                        return; // 不关闭窗口
                }
            });
        }

        #region Pages Switch

        /// <summary>
        /// 0-总览 1-安排 2-提醒 3-集控
        /// </summary>
        private int _currentGrid = 0;
        public int CurrentGrid
        {
            get { return _currentGrid; }
            set
            {
                if (value < 0 || value > 3)
                    throw new ArgumentOutOfRangeException("value");
                _currentGrid = value;
                OnPropertyChanged(nameof(OutlineGridVisibility));
                OnPropertyChanged(nameof(ArrangementGridVisibility));
                OnPropertyChanged(nameof(AnnouncementGridVisibility));
                OnPropertyChanged(nameof(CalibrationGridVisibility));
                OnPropertyChanged(nameof(Grid0TextBrush));
                OnPropertyChanged(nameof(Grid1TextBrush));
                OnPropertyChanged(nameof(Grid2TextBrush));
                OnPropertyChanged(nameof(Grid3TextBrush));
                OnPropertyChanged();
            }
        }

        // Grid 0
        public Visibility OutlineGridVisibility
        {
            get { return CurrentGrid == 0 ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Brush Grid0TextBrush
        {
            get
            {
                Color color;
                if (CurrentGrid == 0)
                {
                    color = (Color)Application.Current.Resources["ActiveBlue"];
                }
                else
                {
                    color = (Color)Application.Current.Resources["InactiveGray"];
                }

                return new SolidColorBrush(color);
            }
        }
        public ICommand TurnToGrid0Command => new RelayCommand(execute => { CurrentGrid = 0; });

        // Grid 1
        public Visibility ArrangementGridVisibility
        {
            get { return CurrentGrid == 1 ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Brush Grid1TextBrush
        {
            get
            {
                Color color;
                if (CurrentGrid == 1)
                {
                    color = (Color)Application.Current.Resources["ActiveBlue"];
                }
                else
                {
                    color = (Color)Application.Current.Resources["InactiveGray"];
                }

                return new SolidColorBrush(color);
            }
        }
        public ICommand TurnToGrid1Command => new RelayCommand(execute => { CurrentGrid = 1; });

        // Grid 2
        public Visibility AnnouncementGridVisibility
        {
            get { return CurrentGrid == 2 ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Brush Grid2TextBrush
        {
            get
            {
                Color color;
                if (CurrentGrid == 2)
                {
                    color = (Color)Application.Current.Resources["ActiveBlue"];
                }
                else
                {
                    color = (Color)Application.Current.Resources["InactiveGray"];
                }

                return new SolidColorBrush(color);
            }
        }
        public ICommand TurnToGrid2Command => new RelayCommand(execute => { CurrentGrid = 2; });

        // Grid 3
        public Visibility CalibrationGridVisibility
        {
            get { return CurrentGrid == 3 ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Brush Grid3TextBrush
        {
            get
            {
                Color color;
                if (CurrentGrid == 3)
                {
                    color = (Color)Application.Current.Resources["ActiveBlue"];
                }
                else
                {
                    color = (Color)Application.Current.Resources["InactiveGray"];
                }

                return new SolidColorBrush(color);
            }
        }
        public ICommand TurnToGrid3Command => new RelayCommand(execute => { CurrentGrid = 3; });

        #endregion


        #region Grid 0

        /// <summary>
        /// 考试项目
        /// </summary>
        public List<List<ExamItem>> ExamItems
        {
            get => _examItems;
            set => SetProperty(ref _examItems, value);
        }
        private List<List<ExamItem>> _examItems = new List<List<ExamItem>>();

        /// <summary>
        /// 概览页面的控件（日期、考试项目）
        /// </summary>
        public ObservableCollection<UIElement> ExamElements
        {
            get => _examElements;
            set => SetProperty(ref _examElements, value);
        }
        private ObservableCollection<UIElement> _examElements = new ObservableCollection<UIElement>();

        /// <summary>
        /// 语音提醒的信息
        /// </summary>
        public string NoticeText
        {
            get
            {
                var result = "";
                switch (ExamBeginningSoundType)
                {
                    case SoundType.ExamBeginning:
                        result += "开考铃 ";
                        break;
                    case SoundType.None:
                    default:
                        break;
                }
                switch (NoticeBeforeEndingSoundType)
                {
                    case SoundType._15MinBeforeEnding:
                        result += "结束前15分钟 ";
                        break;
                    case SoundType._10MinBeforeEnding:
                        result += "结束前10分钟 ";
                        break;
                    case SoundType.None:
                    default:
                        break;
                }
                switch (ExamEndingSoundType)
                {
                    case SoundType.ExamEnding:
                        result += "结束铃 ";
                        break;
                    case SoundType.None:
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = "无";
                }

                return "语音提醒：" + result;
            }
        }

        /// <summary>
        /// 考场号文本
        /// </summary>
        public string RoomNumberText
        {
            get
            {
                if (RoomNumber == 0) return "未设置考场号";
                return "考场号 | " + RoomNumber.ToString("0000");
            }
        }

        /// <summary>
        /// 考场号
        /// </summary>
        public ushort RoomNumber
        {
            get => Configuration.RoomNumber;
            set
            {
                Configuration.RoomNumber = value;
                if (!Configuration.SaveConfig())
                {
                    MessageBox.Show("考场号保存失败！");
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(RoomNumberText));
            }
        }

        /// <summary>
        /// 按照<see cref="Configuration"/>中的考试时间表初始化
        /// </summary>
        private void InitExamItems()
        {
            ExamItems.Clear();

            if (Configuration.TimeTable == null)
            {
                return;
            }

            ExamItem currentItem = null;
            foreach (var item in Configuration.TimeTable)
            {
                if (currentItem == null || currentItem.BeginTime.Date != item.BeginTime.Date)
                {
                    currentItem = item;
                    ExamItems.Add(new List<ExamItem> { currentItem });
                }
                else
                {
                    ExamItems.Last().Add(item);
                }
            }
        }

        /// <summary>
        /// 导入考试时间表文件
        /// </summary>
        public bool ImportConfig()
        {
            var file = new OpenFileDialog
            {
                Title = "导入考试时间表",
                Filter = "spf文件|*.spf"
            };
            if (file.ShowDialog() == false)
            {
                return false;
            }

            if (file.FileName.Trim() == string.Empty || !file.CheckFileExists)
            {
                return false;
            }

            var succeed = Configuration.LoadConfig(file.FileName.Trim());
            InitExamItems();
            return succeed;
        }

        /// <summary>
        /// 初始化考试时间表的显示
        /// </summary>
        private void InitElements()
        {
            InitExamItems();

            if (ExamElements == null)
            {
                ExamElements = new ObservableCollection<UIElement>();
            }

            ExamElements.Clear();

            if (ExamItems == null || ExamItems.Count == 0)
            {
                return;
            }

            foreach (var item in ExamItems)
            {
                if (item == null || item.Count == 0)
                {
                    continue;
                }

                TextBlock dateLabel = new TextBlock
                {
                    Text = item.First().BeginTime.ToString("yyyy/MM/dd"),
                    FontSize = 25,
                    Margin = new Thickness(0, 10, 0, 5),
                    Foreground = new SolidColorBrush(Color.FromRgb(46, 73, 89)),
                    FontFamily = (FontFamily)Application.Current.Resources["Num"],
                };
                ExamElements.Add(dateLabel);
                foreach (var examItem in item)
                {
                    if (examItem == null)
                    {
                        continue;
                    }

                    var box = new ExamItemBox()
                    {
                        Item = examItem,
                        Height = 40
                    };
                    ExamElements.Add(box);
                }
            }

            OnPropertyChanged(nameof(NoticeText));
            OnPropertyChanged(nameof(ExamElements));
        }

        /// <summary>
        /// 导入配置文件
        /// </summary>
        public ICommand ImportConfigCommand =>
            new RelayCommand(execute =>
            {
                ImportConfig();
                InitElements();
                OnPropertyChanged(nameof(RoomNumberText));
            });

        /// <summary>
        /// 编辑考场号
        /// </summary>
        public ICommand EditRoomNumberCommand =>
            new RelayCommand(execute =>
            {
                var dialog = new UserInputWindow(Configuration.JudgeRoomNumberInput, "请设置考场号", RoomNumber.ToString("0000"));
                dialog.ShowDialog();
                if (dialog.IsValid)
                {
                    RoomNumber = Convert.ToUInt16(dialog.InputText);
                }
            });

        #endregion


        #region Grid 1

        /// <summary>
        /// 考试日程安排的原始文本，用于绑定到UI的文本框
        /// </summary>
        public string TimeTableText
        {
            get => _timeTableText;
            set
            {
                if (_timeTableText == value) return;
                _timeTableText = value;

                OnPropertyChanged();

                IsSaved = false;
            }
        }
        private string _timeTableText;

        /// <summary>
        /// 是否已经保存修改
        /// </summary>
        public bool IsSaved { get; set; } = true;

        /// <summary>
        /// 尝试解析文本为<see cref="ExamItem"/>对象
        /// </summary>
        private bool TryParse(out List<ExamItem> timeTable)
        {
            try
            {
                // 尝试解析时间表文本
                var lines = TimeTableText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                List<ExamItem> parsedItems = new List<ExamItem>();
                foreach (var line in lines)
                {
                    if (ExamItem.TryParse(line, out ExamItem parsedItem))
                    {
                        parsedItems.Add(parsedItem);
                    }
                    else
                    {
                        throw new Exception();  // 解析失败
                    }
                }

                timeTable = parsedItems;
                return true;
            }
            catch
            {
                timeTable = null;
                return false;
            }
        }

        /// <summary>
        /// 检查是否存在重叠的考试时间，如果没有重叠或解析失败，则返回false
        /// </summary>
        public bool CheckIfOverlapping(List<ExamItem> items)
        {
            if (items == null || items.Count == 0) return false;
            // 检查时间表中是否有重叠的考试时间
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (items[i].BeginTime < items[j].BeginTime + items[j].Duration &&
                        items[j].BeginTime < items[i].BeginTime + items[i].Duration)
                    {
                        return true; // 有重叠
                    }
                }
            }

            return false; // 无重叠
        }

        /// <summary>
        /// 初始化“考试日程安排”的文本框
        /// </summary>
        private void InitArrangementText()
        {
            TimeTableText = "";
            for (int i = 0; i < Configuration.TimeTable.Count; i++)
            {
                ExamItem item = Configuration.TimeTable[i];

                if (item == null)
                {
                    continue;
                }

                TimeTableText += $"{item.Subject} {item.BeginTime:yyyy/MM/dd} {item.BeginTime:HH:mm} {(int)item.Duration.TotalMinutes}";

                if (i < Configuration.TimeTable.Count - 1)
                {
                    TimeTableText += "\n";
                }
            }

            IsSaved = true;
        }

        /// <summary>
        /// 保存考试日程安排
        /// </summary>
        public ICommand SaveArrangementCommand => new RelayCommand(execute =>
        {
            if (!TryParse(out List<ExamItem> timeTable))
            {
                MessageBox.Show("请检查格式是否正确", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CheckIfOverlapping(timeTable))
            {
                MessageBox.Show("考试时间表中存在重叠的考试时间，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Configuration.TimeTable = timeTable;
                Configuration.SaveConfig();
                InitElements();
                MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                IsSaved = true;
            }
            catch
            {
                MessageBox.Show("保存失败，请检查配置文件是否可写", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        #endregion


        #region Grid 2

        /// <summary>
        /// 开启/关闭选项
        /// </summary>
        public List<string> OnOffOptions =>
            new List<string>
            {
                "关闭",
                "开启"
            };

        /// <summary>
        /// 结束前提醒选项：关闭/10分钟/15分钟
        /// </summary>
        public List<string> NoticeBeforeEndingOptions =>
            new List<string>
            {
                "关闭",
                "10分钟",
                "15分钟"
            };

        /// <summary>
        /// 选定结束前提醒选项的下标
        /// </summary>
        public int SelectedNoticeBeforeEndingIndex
        {
            get
            {
                switch (Configuration.NoticeBeforeEnding)
                {
                    case SoundType.None:
                        return 0;
                    case SoundType._10MinBeforeEnding:
                        return 1;
                    case SoundType._15MinBeforeEnding:
                        return 2;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        Configuration.NoticeBeforeEnding = SoundType.None;
                        break;
                    case 1:
                        Configuration.NoticeBeforeEnding = SoundType._10MinBeforeEnding;
                        break;
                    case 2:
                        Configuration.NoticeBeforeEnding = SoundType._15MinBeforeEnding;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(NoticeBeforeEndingSoundType));
                OnPropertyChanged(nameof(NoticeText));

                if (!Configuration.SaveConfig())
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }
        public SoundType NoticeBeforeEndingSoundType
        {
            get
            {
                switch (SelectedNoticeBeforeEndingIndex)
                {
                    default:
                        return SoundType.None;
                    case 1:
                        return SoundType._10MinBeforeEnding;
                    case 2:
                        return SoundType._15MinBeforeEnding;
                }
            }
        }

        /// <summary>
        /// 选定开考铃的下标 0-关闭 1-开启
        /// </summary>
        public int SelectedExamBeginningIndex
        {
            get
            {
                switch (Configuration.ExamBeginningNotice)
                {
                    case SoundType.None:
                        return 0;
                    case SoundType.ExamBeginning:
                        return 1;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        Configuration.ExamBeginningNotice = SoundType.None;
                        break;
                    case 1:
                        Configuration.ExamBeginningNotice = SoundType.ExamBeginning;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(ExamBeginningSoundType));
                OnPropertyChanged(nameof(NoticeText));


                if (!Configuration.SaveConfig())
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }
        public SoundType ExamBeginningSoundType
        {
            get
            {
                switch (SelectedExamBeginningIndex)
                {
                    default:
                        return SoundType.None;
                    case 1:
                        return SoundType.ExamBeginning;
                }
            }
        }

        /// <summary>
        /// 选定结束铃的下标 0-关闭 1-开启
        /// </summary>
        public int SelectedExamEndingIndex
        {
            get
            {
                switch (Configuration.ExamEndingNotice)
                {
                    case SoundType.None:
                        return 0;
                    case SoundType.ExamEnding:
                        return 1;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        Configuration.ExamEndingNotice = SoundType.None;
                        break;
                    case 1:
                        Configuration.ExamEndingNotice = SoundType.ExamEnding;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(ExamEndingSoundType));
                OnPropertyChanged(nameof(NoticeText));


                if (!Configuration.SaveConfig())
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }
        public SoundType ExamEndingSoundType
        {
            get
            {
                switch (SelectedExamEndingIndex)
                {
                    default:
                        return SoundType.None;
                    case 1:
                        return SoundType.ExamEnding;
                }
            }
        }

        #endregion


        #region Grid 3

        private readonly SolidColorBrush _redBrush = new SolidColorBrush(Color.FromRgb(255, 44, 44));
        private readonly SolidColorBrush _blueBrush = new SolidColorBrush(Color.FromRgb(44, 142, 255));

        public bool IsKeyLoaded => KeyManager.Loaded;
        public string IsKeyLoadedString => IsKeyLoaded ? "已装载" : "未装载";
        public SolidColorBrush IsKeyLoadedBrush => IsKeyLoaded ? _blueBrush : _redBrush;

        public bool Permission
        {
            get => Configuration.AllowControl;
            set
            {
                if (value == Configuration.AllowControl) return;

                if (value)
                {
                    if (!KeyManager.Loaded)
                    {
                        MessageBox.Show("无法授权！请确保密钥文件存在且有效！");
                        return;
                    }
                    (Application.Current as App).IsClientResponderEnabled = true;
                }
                else
                {
                    (Application.Current as App).IsClientResponderEnabled = false;
                }

                Configuration.AllowControl = value;
                Configuration.SaveConfig();
                OnPropertyChanged(nameof(PermissionString));
                OnPropertyChanged(nameof(PermissionBrush));
            }
        }
        public string PermissionString => Permission ? "完全" : "未授权";
        public SolidColorBrush PermissionBrush => Permission ? _blueBrush : _redBrush;

        public ICommand PermissionSwitchCommand => new RelayCommand(execute =>
        {
            Permission = !Permission;
        });
        #endregion
    }
}