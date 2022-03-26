using System;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ClockApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public static string appVersion = string.Format("{0}.{1}.{2}.{3}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build,
            Package.Current.Id.Version.Revision);
        public SettingPage()
        {
            this.InitializeComponent();
            keepDisplayToggle.IsOn = Common.isKeepDisplayActived;
            dataFileFolderLinkContent.Text = ApplicationData.Current.LocalFolder.Path;
            sortComboBox.SelectedIndex = (int)Common.nowWhoFirst;
        }

        private void KeepDisplayToggle_Toggled(object sender, RoutedEventArgs e)
        {
            Common.isKeepDisplayActived = keepDisplayToggle.IsOn;
            if (Common.isKeepDisplayActived)
            {
                Common.keepDisplay.RequestActive();
            }
            else
            {
                Common.keepDisplay.RequestRelease();
            }
        }

        private async void DataFileFolderLink_Click(object sender, RoutedEventArgs e)
        {
            _ = await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Common.nowWhoFirst = (Common.WhoFirstEnum)sortComboBox.SelectedIndex;
        }
    }
}
