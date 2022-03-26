using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static DispatcherTimer mainPageTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100) };
        public ReminderListItem alarmedItem = new ReminderListItem();
        /// <summary>
        /// 播放闹钟音乐
        /// </summary>
        public MediaPlayer player = new MediaPlayer()
        {
            Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/User/WAVMedia/Alarm.wav")),
            IsLoopingEnabled = true
        };
        public MainPage()
        {
            this.InitializeComponent();
            mainPageTimer.Tick += MainPageTimer_Tick;
            mainPageTimer.Start();
            Common.mainTimer.Start();
            LoadDataFromFile();
        }

        private async void MainPageTimer_Tick(object sender, object e)
        {
            if (Common.needToRemindList.Count > 0 && Common.needToRemindList[0] != alarmedItem &&
                Common.needToRemindList[0].AlarmTime.Date.CompareTo(DateTime.Today) == 0 &&
                Common.needToRemindList[0].AlarmTime.TimeOfDay.CompareTo(new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0)) == 0)
            {
                //开始响闹钟
                alarmedItem = Common.needToRemindList[0];
                Common.needToRemindList.RemoveAt(0);
                var stackPanel = new StackPanel
                {
                    Width = 320
                };
                var scrollViewer = new ScrollViewer
                {
                    Height = 100,
                    Margin = new Thickness(0, 10, 0, 0),
                    Content = new TextBlock { Text = alarmedItem.BodyString, TextWrapping = TextWrapping.WrapWholeWords, Width = 320 }
                };
                stackPanel.Children.Add(new TextBlock { Text = "提醒时间：" + alarmedItem.AlarmTimeString });
                stackPanel.Children.Add(scrollViewer);
                ContentDialog dialog = new ContentDialog
                {
                    Title = alarmedItem.TitleString,
                    PrimaryButtonText = "知道了",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = stackPanel
                };
                switch (alarmedItem.Classification)
                {
                    case ReminderListItem.ListItemEnum.AlarmClock:
                        dialog.Background = new SolidColorBrush(Colors.DodgerBlue);
                        break;
                    case ReminderListItem.ListItemEnum.Schedule:
                        dialog.Background = new SolidColorBrush(Colors.MediumPurple);
                        break;
                    case ReminderListItem.ListItemEnum.ToDo:
                        dialog.Background = new SolidColorBrush(Colors.Orange);
                        break;
                    default:
                        break;
                }
                player.IsMuted = Common.isMuted;
                player.Play();
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    player.Pause();
                    int removeIndex = Common.reminderList.IndexOf(alarmedItem);
                    Common.reminderList.RemoveAt(removeIndex);
                    if (alarmedItem.Classification == ReminderListItem.ListItemEnum.AlarmClock)
                    {
                        bool isLoop = false;
                        for (int i = 0; i < 7; i++)
                        {
                            if (alarmedItem.LoopListForAlarmClock[i].IsChecked == true)
                            {
                                isLoop = true;
                                break;
                            }
                        }
                        if (isLoop)
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                alarmedItem.AlarmTime = alarmedItem.AlarmTime.AddDays(1);
                                if (alarmedItem.LoopListForAlarmClock[(int)alarmedItem.AlarmTime.DayOfWeek].IsChecked)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (alarmedItem.Classification == ReminderListItem.ListItemEnum.Schedule)
                    {
                        if (alarmedItem.LoopListForSchedule[0].IsChecked == true)
                        {
                            alarmedItem.AlarmTime = alarmedItem.AlarmTime.AddYears(1);
                        }
                        else if (alarmedItem.LoopListForSchedule[1].IsChecked == true)
                        {
                            alarmedItem.AlarmTime = alarmedItem.AlarmTime.AddMonths(1);
                        }
                    }
                    Common.reminderList.Insert(removeIndex, alarmedItem);
                    if (Common.nowWhoFirst == Common.WhoFirstEnum.AlarmTimeFirst && Common.reminderList.Count > 1)
                    {
                        IOrderedEnumerable<ReminderListItem> orderList = from item in Common.reminderList
                                                                         orderby item.AlarmTime ascending
                                                                         select item;
                        var templist = new ObservableCollection<ReminderListItem>(orderList);
                        for (int i = 0; i < Common.reminderList.Count - 1; i++)
                        {
                            int index = Common.reminderList.IndexOf(templist[i]);
                            Common.reminderList.Move(index, i);
                        }
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
                    }
                }
            }
        }

        private async void LoadDataFromFile()
        {
            StorageFile secretFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/XMLData/WeatherAPIIdAndSecret.xml"));
            XmlDocument secretXml = new XmlDocument();
            secretXml.Load(new StreamReader(await secretFile.OpenStreamForReadAsync()));
            Common.appid = secretXml.SelectSingleNode("/API").Attributes["ID"].Value;
            Common.appsecret = secretXml.SelectSingleNode("/API").Attributes["Secret"].Value;
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            IStorageItem settingPageFile = await folder.TryGetItemAsync("SettingPageInformation.txt");
            if (settingPageFile == null)
            {
                //create new file
                StorageFile file = await folder.CreateFileAsync("SettingPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, new List<string>
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
                //read the data from file
                StorageFile file = await folder.GetFileAsync("SettingPageInformation.txt");
                IList<string> vector = await FileIO.ReadLinesAsync(file);
                if (vector[vector.IndexOf("[Common.isKeepDisplayActived]") + 2] == "[.Common.isKeepDisplayActived]")
                {
                    if (vector[vector.IndexOf("[Common.isKeepDisplayActived]") + 1] == "True")
                    {
                        Common.isKeepDisplayActived = true;
                    }
                    else if (vector[vector.IndexOf("[Common.isKeepDisplayActived]") + 1] == "False")
                    {
                        Common.isKeepDisplayActived = false;
                    }
                }
                if (vector[vector.IndexOf("[Common.isHourlyReminder]") + 2] == "[.Common.isHourlyReminder]")
                {
                    if (vector[vector.IndexOf("[Common.isHourlyReminder]") + 1] == "True")
                    {
                        Common.isHourlyReminder = true;
                    }
                    else if (vector[vector.IndexOf("[Common.isHourlyReminder]") + 1] == "False")
                    {
                        Common.isHourlyReminder = false;
                    }
                }
                if (vector[vector.IndexOf("[Common.clockPagePhotosChangeNum]") + 2] == "[.Common.clockPagePhotosChangeNum]")
                {
                    Common.clockPagePhotosChangeNum = double.Parse(vector[vector.IndexOf("[Common.clockPagePhotosChangeNum]") + 1]);
                }
                if (vector[vector.IndexOf("[Common.clockPageBingPhotosCacheNum]") + 2] == "[.Common.clockPageBingPhotosCacheNum]")
                {
                    Common.clockPageBingPhotosCacheNum = uint.Parse(vector[vector.IndexOf("[Common.clockPageBingPhotosCacheNum]") + 1]);
                }
                if (vector[vector.IndexOf("[Common.isMuted]") + 2] == "[.Common.isMuted]")
                {
                    if (vector[vector.IndexOf("[Common.isMuted]") + 1] == "True")
                    {
                        Common.isMuted = true;
                    }
                    else if (vector[vector.IndexOf("[Common.isMuted]") + 1] == "False")
                    {
                        Common.isMuted = false;
                    }
                }
                if (vector[vector.IndexOf("[Common.nowWhoFirst]") + 2] == "[.Common.nowWhoFirst]")
                {
                    Common.nowWhoFirst = (Common.WhoFirstEnum)int.Parse(vector[vector.IndexOf("[Common.nowWhoFirst]") + 1]);
                }
            }

            IStorageItem clockPageFile = await folder.TryGetItemAsync("ClockPageInformation.txt");
            if (clockPageFile == null)
            {
                //create the file
                StorageFile file = await folder.CreateFileAsync("ClockPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, new List<string>
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
                navi.SelectedItem = clockPage;
            }
            else
            {
                //read the data from file
                StorageFile file = await folder.GetFileAsync("ClockPageInformation.txt");
                IList<string> backgroundString = await FileIO.ReadLinesAsync(file);
                if (backgroundString[backgroundString.IndexOf("[Common.markOfChangeMode]") + 2] == "[.Common.markOfChangeMode]")
                {
                    Common.markOfChangeMode = int.Parse(backgroundString[backgroundString.IndexOf("[Common.markOfChangeMode]") + 1]);
                }
                if (backgroundString[backgroundString.IndexOf("[Common.cityId]") + 2] == "[.Common.cityId]")
                {
                    Common.cityId = backgroundString[backgroundString.IndexOf("[Common.cityId]") + 1];
                }
                if (backgroundString.IndexOf("[Common.clockPageNowBackgroundBrush]") != -1 && backgroundString.Last() == "[.Common.clockPageNowBackgroundBrush]")
                {
                    switch (backgroundString[1])
                    {
                        case "[LinearGradientBrush]":
                            LinearGradientBrush gradientBrush = new LinearGradientBrush
                            {
                                StartPoint = new Point(double.Parse(backgroundString[2].Split(',')[0]), double.Parse(backgroundString[2].Split(',')[1])),
                                EndPoint = new Point(double.Parse(backgroundString[3].Split(',')[0]), double.Parse(backgroundString[3].Split(',')[1]))
                            };
                            GradientStopCollection stops = new GradientStopCollection();
                            for (int i = 4; i < backgroundString.Count - 3; i += 2)
                            {
                                GradientStop stop = new GradientStop
                                {
                                    Color = Color.FromArgb(byte.Parse(backgroundString[i].Split(',')[0]), byte.Parse(backgroundString[i].Split(',')[1]), byte.Parse(backgroundString[i].Split(',')[2]), byte.Parse(backgroundString[i].Split(',')[3])),
                                    Offset = double.Parse(backgroundString[i + 1])
                                };
                                stops.Add(stop);
                            }
                            gradientBrush.GradientStops = stops;
                            Common.clockPageNowBackgroundBrush = gradientBrush;
                            break;
                        case "[ImageBrush]":
                            IStorageItem imageBrushFile = await folder.TryGetItemAsync(backgroundString[2]);
                            if (imageBrushFile != null)
                            {
                                StorageFile imageFile = await folder.GetFileAsync(backgroundString[2]);
                                using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                                {
                                    BitmapImage bitmapImage = new BitmapImage();
                                    await bitmapImage.SetSourceAsync(fileStream);
                                    Common.clockPageNowBackgroundBrush = new ImageBrush
                                    {
                                        ImageSource = bitmapImage,
                                        Stretch = Stretch.UniformToFill
                                    };
                                }
                            }
                            break;
                        case "[SolidColorBrush]":
                            SolidColorBrush solidBrush = new SolidColorBrush
                            {
                                Color = Color.FromArgb(byte.Parse(backgroundString[2].Split(',')[0]), byte.Parse(backgroundString[2].Split(',')[1]), byte.Parse(backgroundString[2].Split(',')[2]), byte.Parse(backgroundString[2].Split(',')[3]))
                            };
                            Common.clockPageNowBackgroundBrush = solidBrush;
                            break;
                        default:
                            break;
                    }
                }
                navi.SelectedItem = clockPage;
            }

            IStorageItem reminderPageFile = await folder.TryGetItemAsync("ReminderPageInformation.txt");
            if (reminderPageFile == null)
            {
                //create the file
                StorageFile file = await folder.CreateFileAsync("ReminderPageInformation.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, new List<string> { "[Common.reminderList]", "0", "[.Common.reminderList]" });
            }
            else
            {
                //read the data from file
                StorageFile file = await folder.GetFileAsync("ReminderPageInformation.txt");
                IList<string> resultList = await FileIO.ReadLinesAsync(file);
                Common.TranslateTheStringListToReminderList(resultList);
            }
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingPage), null, new DrillInNavigationTransitionInfo());
            }
            else if (args.SelectedItem == clockPage)
            {
                contentFrame.Navigate(typeof(ClockPage), null, new DrillInNavigationTransitionInfo());
            }
            else if (args.SelectedItem == weatherPage)
            {
                contentFrame.Navigate(typeof(WeatherPage), null, new DrillInNavigationTransitionInfo());
            }
            else if (args.SelectedItem == reminderPage)
            {
                contentFrame.Navigate(typeof(ReminderPage), null, new DrillInNavigationTransitionInfo());
            }
            else if (args.SelectedItem == timerPage)
            {
                contentFrame.Navigate(typeof(TimerPage), null, new DrillInNavigationTransitionInfo());
            }
            else if (args.SelectedItem == countdownPage)
            {
                contentFrame.Navigate(typeof(CountdownPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                navi.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
                navi.IsPaneToggleButtonVisible = false;
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            }
            else
            {
                navi.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Auto;
                navi.IsPaneToggleButtonVisible = true;
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            }
        }
    }
}
