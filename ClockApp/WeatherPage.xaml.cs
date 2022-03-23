using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WeatherPage : Page
    {
        /// <summary>
        /// 天气城市代码表
        /// </summary>
        private XmlDocument citiesXml = new XmlDocument();
        /// <summary>
        /// 城市代码
        /// </summary>
        private string cityId = string.Empty;
        /// <summary>
        /// 省列表
        /// </summary>
        private List<string> provincesList = new List<string>();

        private string[] sevenDate = new string[7];
        private string[] sevenDaysWeatherIconString = new string[14];
        private string[] sevenDaysTemperatureString = new string[14];
        private int[] sevenDaysTemperatureNumber = new int[14];
        private int SevenDaysTemperatureInstance
        {
            get
            {
                return sevenDaysTemperatureMax - sevenDaysTemperatureMin;
            }
        }
        private int sevenDaysTemperatureMax = 0;
        private int sevenDaysTemperatureMin = 0;
        private string[] sevenDaysAirString = new string[7];

        public WeatherPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/User/XMLData/WeatherCitiesData.xml"));
            citiesXml.Load(new StreamReader(await file.OpenStreamForReadAsync()));
            using (XmlNodeList nodeList = citiesXml.SelectNodes("/WeatherCitiesData/Province"))
            {
                foreach (XmlNode node in nodeList)
                {
                    provincesList.Add(node.Attributes["Name"].Value);
                }
            }
            locationProvinceComboBox.ItemsSource = provincesList;
            locationProvinceComboBox.SelectedIndex = int.Parse(Common.cityId.Substring(3, 2)) - 1;
            UpdateWeatherData();
        }

        private async void UpdateWeatherData()
        {
            updatingRing.IsActive = true;
            await UpdateWeatherString();
            UpdateTheSingleDayWeatherDisplayGrid();
            UpdateTheSevenDaysWeatherDisplayGrid();
            updatingRing.IsActive = false;
            UpdateWeatherDetailModel(0);
            TemperatureLineStoryboard.Begin();
        }

        private async Task UpdateWeatherString()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    Common.singleDayWeatherTotalString = await httpClient.GetStringAsync(new Uri(string.Format("https://www.tianqiapi.com/api?unescape=1&version=v6&appid={0}&appsecret={1}&cityid={2}", Common.appid, Common.appsecret, Common.cityId)));
                    Common.sevenDaysWeatherTotalString  = await httpClient.GetStringAsync(new Uri(string.Format("https://www.tianqiapi.com/api?unescape=1&version=v1&appid={0}&appsecret={1}&cityid={2}", Common.appid, Common.appsecret, Common.cityId)));//四会101280903
                    //标注刷新时间
                    refreshTimeTextBlock.Text = DateTime.Now.ToString("t");
                }
            }
            catch (Exception e)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Content = e.Message + "\r\n请检查网络是否畅通",
                    CloseButtonText = "知道了"
                };
                _ = await dialog.ShowAsync();
                //不标注该次刷新时间
            }
        }

        private void LocationProvinceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                using (XmlNodeList nodeList = citiesXml.SelectNodes(string.Format("/WeatherCitiesData/Province[{0}]/City", (sender as ComboBox).SelectedIndex + 1)))
                {
                    locationCityComboBox.Items.Clear();
                    foreach (XmlNode node in nodeList)
                    {
                        locationCityComboBox.Items.Add(node.Attributes["Name"].Value);
                    }
                }
                if (cityId == string.Empty)
                {
                    if (((sender as ComboBox).SelectedItem as String) == "北京" ||
                        ((sender as ComboBox).SelectedItem as String) == "天津" ||
                        ((sender as ComboBox).SelectedItem as String) == "上海" ||
                        ((sender as ComboBox).SelectedItem as String) == "重庆")
                    {
                        locationCityComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        locationCityComboBox.SelectedIndex = int.Parse(Common.cityId.Substring(5, 2)) - 1;
                    }
                }
            }
            else
            {
                locationCityComboBox.Items.Clear();
            }
        }

        private void LocationCityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                using (XmlNodeList nodeList = citiesXml.SelectNodes(string.Format("/WeatherCitiesData/Province[{0}]/City[{1}]/Block", locationProvinceComboBox.SelectedIndex + 1, (sender as ComboBox).SelectedIndex + 1)))
                {
                    locationBlockComboBox.Items.Clear();
                    foreach (XmlNode node in nodeList)
                    {
                        locationBlockComboBox.Items.Add(node.Attributes["Name"].Value);
                    }
                }
                if (cityId == string.Empty)
                {
                    locationBlockComboBox.SelectedItem = citiesXml.SelectSingleNode(string.Format("/WeatherCitiesData/Province/City/Block[@Id = {0}]", Common.cityId)).Attributes["Name"].Value;
                }
            }
            else
            {
                locationBlockComboBox.Items.Clear();
            }
        }

        private void LocationBlockComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                cityId = citiesXml.SelectSingleNode(string.Format("/WeatherCitiesData/Province[{0}]/City[{1}]/Block[{2}]", locationProvinceComboBox.SelectedIndex + 1, locationCityComboBox.SelectedIndex + 1, (sender as ComboBox).SelectedIndex + 1)).Attributes["Id"].Value;
            }
        }

        private void LocationConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            locationTextBlock.Text = (string)locationBlockComboBox.SelectedItem;
            Common.cityId = cityId;
            locationFlyout.Hide();
            UpdateWeatherData();
        }

        private void RefreshTimeTextBlock_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateWeatherData();
        }

        private void UpdateTheSingleDayWeatherDisplayGrid()
        {
            TimeSpan sunriseTime = TimeSpan.Parse(
                Common.sevenDaysWeatherXml.SelectSingleNode("/total/data[1]").Attributes["sunrise"].Value);
            TimeSpan sunsetTime = TimeSpan.Parse(
                Common.sevenDaysWeatherXml.SelectSingleNode("/total/data[1]").Attributes["sunset"].Value);
            string[] list = Common.singleDayWeatherTotalString.Split('\"');
            locationTextBlock.Text = list[19];
            singleDayWeatherTemperature.Text = list[43];
            string dayNight = string.Empty;
            if (list[35] == "晴" || list[35] == "多云" || list[35] == "阵雨")
            {
                dayNight = DateTime.Now.TimeOfDay.CompareTo(sunriseTime) > 0 && DateTime.Now.TimeOfDay.CompareTo(sunsetTime) < 0 ? "白天" : "晚上";
            }
            var binding = new Binding
            {
                Source = Application.Current.Resources[string.Format("{0}{1}", dayNight, list[35])] as string
            };
            BindingOperations.SetBinding(singleDayWeatherPathIcon, PathIcon.DataProperty, binding);
            singleDayWeatherPathIconLabel.Text = list[35];
        }

        /// <summary>
        /// 更新单天的详情单元
        /// </summary>
        /// <param name="dayOffset">今天为0，明天为1，以此类推</param>
        private void UpdateWeatherDetailModel(int dayOffset)
        {
            dayOffset++;
            TimeSpan sunriseTime = TimeSpan.Parse(
                Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["sunrise"].Value);
            TimeSpan sunsetTime = TimeSpan.Parse(
                Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["sunset"].Value);
            singleDayWeatherSunriseTimeText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["sunrise"].Value;
            singleDayWeatherSunsetTimeText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["sunset"].Value;
            double nowTime = DateTime.Now.TimeOfDay.TotalMinutes;
            if (nowTime > sunriseTime.TotalMinutes && nowTime < sunsetTime.TotalMinutes && dayOffset == 1)
            {
                singleDayWeatherBigSunIcon.Visibility = Visibility.Visible;
                singleDayWeatherBigSunRotation.Rotation = (nowTime - sunriseTime.TotalMinutes) * 180.0 / (sunsetTime.TotalMinutes - sunriseTime.TotalMinutes);
                
                var animation = new DoubleAnimation
                {
                    EnableDependentAnimation = true,
                    Duration = TimeSpan.FromSeconds(1),
                    From = 0,
                    To = singleDayWeatherBigSunRotation.Rotation
                };

                Storyboard.SetTargetProperty(animation, nameof(singleDayWeatherBigSunRotation.Rotation));
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                storyboard.FillBehavior = FillBehavior.Stop;
                Storyboard.SetTarget(storyboard.Children[0] as DoubleAnimation, singleDayWeatherBigSunRotation);
                storyboard.Begin();
            }
            else
            {
                singleDayWeatherBigSunIcon.Visibility = Visibility.Collapsed;
            }
            singleDayWeatherTem1AndTem2.Text = string.Format("{0}/{1}",
                Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["tem1"].Value,
                Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["tem2"].Value);
            singleDayWeatherDescriptionText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["wea"].Value;

            try
            {
                singleDayWeatherHumidity.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["humidity"].Value;
                singleDayWeatherHumidityRing.Value = double.Parse(singleDayWeatherHumidity.Text.Split('%')[0]);
            }
            catch (Exception)
            {
                singleDayWeatherHumidityRing.Value = 0;
                singleDayWeatherHumidity.Text = "--%";
            }

            singleDayWeatherVisibilityText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["visibility"].Value;
            if (singleDayWeatherVisibilityText.Text != "")
            {
                double parseValue = double.Parse(singleDayWeatherVisibilityText.Text.Split('k')[0]);
                if (parseValue >= 1)
                {
                    singleDayWeatherVisibilityRing.MinValue = 1;
                    singleDayWeatherVisibilityRing.MaxValue = 10;
                    
                    if (parseValue >= 1 && parseValue < 2)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "较差";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Orange);
                    }
                    else if (parseValue >= 2 && parseValue < 10)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "一般";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (parseValue >= 10)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "好";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Green);
                    }
                }
                else
                {
                    singleDayWeatherVisibilityRing.MinValue = 0;
                    singleDayWeatherVisibilityRing.MaxValue = 1;
                    singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x00, G = 0x78, B = 0xD7 });
                    if (parseValue >= 0 && parseValue < 0.05)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "极差";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Purple);
                    }
                    else if (parseValue >= 0.05 && parseValue < 0.5)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "差";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (parseValue >= 0.5 && parseValue < 1)
                    {
                        singleDayWeatherVisibilityLevelText.Text = "较差";
                        singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(Colors.Orange);
                    }
                }
                singleDayWeatherVisibilityRing.Value = parseValue;
            }
            else
            {
                singleDayWeatherVisibilityText.Text = "--";
                singleDayWeatherVisibilityLevelText.Text = "";
                singleDayWeatherVisibilityRing.Value = singleDayWeatherVisibilityRing.MinValue;
                singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x00, G = 0x78, B = 0xD7 });
            }

            singleDayWeatherPressureText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["pressure"].Value + "hpa";
            if (singleDayWeatherPressureText.Text == "hpa")
            {
                singleDayWeatherPressureText.Text = "--hpa";
            }

            string str = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["air"].Value;
            if (str == "null" || str == "")
            {
                singleDayWeatherAirQualityText.Text = "--";
                singleDayWeatherAirLevelText.Text = "";
                singleDayWeatherAirQualityRing.Value = singleDayWeatherAirQualityRing.MinValue;
                singleDayWeatherVisibilityRing.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x00, G = 0x78, B = 0xD7 });
            }
            else
            {
                singleDayWeatherAirQualityText.Text = str;
                singleDayWeatherAirLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", dayOffset)).Attributes["air_level"].Value;
                switch (singleDayWeatherAirLevelText.Text)
                {
                    case "优":
                        singleDayWeatherAirQualityRing.MinValue = 0;
                        singleDayWeatherAirQualityRing.MaxValue = 50;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case "良":
                        singleDayWeatherAirQualityRing.MinValue = 51;
                        singleDayWeatherAirQualityRing.MaxValue = 100;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.Blue);
                        break;
                    case "轻度污染":
                        singleDayWeatherAirQualityRing.MinValue = 101;
                        singleDayWeatherAirQualityRing.MaxValue = 150;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.Orange);
                        break;
                    case "中度污染":
                        singleDayWeatherAirQualityRing.MinValue = 151;
                        singleDayWeatherAirQualityRing.MaxValue = 200;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    case "重度污染":
                        singleDayWeatherAirQualityRing.MinValue = 201;
                        singleDayWeatherAirQualityRing.MaxValue = 300;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.Purple);
                        break;
                    case "严重污染":
                        singleDayWeatherAirQualityRing.MinValue = 301;
                        singleDayWeatherAirQualityRing.MaxValue = 500;
                        singleDayWeatherAirQualityRing.Foreground = new SolidColorBrush(Colors.DarkRed);
                        break;
                    default:
                        break;
                }
                singleDayWeatherAirQualityRing.Value = double.Parse(str);
            }

            singleDayIndexUltravioletText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[1]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexUltravioletLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[1]", dayOffset)).Attributes["level"].Value;

            singleDayIndexLoseWeightText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[2]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexLoseWeightLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[2]", dayOffset)).Attributes["level"].Value;

            singleDayIndexBloodSugerText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[3]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexBloodSugerLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[3]", dayOffset)).Attributes["level"].Value;

            singleDayIndexClothText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[4]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexClothLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[4]", dayOffset)).Attributes["level"].Value;

            singleDayIndexCarWashText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[5]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexCarWashLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[5]", dayOffset)).Attributes["level"].Value;

            singleDayIndexSpreadText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[6]", dayOffset)).Attributes["desc"].Value;
            singleDayIndexSpreadLevelText.Text = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]/index[6]", dayOffset)).Attributes["level"].Value;

            var nodeList = Common.sevenDaysWeatherXml.SelectNodes(string.Format("/total/data[{0}]/hours", dayOffset));
            singleHourLineDisplayGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < nodeList.Count; i++)
            {
                singleHourLineDisplayGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            }
            singleHourLineDisplayGrid.Children.Clear();
            List<int> temList = new List<int>();
            int temMax = 0;
            int temMin = 0;
            for (int i = 0; i < nodeList.Count; i++)
            {
                Grid grid = CreateTheSingleTimeGrid(sunriseTime, sunsetTime, nodeList.Item(i));
                singleHourLineDisplayGrid.Children.Add(grid);
                Grid.SetColumn(grid, i);

                int temValue = int.Parse(nodeList.Item(i).Attributes["tem"].Value);
                if (i == 0)
                {
                    temMax = temMin = temValue;
                }
                else
                {
                    temMin = Math.Min(temMin, temValue);
                    temMax = Math.Max(temMax, temValue);
                }
                temList.Add(temValue);
            }
            Windows.UI.Xaml.Shapes.Path temLine = new Windows.UI.Xaml.Shapes.Path
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 95),
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = new PathGeometry()
            };
            for (int i = 0; i < temList.Count; i++)
            {
                (singleHourLineDisplayGrid.Children[i] as Grid).RowDefinitions[2].Height = new GridLength((temMax - temMin) * 10.0 + 50.0);
                TextBlock temText = new TextBlock
                {
                    Margin = new Thickness(0, 10 * (temMax - temList[i]) + 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Text = temList[i].ToString() + "℃"
                };
                (singleHourLineDisplayGrid.Children[i] as Grid).Children.Add(temText);
                Grid.SetRow(temText, 2);


                if (i == 0)
                {
                    (temLine.Data as PathGeometry).Figures.Add(new PathFigure { StartPoint = new Point(10, (temMax - temList[i]) * 10) });
                }
                else if (i == temList.Count - 1)
                {
                    (temLine.Data as PathGeometry).Figures[0].Segments.Add(new BezierSegment
                    {
                        Point1 = new Point(60 * i, (temMax - temList[i - 1]) * 10),
                        Point2 = new Point(60 * i, (temMax - temList[i]) * 10),
                        Point3 = new Point(60 * i + 50, (temMax - temList[i]) * 10)
                    });
                }
                else
                {
                    (temLine.Data as PathGeometry).Figures[0].Segments.Add(new BezierSegment
                    {
                        Point1 = new Point(60 * i, (temMax - temList[i - 1]) * 10),
                        Point2 = new Point(60 * i, (temMax - temList[i]) * 10),
                        Point3 = new Point(60 * i + 30, (temMax - temList[i]) * 10)
                    });
                }
            }
            singleHourLineDisplayGrid.Children.Add(temLine);
            Grid.SetColumnSpan(temLine, temList.Count);
        }

        private Grid CreateTheSingleTimeGrid(TimeSpan sunriseTime, TimeSpan sunsetTime, XmlNode node)
        {
            string wea = node.Attributes["wea"].Value;
            string hours = node.Attributes["hours"].Value;
            string win = node.Attributes["win"].Value;
            string win_speed = node.Attributes["win_speed"].Value;
            TimeSpan time = TimeSpan.FromHours(double.Parse(hours.Substring(0,2)));
            Grid result = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                RowDefinitions = 
                {
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto },
                    new RowDefinition{ Height = GridLength.Auto }
                }
            };
            PathIcon weatherIcon = new PathIcon
            {
                Width = 24,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new CompositeTransform
                {
                    ScaleX = 1.25,
                    ScaleY = 1.25
                }
            };
            string dayNight = string.Empty;
            if (wea == "晴" || wea == "多云" || wea == "阵雨")
            {
                dayNight = time.CompareTo(sunriseTime) > 0 && time.CompareTo(sunsetTime) < 0 ? "白天" : "晚上";
            }
            var binding = new Binding
            {
                Source = Application.Current.Resources[string.Format("{0}{1}", dayNight, wea)] as string
            };
            BindingOperations.SetBinding(weatherIcon, PathIcon.DataProperty, binding);

            TextBlock weatherText = new TextBlock
            {
                Text = wea,
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            PathIcon windIcon = new PathIcon
            {
                Width = 24,
                Height = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
            };
            binding = new Binding
            {
                Source = Application.Current.Resources[win] as string
            };
            BindingOperations.SetBinding(windIcon, PathIcon.DataProperty, binding);

            TextBlock windLevelText = new TextBlock
            {
                Text = win_speed,
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            TextBlock hoursText = new TextBlock
            {
                Text = hours,
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            result.Children.Add(weatherIcon);
            result.Children.Add(weatherText);
            result.Children.Add(windIcon);
            result.Children.Add(windLevelText);
            result.Children.Add(hoursText);

            Grid.SetRow(weatherIcon, 0);
            Grid.SetRow(weatherText, 1);
            Grid.SetRow(windIcon, 3);
            Grid.SetRow(windLevelText, 4);
            Grid.SetRow(hoursText, 5);

            return result;
        }

        private void UpdateTheSevenDaysWeatherDisplayGrid()
        {
            GetNowSevenDaysWeatherTotalDataString();
            TranslateStringArrayToNumberArrayAndOther(sevenDaysTemperatureString);
            
            sevenDaysTemperatrueLinesDisplayGrid.Height = new GridLength(88 + 10 * SevenDaysTemperatureInstance);
            SetTheSevenDaysTextBlocks();
            SetTheSevenDaysBezierPoints();
            SetTheSevenDaysColors();
            SetTheSevenDaysWeatherIcon();
        }

        private void GetNowSevenDaysWeatherTotalDataString()
        {
            //从Common类获取当前获取到的七天天气数据字符串
            //分解并装入相应的变量
            for (int i = 1; i < 8; i++)
            {
                DateTime date = DateTime.Parse(
                    Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["date"].Value);
                sevenDate[i - 1] = date.ToString("MM/dd");
                sevenDaysWeatherIconString[(i - 1) * 2] = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["wea_day"].Value;
                sevenDaysWeatherIconString[(i - 1) * 2 + 1] = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["wea_night"].Value;
                sevenDaysTemperatureString[(i - 1) * 2] = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["tem1"].Value;
                sevenDaysTemperatureString[(i - 1) * 2 + 1] = Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["tem2"].Value;
                sevenDaysAirString[i - 1] = string.Format("{0} {1}",
                    Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["air_level"].Value,
                    Common.sevenDaysWeatherXml.SelectSingleNode(string.Format("/total/data[{0}]", i)).Attributes["air"].Value);
                if (sevenDaysAirString[i - 1].Contains("null") || sevenDaysAirString[i - 1] == " ")
                {
                    sevenDaysAirString[i - 1] = "null";
                }
            }
        }

        private void TranslateStringArrayToNumberArrayAndOther(string[] str)
        {
            //将字符串数组转换成数字数组
            //设置其他相关数值变量的值
            int numMax = 0;
            int numMin = 0;
            for (int i = 0; i < str.Length; i++)
            {
                sevenDaysTemperatureNumber[i] = int.Parse(str[i].Remove(str[i].Length - 1));
                if (i == 0)
                {
                    numMax = numMin = sevenDaysTemperatureNumber[i];
                }
                numMax = Math.Max(numMax, sevenDaysTemperatureNumber[i]);
                numMin = Math.Min(numMin, sevenDaysTemperatureNumber[i]);
            }
            sevenDaysTemperatureMax = numMax;
            sevenDaysTemperatureMin = numMin;
        }

        private void SetTheSevenDaysTextBlocks()
        {
            firstDayDate.Text = sevenDate[0];
            secondDayDate.Text = sevenDate[1];
            thirdDayDate.Text = sevenDate[2];
            fourthDayDate.Text = sevenDate[3];
            fifthDayDate.Text = sevenDate[4];
            sixthDayDate.Text = sevenDate[5];
            seventhDayDate.Text = sevenDate[6];
            for (int i = 0; i < 7; i++)
            {
                (singleDayWeatherSelection.Items[i] as PivotItem).Header = sevenDate[i];
            }

            textBlock.Text = sevenDaysTemperatureString[0];
            textBlock1.Text = sevenDaysTemperatureString[1];
            textBlock2.Text = sevenDaysTemperatureString[2];
            textBlock3.Text = sevenDaysTemperatureString[3];
            textBlock4.Text = sevenDaysTemperatureString[4];
            textBlock5.Text = sevenDaysTemperatureString[5];
            textBlock6.Text = sevenDaysTemperatureString[6];
            textBlock7.Text = sevenDaysTemperatureString[7];
            textBlock8.Text = sevenDaysTemperatureString[8];
            textBlock9.Text = sevenDaysTemperatureString[9];
            textBlock10.Text = sevenDaysTemperatureString[10];
            textBlock11.Text = sevenDaysTemperatureString[11];
            textBlock12.Text = sevenDaysTemperatureString[12];
            textBlock13.Text = sevenDaysTemperatureString[13];

            textBlock.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[0]) * 10, 0, 0);
            textBlock1.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[1] - sevenDaysTemperatureMin) * 10);
            textBlock2.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[2]) * 10, 0, 0);
            textBlock3.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[3] - sevenDaysTemperatureMin) * 10);
            textBlock4.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[4]) * 10, 0, 0);
            textBlock5.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[5] - sevenDaysTemperatureMin) * 10);
            textBlock6.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[6]) * 10, 0, 0);
            textBlock7.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[7] - sevenDaysTemperatureMin) * 10);
            textBlock8.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[8]) * 10, 0, 0);
            textBlock9.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[9] - sevenDaysTemperatureMin) * 10);
            textBlock10.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[10]) * 10, 0, 0);
            textBlock11.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[11] - sevenDaysTemperatureMin) * 10);
            textBlock12.Margin = new Thickness(0, (sevenDaysTemperatureMax - sevenDaysTemperatureNumber[12]) * 10, 0, 0);
            textBlock13.Margin = new Thickness(0, 0, 0, (sevenDaysTemperatureNumber[13] - sevenDaysTemperatureMin) * 10);

            (firstDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[0];
            switch (sevenDaysAirString[0].Split(" ")[0])
            {
                case "优":
                    firstDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    firstDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    firstDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    firstDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    firstDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    firstDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    firstDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (secondDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[1];
            switch (sevenDaysAirString[1].Split(" ")[0])
            {
                case "优":
                    secondDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    secondDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    secondDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    secondDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    secondDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    secondDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    secondDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (thirdDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[2];
            switch (sevenDaysAirString[2].Split(" ")[0])
            {
                case "优":
                    thirdDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    thirdDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    thirdDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    thirdDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    thirdDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    thirdDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    thirdDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (fourthDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[3];
            switch (sevenDaysAirString[3].Split(" ")[0])
            {
                case "优":
                    fourthDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    fourthDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    fourthDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    fourthDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    fourthDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    fourthDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    fourthDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (fifthDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[4];
            switch (sevenDaysAirString[4].Split(" ")[0])
            {
                case "优":
                    fifthDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    fifthDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    fifthDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    fifthDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    fifthDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    fifthDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    fifthDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (sixthDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[5];
            switch (sevenDaysAirString[5].Split(" ")[0])
            {
                case "优":
                    sixthDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    sixthDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    sixthDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    sixthDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    sixthDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    sixthDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    sixthDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
            (seventhDayAir.Children[0] as TextBlock).Text = sevenDaysAirString[6];
            switch (sevenDaysAirString[6].Split(" ")[0])
            {
                case "优":
                    seventhDayAir.Background = new SolidColorBrush(Colors.Green);
                    break;
                case "良":
                    seventhDayAir.Background = new SolidColorBrush(Colors.Blue);
                    break;
                case "轻度污染":
                    seventhDayAir.Background = new SolidColorBrush(Colors.Orange);
                    break;
                case "中度污染":
                    seventhDayAir.Background = new SolidColorBrush(Colors.Red);
                    break;
                case "重度污染":
                    seventhDayAir.Background = new SolidColorBrush(Colors.Purple);
                    break;
                case "严重污染":
                    seventhDayAir.Background = new SolidColorBrush(Colors.DarkRed);
                    break;
                default:
                    seventhDayAir.Background = new SolidColorBrush(Colors.Gray);
                    break;
            }
        }

        private void SetTheSevenDaysColors()
        {
            int[] redList = new int[6];
            int[] blueList = new int[6];
            int[] redRNum = new int[6];
            int[] redBNum = new int[6];
            int[] blueRNum = new int[6];
            int[] blueBNum = new int[6];
            for (int i = 2; i < sevenDaysTemperatureNumber.Length; i += 2)
            {
                redList[i / 2 - 1] = sevenDaysTemperatureNumber[0] - sevenDaysTemperatureNumber[i];
                if (redList[i / 2 - 1] < -0xC)
                {
                    redRNum[i / 2 - 1] = redList[i / 2 - 1] + 0xC;
                    redList[i / 2 - 1] = -0xC;
                    redBNum[i / 2 - 1] = 0;
                }
                else if (redList[i / 2 - 1] > 0x3)
                {
                    redBNum[i / 2 - 1] = redList[i / 2 - 1] - 0x3;
                    redList[i / 2 - 1] = 0x3;
                    redRNum[i / 2 - 1] = 0;
                }
                else
                {
                    redRNum[i / 2 - 1] = 0;
                    redBNum[i / 2 - 1] = 0;
                }
                blueList[i / 2 - 1] = sevenDaysTemperatureNumber[i + 1] - sevenDaysTemperatureNumber[1];
                if (blueList[i / 2 - 1] < -0x9)
                {
                    blueRNum[i / 2 - 1] = -0x9 - blueList[i / 2 - 1];
                    blueList[i / 2 - 1] = -0x9;
                    blueBNum[i / 2 - 1] = 0;
                }
                else if (blueList[i / 2 - 1] > 0x6)
                {
                    blueBNum[i / 2 - 1] = 0x6 - blueList[i / 2 - 1];
                    blueList[i / 2 - 1] = 0x6;
                    blueRNum[i / 2 - 1] = 0;
                }
                else
                {
                    blueRNum[i / 2 - 1] = 0;
                    blueBNum[i / 2 - 1] = 0;
                }
            }
            for (int i = 0; i < 6; i++)
            {
                redList[i] *= 0x10;
                blueList[i] *= 0x10;
                redRNum[i] *= 0x10;
                redBNum[i] *= 0x10;
                blueRNum[i] *= 0x10;
                blueBNum[i] *= 0x10;
            }
        //#FFFFC200 #FF0099FF
            grid6DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[0]), G = (byte)(0xC2 + redList[0]), B = (byte)redBNum[0]
            };
            grid5DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[1]), G = (byte)(0xC2 + redList[1]), B = (byte)redBNum[1]
            };
            grid4DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[2]), G = (byte)(0xC2 + redList[2]), B = (byte)redBNum[2]
            };
            grid3DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[3]), G = (byte)(0xC2 + redList[3]), B = (byte)redBNum[3]
            };
            grid2DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[4]), G = (byte)(0xC2 + redList[4]), B = (byte)redBNum[4]
            };
            grid1DayColor.Value = new Color
            {
                A = 0xFF, R = (byte)(0xFF + redRNum[5]), G = (byte)(0xC2 + redList[5]), B = (byte)redBNum[5]
            };

            grid6NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[0], G = (byte)(0x99 + blueList[0]), B = (byte)(0xFF - blueBNum[0])
            };
            grid5NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[1], G = (byte)(0x99 + blueList[1]), B = (byte)(0xFF - blueBNum[1])
            };
            grid4NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[2], G = (byte)(0x99 + blueList[2]), B = (byte)(0xFF - blueBNum[2])
            };
            grid3NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[3], G = (byte)(0x99 + blueList[3]), B = (byte)(0xFF - blueBNum[3])
            };
            grid2NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[4], G = (byte)(0x99 + blueList[4]), B = (byte)(0xFF - blueBNum[4])
            };
            grid1NightColor.Value = new Color
            {
                A = 0xFF, R = (byte)blueRNum[5], G = (byte)(0x99 + blueList[5]), B = (byte)(0xFF - blueBNum[5])
            };
        }

        private void SetTheSevenDaysBezierPoints()
        {
            var basicWidth = sevenDaysDisplayGrid.ActualWidth / 7;
            double[] points1Or2X = {basicWidth, basicWidth * 2, basicWidth * 3, basicWidth * 4, basicWidth * 5, basicWidth * 6 };
            double[] points3X = new double[6];
            for (int i = 0; i < points1Or2X.Length; i++)
            {
                points3X[i] = points1Or2X[i] + basicWidth / 2;
            }
            points3X[5] = sevenDaysDisplayGrid.ActualWidth - 10;

            redTemperatrueLine.StartPoint = new Point(10, textBlock.Margin.Top);
            redTemperatrueLine1.Point1 = new Point(points1Or2X[0], textBlock.Margin.Top);
            redTemperatrueLine1.Point2 = new Point(points1Or2X[0], textBlock2.Margin.Top);
            redTemperatrueLine1.Point3 = new Point(points3X[0], textBlock2.Margin.Top);
            redTemperatrueLine2.Point1 = new Point(points1Or2X[1], textBlock2.Margin.Top);
            redTemperatrueLine2.Point2 = new Point(points1Or2X[1], textBlock4.Margin.Top);
            redTemperatrueLine2.Point3 = new Point(points3X[1], textBlock4.Margin.Top);
            redTemperatrueLine3.Point1 = new Point(points1Or2X[2], textBlock4.Margin.Top);
            redTemperatrueLine3.Point2 = new Point(points1Or2X[2], textBlock6.Margin.Top);
            redTemperatrueLine3.Point3 = new Point(points3X[2], textBlock6.Margin.Top);
            redTemperatrueLine4.Point1 = new Point(points1Or2X[3], textBlock6.Margin.Top);
            redTemperatrueLine4.Point2 = new Point(points1Or2X[3], textBlock8.Margin.Top);
            redTemperatrueLine4.Point3 = new Point(points3X[3], textBlock8.Margin.Top);
            redTemperatrueLine5.Point1 = new Point(points1Or2X[4], textBlock8.Margin.Top);
            redTemperatrueLine5.Point2 = new Point(points1Or2X[4], textBlock10.Margin.Top);
            redTemperatrueLine5.Point3 = new Point(points3X[4], textBlock10.Margin.Top);
            redTemperatrueLine6.Point1 = new Point(points1Or2X[5], textBlock10.Margin.Top);
            redTemperatrueLine6.Point2 = new Point(points1Or2X[5], textBlock12.Margin.Top);
            redTemperatrueLine6.Point3 = new Point(points3X[5], textBlock12.Margin.Top);

            blueTemperatrueLine.StartPoint = new Point(10, 0 - textBlock1.Margin.Bottom);
            blueTemperatrueLine1.Point1 = new Point(points1Or2X[0], 0 - textBlock1.Margin.Bottom);
            blueTemperatrueLine1.Point2 = new Point(points1Or2X[0], 0 - textBlock3.Margin.Bottom);
            blueTemperatrueLine1.Point3 = new Point(points3X[0], 0 - textBlock3.Margin.Bottom);
            blueTemperatrueLine2.Point1 = new Point(points1Or2X[1], 0 - textBlock3.Margin.Bottom);
            blueTemperatrueLine2.Point2 = new Point(points1Or2X[1], 0 - textBlock5.Margin.Bottom);
            blueTemperatrueLine2.Point3 = new Point(points3X[1], 0 - textBlock5.Margin.Bottom);
            blueTemperatrueLine3.Point1 = new Point(points1Or2X[2], 0 - textBlock5.Margin.Bottom);
            blueTemperatrueLine3.Point2 = new Point(points1Or2X[2], 0 - textBlock7.Margin.Bottom);
            blueTemperatrueLine3.Point3 = new Point(points3X[2], 0 - textBlock7.Margin.Bottom);
            blueTemperatrueLine4.Point1 = new Point(points1Or2X[3], 0 - textBlock7.Margin.Bottom);
            blueTemperatrueLine4.Point2 = new Point(points1Or2X[3], 0 - textBlock9.Margin.Bottom);
            blueTemperatrueLine4.Point3 = new Point(points3X[3], 0 - textBlock9.Margin.Bottom);
            blueTemperatrueLine5.Point1 = new Point(points1Or2X[4], 0 - textBlock9.Margin.Bottom);
            blueTemperatrueLine5.Point2 = new Point(points1Or2X[4], 0 - textBlock11.Margin.Bottom);
            blueTemperatrueLine5.Point3 = new Point(points3X[4], 0 - textBlock11.Margin.Bottom);
            blueTemperatrueLine6.Point1 = new Point(points1Or2X[5], 0 - textBlock11.Margin.Bottom);
            blueTemperatrueLine6.Point2 = new Point(points1Or2X[5], 0 - textBlock13.Margin.Bottom);
            blueTemperatrueLine6.Point3 = new Point(points3X[5], 0 - textBlock13.Margin.Bottom);
        }

        private void SetTheSevenDaysWeatherIcon()
        {
            string[] str = new string[14];
            for (int i = 0; i < sevenDaysWeatherIconString.Length; i++)
            {
                try
                {
                    str[i] = sevenDaysWeatherIconString[i].Split("到")[1];
                }
                catch (Exception)
                {
                    str[i] = sevenDaysWeatherIconString[i];
                }
            }
            (firstDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[0];
            Binding binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[0] == "多云" || str[0] == "晴" || str[0] == "阵雨" ? "白天" : "", str[0])] as string
            };
            BindingOperations.SetBinding(firstDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (secondDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[2];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[2] == "多云" || str[2] == "晴" || str[2] == "阵雨" ? "白天" : "", str[2])] as string
            };
            BindingOperations.SetBinding(secondDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (thirdDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[4];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[4] == "多云" || str[4] == "晴" || str[4] == "阵雨" ? "白天" : "", str[4])] as string
            };
            BindingOperations.SetBinding(thirdDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (fourthDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[6];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[6] == "多云" || str[6] == "晴" || str[6] == "阵雨" ? "白天" : "", str[6])] as string
            };
            BindingOperations.SetBinding(fourthDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (fifthDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[8];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[8] == "多云" || str[8] == "晴" || str[8] == "阵雨" ? "白天" : "", str[8])] as string
            };
            BindingOperations.SetBinding(fifthDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (sixthDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[10];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[10] == "多云" || str[10] == "晴" || str[10] == "阵雨" ? "白天" : "", str[10])] as string
            };
            BindingOperations.SetBinding(sixthDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            (seventhDayDayWeather.Children[1] as TextBlock).Text = sevenDaysWeatherIconString[12];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[12] == "多云" || str[12] == "晴" || str[12] == "阵雨" ? "白天" : "", str[12])] as string
            };
            BindingOperations.SetBinding(seventhDayDayWeather.Children[0] as PathIcon, PathIcon.DataProperty, binding);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            (firstDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[1];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[1] == "多云" || str[1] == "晴" || str[1] == "阵雨" ? "晚上" : "", str[1])] as string
            };
            BindingOperations.SetBinding(firstDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (secondDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[3];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[3] == "多云" || str[3] == "晴" || str[3] == "阵雨" ? "晚上" : "", str[3])] as string
            };
            BindingOperations.SetBinding(secondDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (thirdDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[5];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[5] == "多云" || str[5] == "晴" || str[5] == "阵雨" ? "晚上" : "", str[5])] as string
            };
            BindingOperations.SetBinding(thirdDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (fourthDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[7];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[7] == "多云" || str[7] == "晴" || str[7] == "阵雨" ? "晚上" : "", str[7])] as string
            };
            BindingOperations.SetBinding(fourthDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (fifthDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[9];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[9] == "多云" || str[9] == "晴" || str[9] == "阵雨" ? "晚上" : "", str[9])] as string
            };
            BindingOperations.SetBinding(fifthDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (sixthDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[11];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[11] == "多云" || str[11] == "晴" || str[11] == "阵雨" ? "晚上" : "", str[11])] as string
            };
            BindingOperations.SetBinding(sixthDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
            (seventhDayNightWeather.Children[0] as TextBlock).Text = sevenDaysWeatherIconString[13];
            binding = new Binding
            {
                Source = Application.Current.Resources[
                    string.Format("{0}{1}", str[13] == "多云" || str[13] == "晴" || str[13] == "阵雨" ? "晚上" : "", str[13])] as string
            };
            BindingOperations.SetBinding(seventhDayNightWeather.Children[1] as PathIcon, PathIcon.DataProperty, binding);
        }

        private void SevenDaysDisplayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //更新温度线
            SetTheSevenDaysBezierPoints();
        }

        private void SingleDayWeatherSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ob = sender as Pivot;
            UpdateWeatherDetailModel(ob.SelectedIndex);
        }
    }
}