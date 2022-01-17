using Dimensions.Bll.Spec;
using Dimensions.Bll.Mdd;
using Dimensions.Bll.File;
using Dimensions.Client.Properties;
using Dimensions.Client.Singleton;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Media;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace Dimensions.Client.ViewModels
{
    class SpecContentViewModel : ViewModelBase
    {

        public SpecContentViewModel()
        {
            _spec = new SpecDocument(
                _setting.AddSubGroup,
                _setting.AutoRemoveHeaderType ? Bll.SpecRemoveHeaderRowType.Auto : Bll.SpecRemoveHeaderRowType.Custom,
                _setting.LoadWithFixedHeaderType ? Bll.SpecAnalysisType.Fixed : Bll.SpecAnalysisType.Normal,
                _setting.HorizenHeader ? Bll.HeaderAnalysisType.Horizen : Bll.HeaderAnalysisType.Normal,
                _setting.HeaderRowCount);
            _spec.SetKeys(SpecKeys.SpecSheetKeys, _setting.SpecSheetKeys.Split(','));
            _spec.SetKeys(SpecKeys.SpecHeaderKeys, _setting.HeaderSheetKeys.Split(','));
            _spec.SetKeys(SpecKeys.TopBottomKeys, _setting.TopBottomKeys.Split(','));
            _spec.SetKeys(SpecKeys.NetKeys, _setting.NetKeys.Split(','));
            _spec.SetKeys(SpecKeys.HighFactorKeys, _setting.HighFactorKeys.Split(','));
            _spec.SetKeys(SpecKeys.AverageKeys, _setting.AverageKeys.Split(','));
            //
            _spec.SetKeys(SpecKeys.FixedLabelKeys, _setting.FixedLabelKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedNameKeys, _setting.FixedNameKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedBaseKeys, _setting.FixedBaseKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedMeanKeys, _setting.FixedMeanKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedAverageKeys, _setting.FixedAverageKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedTopBottomKeys, _setting.FixedTopBottomKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedValueKeys, _setting.FixedValueKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedSummaryKeys, _setting.FixedSummaryKeys.Split(','));
            _spec.SetKeys(SpecKeys.FixedRemarkKeys, _setting.FixedRemarkKeys.Split(','));
            //
            _spec.OnReportLoadProgress += Spec_OnReportLoadProgress;
            // Properties
            AfterLoadVisibility = Visibility.Hidden;
            LoadVisibility = Visibility.Hidden;
            MddStatus = "MDD未选择";
            SpecStatus = "SPEC未选择";
            MddStatusColor = new SolidColorBrush(Colors.Red);
            SpecStatusColor = new SolidColorBrush(Colors.Red);
            // Event
            _specBkWorker.DoWork += SpecBkWorker_DoWork;
            _specBkWorker.RunWorkerCompleted += SpecBkWorker_RunWorkerCompleted;

            _runBkWorker.DoWork += RunBkWorker_DoWorker;
            _runBkWorker.RunWorkerCompleted += RunBkWorker_RunWorkerCompleted;
        }

        private readonly Settings _setting = Settings.Default;

        private readonly SpecDocument _spec;
        private MddDocument _mdd;
        private FileWriter _file;

        private bool _mddLoaded = false;
        private bool _specLoaded = false;

        // 当前sheet序号
        private int _sheetIndex = 0;

        // 备份数据
        private DataSet _bkData;

        // 鼠标点击时所在的列名
        private string _columnName;

        //
        // BackgroundWorker
        //

        private void Spec_OnReportLoadProgress(string section, double progress)
        {
            LoadPercentage = progress.ToString() + "%";
            LoadMessage = section;
        }

        private readonly BackgroundWorker _specBkWorker = new BackgroundWorker();
        private readonly BackgroundWorker _runBkWorker = new BackgroundWorker();

        private void SpecBkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    LoadVisibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(_mddPath) && File.Exists(_mddPath))
                    {
                        LoadMessage = "载入Mdd数据";
                        _mdd = new MddDocument(_mddPath);
                        _mddLoaded = true;
                        MddLngItems = _mdd.Languages;
                        MddLngIndex = 0;
                    }
                    if (!string.IsNullOrEmpty(_specPath) && File.Exists(_specPath) && _specPath != _spec.Path)
                    {
                        AfterLoadVisibility = Visibility.Hidden;
                        _spec.Clear();
                        _spec.Load(_specPath);
                        _specLoaded = true;
                    }
                    if (_spec.SpecCurrentData != null)
                    {
                        _bkData = _spec.SpecCurrentData.Copy();
                        if (_spec.SpecCurrentData.Tables.Count > 0)
                            CurrentSheetName = _spec.SpecCurrentData.Tables[0].TableName;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.WirteLine($"Source:{ex.Source} Message:{ex.Message} Trace:{ex.StackTrace}");
                    MessageBox.Show(ex.Message);
                    return;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void SpecBkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadVisibility = Visibility.Hidden;
            AfterLoadVisibility = Visibility.Visible;
            Action setMenu = new Action(SetMenuItems);
            if (_spec != null && _spec.SpecCurrentData != null && _spec.SpecCurrentData.Tables.Count >= 1)
            {
                CurrentView = _spec.SpecCurrentData.Tables[0].DefaultView;
                setMenu();
                _sheetIndex = 0;
            }
        }

        private void RunBkWorker_DoWorker(object sender, DoWorkEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    LoadMessage = string.Empty;
                    LoadPercentage = string.Empty;
                    LoadVisibility = Visibility.Visible;
                    bool subTt = false;
                    if (SubTtIndex == 1) subTt = true;
                    _spec.LoadSpecItem(_mdd);
                    _spec.LoadTopItem(_mdd, subTt);
                    _file = new FileWriter(Path.GetDirectoryName(_specPath), subTt);
                    _file.WriteAll(_mdd);
                    _file.WriteAll(_spec.SpecItems, _mdd);
                    _file.WriteAllList(_mdd);
                    _file.WriteAll(_spec.Top);
                }
                catch (Exception ex)
                {
                    ErrorLogger.WirteLine($"Source:{ex.Source} Message:{ex.Message} Trace:{ex.StackTrace}");
                    MessageBox.Show(ex.Message);
                    return;
                }
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void RunBkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadVisibility = Visibility.Hidden;
            ShowMessageInstance.GetInstance().ShowFilePathDialog(
                "文件已生成，路径: " + Path.Combine(Path.GetDirectoryName(_specPath), "Files"), 
                "注意",
                Path.Combine(Path.GetDirectoryName(_specPath), "Files"));
        }

        // 设定右键菜单内容方法
        private void SetMenuItems()
        {
            if (string.IsNullOrEmpty(CurrentSheetName)) return;
            if (!_spec.SpecRawData.Tables.Contains(CurrentSheetName)) return;
            DataTable rawTable = _spec.SpecRawData.Tables[CurrentSheetName];
            MenuItem[] menuItems = new MenuItem[rawTable.Columns.Count + 3];
            for (int i = 0; i < rawTable.Columns.Count; i++)
            {
                string colName = rawTable.Columns[i].ColumnName;
                menuItems[i] = new MenuItem { Header = colName };
                menuItems[i].Click += new RoutedEventHandler((s, ea) =>
                {
                    if (!string.IsNullOrEmpty(colName) &&
                    rawTable.Columns.Contains(colName) && _spec.SpecCurrentData.Tables[CurrentSheetName].Columns.Contains(_columnName))
                        _spec.SetCurrentColumnData(CurrentSheetName, colName, _columnName);
                    CurrentView = _spec.SpecCurrentData.Tables[CurrentSheetName].DefaultView;
                });
            }
            menuItems[menuItems.Length - 3] = new MenuItem { Header = "清空" };
            menuItems[menuItems.Length - 3].Click += new RoutedEventHandler((s, ea) =>
            {
                for (int i = 0; i < _spec.SpecCurrentData.Tables[CurrentSheetName].Rows.Count; i++)
                    _spec.SpecCurrentData.Tables[CurrentSheetName].Rows[i][_columnName] = string.Empty;
                CurrentView = _spec.SpecCurrentData.Tables[CurrentSheetName].DefaultView;
            });
            menuItems[menuItems.Length - 2] = new MenuItem { Header = "还原列" };
            menuItems[menuItems.Length - 2].Click += new RoutedEventHandler((s, ea) =>
            {
                if (_bkData is null || !_bkData.Tables.Contains(CurrentSheetName)) return;
                if (!_bkData.Tables[CurrentSheetName].Columns.Contains(_columnName)) return;
                DataTable bkTable = _bkData.Tables[CurrentSheetName];
                DataTable ctTable = _spec.SpecCurrentData.Tables[CurrentSheetName];
                for (int i = 0; i < ctTable.Rows.Count; i++)
                {
                    if (i < bkTable.Rows.Count)
                        ctTable.Rows[i][_columnName] = bkTable.Rows[i][_columnName].ToString();
                    else
                        ctTable.Rows[i][_columnName] = string.Empty;
                }
            });
            menuItems[menuItems.Length - 1] = new MenuItem { Header = "还原表" };
            menuItems[menuItems.Length - 1].Click += new RoutedEventHandler((s, ea) =>
            {
                if (_bkData is null || !_bkData.Tables.Contains(CurrentSheetName)) return;
                _spec.SpecCurrentData.Tables[CurrentSheetName].Clear();
                _spec.SpecCurrentData.Tables[CurrentSheetName].Merge(_bkData.Tables[CurrentSheetName].Copy());
                CurrentView = _spec.SpecCurrentData.Tables[CurrentSheetName].DefaultView;
            });
            MenuItems = menuItems;
        }

        //
        // Properties
        //

        // 
        private int _subTtIndex;
        public int SubTtIndex
        {
            get { return _subTtIndex; }
            set { Set(ref _subTtIndex, value); }
        }

        // Mdd 语言选择序号
        private int _mddLngIndex;
        public int MddLngIndex
        {
            get { return _mddLngIndex; }
            set { Set(ref _mddLngIndex, value); }
        }

        // 当前Sheet名称
        private string _currentSheetName;
        public string CurrentSheetName
        {
            get { return _currentSheetName; }
            set { Set(ref _currentSheetName, value); }
        }

        // Spec 路径
        private string _specPath;
        public string SpecPath
        {
            get { return _specPath; }
            set { Set(ref _specPath, value); }
        }

        // Mdd 路径
        private string _mddPath;
        public string MddPath
        {
            get { return _mddPath; }
            set { Set(ref _mddPath, value); }
        }

        // Spec选择状态--已选择或未选择
        private string _specStatus;
        public string SpecStatus
        {
            get { return _specStatus; }
            set { Set(ref _specStatus, value); }
        }

        // Mdd选择状态--已选择或未选择
        private string _mddStatus;
        public string MddStatus
        {
            get { return _mddStatus; }
            set { Set(ref _mddStatus, value); }
        }

        // Spec选择状态文字颜色
        private Brush _specStatusColor;
        public Brush SpecStatusColor
        {
            get { return _specStatusColor; }
            set { Set(ref _specStatusColor, value); }
        }

        // Mdd选择状态文字颜色
        private Brush _mddStatusColor;
        public Brush MddStatusColor
        {
            get { return _mddStatusColor; }
            set { Set(ref _mddStatusColor, value); }
        }

        // 控制读取后才显示的一些控件的显示状态
        private Visibility _loadVisibility;
        public Visibility LoadVisibility
        {
            get { return _loadVisibility; }
            set { Set(ref _loadVisibility, value); }
        }

        // 当前DataGrid内容
        private DataView _currentView;
        public DataView CurrentView
        {
            get { return _currentView; }
            set 
            {
                value.ListChanged += (o, e) =>
                {
                    if (e.ListChangedType == ListChangedType.ItemDeleted)
                    {
                        var index = e.NewIndex;
                        _spec.SpecCurrentData.Tables[CurrentSheetName].Rows.RemoveAt(index);
                    }
                };
                Set(ref _currentView, value); 
            }
        }

        // Mdd语言列表内容
        private string[] _mddLngItems;
        public string[] MddLngItems
        {
            get { return _mddLngItems; }
            set { Set(ref _mddLngItems, value); }
        }

        // DataGrid 右键菜单内容
        private MenuItem[] _menuItems;
        public MenuItem[] MenuItems
        {
            get { return _menuItems; }
            set { Set(ref _menuItems, value); }
        }

        // 读取进度百分比
        private string _loadPercentage;
        public string LoadPercentage
        {
            get { return _loadPercentage; }
            set { Set(ref _loadPercentage, value); }
        }

        // 读取进度信息
        private string _loadMessage;
        public string LoadMessage
        {
            get { return _loadMessage; }
            set { Set(ref _loadMessage, value); }
        }

        private Visibility _afterLoadVisibility;
        public Visibility AfterLoadVisibility
        {
            get { return _afterLoadVisibility; }
            set { Set(ref _afterLoadVisibility, value); }
        }

        //
        // Commands
        //

        public ICommand OpenMddCommand => new RelayCommand(OpenMdd);
        private void OpenMdd()
        {
            FileDialog dlg = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Mdd File|*.mdd"
            };
            if (dlg.ShowDialog() == true)
            {
                MddPath = dlg.FileName;
                MddStatus = "MDD已选择";
                MddStatusColor = new SolidColorBrush(Colors.Green);
            }
        }

        public ICommand OpenSpecCommand => new RelayCommand(OpenSpec);
        private void OpenSpec()
        {
            FileDialog dlg = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Excel File|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                SpecPath = dlg.FileName;
                SpecStatus = "Spec已选择";
                SpecStatusColor = new SolidColorBrush(Colors.Green);
            }
        }
        // 载入数据
        public ICommand LoadCommand => new RelayCommand(Load);
        private void Load()
        {
            if (string.IsNullOrEmpty(_specPath) || !File.Exists(_specPath))
            {
                ShowMessageInstance.GetInstance()?.ShowMessageDialog("未选择SPEC文件或SPEC文件不存在", "注意");
                return;
            }
            if (string.IsNullOrEmpty(_mddPath) || !File.Exists(_mddPath))
            {
                ShowMessageInstance.GetInstance()?.ShowMessageDialog("未选择MDD文件或MDD文件不存在", "注意");
                return;
            }
            if (_specBkWorker.IsBusy) return;
            _specBkWorker.RunWorkerAsync();
        }

        // 运行程序命令
        public ICommand RunCommand => new RelayCommand(Run);
        private void Run()
        {
            if (!_mddLoaded || !_specLoaded || _runBkWorker.IsBusy) return;
            _runBkWorker.RunWorkerAsync();
        }

        // 上一个sheet
        public ICommand LastSheetCommand => new RelayCommand(LastSheet);
        private void LastSheet()
        {
            if (_sheetIndex - 1 < 0) return;
            CurrentView = _spec.SpecCurrentData.Tables[--_sheetIndex].DefaultView;
            CurrentSheetName = _spec.SpecCurrentData.Tables[_sheetIndex].TableName;
            SetMenuItems();
        }

        // 下一个sheet
        public ICommand NextSheetCommand => new RelayCommand(NextSheet);
        private void NextSheet()
        {
            if (_sheetIndex + 1 >= _spec.SpecCurrentData.Tables.Count) return;
            CurrentView = _spec.SpecCurrentData.Tables[++_sheetIndex].DefaultView;
            CurrentSheetName = _spec.SpecCurrentData.Tables[_sheetIndex].TableName;
            SetMenuItems();
        }

        //
        // 事件
        //
        // 鼠标右击事件
        private bool _hasRightMouseHandler = false;
        public ICommand DataGridRightMouseDownCommand => new RelayCommand<object>(OnDataGridRightMouseDown);
        private void OnDataGridRightMouseDown(object sender)
        {
            if (_hasRightMouseHandler) return;
            DataGrid dg = sender as DataGrid;
            dg.MouseRightButtonDown += new MouseButtonEventHandler((object s, MouseButtonEventArgs ea) =>
            {
                DependencyObject dependency = (DependencyObject)ea.OriginalSource;
                while (dependency != null && !(dependency is DataGridCell) && !(dependency is DataGridColumnHeader))
                    dependency = VisualTreeHelper.GetParent(dependency);
                if (dependency is null) return;
                if (dependency is DataGridColumnHeader)
                {
                    DataGridColumnHeader columnHeader = dependency as DataGridColumnHeader;
                    _columnName = columnHeader.Content.ToString();
                }
                if (dependency is DataGridCell)
                {
                    DataGridCell cell = dependency as DataGridCell;
                    _columnName = cell.Column.Header.ToString();
                }
            });
            _hasRightMouseHandler = true;
        }

        // 数据表更新事件
        public ICommand DataGridCellEditEndingCommand => new RelayCommand<DataGridCellEditEndingEventArgs>(OnDataGridCellEditEnding);
        private void OnDataGridCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;
            int row = e.Row.GetIndex();
            int col = e.Column.DisplayIndex;
            _spec.SetCurrentData(CurrentSheetName, row, col, newValue);
        }



    }
}
