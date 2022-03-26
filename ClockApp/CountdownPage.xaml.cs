using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CountdownPage : Page
    {
        public static bool isTimerStart = false;
        /// <summary>
        /// 倒计时开始的时间
        /// </summary>
        public static DateTime startTime = new DateTime(0);
        /// <summary>
        /// 倒计时总时间
        /// </summary>
        public static TimeSpan totalTime = new TimeSpan(0);
        /// <summary>
        /// 播放闹钟音乐
        /// </summary>
        public MediaPlayer player = new MediaPlayer()
        {
            Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/User/WAVMedia/Alarm.wav")),
            IsLoopingEnabled = true
        };

        public CountdownPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (isTimerStart)
            {
                countdownProgress.Foreground = new SolidColorBrush(new Color() { A = 0xFF, R = 0x00, G = 0x5A, B = 0x9E });

                startOrPauseCountdownButton.Style = Resources["PauseButtonStyle"] as Style;
                startOrPauseCountdownButton.Label = "停止";
                startOrPauseCountdownButton.Icon = new SymbolIcon(Symbol.Stop);
                startOrPauseCountdownButton.Background = new SolidColorBrush(Colors.OrangeRed);
                startOrPauseCountdownButton.IsEnabled = true;

                hourNumSetBox.Visibility = Visibility.Collapsed;
                minuteNumSetBox.Visibility = Visibility.Collapsed;
                secondNumSetBox.Visibility = Visibility.Collapsed;
                hourNumTextBlock.Text = hourNumSetBox.Text;
                minuteNumTextBlock.Text = minuteNumSetBox.Text;
                secondNumTextBlock.Text = secondNumSetBox.Text;
                hourNumTextBlock.Visibility = Visibility.Visible;
                minuteNumTextBlock.Visibility = Visibility.Visible;
                secondNumTextBlock.Visibility = Visibility.Visible;
            }
            Common.mainTimer.Tick += Countdown_Tick;
        }

        private void Countdown_Tick(object sender, object e)
        {
            if (totalTime.Ticks != 0 && isTimerStart)
            {
                TimeSpan span = totalTime - (DateTime.Now - startTime) + new TimeSpan(0, 0, 1);
                hourNumTextBlock.Text = span.Hours.ToString("D2");
                minuteNumTextBlock.Text = span.Minutes.ToString("D2");
                secondNumTextBlock.Text = span.Seconds.ToString("D2");
                countdownProgress.Value = 100 - (span - new TimeSpan(0, 0, 1)) / totalTime * 100;
            }
        }

        private void HourNumSetBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (sender.IsLoaded)
            {
                var temp = args.NewValue;
                if (temp == -1)
                    sender.Value = 23;
                else if (temp == 24)
                    sender.Value = 0;
                else if (temp >= 25)
                {
                    sender.Value = temp % 24;
                    info.IsOpen = true;
                    infoBarOpen.Begin();
                }
                startOrPauseCountdownButton.IsEnabled = !(hourNumSetBox.Value == 0 && minuteNumSetBox.Value == 0 && secondNumSetBox.Value == 0);
            }
        }

        private void MinuteNumSetBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (sender.IsLoaded)
            {
                if (args.NewValue == -1)
                    sender.Value = 59;
                else if (args.NewValue == 60)
                    sender.Value = 0;
                startOrPauseCountdownButton.IsEnabled = !(hourNumSetBox.Value == 0 && minuteNumSetBox.Value == 0 && secondNumSetBox.Value == 0);
            }
        }

        private void SecondNumSetBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (sender.IsLoaded)
            {
                if (args.NewValue == -1)
                    sender.Value = 59;
                else if (args.NewValue == 60)
                    sender.Value = 0;
                startOrPauseCountdownButton.IsEnabled = !(hourNumSetBox.Value == 0 && minuteNumSetBox.Value == 0 && secondNumSetBox.Value == 0);
            }
        }

        private void StartOrPauseCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (startOrPauseCountdownButton.Label == "开始")
            {
                //start the counter
                startTime = DateTime.Now;
                totalTime = new TimeSpan(0, (int)hourNumSetBox.Value, (int)minuteNumSetBox.Value, (int)secondNumSetBox.Value, 0);
                isTimerStart = true;
                countdownProgress.Foreground = new SolidColorBrush(new Color() { A = 0xFF, R = 0x00, G = 0x5A, B = 0x9E });

                startOrPauseCountdownButton.Style = Resources["PauseButtonStyle"] as Style;
                startOrPauseCountdownButton.Label = "停止";
                startOrPauseCountdownButton.Icon = new SymbolIcon(Symbol.Stop);
                startOrPauseCountdownButton.Background = new SolidColorBrush(Colors.OrangeRed);

                hourNumSetBox.Visibility = Visibility.Collapsed;
                minuteNumSetBox.Visibility = Visibility.Collapsed;
                secondNumSetBox.Visibility = Visibility.Collapsed;
                hourNumTextBlock.Text = hourNumSetBox.Text;
                minuteNumTextBlock.Text = minuteNumSetBox.Text;
                secondNumTextBlock.Text = secondNumSetBox.Text;
                hourNumTextBlock.Visibility = Visibility.Visible;
                minuteNumTextBlock.Visibility = Visibility.Visible;
                secondNumTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                //stop the counter
                isTimerStart = false;

                startOrPauseCountdownButton.Style = Resources["StartButtonStyle"] as Style;
                startOrPauseCountdownButton.Label = "开始";
                startOrPauseCountdownButton.Icon = new SymbolIcon(Symbol.Play);
                startOrPauseCountdownButton.Background = new SolidColorBrush(Colors.DarkGreen);

                hourNumSetBox.Visibility = Visibility.Visible;
                minuteNumSetBox.Visibility = Visibility.Visible;
                secondNumSetBox.Visibility = Visibility.Visible;
                hourNumSetBox.Text = hourNumTextBlock.Text;
                minuteNumSetBox.Text = minuteNumTextBlock.Text;
                secondNumSetBox.Text = secondNumTextBlock.Text;
                hourNumTextBlock.Visibility = Visibility.Collapsed;
                minuteNumTextBlock.Visibility = Visibility.Collapsed;
                secondNumTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private async void CountdownProgress_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue == 100)
            {
                countdownProgress.Foreground = new SolidColorBrush(Colors.Green);
                totalTime = new TimeSpan(0);
                ContentDialog dialog = new ContentDialog
                {
                    Title = "倒计时",
                    PrimaryButtonText = "知道了",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = "你设置的倒计时已结束"
                };

                StartOrPauseCountdownButton_Click(new object(), new RoutedEventArgs());
                player.Play();

                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    player.Pause();
                    player.PlaybackSession.Position = new TimeSpan(0);
                    countdownProgress.Value = 0;
                }
            }
        }
    }
}
