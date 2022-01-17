using Dimensions.Client.ViewModels;
using System.Windows.Controls;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
            DataContext = new SettingPageViewModel();
        }

        private void Grid_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

    }
}
