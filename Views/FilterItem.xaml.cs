using System.Windows.Controls;
using Dimensions.Client.Singleton;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// FilterItem.xaml 的交互逻辑
    /// </summary>
    public partial class FilterItem : UserControl
    {
        public FilterItem()
        {
            InitializeComponent();
        }

        private void Logic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DMQueryInstance.GetInstance().SetQuery?.Invoke();
        }
    }
}
