using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 包含了必应位图和相关描述文字信息的类
    /// </summary>
    public class WebImage
    {
        private BitmapImage _image;
        public enum WebImageFrom
        {
            bing,
            unsplash,
            nothing
        };
        private WebImageFrom _imageFrom;
        private string _title;
        private string _description;
        private string _headline;
        private string _copyright;

        /// <summary>
        /// 位图图片
        /// </summary>
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }
        /// <summary>
        /// 图片来源
        /// </summary>
        public WebImageFrom ImageFrom
        {
            get { return _imageFrom; }
            set { _imageFrom = value; }
        }
        /// <summary>
        /// 大标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        /// <summary>
        /// 图片描述文段
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        /// <summary>
        /// 概要性标题
        /// </summary>
        public string Headline
        {
            get { return _headline; }
            set { _headline = value; }
        }
        /// <summary>
        /// 图片版权信息
        /// </summary>
        public string Copyright
        {
            get { return _copyright; }
            set { _copyright = value; }
        }

        /// <summary>
        /// 各项属性均为空
        /// </summary>
        public WebImage()
        {
            _image = null;
            _imageFrom = WebImageFrom.nothing;
            _title = string.Empty;
            _description = string.Empty;
            _headline = string.Empty;
            _copyright = string.Empty;
        }
        /// <summary>
        /// 解析含有文字描述信息的长字符串，并设置到对应属性（从网络）,使用前应确定ImageFrom
        /// </summary>
        /// <param name="infoString">含有文字描述信息的长字符串</param>
        public void TranslateFromHttp(string infoString)
        {
            switch (_imageFrom)
            {
                case WebImageFrom.bing:
                    _title = infoString.Split("\"title\">").Last().Split('<')[0];
                    _headline = infoString.Split("\"headline\">").Last().Split('<')[0];
                    _copyright = infoString.Split("\"copyright\">").Last().Split('<')[0];
                    _description = infoString.Split("\"Description\":\"")[1].Split('\"')[0];
                    _description = Regex.Unescape(_description);
                    break;
                case WebImageFrom.unsplash:
                    _title = "相机型号";
                    _headline = "Unsplash每日一图";
                    _copyright = "by " + infoString.Split("class=\"yzAnJ\"")[1].Split("</a>")[0].Split('>').Last();
                    _description = infoString.Contains("span class=\"Yhept\">") ? infoString.Split("span class=\"Yhept\">").Last().Split("<div")[0] : "无相机";
                    _description = Regex.Unescape(_description);
                    break;
                case WebImageFrom.nothing:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 解析含有文字描述信息的长字符串，并设置到对应属性（从文件）,使用前应确定ImageFrom
        /// </summary>
        /// <param name="infoString">含有文字描述信息的长字符串</param>
        public void TranslateFromFile(string infoString)
        {
            switch (_imageFrom)
            {
                case WebImageFrom.bing:
                    _title = infoString.Split("<Title>")[1];
                    _headline = infoString.Split("<Headline>")[1];
                    _copyright = infoString.Split("<Copyright>")[1];
                    _description = infoString.Split("<Description>")[1];
                    break;
                case WebImageFrom.unsplash:
                    _title = "相机型号";
                    _headline = "Unsplash每日一图";
                    _copyright = infoString.Split("<Copyright>")[1];
                    _description = infoString.Split("<Description>")[1];
                    break;
                case WebImageFrom.nothing:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 打包信息参数以存储至文档，使用前应确定ImageFrom
        /// </summary>
        /// <returns>完整的待存储文本</returns>
        public string PackInformationForFile()
        {
            string result = string.Empty;
            switch (_imageFrom)
            {
                case WebImageFrom.bing:
                    result = string.Format(
                        "<Title>{0}<Title><Headline>{1}<Headline><Copyright>{2}<Copyright><Description>{3}<Description>",
                        _title, _headline, _copyright, _description);
                    break;
                case WebImageFrom.unsplash:
                    result = string.Format(
                        "<Copyright>{0}<Copyright><Description>{1}<Description>",
                        _copyright, _description);
                    break;
                case WebImageFrom.nothing:
                    break;
                default:
                    break;
            }
            return result;
        }
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ClockPage : Page
    {
        /// <summary>
        /// 整点提醒媒体
        /// </summary>
        public MediaPlayer player = new MediaPlayer()
        {
            Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/User/WAVMedia/ClockAlarm.wav")),
            IsLoopingEnabled = false
        };
        /// <summary>
        /// 整点提醒媒体播放次数
        /// </summary>
        public int playerPlayTimes = 0;
        /// <summary>
        /// 记录上次更换多图背景的时间，0为初始状态，每次都会初始化为0
        /// </summary>
        public DateTime lastChangeBackgroundTime = new DateTime(0);
        /// <summary>
        /// 多图列表的索引参数
        /// </summary>
        public int lastChangeBackgroundIndex = 0;
        /// <summary>
        /// 渐变色的Stop信息列表的对象
        /// </summary>
        public class GradientColorListViewItem
        {
            public double Offset { get; set; }
            public SolidColorBrush ColorBrush { get; set; }
            public string OffsetString { get; set; }
            public void SetValue(Color color, double offset)
            {
                Offset = offset;
                ColorBrush = new SolidColorBrush(color);
                OffsetString = offset.ToString();
            }
        }
        /// <summary>
        /// 设置背景颜色的预览矩形
        /// </summary>
        public LinearGradientBrush testRectangleFill = new LinearGradientBrush();
        /// <summary>
        /// 存储每个时钟刻点的动画
        /// </summary>
        public List<Storyboard> stories = new List<Storyboard>();
        /// <summary>
        /// 是否可以更新必应图片
        /// </summary>
        public bool canUpdateWebPhoto = true;
        /// <summary>
        /// 天气城市代码表
        /// </summary>
        public XmlDocument citiesXml = new XmlDocument();
        /// <summary>
        /// 城市代码
        /// </summary>
        public string cityId = string.Empty;
        /// <summary>
        /// 省列表
        /// </summary>
        public List<string> provincesList = new List<string>();
        /// <summary>
        /// 是否能更新天气
        /// </summary>
        public bool canUpdateWeather = true;
        /// <summary>
        /// 日出时间
        /// </summary>
        public TimeSpan sunriseTime;
        /// <summary>
        /// 日落时间
        /// </summary>
        public TimeSpan sunsetTime;
        /// <summary>
        /// 辨别用户发出的修改背景请求类别（颜色选择器）
        /// true 单色背景
        /// false 渐变背景
        /// </summary>
        public bool singleOrGradient;

        public ClockPage()
        {
            this.InitializeComponent();
            player.MediaEnded += Player_MediaEnded;
            stories = new List<Storyboard>
            {
                SecondsStoryboard0,SecondsStoryboard1,SecondsStoryboard2,SecondsStoryboard3,SecondsStoryboard4,SecondsStoryboard5,SecondsStoryboard6,SecondsStoryboard7,SecondsStoryboard8,SecondsStoryboard9,
                SecondsStoryboard10,SecondsStoryboard11,SecondsStoryboard12,SecondsStoryboard13,SecondsStoryboard14,SecondsStoryboard15,SecondsStoryboard16,SecondsStoryboard17,SecondsStoryboard18,SecondsStoryboard19,
                SecondsStoryboard20,SecondsStoryboard21,SecondsStoryboard22,SecondsStoryboard23,SecondsStoryboard24,SecondsStoryboard25,SecondsStoryboard26,SecondsStoryboard27,SecondsStoryboard28,SecondsStoryboard29,
                SecondsStoryboard30,SecondsStoryboard31,SecondsStoryboard32,SecondsStoryboard33,SecondsStoryboard34,SecondsStoryboard35,SecondsStoryboard36,SecondsStoryboard37,SecondsStoryboard38,SecondsStoryboard39,
                SecondsStoryboard40,SecondsStoryboard41,SecondsStoryboard42,SecondsStoryboard43,SecondsStoryboard44,SecondsStoryboard45,SecondsStoryboard46,SecondsStoryboard47,SecondsStoryboard48,SecondsStoryboard49,
                SecondsStoryboard50,SecondsStoryboard51,SecondsStoryboard52,SecondsStoryboard53,SecondsStoryboard54,SecondsStoryboard55,SecondsStoryboard56,SecondsStoryboard57,SecondsStoryboard58,SecondsStoryboard59
            };
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            //throw new NotImplementedException();
            if (playerPlayTimes < 3)
            {
                player.Play();
                playerPlayTimes++;
            }
            else
            {
                playerPlayTimes = 0;
            }
        }

        private async void ClockPage_Loaded(object sender, RoutedEventArgs e)
        {
            clockPageGrid.Background = Common.clockPageNowBackgroundBrush;

            if (Common.clockPageNowBackgroundBrush.GetType() != testRectangleFill.GetType())
            {
                testRectangleFill = new LinearGradientBrush
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
            }
            else
            {
                testRectangleFill = new LinearGradientBrush
                {
                    StartPoint = (Point)(Common.clockPageNowBackgroundBrush as LinearGradientBrush).GetValue(LinearGradientBrush.StartPointProperty),
                    EndPoint = (Point)(Common.clockPageNowBackgroundBrush as LinearGradientBrush).GetValue(LinearGradientBrush.EndPointProperty)
                };
                GradientStop[] stops = new GradientStop[(Common.clockPageNowBackgroundBrush as LinearGradientBrush).GradientStops.Count];
                (Common.clockPageNowBackgroundBrush as LinearGradientBrush).GradientStops.CopyTo(stops, 0);
                foreach (GradientStop stop in stops)
                {
                    var item = new GradientStop
                    {
                        Color = stop.Color,
                        Offset = stop.Offset
                    };
                    testRectangleFill.GradientStops.Add(item);
                }
            }
            gradientColorTestRectangle.Fill = testRectangleFill;
            startPointX.Value = testRectangleFill.StartPoint.X;
            startPointY.Value = testRectangleFill.StartPoint.Y;
            endPointX.Value = testRectangleFill.EndPoint.X;
            endPointY.Value = testRectangleFill.EndPoint.Y;
            foreach (GradientStop stop in testRectangleFill.GradientStops)
            {
                GradientColorListViewItem item = new GradientColorListViewItem();
                item.SetValue(stop.Color, stop.Offset);
                gradientColorListView.Items.Add(item);
            }

            var nowTime = DateTime.Now;
            var newAngleOfHou = new CompositeTransform
            {
                Rotation = nowTime.Hour % 12 * 30 + nowTime.Minute * 0.5
            };
            hourHand.RenderTransform = newAngleOfHou;
            var newAngleOfMin = new CompositeTransform
            {
                Rotation = nowTime.Minute * 6 + nowTime.Second * 0.1
            };
            minuteHand.RenderTransform = newAngleOfMin;
            var newAngleOfSec = new CompositeTransform
            {
                Rotation = nowTime.Second * 6
            };
            secondHand.RenderTransform = newAngleOfSec;

            UpdateWeather();

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/XMLData/WeatherCitiesData.xml"));
            citiesXml.Load(new StreamReader(await file.OpenStreamForReadAsync()));
            using (XmlNodeList nodeList = citiesXml.SelectNodes("/WeatherCitiesData/Province"))
            {
                foreach (XmlNode node in nodeList)
                {
                    provincesList.Add(node.Attributes["Name"].Value);
                }
            }
            temperatureLocationProvinceComboBox.ItemsSource = provincesList;
            temperatureLocationProvinceComboBox.SelectedIndex = int.Parse(Common.cityId.Substring(3, 2)) - 1;

            if (Common.isKeepDisplayActived)
            {
                Common.keepDisplay.RequestActive();
            }
            else
            {
                Common.keepDisplay.RequestRelease();
            }

            switch (Common.markOfChangeMode)
            {
                case 5://bing photo
                    BingPhotoBackground_Click(bingPhotoBackground, new RoutedEventArgs());
                    break;
                case 6://unsplash photo
                    UnsplashPhotoBackground_Click(unsplashPhotoBackground, new RoutedEventArgs());
                    break;
                default:
                    break;
            }

            MainPage.mainPageTimer.Tick += ClockTimer_Tick;
        }

        private void ClockTimer_Tick(object sender, object e)
        {
            var nowTime = DateTime.Now;
            stories[nowTime.Second].Begin();
            if (nowTime.Second == 0 && nowTime.Minute == 0 && playerPlayTimes == 0)
            {
                player.IsMuted = !Common.isHourlyReminder;
                player.Play();
                playerPlayTimes++;
            }
            var newAngleOfHou = new CompositeTransform
            {
                Rotation = nowTime.Hour % 12 * 30 + nowTime.Minute * 0.5
            };
            hourHand.RenderTransform = newAngleOfHou;
            var newAngleOfMin = new CompositeTransform
            {
                Rotation = nowTime.Minute * 6 + nowTime.Second * 0.1
            };
            minuteHand.RenderTransform = newAngleOfMin;
            if ((secondHand.RenderTransform as CompositeTransform).Rotation != nowTime.Second * 6
                && secondChangeStoryBoard.GetCurrentState() == ClockState.Stopped)
            {
                secondChangeAnimation.From = nowTime.Second * 6 - 6;
                secondChangeAnimation.To = nowTime.Second * 6;
                secondHand.RenderTransform = new CompositeTransform
                {
                    Rotation = nowTime.Second * 6
                };
                secondChangeStoryBoard.Begin();
            }

            hourTextBlockTen.Text = nowTime.ToString("HH").Remove(1);
            hourTextBlockSingle.Text = nowTime.ToString("HH").Remove(0, 1);
            minuteTextBlockTen.Text = nowTime.ToString("mm").Remove(1);
            minuteTextBlockSingle.Text = nowTime.ToString("mm").Remove(0, 1);
            secondTextBlockTen.Text = nowTime.ToString("ss").Remove(1);//十位数
            secondTextBlockSingle.Text = nowTime.ToString("ss").Remove(0, 1);//个位数

            dateTextBlockYear.Text = nowTime.ToString("yyyy");
            dateTextBlockMonthDay.Text = nowTime.ToString("MMdd");
            dateTextBlockDayOfWeek.Text = nowTime.DayOfWeek.ToString().ToUpper().Remove(3);

            if (Common.markOfChangeMode == 3 && nowTime.Subtract(lastChangeBackgroundTime) > new TimeSpan(0, 0, (int)Common.clockPagePhotosChangeNum))
            {
                MultiplePhotosChange();
            }
            if (Common.markOfChangeMode == 5 && nowTime.Hour == 0 && nowTime.Minute == 5 && nowTime.Second == 0 && canUpdateWebPhoto)
                UpdateWebPhotoBackgtound();
            if (Common.markOfChangeMode == 5 && nowTime.Hour == 0 && nowTime.Minute == 10 && nowTime.Second == 0)
                canUpdateWebPhoto = true;
            if (Common.markOfChangeMode == 6 && nowTime.Hour == 16 && nowTime.Minute == 55 && nowTime.Second == 0 && canUpdateWebPhoto)
                UpdateWebPhotoBackgtound();
            if (Common.markOfChangeMode == 6 && nowTime.Hour == 17 && nowTime.Minute == 5 && nowTime.Second == 0)
                canUpdateWebPhoto = true;

            if (nowTime.Minute % 30 == 0 && nowTime.Second == 0 && canUpdateWeather)
            {
                canUpdateWeather = false;
                UpdateWeather();
            }
            if (nowTime.Minute % 30 == 0 && nowTime.Second == 30)
            {
                canUpdateWeather = true;
            }
        }

        private async void UpdateWeather()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string temperatureString = await httpClient.GetStringAsync(new Uri(string.Format("https://www.tianqiapi.com/api?unescape=1&version=v6&appid={0}&appsecret={1}&cityid={2}", Common.appid, Common.appsecret, Common.cityId)));
                    Common.singleDayWeatherTotalString = temperatureString;
                    var stringList = temperatureString.Split('\"');
                    temperatureLocationTextBlock.Text = stringList[19];
                    temperatureTextBlockWea.Text = stringList[35];
                    temperatureTextBlockReal.Text = stringList[43];

                    Common.sevenDaysWeatherTotalString = temperatureString = await httpClient.GetStringAsync(new Uri(string.Format("https://www.tianqiapi.com/api?unescape=1&version=v1&appid={0}&appsecret={1}&cityid={2}", Common.appid, Common.appsecret, Common.cityId)));//四会101280903
                    stringList = temperatureString.Split('\"');
                    temperatureTextBlockTem1.Text = stringList[69].Replace("℃", "");
                    temperatureTextBlockTem2.Text = stringList[73].Replace("℃", "");
                    sunriseTime = TimeSpan.Parse(stringList[103]);
                    sunsetTime = TimeSpan.Parse(stringList[107]);

                    string dayNight = string.Empty;
                    if (temperatureTextBlockWea.Text == "晴" || temperatureTextBlockWea.Text == "多云" || temperatureTextBlockWea.Text == "阵雨")
                    {
                        dayNight = DateTime.Now.TimeOfDay.CompareTo(sunriseTime) > 0 && DateTime.Now.TimeOfDay.CompareTo(sunsetTime) < 0 ? "白天" : "晚上";
                    }
                    Binding binding = new Binding
                    {
                        Source = Application.Current.Resources[
                            string.Format("{0}{1}", dayNight, temperatureTextBlockWea.Text)] as string
                    };
                    BindingOperations.SetBinding(temperatureTextBlockWeaIcon, PathIcon.DataProperty, binding);
                }
            }
            catch (Exception)
            {
                canUpdateWeather = true;
                temperatureTextBlockWeaIcon = null;
                temperatureTextBlockReal.Text = "NA";
                temperatureTextBlockWea.Text = "N/A";
                temperatureTextBlockTem1.Text = "N";
                temperatureTextBlockTem2.Text = "A";
            }
        }

        private void SecondChangeStoryBoard_Completed(object sender, object e)
        {
            secondChangeStoryBoard.Stop();
        }

        private async void MultiplePhotosChange()
        {
            lastChangeBackgroundTime = DateTime.Now;
            StorageFile file;
            file = Common.photosList[lastChangeBackgroundIndex];
            lastChangeBackgroundIndex += new Random().Next(1, Common.photosList.Count);
            lastChangeBackgroundIndex %= Common.photosList.Count;
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                oldBackground.Background = clockPageGrid.Background;
                oldBackground.Opacity = 1;
                oldBackground.Visibility = Visibility.Visible;
                newBackground.Opacity = 0;
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                clockPageGrid.Background = newBackground.Background = new ImageBrush
                {
                    ImageSource = bitmapImage,
                    Stretch = Stretch.UniformToFill
                };
                newBackground.Visibility = Visibility.Visible;
                backgroundChangeStoryboard.Begin();

                Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                await file.CopyAsync(ApplicationData.Current.LocalFolder, "BackgroundImage", NameCollisionOption.ReplaceExisting);
            }
        }

        private void NumOrDialSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (clockDialGrid.Visibility == Visibility.Visible)
            {
                clockDialGridStoryOut.Begin();
            }
            else
            {
                clockNumGridStoryOut.Begin();
            }
        }

        private void ClockNumGridFadeOut_Completed(object sender, object e)
        {
            clockNumGrid.Visibility = Visibility.Collapsed;
            clockDialGrid.Visibility = Visibility.Visible;
            clockDialGridStoryIn.Begin();
        }

        private void ClockDialGridFadeOut_Completed(object sender, object e)
        {
            clockDialGrid.Visibility = Visibility.Collapsed;
            clockNumGrid.Visibility = Visibility.Visible;
            clockNumGridStoryIn.Begin();
        }

        private void CommonColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            switch (Common.markOfChangeMode)
            {
                case 1:
                    clockPageGrid.Background = Common.clockPageNowBackgroundBrush;
                    Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }

        private void ResetBackground_Click(object sender, RoutedEventArgs e)
        {
            Common.markOfChangeMode = 0;
            oldBackground.Background = clockPageGrid.Background;
            oldBackground.Opacity = 1;
            oldBackground.Visibility = Visibility.Visible;
            newBackground.Background = Common.gradientBrush;
            newBackground.Opacity = 0;
            newBackground.Visibility = Visibility.Visible;
            backgroundChangeStoryboard.Begin();
            clockPageGrid.Background = newBackground.Background;
            Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
            testRectangleFill = new LinearGradientBrush
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
            gradientColorTestRectangle.Fill = testRectangleFill;
            startPointX.Value = Common.gradientBrush.StartPoint.X;
            startPointY.Value = Common.gradientBrush.StartPoint.Y;
            endPointX.Value = Common.gradientBrush.EndPoint.X;
            endPointY.Value = Common.gradientBrush.EndPoint.Y;
            gradientColorListView.Items.Clear();
            foreach (GradientStop stop in testRectangleFill.GradientStops)
            {
                GradientColorListViewItem item = new GradientColorListViewItem();
                item.SetValue(stop.Color, stop.Offset);
                gradientColorListView.Items.Add(item);
            }
        }

        private void SolidColorBackground_Click(object sender, RoutedEventArgs e)
        {
            backgroundChange.IsEnabled = false;
            numOrDialSwitch.IsEnabled = false;
            
            gradientColorPartGrid.Visibility = Visibility.Collapsed;
            commonColorPickerInAnimation.FromHorizontalOffset = 354.4;
            commonColorPickerOutAnimation.FromHorizontalOffset = -354.4;
            commonColorPickerStackPanel.Visibility = Visibility.Visible;
            commonColorPickerInStoryboard.Begin();
            singleOrGradient = true;
        }

        private void GradientColorBackground_Click(object sender, RoutedEventArgs e)
        {
            backgroundChange.IsEnabled = false;
            numOrDialSwitch.IsEnabled = false;
            
            startPointX.IsEnabled = true;
            startPointY.IsEnabled = true;
            endPointX.IsEnabled = true;
            endPointY.IsEnabled = true;
            gradientColorPartGrid.Visibility = Visibility.Visible;
            commonColorPickerInAnimation.FromHorizontalOffset = 564.4;
            commonColorPickerOutAnimation.FromHorizontalOffset = -564.4;
            commonColorPickerStackPanel.Visibility = Visibility.Visible;
            commonColorPickerInStoryboard.Begin();
            singleOrGradient = false;
        }

        private async void PhotoBackground_Click(object sender, RoutedEventArgs e)
        {
            
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Common.markOfChangeMode = 4;
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    oldBackground.Background = clockPageGrid.Background;
                    oldBackground.Opacity = 1;
                    oldBackground.Visibility = Visibility.Visible;
                    newBackground.Opacity = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    clockPageGrid.Background = newBackground.Background = new ImageBrush
                    {
                        ImageSource = bitmapImage,
                        Stretch = Stretch.UniformToFill
                    };
                    newBackground.Visibility = Visibility.Visible;
                    backgroundChangeStoryboard.Begin();

                    Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, "BackgroundImage", NameCollisionOption.ReplaceExisting);
                }
            }
        }

        private async void MultiPhotosBackground_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                List<StorageFile> storageFiles = new List<StorageFile>();
                IReadOnlyList<StorageFile> list = await picker.PickMultipleFilesAsync();
                list.ToList().ForEach(file => storageFiles.Add(file));
                if (storageFiles.Count > 0)
                {
                    if (storageFiles.Count == 1)
                    {
                        ContentDialog dialog = new ContentDialog
                        {
                            Title = "错误",
                            Content = "请选择多张图片",
                            PrimaryButtonText = "重试",
                            SecondaryButtonText = "放弃",
                            Background = new SolidColorBrush(Colors.DarkRed)
                        };
                        ContentDialogResult dialogResult = await dialog.ShowAsync();
                        if (dialogResult == ContentDialogResult.Primary)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        Common.photosList = storageFiles;
                        Common.markOfChangeMode = 3;
                        lastChangeBackgroundTime = new DateTime(0);
                        lastChangeBackgroundIndex = 0;
                    }
                }
                break;
            }
        }

        private void BackgroundChangeStoryboard_Completed(object sender, object e)
        {
            oldBackground.Visibility = Visibility.Collapsed;
            newBackground.Visibility = Visibility.Collapsed;
            switch (Common.markOfChangeMode)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    clockPageWebBackgroundInfoButton.Visibility = clockPageWebBackgroundInfoStackPanel.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                case 6:
                    clockPageWebBackgroundInfoButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void CommonColorPickerConfirm_Click(object sender, RoutedEventArgs e)
        {
            backgroundChange.IsEnabled = true;
            numOrDialSwitch.IsEnabled = true;
            oldBackground.Background = clockPageGrid.Background;
            oldBackground.Opacity = 1;
            oldBackground.Visibility = Visibility.Visible;
            newBackground.Opacity = 0;
            if (singleOrGradient)
            {
                clockPageGrid.Background = newBackground.Background = new SolidColorBrush(commonColorPicker.Color);
                Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                commonColorPickerStackPanel.Margin = new Thickness(0, 0, -354.4, 0);
                
                Common.markOfChangeMode = 1;
            }
            else
            {
                var newLinearBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(startPointX.Value, startPointY.Value),
                    EndPoint = new Point(endPointX.Value, endPointY.Value)
                };
                foreach (GradientColorListViewItem stopItem in gradientColorListView.Items)
                {
                    newLinearBrush.GradientStops.Add(new GradientStop
                    {
                        Color = stopItem.ColorBrush.Color,
                        Offset = stopItem.Offset
                    });
                }
                clockPageGrid.Background = newBackground.Background = newLinearBrush;
                Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                commonColorPickerStackPanel.Margin = new Thickness(0, 0, -564.4, 0);

                Common.markOfChangeMode = 2;
            }
            commonColorPickerOutStoryboard.Begin();
            newBackground.Visibility = Visibility.Visible;
            backgroundChangeStoryboard.Begin();
        }

        private void CommonColorPickerCancel_Click(object sender, RoutedEventArgs e)
        {
            backgroundChange.IsEnabled = true;
            numOrDialSwitch.IsEnabled = true;
            if (singleOrGradient)
            {
                commonColorPickerStackPanel.Margin = new Thickness(0, 0, -354.4, 0);
            }
            else
            {
                commonColorPickerStackPanel.Margin = new Thickness(0, 0, -564.4, 0);
            }
            commonColorPickerOutStoryboard.Begin();
        }

        private void RepositionThemeAnimation_Completed(object sender, object e)
        {
            commonColorPickerStackPanel.Visibility = Visibility.Collapsed;
            commonColorPickerStackPanel.Margin = new Thickness(0, 0, 0, 0);
        }

        private void GradientColorListViewAddButton_Click(object sender, RoutedEventArgs e)
        {
            var item = new GradientColorListViewItem();
            item.SetValue(commonColorPicker.Color, offsetNumberBox.Value);
            gradientColorListView.Items.Add(item);
            testRectangleFill.GradientStops.Add(new GradientStop() { Color = commonColorPicker.Color, Offset = offsetNumberBox.Value });
            testRectangleFill.StartPoint = new Point(startPointX.Value, startPointY.Value);
            testRectangleFill.EndPoint = new Point(endPointX.Value, endPointY.Value);
            gradientColorTestRectangle.Fill = testRectangleFill;
        }

        private void GradientColorListViewDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (gradientColorListView.Items.Count > 0)
            {
                testRectangleFill.GradientStops.RemoveAt(gradientColorListView.SelectedIndex);
                gradientColorListView.Items.RemoveAt(gradientColorListView.SelectedIndex);
            }
        }

        private void NumberBoxValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (this.IsLoaded)
            {
                (gradientColorTestRectangle.Fill as LinearGradientBrush).StartPoint = new Point(startPointX.Value, startPointY.Value);
                (gradientColorTestRectangle.Fill as LinearGradientBrush).EndPoint = new Point(endPointX.Value, endPointY.Value);
            }
        }

        private void FullScreenSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                fullScreenSwitch.Icon = new SymbolIcon(Symbol.BackToWindow);
                fullScreenSwitch.Text = "退出全屏";
            }
            else
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                fullScreenSwitch.Icon = new SymbolIcon(Symbol.FullScreen);
                fullScreenSwitch.Text = "进入全屏";
            }
        }

        private void ClockPageWebBackgroundInfoButton_Click(object sender, RoutedEventArgs e)
        {
            clockPageWebBackgroundInfoStackPanel.Visibility = 
                clockPageWebBackgroundInfoStackPanel.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private async void UpdateWebPhotoBackgtound()
        {
            canUpdateWebPhoto = false;
            switch (Common.markOfChangeMode)
            {
                case 5:
                    using (HttpClient httpClient = new HttpClient())
                    {
                        WebImage bingImage = await GetWebImage("https://cn.bing.com");
                        ClearOutOfRangeCache(Common.clockPageBingPhotosCacheNum);
                        if (bingImage != null)
                        {
                            oldBackground.Background = clockPageGrid.Background;
                            oldBackground.Opacity = 1;
                            oldBackground.Visibility = Visibility.Visible;
                            newBackground.Opacity = 0;
                            ImageBrush imageBrush = new ImageBrush
                            {
                                ImageSource = bingImage.Image,
                                Stretch = Stretch.UniformToFill
                            };
                            clockPageGrid.Background = newBackground.Background = imageBrush;
                            Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                            newBackground.Visibility = Visibility.Visible;
                            backgroundChangeStoryboard.Begin();

                            webPhotoHeadline.Text = bingImage.Headline;
                            webPhotoTitle.Text = bingImage.Title;
                                webPhotoDescription.Text = bingImage.Description;
                            webPhotoCopyright.Text = bingImage.Copyright;
                            StorageFile tempFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/PNGIcon/必应图标.png"));
                            using (IRandomAccessStream stream = await tempFile.OpenAsync(FileAccessMode.Read))
                            {
                                var tmpBitmapImage = new BitmapImage();
                                await tmpBitmapImage.SetSourceAsync(stream);
                                webPhotoUriIcon.Source = tmpBitmapImage;
                            }
                            webPhotoUriText.Text = "https://cn.bing.com";
                        }
                    }
                    break;
                case 6:
                    using (HttpClient httpClient = new HttpClient())
                    {
                        WebImage unsplashImage = await GetWebImage("https://unsplash.com");
                        ClearOutOfRangeCache(Common.clockPageBingPhotosCacheNum);
                        if (unsplashImage != null)
                        {
                            oldBackground.Background = clockPageGrid.Background;
                            oldBackground.Opacity = 1;
                            oldBackground.Visibility = Visibility.Visible;
                            newBackground.Opacity = 0;
                            ImageBrush imageBrush = new ImageBrush
                            {
                                ImageSource = unsplashImage.Image,
                                Stretch = Stretch.UniformToFill
                            };
                            clockPageGrid.Background = newBackground.Background = imageBrush;
                            Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                            newBackground.Visibility = Visibility.Visible;
                            backgroundChangeStoryboard.Begin();

                            webPhotoHeadline.Text = unsplashImage.Headline;
                            webPhotoTitle.Text = unsplashImage.Title;
                            webPhotoDescription.Text = unsplashImage.Description;
                            webPhotoCopyright.Text = unsplashImage.Copyright;
                            StorageFile tempFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/PNGIcon/unsplash图标.png"));
                            using (IRandomAccessStream stream = await tempFile.OpenAsync(FileAccessMode.Read))
                            {
                                var tmpBitmapImage = new BitmapImage();
                                await tmpBitmapImage.SetSourceAsync(stream);
                                webPhotoUriIcon.Source = tmpBitmapImage;
                            }
                            webPhotoUriText.Text = "https://unsplash.com";
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private async void BingPhotoBackground_Click(object sender, RoutedEventArgs e)
        {
            Common.markOfChangeMode = 5;
            clockPageWebBackgroundInfoButton.Visibility = Visibility.Visible;
            webPhotoHeadline.Text = "加载中...";

            canUpdateWebPhoto = false;
            //下载图片并设置背景

            WebImage bingImage = await GetWebImage("https://cn.bing.com");
            ClearOutOfRangeCache(Common.clockPageBingPhotosCacheNum);
            if (bingImage != null)
            {
                oldBackground.Background = clockPageGrid.Background;
                oldBackground.Opacity = 1;
                oldBackground.Visibility = Visibility.Visible;
                newBackground.Opacity = 0;
                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = bingImage.Image,
                    Stretch = Stretch.UniformToFill
                };
                clockPageGrid.Background = newBackground.Background = imageBrush;
                Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                newBackground.Visibility = Visibility.Visible;
                backgroundChangeStoryboard.Begin();

                webPhotoHeadline.Text = bingImage.Headline.Replace("&quot;", "\"");
                webPhotoTitle.Text = bingImage.Title.Replace("&quot;", "\"");
                webPhotoDescription.Text = bingImage.Description.Replace("&quot;", "\"");
                webPhotoCopyright.Text = bingImage.Copyright;
                StorageFile tempFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/PNGIcon/必应图标.png"));
                using (IRandomAccessStream stream = await tempFile.OpenAsync(FileAccessMode.Read))
                {
                    var tmpBitmapImage = new BitmapImage();
                    await tmpBitmapImage.SetSourceAsync(stream);
                    webPhotoUriIcon.Source = tmpBitmapImage;
                }
                webPhotoUriText.Text = "https://cn.bing.com";
            }
            else
            {
                clockPageWebBackgroundInfoButton.Visibility = Visibility.Collapsed;
                ResetBackground_Click(resetBackground, new RoutedEventArgs());
            }
            canUpdateWebPhoto = true;
        }

        private async void UnsplashPhotoBackground_Click(object sender, RoutedEventArgs e)
        {
            Common.markOfChangeMode = 6;
            clockPageWebBackgroundInfoButton.Visibility = Visibility.Visible;
            webPhotoHeadline.Text = "加载中...";

            canUpdateWebPhoto = false;
            //下载图片并设置背景

            WebImage unsplashImage = await GetWebImage("https://unsplash.com");
            ClearOutOfRangeCache(Common.clockPageBingPhotosCacheNum);
            if (unsplashImage != null)
            {
                oldBackground.Background = clockPageGrid.Background;
                oldBackground.Opacity = 1;
                oldBackground.Visibility = Visibility.Visible;
                newBackground.Opacity = 0;
                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = unsplashImage.Image,
                    Stretch = Stretch.UniformToFill
                };
                clockPageGrid.Background = newBackground.Background = imageBrush;
                Common.clockPageNowBackgroundBrush = clockPageGrid.Background;
                newBackground.Visibility = Visibility.Visible;
                backgroundChangeStoryboard.Begin();

                webPhotoHeadline.Text = unsplashImage.Headline;
                webPhotoTitle.Text = unsplashImage.Title;
                webPhotoDescription.Text = unsplashImage.Description;
                webPhotoCopyright.Text = unsplashImage.Copyright;
                StorageFile tempFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/PNGIcon/unsplash图标.png"));
                using (IRandomAccessStream stream = await tempFile.OpenAsync(FileAccessMode.Read))
                {
                    var tmpBitmapImage = new BitmapImage();
                    await tmpBitmapImage.SetSourceAsync(stream);
                    webPhotoUriIcon.Source = tmpBitmapImage;
                }
                webPhotoUriText.Text = "https://unsplash.com";
            }
            else
            {
                clockPageWebBackgroundInfoButton.Visibility = Visibility.Collapsed;
                ResetBackground_Click(resetBackground, new RoutedEventArgs());
            }
            canUpdateWebPhoto = true;
        }

        /// <summary>
        /// 清理超出范围的缓存文件
        /// </summary>
        /// <param name="range">超出range天之后的文件清除</param>
        public static async void ClearOutOfRangeCache(uint range)
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("imagesfrombing");
            var files = await folder.GetFilesAsync();
            foreach (var item in files)
            {
                if (DateTime.Today.Subtract(item.DateCreated.Date).Days > range)
                {
                    await item.DeleteAsync();
                }
            }
        }

        ///// <summary>
        ///// 清理所有缓存文件
        ///// </summary>
        //public static async void ClearCache()
        //{
        //    StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("imagesfrombing");
        //    var files = await folder.GetFilesAsync();
        //    foreach (var item in files)
        //    {
        //        await item.DeleteAsync();
        //    }
        //}

        /// <summary>
        /// 获取图片
        /// 如果本地存在，就获取本地
        /// 如果本地不存在，获取网络
        /// </summary>
        /// <param name="uriFirstString">https://cn.bing.com or https://unsplash.com</param>
        /// <returns></returns>
        public static async Task<WebImage> GetWebImage(string uriFirstString)
        {
            string html = string.Empty;
            string htmlIncludeInfo = string.Empty;
            switch (uriFirstString.Replace("https://", "").Split(".com")[0].Split('.').Last())
            {
                case "bing":
                    try
                    {
                        //using使用，一般用来自动释放资源，也就是 Dispose()方法
                        using (HttpClient http = new HttpClient())
                        {
                            htmlIncludeInfo = await http.GetStringAsync(new Uri(uriFirstString));
                            html = htmlIncludeInfo.Split("\"Description\":\"")[1].Split('\"')[6].Replace("s.cn.bing.net", "cn.bing.com");
                        }
                    }
                    catch (Exception)
                    {
                        return await GetLocalFolderImage(WebImage.WebImageFrom.bing);
                    }
                    break;
                case "unsplash":
                    try
                    {
                        using (HttpClient http = new HttpClient())
                        {
                            string tmp = await http.GetStringAsync(new Uri(uriFirstString));
                            html = uriFirstString + tmp.Split("\">Photo of the Day</a>")[0].Split("href=\"").Last();
                            htmlIncludeInfo = await http.GetStringAsync(new Uri(html));
                            html = htmlIncludeInfo.Split("srcSet=\"")[2].Split("w=")[0].Replace("amp;", "") + "w=1920&h=1080";
                        }
                    }
                    catch (Exception)
                    {
                        return await GetLocalFolderImage(WebImage.WebImageFrom.unsplash);
                    }
                    break;
                default:
                    break;
            }
            html = Regex.Unescape(html);
            if (html == string.Empty)
                return null;
            return await GetLocalFolderImage(html) ?? await GetHttpImage(html, htmlIncludeInfo);
        }

        /// <summary>
        /// 从本地获取图片
        /// </summary>
        private static async Task<WebImage> GetLocalFolderImage(string htmlString)
        {
            StorageFolder folder = await GetImageFolder(htmlString);
            string fileName = Md5(htmlString);
            try
            {
                WebImage localImage = new WebImage();
                switch (folder.DisplayName)
                {
                    case "ImagesFromBing":
                        localImage.ImageFrom = WebImage.WebImageFrom.bing;
                        break;
                    case "ImagesFromUnsplash":
                        localImage.ImageFrom = WebImage.WebImageFrom.unsplash;
                        break;
                    default:
                        localImage.ImageFrom = WebImage.WebImageFrom.nothing;
                        break;
                }
                StorageFile file = await folder.GetFileAsync(fileName + ".jpg");
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage img = new BitmapImage();
                    await img.SetSourceAsync(stream);
                    localImage.Image = img;
                }
                file = await folder.GetFileAsync(fileName + "_info.txt");
                var result = await FileIO.ReadTextAsync(file);
                localImage.TranslateFromFile(result);
                return localImage;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static async Task<WebImage> GetLocalFolderImage(WebImage.WebImageFrom imageFrom)
        {
            WebImage localImage = new WebImage();
            localImage.ImageFrom = imageFrom;
            StorageFolder folder = null;
            switch (imageFrom)
            {
                case WebImage.WebImageFrom.bing:
                    folder = await GetImageFolder("bing");
                    break;
                case WebImage.WebImageFrom.unsplash:
                    folder = await GetImageFolder("unsplash");
                    break;
                case WebImage.WebImageFrom.nothing:
                    break;
                default:
                    break;
            }
            try
            {
                StorageFile fileJPG = null;
                StorageFile fileTXT = null;
                var fileList = await folder.GetFilesAsync();
                for (int i = fileList.Count - 1; i > fileList.Count - 3; i--)
                {
                    if (fileList[i].FileType == ".jpg")
                    {
                        fileJPG = fileList[i];
                    }
                    if (fileList[i].FileType == ".txt")
                    {
                        fileTXT = fileList[i];
                    }
                }
                if (fileJPG == null || fileTXT == null)
                {
                    return null;
                }
                using (var stream = await fileJPG.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage img = new BitmapImage();
                    await img.SetSourceAsync(stream);
                    localImage.Image = img;
                }
                var result = await FileIO.ReadTextAsync(fileTXT);
                localImage.TranslateFromFile(result);
                return localImage;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<WebImage> GetHttpImage(string htmlString, string htmlIncludeInfo)
        {
            try
            {
                WebImage webImage = new WebImage();
                switch (htmlString.Replace("https://", "").Split(".com")[0].Split('.').Last())
                {
                    case "bing":
                        webImage.ImageFrom = WebImage.WebImageFrom.bing;
                        break;
                    case "unsplash":
                        webImage.ImageFrom = WebImage.WebImageFrom.unsplash;
                        break;
                    default:
                        break;
                }
                IBuffer buffer;
                using (HttpClient http = new HttpClient())
                {
                    buffer = await http.GetBufferAsync(new Uri(htmlString));
                }
                
                BitmapImage img = new BitmapImage();
                using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(buffer);
                    stream.Seek(0);
                    await img.SetSourceAsync(stream);
                    webImage.Image = img;
                    webImage.TranslateFromHttp(htmlIncludeInfo);
                    await StorageImageFolder(stream, webImage.PackInformationForFile(), htmlString);
                }
                return webImage;
            }
            catch (Exception e)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Content = e.Message + "\r\n请检查网络是否畅通",
                    CloseButtonText = "知道了"
                };
                _ = await dialog.ShowAsync();
                return null;
            }
        }

        private static async Task StorageImageFolder(IRandomAccessStream stream, string info, string htmlString)
        {
            StorageFolder folder = await GetImageFolder(htmlString);
            string fileName = Md5(htmlString);
            try
            {
                StorageFile file = await folder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBytesAsync(file, await ConvertIRandomAccessStreamByte(stream));
                file = await folder.CreateFileAsync(fileName + "_info.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, info);
            }
            catch (Exception)
            {

            }
        }

        private static async Task<byte[]> ConvertIRandomAccessStreamByte(IRandomAccessStream stream)
        {
            DataReader read = new DataReader(stream.GetInputStreamAt(0));
            await read.LoadAsync((uint)stream.Size);
            byte[] temp = new byte[stream.Size];
            read.ReadBytes(temp);
            return temp;
        }

        private static async Task<StorageFolder> GetImageFolder(string uriFirstString)
        {
            //文件夹
            string name = string.Empty;
            switch (uriFirstString.Replace("https://", "").Split(".com")[0].Split('.').Last())
            {
                case "bing":
                    name = "ImagesFromBing";
                    break;
                case "unsplash":
                    name = "ImagesFromUnsplash";
                    break;
                default:
                    break;
            }
            StorageFolder folder;
            //从本地获取文件夹
            try
            {
                folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(name);
            }
            catch (FileNotFoundException)
            {
                //没找到
                folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(name);
            }
            return folder;
        }

        private static string Md5(string str)
        {
            HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            CryptographicHash cryptographic = hashAlgorithm.CreateHash();
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            cryptographic.Append(buffer);
            return CryptographicBuffer.EncodeToHexString(cryptographic.GetValueAndReset());
        }

        private void ClockPageOperationCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                fullScreenSwitch.Icon = new SymbolIcon(Symbol.BackToWindow);
                fullScreenSwitch.Text = "退出全屏";
            }
            else
            {
                fullScreenSwitch.Icon = new SymbolIcon(Symbol.FullScreen);
                fullScreenSwitch.Text = "进入全屏";
            }
        }

        private void TemperatureLocationProvinceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                using (XmlNodeList nodeList = citiesXml.SelectNodes(string.Format("/WeatherCitiesData/Province[{0}]/City", (sender as ComboBox).SelectedIndex + 1)))
                {
                    temperatureLocationCityComboBox.Items.Clear();
                    foreach (XmlNode node in nodeList)
                    {
                        temperatureLocationCityComboBox.Items.Add(node.Attributes["Name"].Value);
                    }
                }
                if (cityId == string.Empty)
                {
                    if (((sender as ComboBox).SelectedItem as String) == "北京" ||
                        ((sender as ComboBox).SelectedItem as String) == "天津" ||
                        ((sender as ComboBox).SelectedItem as String) == "上海" ||
                        ((sender as ComboBox).SelectedItem as String) == "重庆")
                    {
                        temperatureLocationCityComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        temperatureLocationCityComboBox.SelectedIndex = int.Parse(Common.cityId.Substring(5, 2)) - 1;
                    }
                }
            }
            else
            {
                temperatureLocationCityComboBox.Items.Clear();
            }
        }

        private void TemperatureLocationCityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                using (XmlNodeList nodeList = citiesXml.SelectNodes(string.Format("/WeatherCitiesData/Province[{0}]/City[{1}]/Block", temperatureLocationProvinceComboBox.SelectedIndex + 1, (sender as ComboBox).SelectedIndex + 1)))
                {
                    temperatureLocationBlockComboBox.Items.Clear();
                    foreach (XmlNode node in nodeList)
                    {
                        temperatureLocationBlockComboBox.Items.Add(node.Attributes["Name"].Value);
                    }
                }
                if (cityId == string.Empty)
                {
                    temperatureLocationBlockComboBox.SelectedItem = citiesXml.SelectSingleNode(string.Format("/WeatherCitiesData/Province/City/Block[@Id = {0}]", Common.cityId)).Attributes["Name"].Value;
                }
            }
            else
            {
                temperatureLocationBlockComboBox.Items.Clear();
            }
        }

        private void TemperatureLocationBlockComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                cityId = citiesXml.SelectSingleNode(string.Format("/WeatherCitiesData/Province[{0}]/City[{1}]/Block[{2}]", temperatureLocationProvinceComboBox.SelectedIndex + 1, temperatureLocationCityComboBox.SelectedIndex + 1, (sender as ComboBox).SelectedIndex + 1)).Attributes["Id"].Value;
            }
        }

        private void TemperatureLocationConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            temperatureLocationTextBlock.Text = (string)temperatureLocationBlockComboBox.SelectedItem;
            Common.cityId = cityId;
            temperatureLocationFlyout.Hide();
            UpdateWeather();
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                fullScreenSwitch.Icon = new SymbolIcon(Symbol.FullScreen);
                fullScreenSwitch.Text = "进入全屏";
            }
        }

        private async void WebPhotoHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            _ = await Launcher.LaunchUriAsync(new Uri((((sender as HyperlinkButton).Content as StackPanel).Children[1] as TextBlock).Text));
        }
    }
}
