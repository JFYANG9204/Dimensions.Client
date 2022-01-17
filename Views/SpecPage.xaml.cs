using System.Windows.Controls;
using System.Windows.Data;
using Dimensions.Client.ViewModels;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// SpecPage.xaml 的交互逻辑
    /// </summary>
    public partial class SpecPage : Page
    {
        public SpecPage()
        {
            InitializeComponent();
            DataContext = new SpecContentViewModel();
        }
        private void AutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string columnName = e.PropertyName;
            if (e.Column is DataGridColumn &&
                (columnName.Contains(".") ||
                 columnName.Contains("\\") ||
                 columnName.Contains("/") ||
                 columnName.Contains("[") ||
                 columnName.Contains("]") ||
                 columnName.Contains("(") ||
                 columnName.Contains(")")))
            {
                DataGridBoundColumn dataGridBoundColumn = e.Column as DataGridBoundColumn;
                dataGridBoundColumn.Binding = new Binding("[" + e.PropertyName + "]");
            }
        }

    }
}
