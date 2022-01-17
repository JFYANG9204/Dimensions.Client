using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dimensions.Bll;
using Dimensions.Bll.Mdd;
using Dimensions.Client.Singleton;
using Dimensions.Bll.Generic;
using SPSSMR.Data.Transformations;
using Excel = Microsoft.Office.Interop.Excel;

namespace Dimensions.Client.ViewModels
{
    public class DMQueryViewModel : ViewModelBase
    {
        public DMQueryViewModel()
        {
            DMQueryInstance.GetInstance().LoadData = new Action(LoadConnection);

            _loadMddWorker.DoWork += (o, e) =>
            {
                try
                {
                    Action load = new Action(() => LoadFields());
                    if (string.IsNullOrEmpty(_mddPath)) return;
                    _mdd = new MddDocument(_mddPath);
                    load.Invoke();
                    DMQueryInstance.GetInstance().Mdd = _mdd;
                    DMQueryInstance.GetInstance().MddLoaded = true;
                }
                catch (Exception ex)
                {
                    LoadProgressVisibility = Visibility.Hidden;
                    MessageBox.Show(ex.Message);
                }
            };
            _exportWorker.DoWork += OnExportFile;
            _exportWorker.RunWorkerCompleted += (o, e) =>
            {
                if (!_outputed) return;
                string path = _savePath;
                if (!string.IsNullOrEmpty(path)) path = Path.GetDirectoryName(path);
                ShowMessageInstance.GetInstance().ShowFilePathDialog(
                    "文件已生成，路径: " + path,
                    "注意",
                    path);
                LoadProgressVisibility = Visibility.Hidden;
                _outputed = false;
            };

            // Properties
            MddStatus = "MDD未选择";
            DdfStatus = "DDF未选择";
            MddStatusColor = new SolidColorBrush(Colors.Red);
            DdfStatusColor = new SolidColorBrush(Colors.Red);
            
            LoadProgressVisibility = Visibility.Hidden;
            ExportProgressVisibility = Visibility.Hidden;
            ExportPercentage = string.Empty;
            AfterLoadVisibility = Visibility.Hidden;

            _readType = "1";
            if (Properties.Settings.Default.QueryShowValue)
            {
                _readType = "0";
            }
        }

        private readonly BackgroundWorker _loadMddWorker = new BackgroundWorker();
        private readonly BackgroundWorker _exportWorker = new BackgroundWorker();

        private string _mddPath;
        private string _ddfPath;
        private string _readType;
        private string _savePath;

        private bool _outputed = false;

        private IMddDocument _mdd;

        private readonly ADODataSet _aDODataSet = new ADODataSet();

        private DataTable _resultTable;

        // Properties
        // Ddf选择状态--已选择或未选择
        private string _ddfStatus;
        public string DdfStatus
        {
            get { return _ddfStatus; }
            set { Set(ref _ddfStatus, value); }
        }

        // Mdd选择状态--已选择或未选择
        private string _mddStatus;
        public string MddStatus
        {
            get { return _mddStatus; }
            set { Set(ref _mddStatus, value); }
        }

        // Ddf选择状态文字颜色
        private Brush _ddfStatusColor;
        public Brush DdfStatusColor
        {
            get { return _ddfStatusColor; }
            set { Set(ref _ddfStatusColor, value); }
        }

        // Mdd选择状态文字颜色
        private Brush _mddStatusColor;
        public Brush MddStatusColor
        {
            get { return _mddStatusColor; }
            set { Set(ref _mddStatusColor, value); }
        }

        // Mdd Fields
        private ObservableCollection<string> _fields;
        public ObservableCollection<string> Fields
        {
            get { return _fields; }
            set { Set(ref _fields, value); }
        }

        // 结果
        private DataView _resultView;
        public DataView ResultView
        {
            get { return _resultView; }
            set { Set(ref _resultView, value); }
        }


        private Visibility _loadProgressVisibility;
        public Visibility LoadProgressVisibility
        {
            get { return _loadProgressVisibility; }
            set { Set(ref _loadProgressVisibility, value); }
        }

        private Visibility _exportProgressVisibility;
        public Visibility ExportProgressVisibility
        {
            get { return _exportProgressVisibility; }
            set { Set(ref _exportProgressVisibility, value); }
        }


        private string _exportPercentage;
        public string ExportPercentage
        {
            get { return _exportPercentage; }
            set { Set(ref _exportPercentage, value); }
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
                _mddPath = dlg.FileName;
                MddStatus = "MDD已选择";
                _loadMddWorker.RunWorkerAsync();
                MddStatusColor = new SolidColorBrush(Colors.Green);
                DMQueryInstance.GetInstance().Clear();
            }
        }

        public ICommand OpenDdfCommand => new RelayCommand(OpenDdf);
        private void OpenDdf()
        {
            FileDialog dlg = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Ddf File|*.ddf"
            };
            if (dlg.ShowDialog() == true)
            {
                _ddfPath = dlg.FileName;
                DdfStatus = "DDF已选择";
                DdfStatusColor = new SolidColorBrush(Colors.Green);
            }
        }

        private void LoadFields()
        {
            LoadProgressVisibility = Visibility.Visible;
            _mdd = new MddDocument(_mddPath);
            DMQueryInstance.GetInstance().Mdd = _mdd;
            ObservableCollection<string> fullFields = new ObservableCollection<string>();
            foreach (var field in _mdd.Fields)
            {
                switch (field.VariableType)
                {
                    case VariableType.Normal:
                        fullFields.Add(field.Name);
                        break;
                    case VariableType.Loop:
                        if (!field.HasChildren) continue;
                        foreach (var child in field.Children)
                        {
                            if (field.UseList)
                            {
                                ICodeList list = _mdd.ListFields.Get(field.ListId).CodeList;
                                foreach (var code in list)
                                {
                                    fullFields.Add($"{field.Name}[{{{code.Name}}}].{child.Name}");
                                }
                                if (field.CodeList != null && field.CodeList.Count > 0)
                                {
                                    foreach (var code in field.CodeList)
                                    {
                                        fullFields.Add($"{field.Name}[{{{code.Name}}}].{child.Name}");
                                    }
                                }
                            }
                            else
                            {
                                ICodeList list = field.CodeList;
                                if (list is null || list.Count == 0) continue;
                                foreach (var code in list)
                                {
                                    fullFields.Add($"{field.Name}[{{{code.Name}}}].{child.Name}");
                                }
                            }
                        }
                        break;
                    case VariableType.Block:
                        break;
                    case VariableType.System:
                        if (!field.HasChildren) continue;
                        foreach (var child in field.Children)
                        {
                            fullFields.Add($"{field.Name}.{child.Name}");
                        }
                        break;
                    default:
                        break;
                }
            }
            LoadProgressVisibility = Visibility.Hidden;
            DMQueryInstance.GetInstance().SetProperty(fullFields);
            DMQueryInstance.GetInstance().Fields = fullFields;
        }

        public ICommand ShowSettingCommand => new RelayCommand(ShowSetting);
        private void ShowSetting()
        {
            DMQueryInstance.GetInstance()?.ShowDMQuerySettingFlyout();
        }

        public ICommand ExportFileCommand => new RelayCommand(ExportFile);
        private void ExportFile()
        {
            _exportWorker.RunWorkerAsync();
        }

        //
        //
        //

        private void LoadConnection()
        {
            try
            {
                if (string.IsNullOrEmpty(_mddPath) || !File.Exists(_mddPath)) return;
                if (string.IsNullOrEmpty(_ddfPath) || !File.Exists(_ddfPath)) return;
                LoadProgressVisibility = Visibility.Visible;
                AfterLoadVisibility = Visibility.Hidden;
                string connectString = $"Provider=mrOleDB.Provider.2; Data Source = mrDataFileDsc; MR Init MDM Version = {{ }}; MR Init Access = 1; MR Init Category Names = {_readType}; Initial Catalog = {_mddPath}; Location={_ddfPath}";
                _aDODataSet.OleDbConnect(connectString);
                string queryString = "Select * From vdata";
                if (!string.IsNullOrEmpty(DMQueryInstance.GetInstance().QueryString)) queryString = DMQueryInstance.GetInstance().QueryString;
                DataSet _resultSet = _aDODataSet.GetDataSet(queryString);
                if (_resultSet is null) return;
                _resultTable = _resultSet.Tables[0];
                DataTableAddNo(_resultTable);
                LoadProgressVisibility = Visibility.Hidden;
                ResultView = _resultTable.DefaultView;
                AfterLoadVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void DataTableAddNo(DataTable dataTable)
        {
            dataTable.Columns.Add("No").SetOrdinal(0);
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i]["No"] = (i + 1).ToString();
            }
        }

        private void OnExportFile(object sender, DoWorkEventArgs e)
        {
            Action<Visibility> SetVisibility = new Action<Visibility>((v) =>
            {
                LoadProgressVisibility = v;
                ExportProgressVisibility = v;
            });
            Action<string> SetMessage = new Action<string>((s) => ExportPercentage = s);
            //
            if (_resultTable is null) return;
            SaveFileDialog dlg = new SaveFileDialog()
            {
                RestoreDirectory = true,
                Filter = "Excel File|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                _outputed = true;
                string savePath = dlg.FileName;
                _savePath = savePath;
                if (File.Exists(savePath)) File.Delete(savePath);
                //
                int totalCells = (_resultTable.Rows.Count + 1) * _resultTable.Columns.Count;
                int currentCells = 0;
                //
                Excel.Application app = new Excel.Application { Visible = false };
                try
                {
                    Excel.Workbook book = app.Workbooks.Add(Missing.Value);
                    Excel.Worksheet sheet = book.Sheets[1];
                    sheet.Name = _resultTable.TableName;
                    //
                    SetVisibility(Visibility.Visible);
                    for (int i = 0; i < _resultTable.Columns.Count; i++)
                    {
                        SetMessage((Math.Round((double)(++currentCells) / (double)totalCells, 2) * 100).ToString() + "%");
                        sheet.Cells[1, i + 1].Value2 = _resultTable.Columns[i].ColumnName;
                    }
                    //
                    for (int i = 0; i < _resultTable.Columns.Count; i++)
                    {
                        for (int j = 0; j < _resultTable.Rows.Count; j++)
                        {
                            SetMessage((Math.Round((double)(++currentCells) / (double)totalCells, 2) * 100).ToString() + "%");
                            sheet.Cells[j + 2, i + 1].Value2 = _resultTable.Rows[j][i].ToString();
                        }
                    }
                    SetMessage("存储文件中...");
                    book.SaveAs(savePath);
                    SetMessage("存储完成");
                    book.Close(false);
                    book = null;
                    SetVisibility(Visibility.Hidden);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    app.Quit();
                    app = null;
                }
            }
        }

    }
}
