using System.Windows.Controls;
using System.Windows.Input;
using Dimensions.Client.Singleton;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Dimensions.Client.ViewModels
{
    public class SettingPageViewModel : ViewModelBase
    {

        public SettingPageViewModel()
        {
            HeaderRowsCount = Properties.Settings.Default.HeaderRowCount.ToString();

            if (Properties.Settings.Default.LoadWithFixedHeaderType) FixedAnalysisSpec = true;
            else AutoAnalysisSpec = true;

            if (Properties.Settings.Default.AutoRemoveHeaderType) AutoRemoveHeaderRows = true;
            else CustomRemoveHeaderRows = true;

            if (Properties.Settings.Default.QueryShowValue) DmQueryShowValue = true;
            else DmQueryShowLabel = true;

            if (Properties.Settings.Default.HorizenHeader) HorizenHeader = true;
            else VerticalHeader = true;

            EditSpecKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("Spec Sheet名关键字", "SpecSheetKeys"));
            EditHeaderKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("Header Sheet名关键字", "HeaderSheetKeys"));
            EditHighFactorKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("打分题高分标签关键字", "HighFactorKeys"));
            EditNetKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("Net关键字", "NetKeys"));
            EditTopBottomKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("Top/Bottom Box关键字", "TopBottomKeys"));
            EditAverageKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditKeysDialog("平均值关键字（Mean和平均提及）", "AverageKeys"));

            EditFixedKeysCommand = new RelayCommand(() => EditSettingDialogInstance.GetInstance().ShowEditFixedKeysDialog());
        }

        private bool _fixedAnalysisSpec;
        public bool FixedAnalysisSpec
        {
            get { return _fixedAnalysisSpec; }
            set { Set(ref _fixedAnalysisSpec, value); }
        }

        private bool _autoAnalysisSpec;
        public bool AutoAnalysisSpec
        {
            get { return _autoAnalysisSpec; }
            set { Set(ref _autoAnalysisSpec, value); }
        }

        private bool _verticalHeader;
        public bool VerticalHeader
        {
            get { return _verticalHeader; }
            set { Set(ref _verticalHeader, value); }
        }

        private bool _horizenHeader;
        public bool HorizenHeader
        {
            get { return _horizenHeader; }
            set { Set(ref _horizenHeader, value); }
        }

        private bool _autoRemoveHeaderRows;
        public bool AutoRemoveHeaderRows
        {
            get { return _autoRemoveHeaderRows; }
            set { Set(ref _autoRemoveHeaderRows, value); }
        }

        private bool _customRemoveHeaderRows;
        public bool CustomRemoveHeaderRows
        {
            get { return _customRemoveHeaderRows; }
            set { Set(ref _customRemoveHeaderRows, value); }
        }

        private string _headerRowsCount;
        public string HeaderRowsCount
        {
            get { return _headerRowsCount; }
            set { Set(ref _headerRowsCount, value); }
        }

        private bool _dmqueryShowLabel;
        public bool DmQueryShowLabel
        {
            get { return _dmqueryShowLabel; }
            set { Set(ref _dmqueryShowLabel, value); }
        }

        private bool _dmqueryShowValue;
        public bool DmQueryShowValue
        {
            get { return _dmqueryShowValue; }
            set { Set(ref _dmqueryShowValue, value); }
        }

        //

        public ICommand EditSpecKeysCommand { get; }
        public ICommand EditHeaderKeysCommand { get; }
        public ICommand EditHighFactorKeysCommand { get; }
        public ICommand EditNetKeysCommand { get; }
        public ICommand EditTopBottomKeysCommand { get; }
        public ICommand EditAverageKeysCommand { get; }
        public ICommand EditFixedKeysCommand { get; }

        // 事件
        public ICommand AutoAnalysisSpecCommand => new RelayCommand<object>(OnCheckAutoAnalysisSpec);
        private void OnCheckAutoAnalysisSpec(object sender)
        {
            if (Properties.Settings.Default.LoadWithFixedHeaderType)
            {
                Properties.Settings.Default.LoadWithFixedHeaderType = false;
            }
        }

        public ICommand FixedAnalysisSpecCommand => new RelayCommand<object>(OnCheckFixedAnalysisSpec);
        private void OnCheckFixedAnalysisSpec(object sender)
        {
            if (!Properties.Settings.Default.LoadWithFixedHeaderType)
            {
                Properties.Settings.Default.LoadWithFixedHeaderType = true;
            }
        }

        public ICommand VerticalHeaderCommand => new RelayCommand<object>(OnVerticalHeaderChecked);
        private void OnVerticalHeaderChecked(object sender)
        {
            if (Properties.Settings.Default.HorizenHeader)
            {
                Properties.Settings.Default.HorizenHeader = false;
            }
        }

        public ICommand HorizenHeaderCommand => new RelayCommand<object>(OnHorizenHeaderChecked);
        private void OnHorizenHeaderChecked(object sender)
        {
            if (!Properties.Settings.Default.HorizenHeader)
            {
                Properties.Settings.Default.HorizenHeader = true;
            }
        }

        public ICommand AutoRemoveHeaderRowsCommand => new RelayCommand<object>(OnCheckAutoRemoveHeaderRows);
        private void OnCheckAutoRemoveHeaderRows(object sender)
        {
            if (!Properties.Settings.Default.AutoRemoveHeaderType)
            {
                Properties.Settings.Default.AutoRemoveHeaderType = true;
            }
        }

        public ICommand CustomRemoveHeaderRowsCommand => new RelayCommand<object>(OnCheckCustomRemoveHeaderRows);
        private void OnCheckCustomRemoveHeaderRows(object sender)
        {
            if (Properties.Settings.Default.AutoRemoveHeaderType)
            {
                Properties.Settings.Default.AutoRemoveHeaderType = false;
            }
        }

        public ICommand DmQueryShowLabelCommand => new RelayCommand<object>(OnCheckDmQueryShowLabel);
        private void OnCheckDmQueryShowLabel(object sender)
        {
            if (Properties.Settings.Default.QueryShowValue)
            {
                Properties.Settings.Default.QueryShowValue = false;
            }
        }

        public ICommand DmQueryShowValueCommand => new RelayCommand<object>(OnCheckDmQueryShowValue);
        private void OnCheckDmQueryShowValue(object sender)
        {
            if (!Properties.Settings.Default.QueryShowValue)
            {
                Properties.Settings.Default.QueryShowValue = true;
            }
        }

        public ICommand HeaderRowCountTextChangedCommand => new RelayCommand<object>(OnHeaderRowCountTextChanged);
        private void OnHeaderRowCountTextChanged(object sender)
        {
            TextBox box = sender as TextBox;
            if (!string.IsNullOrEmpty(box.Text) && int.TryParse(box.Text, out int row))
            {
                Properties.Settings.Default.HeaderRowCount = row;
            }
        }
    }
}
