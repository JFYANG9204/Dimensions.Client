using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using GalaSoft.MvvmLight.Command;
using Dimensions.Client.Singleton;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// MddReaderPage.xaml 的交互逻辑
    /// </summary>
    public partial class TextEditorPage : Page
    {
        public TextEditorPage()
        {
            InitializeComponent();
            ContentFontSize.SelectedIndex = 0;
            MainWindowInstance.GetInstance().OnTextEditorPageClosing = PageClear;
        }

        private readonly List<string> _filePaths = new List<string>();
        private int _currentIndex = -1;

        private void OnOpenFileButtonClicked(object sender, RoutedEventArgs e)
        {
            FileDialog dlg = new OpenFileDialog()
            {
                Multiselect = false
            };
            if (dlg.ShowDialog() ?? false)
            {
                string path = dlg.FileName;
                if (_filePaths.Contains(path)) return;
                _filePaths.Add(path);
                TextEditorView item = new TextEditorView();
                item.Load(path);
                MetroTabItem tabItem = new MetroTabItem
                {
                    Content = item,
                    Header = Path.GetFileName(path),
                    CloseButtonEnabled = true,
                    CloseTabCommand = CloseTabItemCommand
                };
                Tabs.Items.Add(tabItem);
                _currentIndex = _filePaths.Count - 1;
                Tabs.SelectedIndex = _currentIndex;
                Tabs.Items.Refresh();
            }
        }

        private void OnFontSizeSelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            if (_currentIndex > -1)
            {
                TextEditorView editor = ((MetroTabItem)Tabs.Items[_currentIndex]).Content as TextEditorView;
                editor.SetFontSize(box.SelectedIndex + 12);
            }
        }

        private void OnTabControlSelectionChanged(object sender, RoutedEventArgs e)
        {
            TabControl tab = sender as TabControl;
            _currentIndex = tab.SelectedIndex;
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < 0 || Tabs.Items.Count == 0) return;
            TextEditorView editor = (TextEditorView)((MetroTabItem)Tabs.Items[_currentIndex]).Content;
            editor.Save();
        }

        private void OnSaveAsButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < 0 || Tabs.Items.Count == 0) return;
            FileDialog dlg = new SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "所有文件|*.*"
            };
            if (dlg.ShowDialog() ?? false)
            {
                TextEditorView editor = (TextEditorView)((MetroTabItem)Tabs.Items[_currentIndex]).Content;
                editor.Save(dlg.FileName);
            }
        }

        //private void OnTabItemClosed(object sender, RoutedEventArgs e)
        //{
        //    DependencyObject dependency = (DependencyObject)e.OriginalSource;
        //    while (dependency != null && !(dependency is TextEditorView))
        //        dependency = VisualTreeHelper.GetParent(dependency);
        //    if (dependency is TextEditorView)
        //    {
        //        TextEditorView editor = dependency as TextEditorView;
        //        editor.DeleteTempFile();
        //        editor.Close();
        //    }
        //    _filePaths.RemoveAt(_currentIndex);
        //    Tabs.Items.RemoveAt(_currentIndex);
        //    Tabs.Items.Refresh();
        //    if (_currentIndex > 0)
        //    {
        //        _currentIndex--;
        //        Tabs.SelectedIndex = _currentIndex;
        //    }
        //    else
        //    {
        //        _currentIndex = -1;
        //    }
        //}


        public ICommand CloseTabItemCommand => new RelayCommand(CloseTabItem);
        private void CloseTabItem()
        {
            TextEditorView editor = (TextEditorView)((MetroTabItem)Tabs.Items[_currentIndex]).Content;
            editor.DeleteTempFile();
            editor.Close();
            _filePaths.RemoveAt(_currentIndex);
            if (_currentIndex > 0)
            {
                _currentIndex--;
                Tabs.SelectedIndex = _currentIndex;
            }
        }

        private void PageClear()
        {
            if (Tabs.Items.Count > 0)
            {
                for (int i = 0; i < Tabs.Items.Count; i++)
                {
                    TextEditorView editor = (TextEditorView)((MetroTabItem)Tabs.Items[i]).Content;
                    editor.DeleteTempFile();
                    editor.Close();
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            PageClear();
        }
    }
}
