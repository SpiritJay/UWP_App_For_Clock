using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 副待办事项列表项目
    /// </summary>
    public class SubToDoListItem
    {
        public SubToDoListItem()
        {
            IsChecked = false;
            Content = "无内容";
        }
        public SubToDoListItem(bool CheckedValue, string ContentValue)
        {
            IsChecked = CheckedValue;
            Content = ContentValue;
        }
        public SubToDoListItem(string ContentValue)
        {
            IsChecked = false;
            Content = ContentValue;
        }
        public bool IsChecked { get; set; }
        public string Content { get; set; }
    }
    /// <summary>
    /// 用来显示提醒列表的项目
    /// </summary>
    public class ReminderListItem
    {
        /// <summary>
        /// 重复频率的列表项目
        /// </summary>
        public class LoopListItem
        {
            private bool isChecked = false;
            public bool IsChecked
            {
                get { return isChecked; }
                set
                {
                    isChecked = value;
                    opacity = value ? 1 : 0.3;
                }
            }
            private double opacity = 0.3;
            //有引用！！！
            public double Opacity
            {
                get { return opacity; }
                set
                {
                    opacity = value;
                    isChecked = value > 0.5;
                }
            }
        }

        /// <summary>
        /// 标示该提醒类型
        /// </summary>
        public enum ListItemEnum
        {
            /// <summary>
            /// 闹钟
            /// </summary>
            AlarmClock,
            /// <summary>
            /// 日程
            /// </summary>
            Schedule,
            /// <summary>
            /// 待办事项
            /// </summary>
            ToDo
        }
        /// <summary>
        /// 默认构建的是待办事项类型
        /// </summary>
        public ReminderListItem()
        {
            TitleString = string.Empty;
            AlarmTimeString = "0000/00/00        00:00";//8个英文空格
            LoopListForSchedule = new List<LoopListItem>
            {
                new LoopListItem(), new LoopListItem()
            };
            LoopListForAlarmClock = new List<LoopListItem>
            {
                new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem()
            };
            BodyString = string.Empty;
            IsToDoFinish = false;
            SubToDoList = new ObservableCollection<SubToDoListItem>();
        }
        /// <summary>
        /// 根据参数构建不同类型的项目
        /// </summary>
        /// <param name="classificationEnum">需要构建的类型</param>
        public ReminderListItem(ListItemEnum classificationEnum)
        {
            classification = classificationEnum;
            TitleString = string.Empty;
            AlarmTimeString = "0000/00/00        00:00";//8个英文空格
            LoopListForSchedule = new List<LoopListItem>
            {
                new LoopListItem(), new LoopListItem()
            };
            LoopListForAlarmClock = new List<LoopListItem>
            {
                new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem(), new LoopListItem()
            };
            BodyString = string.Empty;
            IsToDoFinish = false;
            SubToDoList = new ObservableCollection<SubToDoListItem>();

            switch (classification)
            {
                //闹钟
                case ListItemEnum.AlarmClock:
                    cornerColorBrush = new SolidColorBrush(Colors.DodgerBlue);
                    glyph = "\uEA8F";
                    loopMarkStackPanelVisibility = Visibility.Visible;
                    loopMarkStackPanelForAlarmClockVisibility = Visibility.Visible;
                    loopMarkStackPanelForScheduleVisibility = Visibility.Collapsed;
                    bodyTextBlockHeight = 38;
                    bodyTextBlockPadding = new Thickness(0, 0, 0, 0);
                    toDoCheckBoxVisibility = Visibility.Collapsed;
                    break;
                //日程
                case ListItemEnum.Schedule:
                    cornerColorBrush = new SolidColorBrush(Colors.MediumPurple);
                    glyph = "\uE737";
                    loopMarkStackPanelVisibility = Visibility.Visible;
                    loopMarkStackPanelForScheduleVisibility = Visibility.Visible;
                    loopMarkStackPanelForAlarmClockVisibility = Visibility.Collapsed;
                    bodyTextBlockHeight = 38;
                    bodyTextBlockPadding = new Thickness(0, 0, 0, 0);
                    toDoCheckBoxVisibility = Visibility.Collapsed;
                    break;
                //待办事项
                case ListItemEnum.ToDo:
                    cornerColorBrush = new SolidColorBrush(Colors.Orange);
                    glyph = "\uE9D5";
                    loopMarkStackPanelVisibility = Visibility.Collapsed;
                    bodyTextBlockHeight = 68;
                    bodyTextBlockPadding = new Thickness(0, 5, 0, 5);
                    toDoCheckBoxVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// 项目类型
        /// </summary>
        private ListItemEnum classification = ListItemEnum.ToDo;
        public ListItemEnum Classification
        {
            get { return classification; }
            set
            {
                classification = value;
                switch (classification)
                {
                    //闹钟
                    case ListItemEnum.AlarmClock:
                        cornerColorBrush.Color = Colors.DodgerBlue;
                        glyph = "\uEA8F";
                        loopMarkStackPanelVisibility = Visibility.Visible;
                        loopMarkStackPanelForAlarmClockVisibility = Visibility.Visible;
                        loopMarkStackPanelForScheduleVisibility = Visibility.Collapsed;
                        bodyTextBlockHeight = 38;
                        bodyTextBlockPadding = new Thickness(0, 0, 0, 0);
                        toDoCheckBoxVisibility = Visibility.Collapsed;
                        break;
                    //日程
                    case ListItemEnum.Schedule:
                        cornerColorBrush.Color = Colors.MediumPurple;
                        glyph = "\uE737";
                        loopMarkStackPanelVisibility = Visibility.Visible;
                        loopMarkStackPanelForScheduleVisibility = Visibility.Visible;
                        loopMarkStackPanelForAlarmClockVisibility = Visibility.Collapsed;
                        bodyTextBlockHeight = 38;
                        bodyTextBlockPadding = new Thickness(0, 0, 0, 0);
                        toDoCheckBoxVisibility = Visibility.Collapsed;
                        break;
                    //待办事项
                    case ListItemEnum.ToDo:
                        cornerColorBrush.Color = Colors.Orange;
                        glyph = "\uE9D5";
                        loopMarkStackPanelVisibility = Visibility.Collapsed;
                        bodyTextBlockHeight = 68;
                        bodyTextBlockPadding = new Thickness(0, 5, 0, 5);
                        toDoCheckBoxVisibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 角标颜色的画笔
        /// </summary>
        private readonly SolidColorBrush cornerColorBrush = new SolidColorBrush(Colors.Orange);
        public SolidColorBrush CornerColorBrush { get { return cornerColorBrush; } }
        /// <summary>
        /// 字体图标字符串
        /// </summary>
        private string glyph = "\uE9D5";
        public string Glyph { get { return glyph; } }
        /// <summary>
        /// 重复频率显示框架的可视性
        /// </summary>
        private Visibility loopMarkStackPanelVisibility = Visibility.Collapsed;
        public Visibility LoopMarkStackPanelVisibility { get { return loopMarkStackPanelVisibility; } }
        /// <summary>
        /// 日程重复频率可视性
        /// </summary>
        private Visibility loopMarkStackPanelForScheduleVisibility = Visibility.Collapsed;
        public Visibility LoopMarkStackPanelForScheduleVisibility { get { return loopMarkStackPanelForScheduleVisibility; } }
        /// <summary>
        /// 闹钟重复频率可视性
        /// </summary>
        private Visibility loopMarkStackPanelForAlarmClockVisibility = Visibility.Collapsed;
        public Visibility LoopMarkStackPanelForAlarmClockVisibility { get { return loopMarkStackPanelForAlarmClockVisibility; } }
        /// <summary>
        /// 内容显示部分的高度
        /// </summary>
        private double bodyTextBlockHeight = 68;
        public double BodyTextBlockHeight { get { return bodyTextBlockHeight; } }
        /// <summary>
        /// 内容显示部分的向内留白程度
        /// </summary>
        private Thickness bodyTextBlockPadding = new Thickness(0, 5, 0, 5);
        public Thickness BodyTextBlockPadding { get { return bodyTextBlockPadding; } }
        /// <summary>
        /// 待办事项的打勾框可视性
        /// </summary>
        private Visibility toDoCheckBoxVisibility = Visibility.Visible;
        public Visibility ToDoCheckBoxVisibility { get { return toDoCheckBoxVisibility; } }
        /// <summary>
        /// 标示待办事项是否已完成
        /// </summary>
        private bool isToDoFinish = false;
        public bool IsToDoFinish
        {
            get
            {
                return isToDoFinish;
            }
            set
            {
                isToDoFinish = value;
                isToDoFinishOpacity = value ? 0.3 : 1;
            }
        }
        private double isToDoFinishOpacity = 1;
        public double IsToDoFinishOpacity
        {
            get
            {
                return isToDoFinishOpacity;
            }
        }
        /// <summary>
        /// 副待办事项列表
        /// </summary>
        public ObservableCollection<SubToDoListItem> SubToDoList { get; set; }
        /// <summary>
        /// 日程重复频率的显示列表
        /// </summary>
        public List<LoopListItem> LoopListForSchedule { get; set; }
        /// <summary>
        /// 闹钟重复频率的显示列表
        /// </summary>
        public List<LoopListItem> LoopListForAlarmClock { get; set; }
        /// <summary>
        /// 提醒时间（年月日时分）
        /// </summary>
        public string AlarmTimeString { get; set; }
        /// <summary>
        /// 项目标题
        /// </summary>
        public string TitleString { get; set; }
        /// <summary>
        /// 项目内容
        /// </summary>
        public string BodyString { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最后的修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 发起提醒事件的时间
        /// </summary>
        private DateTime alarmTime = new DateTime();
        public DateTime AlarmTime
        {
            get
            {
                return alarmTime;
            }
            set
            {
                alarmTime = value;
                if (value == new DateTime(1601, 1, 1, 8, 0, 0))
                    AlarmTimeString = "0000/00/00        00:00";
                else
                    AlarmTimeString = alarmTime.ToString("yyyy/MM/dd") + "        " + alarmTime.ToString("HH:mm");
            }
        }

        /// <summary>
        /// 从另一个对象中复制关键的值
        /// </summary>
        /// <param name="sourceItem">从此对象复制</param>
        public void CopyValueFrom(ReminderListItem sourceItem)
        {
            Classification = sourceItem.Classification;
            AlarmTime = sourceItem.AlarmTime;
            LoopListForAlarmClock = sourceItem.LoopListForAlarmClock;
            LoopListForSchedule = sourceItem.LoopListForSchedule;
            TitleString = sourceItem.TitleString;
            BodyString = sourceItem.BodyString;
            CreateTime = sourceItem.CreateTime;
            UpdateTime = sourceItem.UpdateTime;
            IsToDoFinish = sourceItem.IsToDoFinish;
            SubToDoList.Clear();
            foreach (SubToDoListItem item in sourceItem.SubToDoList)
            {
                SubToDoList.Add(item);
            }
        }

        public int GetTheRestCountOfSubToDoList()
        {
            int result = SubToDoList.Count;
            foreach (SubToDoListItem item in SubToDoList)
            {
                if (item.IsChecked)
                    result--;
            }
            return result;
        }
    }



    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReminderPage : Page
    {
        public static ReminderListItem newItem = new ReminderListItem();//默认待办事项
        public static List<ToggleButton> toggleButtons = new List<ToggleButton>();
        public double scrollViewerVerticalOffset = 0;
        public double pointerPosition = 0;
        public double widthOfStack = 0;
        public double minWidthOfStack = 0;
        public double maxWidthOfStack = 0;
        public double changeValue = 0;
        public bool thisSubToDoItemIsSeletedBefore = false;
        public static string subToDoListCountString = "0";
        public static string subToDoListRestString = "0";
        public DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 50)
        };
        public bool thisSubToDoItemIsAddFromUser = false;

        public ReminderPage()
        {
            this.InitializeComponent();
            toggleButtons = new List<ToggleButton>
            {
                toggle0, toggle1, toggle2, toggle3, toggle4, toggle5, toggle6
            };
            switch (Common.nowWhoFirst)
            {
                case Common.WhoFirstEnum.CreateTimeFirst:
                    createTimeFirstMenuButton.IsChecked = true;
                    CreateTimeFirstMenuButton_Click(new object(), new RoutedEventArgs());
                    break;
                case Common.WhoFirstEnum.UpdateTimeFirst:
                    updateTimeFirstMenuButton.IsChecked = true;
                    UpdateTimeFirstMenuButton_Click(new object(), new RoutedEventArgs());
                    break;
                case Common.WhoFirstEnum.AlarmTimeFirst:
                    alarmTimeFirstMenuButton.IsChecked = true;
                    AlarmTimeFirstMenuButton_Click(new object(), new RoutedEventArgs());
                    break;
                default:
                    break;
            }
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            subToDoListCountTextBlock.Text = subToDoListCountTextBlockInToolTip.Text = newItem.SubToDoList.Count.ToString();
            subToDoListRestTextBlock.Text = subToDoListRestTextBlockInToolTip.Text = newItem.GetTheRestCountOfSubToDoList().ToString();
            subToDoListTheRestCountInfoBadge.Value = newItem.GetTheRestCountOfSubToDoList();
            if (newItem.SubToDoList.Count > 0)
            {
                newItem.IsToDoFinish = newItem.GetTheRestCountOfSubToDoList() == 0;
            }
        }

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (detailGrid.Visibility == Visibility.Collapsed || issueList.SelectedIndex != -1)
            {
                issueList.SelectedIndex = -1;
                classificationOfNewItem.SelectedIndex = 2;
                for (int i = 0; i < 7; i++)
                {
                    toggleButtons[i].IsChecked = false;
                }
                everyMonthToggleButton.IsChecked = false;
                everyYearToggleButton.IsChecked = false;
                newItemBodyTextBlock.Text = string.Empty;
                newItemTitleTextBlock.Text = string.Empty;
                datePickerOfNewItem.SelectedDate = null;
                timePickerOfNewItem.SelectedTime = null;
                newItem.Classification = ReminderListItem.ListItemEnum.ToDo;
                newItem.IsToDoFinish = false;
                newItem.SubToDoList.Clear();
                datePickerOfNewItem.MinYear = new DateTimeOffset(DateTime.Now.AddYears(-1));
                detailGrid.Visibility = Visibility.Visible;
                detailGridInStory.Begin();
            }
        }

        private void DeleteNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (issueList.SelectedIndex != -1)
            {
                Common.reminderList.RemoveAt(issueList.SelectedIndex);
                detailGridOutStory.Begin();
            }
        }

        private void ClassificationOfNewItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (newItem != null && loopForAlarmClockOfNewItem != null && loopForScheduleOfNewItem != null)
            {
                switch (classificationOfNewItem.SelectedIndex)
                {
                    case 0:
                        newItem.Classification = ReminderListItem.ListItemEnum.AlarmClock;
                        VariableSizedWrapGrid.SetColumnSpan(loopForScheduleOfNewItem, 0);
                        VariableSizedWrapGrid.SetColumnSpan(loopForAlarmClockOfNewItem, 50);
                        VariableSizedWrapGrid.SetColumnSpan(datePickerOfNewItem, 0);
                        VariableSizedWrapGrid.SetColumnSpan(subToDoListToggleButtonGrid, 0);
                        break;
                    case 1:
                        newItem.Classification = ReminderListItem.ListItemEnum.Schedule;
                        VariableSizedWrapGrid.SetColumnSpan(loopForScheduleOfNewItem, 25);
                        VariableSizedWrapGrid.SetColumnSpan(loopForAlarmClockOfNewItem, 0);
                        VariableSizedWrapGrid.SetColumnSpan(datePickerOfNewItem, 62);
                        VariableSizedWrapGrid.SetColumnSpan(subToDoListToggleButtonGrid, 0);
                        datePickerOfNewItem.MinYear = new DateTimeOffset(DateTime.Now);
                        break;
                    case 2:
                        newItem.Classification = ReminderListItem.ListItemEnum.ToDo;
                        VariableSizedWrapGrid.SetColumnSpan(loopForScheduleOfNewItem, 0);
                        VariableSizedWrapGrid.SetColumnSpan(loopForAlarmClockOfNewItem, 0);
                        VariableSizedWrapGrid.SetColumnSpan(datePickerOfNewItem, 62);
                        VariableSizedWrapGrid.SetColumnSpan(subToDoListToggleButtonGrid, 24);
                        datePickerOfNewItem.MinYear = new DateTimeOffset(DateTime.Now.AddYears(-1));
                        break;
                    default:
                        break;
                }
                datePickerOfNewItem.SelectedDate = null;
                timePickerOfNewItem.SelectedTime = null;
                wrapGrid.ItemWidth = 6;
                wrapGrid.ItemWidth = 5;
                subToDoListToggleButton.IsChecked = false;
                SubToDoListToggleButton_Click(subToDoListToggleButton, new RoutedEventArgs());
            }
        }

        private void NewItemSaveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (newItem.Classification)
            {
                case ReminderListItem.ListItemEnum.AlarmClock:
                    if (timePickerOfNewItem.SelectedTime == null)
                    {
                        info.Severity = InfoBarSeverity.Error;
                        info.Title = "错误";
                        info.Message = "时间未选择，请先选择时间后再保存项目。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                    }
                    else
                    {
                        if (timePickerOfNewItem.Time < DateTime.Now.TimeOfDay)
                        {
                            newItem.AlarmTime = DateTime.Today.AddDays(1).AddHours(timePickerOfNewItem.Time.TotalHours);
                        }
                        else
                        {
                            newItem.AlarmTime = DateTime.Today.AddHours(timePickerOfNewItem.Time.TotalHours);
                        }
                        bool isLoop = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (newItem.LoopListForAlarmClock[i].IsChecked == true)
                            {
                                isLoop = true;
                                break;
                            }
                        }
                        if (isLoop)
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                if (newItem.LoopListForAlarmClock[(int)newItem.AlarmTime.DayOfWeek].IsChecked)
                                {
                                    break;
                                }
                                else
                                {
                                    newItem.AlarmTime = newItem.AlarmTime.AddDays(1);
                                }
                            }
                        }
                    }
                    break;
                case ReminderListItem.ListItemEnum.Schedule:
                    if (datePickerOfNewItem.SelectedDate == null && timePickerOfNewItem.SelectedTime == null)
                    {
                        info.Severity = InfoBarSeverity.Error;
                        info.Title = "错误";
                        info.Message = "日期与时间均未选择，请先选择后再保存项目。已自动将时间填充软件推荐值。\n\n" +
                            "时间推荐值为 09:00 。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                        timePickerOfNewItem.SelectedTime = new TimeSpan(9, 0, 0);
                    }
                    else if (datePickerOfNewItem.SelectedDate == null)
                    {
                        info.Severity = InfoBarSeverity.Error;
                        info.Title = "错误";
                        info.Message = "日期未选择，请先选择后再保存项目。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                    }
                    else if (timePickerOfNewItem.SelectedTime == null)
                    {
                        info.Severity = InfoBarSeverity.Warning;
                        info.Title = "警告";
                        info.Message = "时间未选择，已自动将时间填充软件推荐值。\n\n" +
                            "时间软件推荐值为 09:00 。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                        timePickerOfNewItem.SelectedTime = new TimeSpan(9, 0, 0);
                    }
                    else
                    {
                        if (datePickerOfNewItem.Date.Date.AddHours(timePickerOfNewItem.Time.TotalHours) > DateTime.Now)
                            newItem.AlarmTime = datePickerOfNewItem.Date.Date.AddHours(timePickerOfNewItem.Time.TotalHours);
                        else
                        {
                            datePickerOfNewItem.SelectedDate = null;
                            timePickerOfNewItem.SelectedTime = null;
                            info.Severity = InfoBarSeverity.Error;
                            info.Title = "错误";
                            info.Message = "选择的日期时间组合不在当前时间之后，请重新选择。";
                            info.IsOpen = true;
                            infoBarOpen.Begin();
                        }
                    }
                    break;
                case ReminderListItem.ListItemEnum.ToDo:
                    if (datePickerOfNewItem.SelectedDate == null && timePickerOfNewItem.SelectedTime == null)
                    {
                        newItem.AlarmTime = new DateTime(1601, 1, 1, 8, 0, 0);
                    }
                    else if (datePickerOfNewItem.SelectedDate == null)
                    {
                        info.Severity = InfoBarSeverity.Error;
                        info.Title = "错误";
                        info.Message = "日期未选择，请先选择后再保存项目。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                    }
                    else if (timePickerOfNewItem.SelectedTime == null)
                    {
                        info.Severity = InfoBarSeverity.Warning;
                        info.Title = "警告";
                        info.Message = "时间未选择，已自动将时间填充软件推荐值。\n\n" +
                            "时间软件推荐值为 09:00 。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                        timePickerOfNewItem.SelectedTime = new TimeSpan(9, 0, 0);
                    }
                    else
                    {
                        if (datePickerOfNewItem.Date.Date.AddHours(timePickerOfNewItem.Time.TotalHours) > DateTime.Now)
                            newItem.AlarmTime = datePickerOfNewItem.Date.Date.AddHours(timePickerOfNewItem.Time.TotalHours);
                        else
                        {
                            datePickerOfNewItem.SelectedDate = null;
                            timePickerOfNewItem.SelectedTime = null;
                            info.Severity = InfoBarSeverity.Error;
                            info.Title = "错误";
                            info.Message = "选择的日期时间组合不在当前时间之后，请重新选择。";
                            info.IsOpen = true;
                            infoBarOpen.Begin();
                        }
                    }
                    break;
                default:
                    break;
            }
            if (info.IsOpen) return;

            if (newItemTitleTextBlock.Text == "")
            {
                newItem.TitleString = "无标题";
            }
            if (newItemBodyTextBlock.Text == "")
            {
                newItem.BodyString = "无内容";
            }
            var item = new ReminderListItem(newItem.Classification);
            item.CopyValueFrom(newItem);

            if (issueList.SelectedIndex == -1)
            {
                //create new item
                item.CreateTime = DateTime.Now;
                Common.reminderList.Insert(SortAnReminderItem(item), item);
                switch (item.Classification)
                {
                    case ReminderListItem.ListItemEnum.AlarmClock:
                        displayAlarmClockCheckBox.IsChecked = true;
                        break;
                    case ReminderListItem.ListItemEnum.Schedule:
                        displayScheduleCheckBox.IsChecked = true;
                        break;
                    case ReminderListItem.ListItemEnum.ToDo:
                        displayUnfinishedCheckBox.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //modify item
                item.UpdateTime = DateTime.Now;
                int index = issueList.SelectedIndex;
                Common.reminderList.RemoveAt(index);
                if (Common.nowWhoFirst == Common.WhoFirstEnum.AlarmTimeFirst)
                {
                    if (item.AlarmTime.Year == 1601)
                    {
                        Common.reminderList.Add(item);
                    }
                    else
                        foreach (ReminderListItem compareItem in Common.reminderList)
                        {
                            if (compareItem.AlarmTime >= item.AlarmTime || compareItem.AlarmTime.CompareTo(DateTime.Now) < 0)
                            {
                                Common.reminderList.Insert(Common.reminderList.IndexOf(compareItem), item);
                                break;
                            }
                            else if (compareItem == Common.reminderList.Last())
                            {
                                Common.reminderList.Add(item);
                                break;
                            }
                        }
                }
                else if (Common.nowWhoFirst == Common.WhoFirstEnum.UpdateTimeFirst)
                {
                    Common.reminderList.Insert(0, item);
                }
                else
                {
                    Common.reminderList.Insert(index, item);
                }

            }
            detailGridOutStory.Begin();

            IOrderedEnumerable<ReminderListItem> orderList = from orderItem in Common.reminderList
                                                             orderby orderItem.AlarmTime ascending
                                                             select orderItem;
            Common.needToRemindList = new ObservableCollection<ReminderListItem>(orderList);
            int totalIgnoreNum = 0;
            int nowIndex = 0;
            while (totalIgnoreNum + nowIndex != Common.reminderList.Count)
            {
                if (Common.needToRemindList[nowIndex].AlarmTime.Year == 1601 || Common.needToRemindList[nowIndex].AlarmTime.CompareTo(DateTime.Now) < 0 ||
                    (Common.needToRemindList[nowIndex].Classification == ReminderListItem.ListItemEnum.ToDo && Common.needToRemindList[nowIndex].IsToDoFinish == true))
                {
                    Common.needToRemindList.RemoveAt(nowIndex);
                    totalIgnoreNum++;
                }
                else nowIndex++;
            }
        }

        private void NewItemCancelButton_Click(object sender, RoutedEventArgs e)
        {
            detailGridOutStory.Begin();
            issueList.SelectedIndex = -1;
        }

        private void DetailGridOutStory_Completed(object sender, object e)
        {
            detailGrid.Visibility = Visibility.Collapsed;
            detailGrid.Opacity = 1;
        }

        private void EveryYearToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            everyMonthToggleButton.IsChecked = false;
        }

        private void EveryMonthToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            everyYearToggleButton.IsChecked = false;
        }

        private void IssueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (issueList.SelectedIndex != -1)
            {
                newItem.CopyValueFrom(Common.reminderList[issueList.SelectedIndex]);
                classificationOfNewItem.SelectedIndex = (int)newItem.Classification;

                if (newItem.Classification == ReminderListItem.ListItemEnum.ToDo)
                {
                    _ = VisualStateManager.GoToState((issueList.ItemsPanelRoot.Children[issueList.SelectedIndex] as ListViewItem).ContentTemplateRoot as Control, "ToDoItemSelected", true);
                    (issueList.ItemsPanelRoot.Children[issueList.SelectedIndex] as ListViewItem).RegisterPropertyChangedCallback(ListViewItem.IsSelectedProperty, ListViewToDoItemIsSelectedChanged);
                }

                for (int i = 0; i < 7; i++)
                {
                    toggleButtons[i].IsChecked = Common.reminderList[issueList.SelectedIndex].LoopListForAlarmClock[i].IsChecked;
                }
                everyYearToggleButton.IsChecked = Common.reminderList[issueList.SelectedIndex].LoopListForSchedule[0].IsChecked;
                everyMonthToggleButton.IsChecked = Common.reminderList[issueList.SelectedIndex].LoopListForSchedule[1].IsChecked;
                newItemBodyTextBlock.Text = Common.reminderList[issueList.SelectedIndex].BodyString;
                newItemTitleTextBlock.Text = Common.reminderList[issueList.SelectedIndex].TitleString;

                if (newItem.AlarmTime.Year != 1601)
                {
                    datePickerOfNewItem.Date = newItem.AlarmTime.Date;
                    timePickerOfNewItem.Time = newItem.AlarmTime.TimeOfDay;
                }
                else
                {
                    datePickerOfNewItem.SelectedDate = null;
                    timePickerOfNewItem.SelectedTime = null;
                }

                newItemTitleTextBlock.Text = Common.reminderList[issueList.SelectedIndex].TitleString;
                newItemBodyTextBlock.Text = Common.reminderList[issueList.SelectedIndex].BodyString;

                detailGrid.Visibility = Visibility.Visible;
                detailGridInStory.Begin();
            }
        }

        private void ListViewToDoItemIsSelectedChanged(DependencyObject sender, DependencyProperty dp)
        {
            _ = VisualStateManager.GoToState((sender as ListViewItem).ContentTemplateRoot as Control, "ToDoItemUnSelected", true);
            (sender as ListViewItem).UnregisterPropertyChangedCallback(dp, 0);
        }

        private void SelectDisplayGroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (displayUnfinishedCheckBox != null &&
                displayFinishedCheckBox != null &&
                displayAlarmClockCheckBox != null &&
                displayScheduleCheckBox != null)
            {
                if (e.OriginalSource.Equals(displayToDoCheckBox))
                {
                    displayUnfinishedCheckBox.IsChecked = true;
                    displayFinishedCheckBox.IsChecked = true;
                }
                else if (e.OriginalSource.Equals(displayUnfinishedCheckBox))
                {
                    if (displayFinishedCheckBox.IsChecked == true)
                    {
                        displayToDoCheckBox.IsChecked = true;
                    }
                    else if (displayFinishedCheckBox.IsChecked == false)
                    {
                        displayToDoCheckBox.IsChecked = null;
                    }
                }
                else if (e.OriginalSource.Equals(displayFinishedCheckBox))
                {
                    if (displayUnfinishedCheckBox.IsChecked == true)
                    {
                        displayToDoCheckBox.IsChecked = true;
                    }
                    else if (displayUnfinishedCheckBox.IsChecked == false)
                    {
                        displayToDoCheckBox.IsChecked = null;
                    }
                }
                UpdateItemsDisplay(true);
            }
        }

        private void SelectDisplayGroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.Equals(displayToDoCheckBox))
            {
                displayUnfinishedCheckBox.IsChecked = false;
                displayFinishedCheckBox.IsChecked = false;
            }
            else if (e.OriginalSource.Equals(displayUnfinishedCheckBox))
            {
                if (displayFinishedCheckBox.IsChecked == true)
                {
                    displayToDoCheckBox.IsChecked = null;
                }
                else if (displayFinishedCheckBox.IsChecked == false)
                {
                    displayToDoCheckBox.IsChecked = false;
                }
            }
            else if (e.OriginalSource.Equals(displayFinishedCheckBox))
            {
                if (displayUnfinishedCheckBox.IsChecked == true)
                {
                    displayToDoCheckBox.IsChecked = null;
                }
                else if (displayUnfinishedCheckBox.IsChecked == false)
                {
                    displayToDoCheckBox.IsChecked = false;
                }
            }
            UpdateItemsDisplay(false);
        }

        private void DisplayToDoCheckBox_Indeterminate(object sender, RoutedEventArgs e)
        {
            //next state of checked is indeterminate
            if (e.OriginalSource.Equals(displayToDoCheckBox))
            {
                if (displayUnfinishedCheckBox.IsChecked == true && displayFinishedCheckBox.IsChecked == true)
                {
                    displayToDoCheckBox.IsChecked = false;
                    UpdateItemsDisplay(false);
                }
                else if (displayUnfinishedCheckBox.IsChecked == false && displayFinishedCheckBox.IsChecked == false)
                {
                    displayToDoCheckBox.IsChecked = true;
                    UpdateItemsDisplay(true);
                }
            }
        }

        private void UpdateItemsDisplay(bool changeToTrueOrFalse)
        {
            if (changeToTrueOrFalse)
            {
                for (int i = 0; i < Common.tempList.Count; i++)
                {
                    ReminderListItem item = Common.tempList[i];
                    switch (item.Classification)
                    {
                        case ReminderListItem.ListItemEnum.AlarmClock:
                            if (displayAlarmClockCheckBox.IsChecked == true)
                            {
                                Common.reminderList.Insert(SortAnReminderItem(item), item);
                                Common.tempList.Remove(item);
                                i--;
                            }
                            break;
                        case ReminderListItem.ListItemEnum.Schedule:
                            if (displayScheduleCheckBox.IsChecked == true)
                            {
                                Common.reminderList.Insert(SortAnReminderItem(item), item);
                                Common.tempList.Remove(item);
                                i--;
                            }
                            break;
                        case ReminderListItem.ListItemEnum.ToDo:
                            if (displayUnfinishedCheckBox.IsChecked == true && item.IsToDoFinish == false)
                            {
                                Common.reminderList.Insert(SortAnReminderItem(item), item);
                                Common.tempList.Remove(item);
                                i--;
                            }
                            if (displayFinishedCheckBox.IsChecked == true && item.IsToDoFinish == true)
                            {
                                Common.reminderList.Insert(SortAnReminderItem(item), item);
                                Common.tempList.Remove(item);
                                i--;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Common.reminderList.Count; i++)
                {
                    ReminderListItem item = Common.reminderList[i];
                    switch (item.Classification)
                    {
                        case ReminderListItem.ListItemEnum.AlarmClock:
                            if (displayAlarmClockCheckBox.IsChecked == false)
                            {
                                Common.tempList.Add(item);
                                Common.reminderList.Remove(item);
                                i--;
                            }
                            break;
                        case ReminderListItem.ListItemEnum.Schedule:
                            if (displayScheduleCheckBox.IsChecked == false)
                            {
                                Common.tempList.Add(item);
                                Common.reminderList.Remove(item);
                                i--;
                            }
                            break;
                        case ReminderListItem.ListItemEnum.ToDo:
                            if (displayUnfinishedCheckBox.IsChecked == false && item.IsToDoFinish == false)
                            {
                                Common.tempList.Add(item);
                                Common.reminderList.Remove(item);
                                i--;
                            }
                            if (displayFinishedCheckBox.IsChecked == false && item.IsToDoFinish == true)
                            {
                                Common.tempList.Add(item);
                                Common.reminderList.Remove(item);
                                i--;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private int SortAnReminderItem(ReminderListItem item)
        {
            int index = Common.reminderList.Count;
            //sort
            if (createTimeFirstMenuButton.IsChecked == true)
            {
                for (int j = 0; j < index; j++)
                {
                    if (item.CreateTime > Common.reminderList[j].CreateTime)
                    {
                        index = j;
                        break;
                    }
                }
            }
            else if (updateTimeFirstMenuButton.IsChecked == true)
            {
                for (int j = 0; j < index; j++)
                {
                    if (item.UpdateTime > Common.reminderList[j].UpdateTime)
                    {
                        index = j;
                        break;
                    }
                }
            }
            else if (alarmTimeFirstMenuButton.IsChecked == true)
            {
                if (item.AlarmTime.Year != 1601)
                {
                    for (int j = 0; j < index; j++)
                    {
                        if (item.AlarmTime < Common.reminderList[j].AlarmTime && Common.reminderList[j].AlarmTime.Year != 1601)
                        {
                            index = j;
                            break;
                        }
                        else if (Common.reminderList[j].AlarmTime.Year == 1601)
                        {
                            index = j;
                            break;
                        }
                    }
                }
            }
            return index;
        }

        private void CreateTimeFirstMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Common.nowWhoFirst = Common.WhoFirstEnum.CreateTimeFirst;
            if (createTimeFirstMenuButton.IsChecked == true)
            {
                updateTimeFirstMenuButton.IsChecked = false;
                alarmTimeFirstMenuButton.IsChecked = false;
                IOrderedEnumerable<ReminderListItem> orderList = from item in Common.reminderList
                                                                 orderby item.CreateTime descending
                                                                 select item;
                Common.reminderList = new ObservableCollection<ReminderListItem>(orderList);
                issueList.ItemsSource = Common.reminderList;
            }
            else
            {
                createTimeFirstMenuButton.IsChecked = true;
            }
        }

        private void UpdateTimeFirstMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Common.nowWhoFirst = Common.WhoFirstEnum.UpdateTimeFirst;
            if (updateTimeFirstMenuButton.IsChecked == true)
            {
                createTimeFirstMenuButton.IsChecked = false;
                alarmTimeFirstMenuButton.IsChecked = false;
                IOrderedEnumerable<ReminderListItem> orderList = from item in Common.reminderList
                                                                 orderby item.UpdateTime descending
                                                                 select item;
                Common.reminderList = new ObservableCollection<ReminderListItem>(orderList);
                issueList.ItemsSource = Common.reminderList;
            }
            else
            {
                updateTimeFirstMenuButton.IsChecked = true;
            }
        }

        private void AlarmTimeFirstMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Common.nowWhoFirst = Common.WhoFirstEnum.AlarmTimeFirst;
            if (alarmTimeFirstMenuButton.IsChecked == true)
            {
                updateTimeFirstMenuButton.IsChecked = false;
                createTimeFirstMenuButton.IsChecked = false;
                IOrderedEnumerable<ReminderListItem> orderList = from item in Common.reminderList
                                                                 orderby item.AlarmTime ascending
                                                                 select item;
                Common.reminderList = new ObservableCollection<ReminderListItem>(orderList);
                int totalIgnoreNum = 0;
                int nowIndex = 0;
                while (totalIgnoreNum + nowIndex != Common.reminderList.Count && Common.reminderList.Count > 0)
                {
                    if (Common.reminderList[nowIndex].AlarmTime.Year == 1601 || Common.reminderList[nowIndex].AlarmTime.CompareTo(DateTime.Now) < 0 ||
                        (Common.reminderList[nowIndex].Classification == ReminderListItem.ListItemEnum.ToDo && Common.reminderList[nowIndex].IsToDoFinish == true))
                    {
                        Common.reminderList.Move(nowIndex, Common.reminderList.Count - 1);
                        totalIgnoreNum++;
                    }
                    else nowIndex++;
                }
                issueList.ItemsSource = Common.reminderList;
            }
            else
            {
                alarmTimeFirstMenuButton.IsChecked = true;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            scrollViewerVerticalOffset = (sender as ScrollViewer).VerticalOffset;
        }

        private async void FinishCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //为了躲避初始化时进入而不使用Checked和UnChecked两个事件

            int selectedIndex = (int)((CoreApplication.GetCurrentView().CoreWindow.PointerPosition.Y + scrollViewerVerticalOffset - CoreApplication.GetCurrentView().CoreWindow.Bounds.Top) / 140);
            ReminderListItem item = Common.reminderList[selectedIndex];
            CheckBox box = sender as CheckBox;
            if (box.IsChecked == true)
            {
                if (item.GetTheRestCountOfSubToDoList() > 0)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = "该项目还有小项未完成，请确认是否都已完成！",
                        PrimaryButtonText = "确认",
                        SecondaryButtonText = "取消",
                        CloseButtonText = "查看详情"
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    switch (result)
                    {
                        case ContentDialogResult.Primary:
                            foreach (SubToDoListItem subItem in item.SubToDoList)
                            {
                                subItem.IsChecked = true;
                            }
                            break;
                        case ContentDialogResult.Secondary:
                            box.IsChecked = false;
                            break;
                        case ContentDialogResult.None:
                            box.IsChecked = false;
                            issueList.SelectedIndex = selectedIndex;
                            break;
                        default:
                            break;
                    }
                    if (box.IsChecked == false)
                    {
                        return;
                    }
                }
                _ = VisualStateManager.GoToState((box.Parent as Grid).Parent as Control, "Finished", true);
                displayFinishedCheckBox.IsChecked = true;
                if (alarmTimeFirstMenuButton.IsChecked == true)
                {
                    item.IsToDoFinish = false;
                    Common.needToRemindList.Remove(item);
                    item.IsToDoFinish = true;
                    foreach (ReminderListItem compareItem in Common.reminderList)
                    {
                        if (compareItem.AlarmTime.CompareTo(DateTime.Now) < 0 || compareItem.IsToDoFinish)//比现在早，即过期项目
                        {
                            if (item == compareItem)
                                continue;
                            if (Common.reminderList.IndexOf(compareItem) - selectedIndex == 1)
                                break;
                            else
                            {
                                Common.reminderList.RemoveAt(selectedIndex);
                                Common.reminderList.Insert(Common.reminderList.IndexOf(compareItem), item);
                                break;
                            }
                        }
                        else if (compareItem == Common.reminderList.Last())
                        {
                            if (item != compareItem)
                            {
                                Common.reminderList.RemoveAt(selectedIndex);
                                Common.reminderList.Add(item);
                                break;
                            }
                        }
                    }
                }
            }
            else if (box.IsChecked == false)
            {
                if (item.SubToDoList.Count > 0)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = "确认将所有该项目的所有小项的状态更改为未完成吗？",
                        PrimaryButtonText = "确认",
                        SecondaryButtonText = "取消",
                        CloseButtonText = "查看详情"
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    switch (result)
                    {
                        case ContentDialogResult.Primary:
                            foreach (SubToDoListItem subItem in item.SubToDoList)
                            {
                                subItem.IsChecked = false;
                            }
                            break;
                        case ContentDialogResult.Secondary:
                            box.IsChecked = true;
                            break;
                        case ContentDialogResult.None:
                            box.IsChecked = true;
                            issueList.SelectedIndex = selectedIndex;
                            break;
                        default:
                            break;
                    }
                    if (box.IsChecked == true)
                    {
                        return;
                    }
                }
                _ = VisualStateManager.GoToState((box.Parent as Grid).Parent as Control, "Normal", true);
                if (alarmTimeFirstMenuButton.IsChecked == true)
                {
                    if (item.AlarmTime.CompareTo(DateTime.Now) > 0)
                    {
                        foreach (ReminderListItem compareItem in Common.reminderList)
                        {
                            if (compareItem.AlarmTime >= item.AlarmTime || compareItem.AlarmTime.CompareTo(DateTime.Now) < 0 || compareItem.IsToDoFinish)
                            {
                                if (item == compareItem)
                                    break;
                                if (Common.reminderList.IndexOf(compareItem) - selectedIndex == 1)
                                    break;
                                Common.reminderList.RemoveAt(selectedIndex);
                                Common.reminderList.Insert(Common.reminderList.IndexOf(compareItem), item);
                                break;
                            }
                            else if (compareItem == Common.reminderList.Last())
                            {
                                //Common.reminderList.Add(item);
                                break;
                            }
                        }
                        foreach (ReminderListItem compareItem in Common.needToRemindList)
                        {
                            if (compareItem.AlarmTime >= item.AlarmTime)
                            {
                                Common.needToRemindList.Insert(Common.needToRemindList.IndexOf(compareItem), item);
                                break;
                            }
                            else if (compareItem == Common.needToRemindList.Last())
                            {
                                Common.needToRemindList.Add(item);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void DatePickerOfNewItem_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if (issueList.SelectedIndex == -1)
            {
                if (args.NewDate != null)
                {
                    if (args.NewDate.Value.Date < DateTime.Today)
                    {
                        info.Severity = InfoBarSeverity.Warning;
                        info.Title = "警告";
                        info.Message = "选择的日期早于今天，若保存会被存为null，显示为“0000/00/00”。";
                        info.IsOpen = true;
                        infoBarOpen.Begin();
                        sender.SelectedDate = null;
                        timePickerOfNewItem.SelectedTime = null;
                    }
                }
            }
        }
        public enum PointerState
        {
            Pressed,
            Released
        }
        public PointerState pointerState = PointerState.Released;

        private void SubToDoWidthSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointerState = PointerState.Pressed;
            WidthSliderTriangleStoryboard.Seek(new TimeSpan(0, 0, 0, 0, 500));
            WidthSliderTriangleStoryboard.Pause();
            isXOffsetEnabled = true;
            pointerPosition = CoreApplication.GetCurrentView().CoreWindow.PointerPosition.X - CoreApplication.GetCurrentView().CoreWindow.Bounds.Left;
            widthOfStack = subToDoListScrollViewer.Width;
            minWidthOfStack = subToDoListScrollViewer.MinWidth;
            maxWidthOfStack = subToDoListScrollViewer.MaxWidth;
            CoreApplication.GetCurrentView().CoreWindow.PointerReleased += SubToDoWidthSlider_CoreWindowPointerReleased;
        }

        private void SubToDoWidthSlider_CoreWindowPointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            pointerState = PointerState.Released;
            WidthSliderTriangleStoryboard.Resume();
            isXOffsetEnabled = false;
            isPointerPressedAndOutOfRange = false;
            if (isPointerExited)
            {
                WidthSliderHideTriangleStoryboard.Begin();
                WidthSliderTriangleStoryboard.Stop();
                WidthSliderPointerExitedRepositionAnimation.Value = widthSliderYLocationOffset;
                WidthSliderPointerExitedRepositionStoryboard.Begin();
                CoreApplication.GetCurrentView().CoreWindow.PointerMoved -= SubToDoListSliderLocation_CoreWindowPointerMoved;
            }
            CoreApplication.GetCurrentView().CoreWindow.PointerReleased -= SubToDoWidthSlider_CoreWindowPointerReleased;
        }

        double widthSliderYLocationOffset = 0;
        private void SubToDoListSliderGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isPointerExited = false;
            if (pointerState == PointerState.Released)
            {
                widthSliderYLocationOffset = (subToDoListSliderGrid.ActualHeight / 2.0) - CoreApplication.GetCurrentView().CoreWindow.Bounds.Bottom + CoreApplication.GetCurrentView().CoreWindow.PointerPosition.Y;
                if ((subToDoListSliderGrid.ActualHeight / 2.0) + widthSliderYLocationOffset < 10)
                {
                    widthSliderYLocationOffset = 10 - (subToDoListSliderGrid.ActualHeight / 2.0);
                }
                else if ((subToDoListSliderGrid.ActualHeight / 2.0) - widthSliderYLocationOffset < 10)
                {
                    widthSliderYLocationOffset = (subToDoListSliderGrid.ActualHeight / 2.0) - 10;
                }
                WidthSliderPointerEnteredRepositionAnimation.Value = widthSliderYLocationOffset;
                WidthSliderPointerEnteredRepositionStoryboard.Begin();
            }
        }

        public bool isXOffsetEnabled = false;
        private void SubToDoListSliderLocation_CoreWindowPointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            widthSliderYLocationOffset = (subToDoListSliderGrid.ActualHeight / 2.0) - CoreApplication.GetCurrentView().CoreWindow.Bounds.Bottom + CoreApplication.GetCurrentView().CoreWindow.PointerPosition.Y;
            if ((subToDoListSliderGrid.ActualHeight / 2.0) + widthSliderYLocationOffset < 10)
            {
                widthSliderYLocationOffset = 10 - (subToDoListSliderGrid.ActualHeight / 2.0);
            }
            else if ((subToDoListSliderGrid.ActualHeight / 2.0) - widthSliderYLocationOffset < 10)
            {
                widthSliderYLocationOffset = (subToDoListSliderGrid.ActualHeight / 2.0) - 10;
            }
            (subToDoWidthSliderControl.RenderTransform as CompositeTransform).TranslateY = widthSliderYLocationOffset;

            if (isXOffsetEnabled)
            {
                changeValue = args.CurrentPoint.Position.X - pointerPosition;
                if (widthOfStack + changeValue <= minWidthOfStack)
                {
                    subToDoListScrollViewer.Width = minWidthOfStack;
                }
                else if (widthOfStack + changeValue >= maxWidthOfStack)
                {
                    subToDoListScrollViewer.Width = maxWidthOfStack;
                }
                else
                {
                    subToDoListScrollViewer.Width = widthOfStack + changeValue;
                }
            }
        }

        public bool isPointerPressedAndOutOfRange = false;
        private void WidthSliderPointerEnteredRepositionStoryboard_Completed(object sender, object e)
        {
            if (!isPointerExited)
            {
                CoreApplication.GetCurrentView().CoreWindow.PointerMoved += SubToDoListSliderLocation_CoreWindowPointerMoved;
                WidthSliderTriangleStoryboard.Begin();
            }
            isPointerPressedAndOutOfRange = false;
        }

        public bool isPointerExited = false;

        private void SubToDoListSliderGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPointerExited = true;
            switch (pointerState)
            {
                case PointerState.Pressed:
                    isPointerPressedAndOutOfRange = true;
                    break;
                case PointerState.Released:
                    CoreApplication.GetCurrentView().CoreWindow.PointerMoved -= SubToDoListSliderLocation_CoreWindowPointerMoved;
                    WidthSliderHideTriangleStoryboard.Begin();
                    WidthSliderTriangleStoryboard.Stop();
                    WidthSliderPointerExitedRepositionAnimation.Value = widthSliderYLocationOffset;
                    WidthSliderPointerExitedRepositionStoryboard.Begin();
                    break;
                default:
                    break;
            }
        }

        private void WidthSliderTriangleStoryboard_Completed(object sender, object e)
        {
            WidthSliderTriangleStoryboard.Begin();
        }

        private void SubToDoItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            (((sender as TextBox).Parent as Grid).Children[1] as TextBlock).Text = (sender as TextBox).Text;
            _ = VisualStateManager.GoToState(((sender as TextBox).Parent as Grid).Parent as Control, "Normal", true);
        }

        private void SubToDoListScrollViewer_ItemClick(object sender, ItemClickEventArgs e)
        {
            thisSubToDoItemIsSeletedBefore = (sender as ListView).SelectedIndex == newItem.SubToDoList.IndexOf(e.ClickedItem as SubToDoListItem);
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (thisSubToDoItemIsSeletedBefore)
            {
                _ = VisualStateManager.GoToState(sender as Control, "Edit", true);
                _ = (((sender as UserControl).Content as Grid).Children[2] as Control).Focus(FocusState.Programmatic);
                thisSubToDoItemIsSeletedBefore = false;
            }
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _ = VisualStateManager.GoToState(sender as Control, "Edit", true);
            _ = (((sender as UserControl).Content as Grid).Children[2] as Control).Focus(FocusState.Programmatic);
            thisSubToDoItemIsSeletedBefore = false;
        }

        private void SubToDoItemTextBox_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
        {
            if (args.Character == '\r')
            {
                _ = VisualStateManager.GoToState(((sender as TextBox).Parent as Grid).Parent as Control, "Normal", true);
                subToDoListScrollViewer.SelectedIndex = -1;
            }
        }

        private void SubToDoListAddButton_Click(object sender, RoutedEventArgs e)
        {
            newItem.SubToDoList.Add(new SubToDoListItem());
            thisSubToDoItemIsAddFromUser = true;
        }

        private void SubToDoListDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (subToDoListScrollViewer.SelectedIndex != -1)
            {
                newItem.SubToDoList.RemoveAt(subToDoListScrollViewer.SelectedIndex);
            }
        }

        private void SubToDoListScrollViewer_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (thisSubToDoItemIsAddFromUser)
            {
                sender.SelectedItem = args.Item;
            }
        }

        private void SubToDoListScrollViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (thisSubToDoItemIsAddFromUser)
            {
                thisSubToDoItemIsAddFromUser = false;
                if ((sender as ListView).ActualHeight - 88 <= (sender as ListView).ItemsPanelRoot.DesiredSize.Height)
                {
                    ((sender as ListView).ItemsPanelRoot.Children[(sender as ListView).SelectedIndex] as Control).StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true, VerticalOffset = -48 });
                }
            }
        }

        private void SubToDoListToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked == true)
            {
                newItemBodyTextBlock.Margin = new Thickness(20, 0, 0, 0);
            }
            else if ((sender as ToggleButton).IsChecked == false)
            {
                newItemBodyTextBlock.Margin = new Thickness(0, 0, 0, 0);
                subToDoListScrollViewer.SelectedIndex = -1;
            }
        }
    }
}
