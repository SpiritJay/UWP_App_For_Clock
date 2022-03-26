using System;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimerPage : Page
    {
        public static bool isTimerStart = false;
        public static bool isTimerPause = false;
        /// <summary>
        /// 开始计时时的实时时间
        /// </summary>
        public static DateTime startTime = new DateTime();
        /// <summary>
        /// 存储暂停时已经记下的时间长度
        /// </summary>
        public static TimeSpan timePauseSpace = new TimeSpan(0);
        /// <summary>
        /// 存储计时时长
        /// </summary>
        public static TimeSpan timeSpace = new TimeSpan(0);
        /// <summary>
        /// 上次记录的标记时间
        /// </summary>
        public static TimeSpan lastRecord = new TimeSpan(0);
        /// <summary>
        /// 在标记列表里显示的项目对象源列表
        /// </summary>
        static ObservableCollection<RecordListItem> recordList = new ObservableCollection<RecordListItem>();
        /// <summary>
        /// 在标记列表里显示的项目对象类
        /// </summary>
        public class RecordListItem
        {
            /// <summary>
            /// 标记的序号列
            /// </summary>
            public string Rank { get; set; }
            /// <summary>
            /// 标记的标记时间列
            /// </summary>
            public string Record { get; set; }
            /// <summary>
            /// 标记的时间差列
            /// </summary>
            public string Increment { get; set; }
            /// <summary>
            /// 设置这次记录的的所需计算参数
            /// </summary>
            /// <param name="NowRecord">现在要记录下来的时间标记</param>
            /// <param name="LastRescord">上一次记录下来的时间标记，初始值为00:00:00.000</param>
            /// <param name="RankOfThis">这次记录的序号</param>
            public void SetValue(TimeSpan NowRecord, TimeSpan LastRescord, int RankOfThis)
            {
                Record = NowRecord.Hours.ToString("D2") + ":" +
                    NowRecord.Minutes.ToString("D2") + ":" +
                    NowRecord.Seconds.ToString("D2") + "." +
                    NowRecord.Milliseconds.ToString("D3");
                TimeSpan incrementSpan = NowRecord - LastRescord;
                Increment = "+" + incrementSpan.Hours.ToString("D2") + ":" +
                    incrementSpan.Minutes.ToString("D2") + ":" +
                    incrementSpan.Seconds.ToString("D2") + "." +
                    incrementSpan.Milliseconds.ToString("D3");
                Rank = RankOfThis.ToString();
            }
        }

        public TimerPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Common.mainTimer.Tick += CountingTimer_Tick;
            recordListView.ItemsSource = recordList;
            if (isTimerStart)
            {
                resetButton.IsEnabled = false;
                markButton.IsEnabled = true;

                startOrPauseCountingButton.Style = Resources["PauseButtonStyle"] as Style;
                startOrPauseCountingButton.Label = "暂停";
                startOrPauseCountingButton.Icon = new SymbolIcon(Symbol.Pause);
                startOrPauseCountingButton.Background = new SolidColorBrush(Colors.OrangeRed);
            }
            else
            {
                if (isTimerPause)
                {
                    millisecondOfCountTimeTextBlock.Text = timeSpace.Milliseconds.ToString("D3");
                    secondOfCountTimeTextBlock.Text = timeSpace.Seconds.ToString("D2");
                    minuteOfCountTimeTextBlock.Text = timeSpace.Minutes.ToString("D2");
                    hourOfCountTimeTextBlock.Text = timeSpace.Hours.ToString("D2");
                }
            }

            //如果不获取焦点，焦点还在 NavigationView 上面，只有焦点在该页面上快捷键才能奏效
            //ClokPage的退出全屏快捷键之所以不用像这里一样获取焦点也能奏效是因为进入全屏需要点击该页面的控件，焦点已经转移
            this.Focus(FocusState.Programmatic);
        }

        private void StartOrPauseCountingButton_Click(object sender, RoutedEventArgs e)
        {
            if (startOrPauseCountingButton.Label == "开始")
            {
                //start the counter
                if (!isTimerPause)
                {
                    timePauseSpace = new TimeSpan(0, 0, 0, 0, 0);
                }
                startTime = DateTime.Now;
                isTimerPause = isTimerStart = true;

                resetButton.IsEnabled = false;
                markButton.IsEnabled = true;

                startOrPauseCountingButton.Style = Resources["PauseButtonStyle"] as Style;
                startOrPauseCountingButton.Label = "暂停";
                startOrPauseCountingButton.Icon = new SymbolIcon(Symbol.Pause);
                startOrPauseCountingButton.Background = new SolidColorBrush(Colors.OrangeRed);
            }
            else
            {
                //stop the counter
                isTimerStart = false;

                timePauseSpace = timeSpace;

                resetButton.IsEnabled = true;
                markButton.IsEnabled = false;

                startOrPauseCountingButton.Style = Resources["StartButtonStyle"] as Style;
                startOrPauseCountingButton.Label = "开始";
                startOrPauseCountingButton.Icon = new SymbolIcon(Symbol.Play);
                startOrPauseCountingButton.Background = new SolidColorBrush(Colors.DarkGreen);
            }
        }

        private void CountingTimer_Tick(object sender, object e)
        {
            if (isTimerStart)
            {
                timeSpace = DateTime.Now - startTime + timePauseSpace;
                millisecondOfCountTimeTextBlock.Text = timeSpace.Milliseconds.ToString("D3");
                secondOfCountTimeTextBlock.Text = timeSpace.Seconds.ToString("D2");
                minuteOfCountTimeTextBlock.Text = timeSpace.Minutes.ToString("D2");
                hourOfCountTimeTextBlock.Text = timeSpace.Hours.ToString("D2");
            }
        }

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            var recordListItem = new RecordListItem();
            recordListItem.SetValue(timeSpace, lastRecord, recordListView.Items.Count + 1);
            lastRecord = timeSpace;
            recordList.Add(recordListItem);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            isTimerPause = false;

            millisecondOfCountTimeTextBlock.Text = "000";
            secondOfCountTimeTextBlock.Text = "00";
            minuteOfCountTimeTextBlock.Text = "00";
            hourOfCountTimeTextBlock.Text = "00";

            recordList.Clear();
            timePauseSpace = new TimeSpan(0);
            timeSpace = new TimeSpan(0);
            lastRecord = new TimeSpan(0);
        }
    }
}
