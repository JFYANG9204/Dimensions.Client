using System.Windows;
using System.Windows.Controls;
using Dimensions.Client.Singleton;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// CategoricalItem.xaml 的交互逻辑
    /// </summary>
    public partial class CategoricalItem : UserControl
    {
        public CategoricalItem()
        {
            InitializeComponent();
        }

        private void Checked_Click(object sender, RoutedEventArgs e)
        {
            DMQueryInstance.GetInstance().SetQuery?.Invoke();
        }
    }
}
