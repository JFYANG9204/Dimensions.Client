using Dimensions.Bll.Generic;
using Dimensions.Bll.Mdd;
using Dimensions.Client.Singleton;
using Dimensions.Client.ViewModels;
using Dimensions.Client.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Dimensions.Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Navigation.NavigationServiceHelper navigationServiceHelper;
        private readonly MainViewModel _viewModel = new MainViewModel();

        public MainWindow()
        {
            DataContext = _viewModel;
            InitializeComponent();

            navigationServiceHelper = new Navigation.NavigationServiceHelper();
            navigationServiceHelper.Navigated += NavigationServiceHelper_OnNavigated;
            HamburgerMenuControl.Content = navigationServiceHelper.Frame;

            Loaded += (sender, e) => navigationServiceHelper.Navigate(new Uri("Views/SpecPage.xaml", UriKind.RelativeOrAbsolute));
            ShowMessageInstance.GetInstance().ShowMessageDialog = new Action<string, string>(ShowMessageBox);
            ShowMessageInstance.GetInstance().ShowFilePathDialog = new Action<string, string, string>(ShowFilePathBox);

            EditSettingDialogInstance.GetInstance().DialogCoordinator = DialogCoordinator.Instance;
            EditSettingDialogInstance.GetInstance().ShowEditKeysDialog = new Action<string, string>(ShowEditDialog);
            EditSettingDialogInstance.GetInstance().ShowEditFixedKeysDialog = new Action(ShowFixedKeysDialog);

            DMQueryInstance.GetInstance().GetFilterString = new Func<string>(GetFilterString);
            DMQueryInstance.GetInstance().RemoveFilterField = new Action(RemoveFilterFields);
        }

        private async void ShowMessageBox(string message, string title)
        {
            var settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "关闭",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            _ = await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, settings);
        }

        private async void ShowFilePathBox(string message, string title, string path)
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "打开文件夹",
                NegativeButtonText = "关闭",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, settings);
            if (result == MessageDialogResult.Affirmative)
            {
                System.Diagnostics.Process.Start(path);
            }
        }

        private async void ShowFixedKeysDialog()
        {
            EventHandler<DialogStateChangedEventArgs> dialogManagerOnDialogOpened = null;
            dialogManagerOnDialogOpened = (o, args) =>
            {
                DialogManager.DialogOpened -= dialogManagerOnDialogOpened;
            };
            DialogManager.DialogOpened += dialogManagerOnDialogOpened;

            EventHandler<DialogStateChangedEventArgs> dialogManagerOnDialogClosed = null;
            dialogManagerOnDialogClosed = (o, args) =>
            {
                DialogManager.DialogClosed -= dialogManagerOnDialogClosed;
            };
            DialogManager.DialogClosed += dialogManagerOnDialogClosed;

            var setterContent = new SettingFixedKeysDialog();
            setterContent.QuestionBox.Text = Properties.Settings.Default.FixedNameKeys;
            setterContent.LabelBox.Text = Properties.Settings.Default.FixedLabelKeys;
            setterContent.BaseBox.Text = Properties.Settings.Default.FixedBaseKeys;
            setterContent.MeanBox.Text = Properties.Settings.Default.FixedMeanKeys;
            setterContent.AverageBox.Text = Properties.Settings.Default.FixedAverageKeys;
            setterContent.TopBottomBox.Text = Properties.Settings.Default.FixedTopBottomKeys;
            setterContent.ValueBox.Text = Properties.Settings.Default.FixedValueKeys;
            setterContent.SummaryBox.Text = Properties.Settings.Default.FixedSummaryKeys;
            setterContent.RemarkBox.Text = Properties.Settings.Default.FixedRemarkKeys;

            setterContent.ConfirmButton.Click += async (o, e) =>
            {
                var dlg = (o as DependencyObject).TryFindParent<BaseMetroDialog>();
                var content = (o as DependencyObject).TryFindParent<SettingFixedKeysDialog>();
                Properties.Settings.Default.FixedNameKeys = content.QuestionBox.Text;
                Properties.Settings.Default.FixedLabelKeys = content.LabelBox.Text;
                Properties.Settings.Default.FixedBaseKeys = content.BaseBox.Text;
                Properties.Settings.Default.FixedMeanKeys = content.MeanBox.Text;
                Properties.Settings.Default.FixedAverageKeys = content.AverageBox.Text;
                Properties.Settings.Default.FixedTopBottomKeys = content.TopBottomBox.Text;
                Properties.Settings.Default.FixedValueKeys = content.ValueBox.Text;
                Properties.Settings.Default.FixedSummaryKeys = content.SummaryBox.Text;
                Properties.Settings.Default.FixedRemarkKeys = content.RemarkBox.Text;
                Properties.Settings.Default.Save();
                await this.HideMetroDialogAsync(dlg);
            };
            setterContent.CancelButton.Click += CloseEditDialog;

            var dialog = new CustomDialog { Content = setterContent };
            await this.ShowMetroDialogAsync(dialog);
        }

        private async void ShowEditDialog(string title, string key)
        {
            EventHandler<DialogStateChangedEventArgs> dialogManagerOnDialogOpened = null;
            dialogManagerOnDialogOpened = (o, args) =>
            {
                DialogManager.DialogOpened -= dialogManagerOnDialogOpened;
            };
            DialogManager.DialogOpened += dialogManagerOnDialogOpened;

            EventHandler<DialogStateChangedEventArgs> dialogManagerOnDialogClosed = null;
            dialogManagerOnDialogClosed = (o, args) =>
            {
                DialogManager.DialogClosed -= dialogManagerOnDialogClosed;
            };
            DialogManager.DialogClosed += dialogManagerOnDialogClosed;

            var setterContent = new SettingDialog();
            setterContent.Title.Content = title;
            setterContent.TextContent.Text = Properties.Settings.Default[key].ToString().Replace(",", "\n");
            setterContent.ConfirmButton.Click += async (o, e) =>
            {
                var dlg = (o as DependencyObject).TryFindParent<BaseMetroDialog>();
                var newValue = (o as DependencyObject).TryFindParent<SettingDialog>().TextContent.Text.Replace(" ", "").Replace("\n", ",").Replace("\r", "");

                if (!string.IsNullOrEmpty(newValue))
                {
                    while (newValue.EndsWith(","))
                    {
                        newValue = newValue.Substring(0, newValue.Length - 1);
                    }
                }
                Properties.Settings.Default[key] = newValue;
                Properties.Settings.Default.Save();
                await this.HideMetroDialogAsync(dlg);
            };
            setterContent.CancelButton.Click += CloseEditDialog;

            var dialog = new CustomDialog() { Content = setterContent };
            await this.ShowMetroDialogAsync(dialog);
        }

        private async void CloseEditDialog(object sender, RoutedEventArgs e)
        {
            var dialog = (sender as DependencyObject).TryFindParent<BaseMetroDialog>();
            await this.HideMetroDialogAsync(dialog);
        }

        private void OnAddFilter(object sender, RoutedEventArgs e)
        {
            IMddDocument mdd = DMQueryInstance.GetInstance().Mdd;
            if (_viewModel.Fields.Count > 0 && _viewModel.SelectedFieldsIndex < _viewModel.Fields.Count)
            {
                string varName = _viewModel.Fields[_viewModel.SelectedFieldsIndex];
                if (varName.Contains("["))
                {
                    string topName = varName.Substring(0, varName.IndexOf('['));
                    string sideName = varName.Substring(varName.IndexOf('.') + 1);
                    IMddVariable variable = mdd.Find(topName);
                    if (variable is null || 
                        variable.VariableType != Bll.VariableType.Loop || 
                        !variable.HasChildren) return;
                    IMddVariable side = null;
                    for (int i = 0; i < variable.Children.Length; i++)
                    {
                        if (variable.Children[i].Name == sideName)
                        {
                            side = variable.Children[i];
                            break;
                        }
                    }
                    if (side is null) return;
                    if (side.ValueType == Bll.ValueType.Categorical)
                    {
                        FilterItem item = new FilterItem();
                        item.VariableName.Text = varName;
                        ICodeList list = new CodeList();
                        if (side.UseList)
                        {
                            list = mdd.ListFields.Get(side.ListId).CodeList;
                        }
                        if (side.CodeList != null)
                        {
                            foreach (var code in side.CodeList)
                            {
                                list.Add(code);
                            }
                        }
                        foreach (var code in list)
                        {
                            CategoricalItem categoricalItem = new CategoricalItem();
                            categoricalItem.CodeName.Text = code.Name;
                            categoricalItem.CodeLabel.Text = code.Label;
                            item.CodeList.Children.Add(categoricalItem);
                        }
                        FilterVariablePanel.Items.Add(item);
                    }
                    else if (side.ValueType == Bll.ValueType.Long || side.ValueType == Bll.ValueType.Double)
                    {
                        FilterDigitVariableItem digitItem = new FilterDigitVariableItem();
                        digitItem.VariableName.Content = varName;
                        FilterVariablePanel.Items.Add(digitItem);
                    }
                }
                else if (mdd.Find(varName) != null && mdd.Find(varName).ValueType == Bll.ValueType.Categorical)
                {
                    IMddVariable variable = mdd.Find(varName);
                    FilterItem item = new FilterItem();
                    item.VariableName.Text = varName;
                    ICodeList list = new CodeList();
                    if (variable.UseList)
                    {
                        list = mdd.ListFields.Get(variable.ListId).CodeList;
                    }
                    if (variable.CodeList != null)
                    {
                        foreach (var code in variable.CodeList)
                        {
                            list.Add(code);
                        }
                    }
                    foreach (var code in list)
                    {
                        CategoricalItem categoricalItem = new CategoricalItem();
                        categoricalItem.CodeName.Text = code.Name;
                        categoricalItem.CodeLabel.Text = code.Label;
                        item.CodeList.Children.Add(categoricalItem);
                    }
                    FilterVariablePanel.Items.Add(item);
                }
                else if (mdd.Find(varName) != null && (mdd.Find(varName).ValueType == Bll.ValueType.Long || mdd.Find(varName).ValueType == Bll.ValueType.Double))
                {
                    FilterDigitVariableItem digitItem = new FilterDigitVariableItem();
                    digitItem.VariableName.Content = varName;
                    FilterVariablePanel.Items.Add(digitItem);
                }
            }
            DMQueryInstance.GetInstance().SetQuery();
        }

        private void RemoveFilterFields()
        {
            if (FilterVariablePanel.SelectedIndex < 0 || FilterVariablePanel.SelectedIndex >= FilterVariablePanel.Items.Count) return;
            FilterVariablePanel.Items.RemoveAt(FilterVariablePanel.SelectedIndex);
        }

        private string GetFilterString()
        {
            string filter = string.Empty;
            if (FilterVariablePanel.Items.Count == 0) return filter;
            int count = 0;
            foreach (var item in FilterVariablePanel.Items)
            {
                if (item is FilterDigitVariableItem digitItem)
                {
                    if (string.IsNullOrEmpty(digitItem.DigitValue.Text) ||
                        !double.TryParse(digitItem.DigitValue.Text, out _))
                        continue;
                    filter += digitItem.VariableName.Content;
                    if (digitItem.InnerLogic.SelectedItem == digitItem.Greater)
                        filter += " > ";
                    else if (digitItem.InnerLogic.SelectedItem == digitItem.GreaterOrEqual)
                        filter += " >= ";
                    else if (digitItem.InnerLogic.SelectedItem == digitItem.Smaller)
                        filter += " < ";
                    else if (digitItem.InnerLogic.SelectedItem == digitItem.SmallerOrEqual)
                        filter += " <= ";
                    else if (digitItem.InnerLogic.SelectedItem == digitItem.Equal)
                        filter += " = ";
                    if (!string.IsNullOrEmpty(digitItem.DigitValue.Text) &&
                        double.TryParse(digitItem.DigitValue.Text, out double value))
                        filter += value;
                    if ((++count) < FilterVariablePanel.Items.Count)
                    {
                        if (digitItem.OutterLogic.SelectedItem == digitItem.OutterAnd)
                            filter += " And ";
                        if (digitItem.OutterLogic.SelectedItem == digitItem.OutterOr)
                            filter += " Or ";
                    }
                }
                else if (item is FilterItem normalItem)
                {
                    string selectedCode = string.Empty;
                    foreach (var codeItem in normalItem.CodeList.Children)
                    {
                        if (codeItem is CategoricalItem code)
                        {
                            if (code.Checked.IsChecked is null || (bool)!code.Checked.IsChecked) continue;
                            selectedCode += code.CodeName.Text + ",";
                        }
                    }
                    if (!string.IsNullOrEmpty(selectedCode))
                    {
                        selectedCode = selectedCode.Substring(0, selectedCode.Length - 1);
                        if (normalItem.Logic.SelectedItem == normalItem.Selected)
                            filter += $"{normalItem.VariableName.Text}.ContainsAny({{{selectedCode}}})";
                        else if (normalItem.Logic.SelectedItem == normalItem.UnSelected)
                            filter += $"Not {normalItem.VariableName.Text}.ContainsAny({{{selectedCode}}})";
                        else if (normalItem.Logic.SelectedItem == normalItem.OnlySelected)
                            filter += $"{normalItem.VariableName.Text}.ContainsAny({{{selectedCode}}}, true)";
                        else if (normalItem.Logic.SelectedItem == normalItem.AllSelected)
                            filter += $"{normalItem.VariableName.Text}.ContainsAll({{{selectedCode}}})";
                        else if (normalItem.Logic.SelectedItem == normalItem.OnlyAllSelected)
                            filter += $"{normalItem.VariableName.Text}.ContainsAll({{{selectedCode}}}, true)";
                        if ((++count) < FilterVariablePanel.Items.Count)
                        {
                            if (normalItem.OutterLogic.SelectedItem == normalItem.OutterAnd)
                                filter += " And ";
                            if (normalItem.OutterLogic.SelectedItem == normalItem.OutterOr)
                                filter += " Or ";
                        }
                    }
                }
            }
            return filter;
        }


        private void OnFilterVariablePanelKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                if (FilterVariablePanel.SelectedIndex < 0 || FilterVariablePanel.SelectedIndex >= FilterVariablePanel.Items.Count) return;
                FilterVariablePanel.Items.RemoveAt(FilterVariablePanel.SelectedIndex);
                DMQueryInstance.GetInstance().SetQuery();
            }
        }

        //
        // Hamburger Menu
        //

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            if (e.InvokedItem is HamMenuItem item && item.IsNavigation)
            {
                navigationServiceHelper.Navigate(item.NavigationDestination);
                DMQueryInstance.GetInstance()?.Clear();
            }
        }

        private void NavigationServiceHelper_OnNavigated(object sender, NavigationEventArgs e)
        {
            HamburgerMenuControl.SelectedItem = HamburgerMenuControl.Items.OfType<HamMenuItem>().FirstOrDefault(
                x => x.NavigationDestination == e.Uri);
            HamburgerMenuControl.SelectedOptionsItem = HamburgerMenuControl.OptionsItems.OfType<HamMenuItem>().FirstOrDefault(
                x => x.NavigationDestination == e.Uri);
        }
    }
}
