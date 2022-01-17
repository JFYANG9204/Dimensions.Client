using System.Windows.Controls;
using System.Windows.Data;
using Dimensions.Client.ViewModels;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// DMQueryPage.xaml 的交互逻辑
    /// </summary>
    public partial class DMQueryPage : Page
    {
        public DMQueryPage()
        {
            InitializeComponent();
            DataContext = new DMQueryViewModel();
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
