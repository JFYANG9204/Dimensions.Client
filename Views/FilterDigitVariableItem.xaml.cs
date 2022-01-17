using System.Windows.Controls;
using Dimensions.Client.Singleton;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// FilterDigitVariableItem.xaml 的交互逻辑
    /// </summary>
    public partial class FilterDigitVariableItem : UserControl
    {
        public FilterDigitVariableItem()
        {
            InitializeComponent();
        }

        private void InnerLogic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DMQueryInstance.GetInstance().SetQuery?.Invoke();
        }

        private void DigitValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            DMQueryInstance.GetInstance().SetQuery?.Invoke();
        }
    }
}
