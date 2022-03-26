using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClockApp
{
    /// <summary>
    /// App中任意页面都可以使用的变量类
    /// </summary>
    public class Common
    {
        public enum WhoFirstEnum
        {
            [Description("创建时间优先")]
            CreateTimeFirst,
            [Description("修改时间优先")]
            UpdateTimeFirst,
            [Description("提醒时间优先")]
            AlarmTimeFirst
        }
        public static DispatcherTimer mainTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1) };
        public static DisplayRequest keepDisplay = new DisplayRequest();
        public static bool isKeepDisplayActived = true;
        /// <summary>
        /// 标示提醒页是否静音
        /// </summary>
        public static bool isMuted = false;
        /// <summary>
        /// 标示是否整点提醒
        /// </summary>
        public static bool isHourlyReminder = true;
        public static double clockPagePhotosChangeNum = 10.000000;
        public static LinearGradientBrush gradientBrush = new LinearGradientBrush()
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            GradientStops = new GradientStopCollection()
                {
                    new GradientStop()
                    {
                        Color = Colors.White,
                        Offset = 0
                    },
                    new GradientStop()
                    {
                        Color = Colors.LightGray,
                        Offset = 1
                    }
                }
        };
        public static Brush clockPageNowBackgroundBrush = gradientBrush;
        public static ObservableCollection<ReminderListItem> reminderList = new ObservableCollection<ReminderListItem>();
        /// <summary>
        /// 临时存放的列表
        /// </summary>
        public static ObservableCollection<ReminderListItem> tempList = new ObservableCollection<ReminderListItem>();
        /// <summary>
        /// 准备发起提醒的列表
        /// </summary>
        public static ObservableCollection<ReminderListItem> needToRemindList = new ObservableCollection<ReminderListItem>();
        /// <summary>
        /// 当前排列优先级
        /// </summary>
        public static WhoFirstEnum nowWhoFirst = WhoFirstEnum.CreateTimeFirst;
        /// <summary>
        /// 指示是什么形式的背景变换
        /// <list>
        /// <item>0 >> 默认</item>
        /// <item>1 >> 纯色</item>
        /// <item>2 >> 渐变色</item>
        /// <item>3 >> 本地多图</item>
        /// <item>4 >> 本地单图</item>
        /// <item>5 >> 必应背景图</item>
        /// <item>6 >> Unsplash背景图</item>
        /// </list>
        /// </summary>
        public static int markOfChangeMode = 0;
        /// <summary>
        /// 多图背景的图片列表
        /// </summary>
        public static List<StorageFile> photosList = new List<StorageFile>();
        /// <summary>
        /// 城市代码字符串
        /// </summary>
        public static string cityId = "101280903";//sihui
        public static string appid = string.Empty;
        public static string appsecret = string.Empty;
        /// <summary>
        /// 实况天气总的信息字符串
        /// </summary>
        public static string singleDayWeatherTotalString = string.Empty;
        /// <summary>
        /// 七天天气总的信息字符串
        /// </summary>
        private static string _sevenDaysWeatherTotalString = string.Empty;
        public static string sevenDaysWeatherTotalString
        {
            get { return _sevenDaysWeatherTotalString; }
            set
            {
                _sevenDaysWeatherTotalString = value;
                sevenDaysWeatherXml = TranslateTheStringToXml(value);
            }
        }
        public static XmlDocument sevenDaysWeatherXml = new XmlDocument();
        /// <summary>
        /// 必应图片缓存数目
        /// </summary>
        public static uint clockPageBingPhotosCacheNum = 10;


        public static XmlDocument TranslateTheStringToXml(string str)
        {
            XmlDocument xml = new XmlDocument();
            str = str.Insert(0, "\"total\":");
            var tmp = str.Split(":[\"");
            for (int i = 1; i < tmp.Length; i++)
            {
                var tmp1 = tmp[i].Split("\"],\"")[0];
                str = str.Replace(":[\"" + tmp1 + "\"],\"", ":\"" + tmp1.Replace("\",\"", "/") + "\",\"");
            }
            string[] strList = str.Split("\":{");
            List<string> titleList = new List<string>();
            for (int i = 0; i < strList.Length; i++)
            {
                if (strList[i].Contains("\":[{"))
                {
                    var list = strList[i].Split("\":[{");
                    for (int j = 0; j < list.Length; j++)
                    {
                        titleList.Add(list[j].Split('\"').Last());
                    }
                }
                else
                {
                    titleList.Add(strList[i].Split('\"').Last());
                }
                if (titleList.Last().Contains('}') || titleList.Last().Contains(']'))
                {
                    titleList.Remove(titleList.Last());
                }
            }
            List<XmlElement> openedNode = new List<XmlElement>();
            List<string> nextNode = new List<string>();
            strList = str.Split("\":");
            for (int i = 0; i < strList.Length; i++)
            {
                strList[i] = strList[i].Replace("\"", "");
            }
            try
            {
                for (int i = 0; i < strList.Length; i++)
                {
                    if (strList[i].Contains('}'))
                    {
                        var valueStr = strList[i].Remove(strList[i].IndexOf('}'));
                        openedNode.Last().SetAttribute(strList[i - 1].Split(',')[1], valueStr);

                        char[] charList = strList[i].Skip(strList[i].IndexOf('}')).ToArray();
                        foreach (char ch in charList)
                        {
                            if (ch == '}')
                            {
                                if (openedNode.Count > 1)
                                {
                                    openedNode[openedNode.Count - 2].AppendChild(openedNode.Last());
                                    openedNode.Remove(openedNode.Last());
                                }
                                else
                                {
                                    xml.AppendChild(openedNode[0]);
                                    xml.Save(ApplicationData.Current.LocalCacheFolder.Path + "\\testXml.xml");
                                    return xml;
                                }
                            }
                            if (ch == ']')
                            {
                                nextNode.Remove(nextNode.Last());
                            }
                            if (ch == '{' || ch == '[')
                                break;
                        }
                        if (openedNode.Last().Name != nextNode.Last())
                            openedNode.Add(xml.CreateElement(nextNode.Last()));

                    }
                    if (titleList.Count > 0 && strList[i] == titleList[0] && strList[i + 1].Contains('{'))
                    {
                        openedNode.Add(xml.CreateElement(titleList[0]));
                        titleList.RemoveAt(0);
                    }
                    else if (titleList.Count > 0 && strList[i].Contains(titleList[0]) && strList[i + 1].Contains('['))
                    {
                        //if (openedNode.Last().Name != titleList[0])
                        //{
                        //openedNode.Last().SetAttribute(strList[i - 1].Split(',')[1], strList[i].Split(',')[0].Replace("}", "").Replace("]", ""));
                        openedNode.Add(xml.CreateElement(titleList[0]));
                        //}

                        nextNode.Add(titleList[0]);
                        titleList.RemoveAt(0);
                    }
                    else if (titleList.Count > 0 && strList[i].Contains(titleList[0]) && strList[i + 1].Contains('{'))
                    {
                        if (openedNode.Last().Name != titleList[0])
                        {
                            openedNode.Last().SetAttribute(strList[i - 1].Split(',')[1], strList[i].Split(',')[0].Replace("}", "").Replace("]", ""));
                            openedNode.Add(xml.CreateElement(titleList[0]));
                        }
                        titleList.RemoveAt(0);
                    }
                    else if (strList[i].Contains('{'))
                    {
                        strList[i] = strList[i].Remove(0, strList[i].IndexOf('{') + 1);
                    }
                    else if (strList[i].Contains(',') && !strList[i].Contains('}'))
                    {
                        if (strList[i - 1].Contains(','))
                        {
                            openedNode.Last().SetAttribute(strList[i - 1].Split(',')[1], strList[i].Split(',')[0]);
                        }
                        else
                        {
                            openedNode.Last().SetAttribute(strList[i - 1], strList[i].Split(',')[0]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Content = "计算过程出现错误，请检查程序代码正确性",
                    Title = "错误",
                    CloseButtonText = "OMG"
                };
                _ = dialog.ShowAsync();
            }
            return xml;
        }
        /// <summary>
        /// 获取Common.WhoFirstEnum的描述列表
        /// </summary>
        /// <returns>Common.WhoFirstEnum的描述字符串列表</returns>
        public static List<string> GetEnumDescriptions()
        {
            List<string> resultList = new List<string>();
            var enumList = Enum.GetValues(nowWhoFirst.GetType());
            foreach (var enumValue in enumList)
            {
                FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
                object[] objects = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objects[0];
                resultList.Add(descriptionAttribute.Description);
            }
            return resultList;
        }
        /// <summary>
        /// 获取输入参数的枚举类型的所有元素描述列表
        /// 枚举元素描述表达式 >>> [Description("描述字符串")]元素名字
        /// </summary>
        /// <param name="enumVar">通过此参数回溯到类型以获取所有枚举元素的描述信息</param>
        /// <returns>枚举类型的所有元素描述信息列表</returns>
        public static List<string> GetEnumDescriptions(Enum enumVar)
        {
            List<string> resultList = new List<string>();
            var enumList = Enum.GetValues(enumVar.GetType());
            foreach (var enumValue in enumList)
            {
                FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
                object[] objects = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objects.Length == 0)
                {
                    resultList.Add("无描述信息");
                }
                else
                {
                    DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objects[0];
                    resultList.Add(descriptionAttribute.Description);
                }
            }
            return resultList;
        }
        /// <summary>
        /// 获取输入参数的元素描述
        /// 枚举元素描述表达式 >>> [Description("描述字符串")]元素名字
        /// </summary>
        /// <param name="enumVar">通过此参数回溯到类型以获取对应枚举元素的描述信息</param>
        /// <returns>输入参数的元素描述信息</returns>
        public static string GetEnumDescription(Enum enumVar)
        {
            FieldInfo field = enumVar.GetType().GetField(enumVar.ToString());
            object[] objects = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objects.Length == 0)
            {
                return "无描述信息";
            }
            else
            {
                DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objects[0];
                return descriptionAttribute.Description;
            }

        }

        public static List<string> BuildTheStorgeListOfReminderList()
        {
            if (tempList.Count > 0)
            {
                foreach (ReminderListItem item in tempList)
                {
                    reminderList.Add(item);
                }
            }
            IOrderedEnumerable<ReminderListItem> orderList = from item in Common.reminderList
                                                             orderby item.CreateTime descending
                                                             select item;
            reminderList = new ObservableCollection<ReminderListItem>(orderList);

            List<string> list = new List<string>();
            list.Insert(0, reminderList.Count.ToString());
            list.Insert(0, "[Common.reminderList]");
            foreach (ReminderListItem item in reminderList)
            {
                list.Add("[ReminderListItem]");
                switch (item.Classification)
                {
                    case ReminderListItem.ListItemEnum.AlarmClock:
                        list.Add("0");
                        break;
                    case ReminderListItem.ListItemEnum.Schedule:
                        list.Add("1");
                        break;
                    case ReminderListItem.ListItemEnum.ToDo:
                        list.Add("2");
                        break;
                    default:
                        break;
                }
                foreach (ReminderListItem.LoopListItem boolList in item.LoopListForAlarmClock)
                {
                    list.Add(boolList.IsChecked.ToString());
                }
                foreach (ReminderListItem.LoopListItem boolList in item.LoopListForSchedule)
                {
                    list.Add(boolList.IsChecked.ToString());
                }
                list.Add(item.IsToDoFinish.ToString());
                list.Add(item.TitleString);
                list.Add(item.BodyString.Replace("\r", "***RETURN***"));
                list.Add(item.CreateTime.ToString("yyyy/MM/dd/HH/mm/ss"));
                list.Add(item.UpdateTime.ToString("yyyy/MM/dd/HH/mm/ss"));
                list.Add(item.AlarmTime.ToString("yyyy/MM/dd/HH/mm/ss"));
                list.Add("[SubToDoList]");
                list.Add(item.SubToDoList.Count.ToString());
                foreach (SubToDoListItem subItem in item.SubToDoList)
                {
                    list.Add(subItem.IsChecked.ToString());
                    list.Add(subItem.Content);
                }
                list.Add("[.SubToDoList]");
                list.Add("[.ReminderListItem]");
            }
            list.Add("[.Common.reminderList]");
            return list;
        }
        public static void TranslateTheStringListToReminderList(IList<string> list)
        {
            if (list[0] == "[Common.reminderList]" && list[list.Count - 1] == "[.Common.reminderList]")
            {
                int itemsCount = int.Parse(list[1]);
                int index = 2;
                for (int i = 0; i < itemsCount; i++)
                {
                    ReminderListItem item = new ReminderListItem();

                    int subItemsCount = 0;
                    if (list[index] == "[ReminderListItem]")
                    {
                        item.Classification = (ReminderListItem.ListItemEnum)int.Parse(list[index + 1]);
                        for (int j = 0; j < 7; j++)
                        {
                            item.LoopListForAlarmClock[j].IsChecked = bool.Parse(list[j + index + 2]);
                        }
                        item.LoopListForSchedule[0].IsChecked = bool.Parse(list[index + 9]);
                        item.LoopListForSchedule[1].IsChecked = bool.Parse(list[index + 10]);
                        item.IsToDoFinish = bool.Parse(list[index + 11]);
                        item.TitleString = list[index + 12];
                        item.BodyString = list[index + 13].Replace("***RETURN***", "\r");
                        var templist = list[index + 14].Split('/');
                        item.CreateTime = new DateTime(int.Parse(templist[0]), int.Parse(templist[1]), int.Parse(templist[2]),
                                                        int.Parse(templist[3]), int.Parse(templist[4]), int.Parse(templist[5]));
                        templist = list[index + 15].Split('/');
                        item.UpdateTime = new DateTime(int.Parse(templist[0]), int.Parse(templist[1]), int.Parse(templist[2]),
                                                        int.Parse(templist[3]), int.Parse(templist[4]), int.Parse(templist[5]));
                        templist = list[index + 16].Split('/');
                        item.AlarmTime = new DateTime(int.Parse(templist[0]), int.Parse(templist[1]), int.Parse(templist[2]),
                                                        int.Parse(templist[3]), int.Parse(templist[4]), int.Parse(templist[5]));
                        if (list[index + 17] == "[SubToDoList]" && list[index + 19 + 2 * subItemsCount] == "[.SubToDoList]")
                        {
                            subItemsCount = int.Parse(list[index + 18]);
                            for (int j = 0; j < subItemsCount; j++)
                            {
                                item.SubToDoList.Add(new SubToDoListItem
                                {
                                    IsChecked = bool.Parse(list[index + 19 + 2 * j]),
                                    Content = list[index + 20 + 2 * j]
                                });
                            }
                            index += 21 + 2 * subItemsCount;
                        }
                        else
                        {
                            index += 18;
                        }
                        reminderList.Add(item);
                    }
                }
                IOrderedEnumerable<ReminderListItem> orderList = from orderItem in reminderList
                                                                 orderby orderItem.AlarmTime ascending
                                                                 select orderItem;
                needToRemindList = new ObservableCollection<ReminderListItem>(orderList);
                int totalIgnoreNum = 0;
                int nowIndex = 0;
                while (totalIgnoreNum + nowIndex != reminderList.Count && reminderList.Count > 0)
                {
                    if (needToRemindList[nowIndex].AlarmTime.Year == 1601 || needToRemindList[nowIndex].AlarmTime.CompareTo(DateTime.Now) < 0 ||
                        (needToRemindList[nowIndex].Classification == ReminderListItem.ListItemEnum.ToDo && needToRemindList[nowIndex].IsToDoFinish == true))
                    {
                        needToRemindList.RemoveAt(nowIndex);
                        totalIgnoreNum++;
                    }
                    else nowIndex++;
                }
            }
        }
    }
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationView.GetForCurrentView().TitleBar.BackgroundColor = Colors.WhiteSmoke;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.WhiteSmoke;
            Common.keepDisplay.RequestActive();

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            if (Common.isKeepDisplayActived)
                Common.keepDisplay.RequestRelease();
            Common.mainTimer.Stop();
            StorageDataToFile(deferral);

        }

        private async void StorageDataToFile(SuspendingDeferral deferral)
        {
            IAsyncAction resultOfSet;
            IAsyncAction resultOfReminder;
            IAsyncAction resultOfClock;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem settingPageFile = await folder.TryGetItemAsync("SettingPageInformation.txt");
            if (settingPageFile == null)
            {
                //create new file
                StorageFile file = await folder.CreateFileAsync("SettingPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                resultOfSet = FileIO.WriteLinesAsync(file, new List<string>
                {
                    "[Common.isKeepDisplayActived]", Common.isKeepDisplayActived.ToString(), "[.Common.isKeepDisplayActived]",
                    "[Common.isHourlyReminder]", Common.isHourlyReminder.ToString(), "[.Common.isHourlyReminder]",
                    "[Common.clockPagePhotosChangeNum]", Common.clockPagePhotosChangeNum.ToString(), "[.Common.clockPagePhotosChangeNum]",
                    "[Common.clockPageBingPhotosCacheNum]", Common.clockPageBingPhotosCacheNum.ToString(), "[.Common.clockPageBingPhotosCacheNum]",
                    "[Common.isMuted]", Common.isMuted.ToString(), "[.Common.isMuted]",
                    "[Common.nowWhoFirst]", ((int)Common.nowWhoFirst).ToString(), "[.Common.nowWhoFirst]"
                });
            }
            else
            {
                //storge the data to file
                StorageFile file = await folder.GetFileAsync("SettingPageInformation.txt");
                resultOfSet = FileIO.WriteLinesAsync(file, new List<string>
                {
                    "[Common.isKeepDisplayActived]", Common.isKeepDisplayActived.ToString(), "[.Common.isKeepDisplayActived]",
                    "[Common.isHourlyReminder]", Common.isHourlyReminder.ToString(), "[.Common.isHourlyReminder]",
                    "[Common.clockPagePhotosChangeNum]", Common.clockPagePhotosChangeNum.ToString(), "[.Common.clockPagePhotosChangeNum]",
                    "[Common.clockPageBingPhotosCacheNum]", Common.clockPageBingPhotosCacheNum.ToString(), "[.Common.clockPageBingPhotosCacheNum]",
                    "[Common.isMuted]", Common.isMuted.ToString(), "[.Common.isMuted]",
                    "[Common.nowWhoFirst]", ((int)Common.nowWhoFirst).ToString(), "[.Common.nowWhoFirst]"
                });

            }

            IStorageItem clockPageFile = await folder.TryGetItemAsync("ClockPageInformation.txt");
            if (clockPageFile == null)
            {
                //create the file
                StorageFile file = await folder.CreateFileAsync("ClockPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                resultOfClock = FileIO.WriteLinesAsync(file, new List<string>
                {
                    "[Common.markOfChangeMode]",
                    Common.markOfChangeMode.ToString(),
                    "[.Common.markOfChangeMode]",
                    "[Common.cityId]",
                    Common.cityId,
                    "[.Common.cityId]",
                    "[Common.clockPageNowBackgroundBrush]",
                    "[LinearGradientBrush]",
                    string.Format("{0},{1}", Common.gradientBrush.StartPoint.X, Common.gradientBrush.StartPoint.Y),
                    string.Format("{0},{1}", Common.gradientBrush.EndPoint.X, Common.gradientBrush.EndPoint.Y),
                    string.Format("{0},{1},{2},{3}",
                        Common.gradientBrush.GradientStops[0].Color.A,
                        Common.gradientBrush.GradientStops[0].Color.R,
                        Common.gradientBrush.GradientStops[0].Color.G,
                        Common.gradientBrush.GradientStops[0].Color.B),
                    Common.gradientBrush.GradientStops[0].Offset.ToString(),
                    string.Format("{0},{1},{2},{3}",
                        Common.gradientBrush.GradientStops[1].Color.A,
                        Common.gradientBrush.GradientStops[1].Color.R,
                        Common.gradientBrush.GradientStops[1].Color.G,
                        Common.gradientBrush.GradientStops[1].Color.B),
                    Common.gradientBrush.GradientStops[1].Offset.ToString(),
                    "[.LinearGradientBrush]",
                    "[.Common.clockPageNowBackgroundBrush]"
                });
            }
            else
            {
                //storage the data to file
                StorageFile file = await folder.GetFileAsync("ClockPageInformation.txt");
                List<string> writeList = new List<string>
                {
                    "[Common.markOfChangeMode]",
                    Common.markOfChangeMode.ToString(),
                    "[.Common.markOfChangeMode]",
                    "[Common.cityId]",
                    Common.cityId,
                    "[.Common.cityId]"
                };
                switch (Common.clockPageNowBackgroundBrush.GetType().Name)
                {
                    case "LinearGradientBrush":
                        writeList.Add("[Common.clockPageNowBackgroundBrush]");
                        writeList.Add("[LinearGradientBrush]");
                        LinearGradientBrush gradientBrush = Common.clockPageNowBackgroundBrush as LinearGradientBrush;
                        writeList.Add(string.Format("{0},{1}", gradientBrush.StartPoint.X, gradientBrush.StartPoint.Y));
                        writeList.Add(string.Format("{0},{1}", gradientBrush.EndPoint.X, gradientBrush.EndPoint.Y));
                        foreach (GradientStop stop in gradientBrush.GradientStops)
                        {
                            writeList.Add(string.Format("{0},{1},{2},{3}", stop.Color.A, stop.Color.R, stop.Color.G, stop.Color.B));
                            writeList.Add(stop.Offset.ToString());
                        }
                        writeList.Add("[.LinearGradientBrush]");
                        writeList.Add("[.Common.clockPageNowBackgroundBrush]");
                        break;
                    case "ImageBrush":
                        writeList.Add("[Common.clockPageNowBackgroundBrush]");
                        writeList.Add("[ImageBrush]");
                        writeList.Add("BackgroundImage");
                        writeList.Add("[.ImageBrush]");
                        writeList.Add("[.Common.clockPageNowBackgroundBrush]");
                        break;
                    case "SolidColorBrush":
                        writeList.Add("[Common.clockPageNowBackgroundBrush]");
                        writeList.Add("[SolidColorBrush]");
                        SolidColorBrush solidBrush = Common.clockPageNowBackgroundBrush as SolidColorBrush;
                        writeList.Add(string.Format("{0},{1},{2},{3}", solidBrush.Color.A, solidBrush.Color.R, solidBrush.Color.G, solidBrush.Color.B));
                        writeList.Add("[.SolidColorBrush]");
                        writeList.Add("[.Common.clockPageNowBackgroundBrush]");
                        break;
                    default:
                        break;
                }
                resultOfClock = FileIO.WriteLinesAsync(file, writeList);
            }


            IStorageItem reminderPageFile = await folder.TryGetItemAsync("ReminderPageInformation.txt");
            if (reminderPageFile == null)
            {
                //create the file
                StorageFile file = await folder.CreateFileAsync("ReminderPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                resultOfReminder = FileIO.WriteLinesAsync(file, new List<string> { "[Common.reminderList]", "0", "[.Common.reminderList]" });
            }
            else
            {
                //storage the data to file
                StorageFile file = await folder.GetFileAsync("ReminderPageInformation.txt");
                resultOfReminder = FileIO.WriteLinesAsync(file, Common.BuildTheStorgeListOfReminderList());
            }
            while (resultOfSet.Status != AsyncStatus.Completed || resultOfReminder.Status != AsyncStatus.Completed || resultOfClock.Status != AsyncStatus.Completed) ;
            deferral.Complete();
        }
    }
}
