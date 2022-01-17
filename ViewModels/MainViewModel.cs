using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.IconPacks;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls; 
using Dimensions.Client.Singleton;
using Dimensions.Client.Views;

namespace Dimensions.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        //private const string BaseConnectString = "Provider=mrOleDB.Provider.2; Data Source = mrDataFileDsc; MR Init MDM Version = { }; MR Init Access = 1; MR Init Category Names = {0}; Initial Catalog = {1}; Location = {2}";
        private const string BaseQueryString = "Select {0} From vdata{1}";

        private static readonly ObservableCollection<HamMenuItem> AppMenu = new ObservableCollection<HamMenuItem>();
        private static readonly ObservableCollection<HamMenuItem> AppOptionsMenu = new ObservableCollection<HamMenuItem>();

        public ObservableCollection<HamMenuItem> Menu => AppMenu;
        public ObservableCollection<HamMenuItem> OptionsMenu => AppOptionsMenu;

        private readonly BackgroundWorker _readBkWorker = new BackgroundWorker();

        public MainViewModel()
        {
            Menu.Add(new HamMenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FileExcelSolid },
                Label = "SPEC自动化",
                NavigationType = typeof(SpecPage),
                NavigationDestination = new Uri("Views/SpecPage.xaml", UriKind.RelativeOrAbsolute)
            });

            Menu.Add(new HamMenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.QuestionCircleSolid },
                Label = "DM Query",
                NavigationType = typeof(DMQueryPage),
                NavigationDestination = new Uri("Views/DMQueryPage.xaml", UriKind.RelativeOrAbsolute)
            });

            Menu.Add(new HamMenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FontSolid },
                Label = "Text Editor",
                NavigationType = typeof(TextEditorPage),
                NavigationDestination = new Uri("Views/TextEditorPage.xaml", UriKind.RelativeOrAbsolute)
            });

            OptionsMenu.Add(new HamMenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CogSolid },
                Label = "设置",
                NavigationType = typeof(SettingPage),
                NavigationDestination = new Uri("Views/SettingPage.xaml", UriKind.RelativeOrAbsolute)
            });
            //
            // Flyout
            //
            SelectedFields = new ObservableCollection<string>();

            _readBkWorker.DoWork += (o, e) => { DMQueryInstance.GetInstance().LoadData(); };

            OpenDMQuerySetting = false;
            DMQueryInstance.GetInstance().ShowDMQuerySettingFlyout = new Action(() => OpenDMQuerySetting = true);
            DMQueryInstance.GetInstance().CloseDMQuerySettingFlyout = new Action(() => OpenDMQuerySetting = false);
            DMQueryInstance.GetInstance().SetProperty = new Action<object>(SetProperty);
            DMQueryInstance.GetInstance().SetQuery = new Action(SetQueryString);

            DMQueryInstance.GetInstance().Clear = () =>
            {
                if (Fields != null) Fields.Clear();
                if (SelectedFields != null) SelectedFields.Clear();
                SelectedFieldsIndex = -1;
                SelectedSelectIndex = -1;
                QueryString = string.Empty;
            };
        }

        private void SetProperty(object value)
        {
            if (value.GetType() == typeof(ObservableCollection<string>))
            {
                Fields = (ObservableCollection<string>)value;
                return;
            }
        }


        //
        // Flyouts
        //

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { Set(ref _searchText, value); }
        }

        private bool _openDMQuerySetting;
        public bool OpenDMQuerySetting
        {
            get { return _openDMQuerySetting; }
            set { Set(ref _openDMQuerySetting, value); }
        }

        private int _selectedFieldsIndex;
        public int SelectedFieldsIndex
        {
            get { return _selectedFieldsIndex; }
            set { Set(ref _selectedFieldsIndex, value); }
        }

        private int _selectedSelectIndex;
        public int SelectedSelectIndex
        {
            get { return _selectedSelectIndex; }
            set { Set(ref _selectedSelectIndex, value); }
        }


        private ObservableCollection<string> _fields;
        public ObservableCollection<string> Fields
        {
            get { return _fields; }
            set { Set(ref _fields, value); }
        }

        private ObservableCollection<string> _selectedFields;
        public ObservableCollection<string> SelectedFields
        {
            get { return _selectedFields; }
            set { Set(ref _selectedFields, value); }
        }
        
        private string _queryString;
        public string QueryString
        {
            get { return _queryString; }
            set { Set(ref _queryString, value); }
        }

        //
        // command
        //

        public ICommand CloseCommand => new RelayCommand(() => OpenDMQuerySetting = false);

        public ICommand AddFieldsCommand => new RelayCommand(AddFields);
        private void AddFields()
        {
            if (SelectedFields is null) SelectedFields = new ObservableCollection<string>();
            if (SelectedFieldsIndex >= 0 && SelectedFieldsIndex < Fields.Count)
            {
                string selected = Fields[SelectedFieldsIndex];
                if (!SelectedFields.Contains(selected))
                {
                    SelectedFields.Add(selected);
                    SetQueryString();
                }
            }
        }


        public ICommand RemoveFieldsCommand => new RelayCommand(RemoveFields);
        private void RemoveFields()
        {
            if (SelectedFields != null && SelectedSelectIndex >= 0 && SelectedSelectIndex < SelectedFields.Count)
            {
                SelectedFields.RemoveAt(SelectedSelectIndex);
                SetQueryString();
            }
        }

        public ICommand ClearSearchBoxCommand => new RelayCommand(() => SearchText = string.Empty);
        public ICommand FieldsDeleteKeyUpCommand => new RelayCommand<KeyEventArgs>(FieldsDeleteKeyUp);

        private void FieldsDeleteKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveFields();
            }
        }

        public ICommand DoubleClickAddFieldsCommand => new RelayCommand<MouseButtonEventArgs>(e => AddFields());
        public ICommand DoubleClickDelFieldsCommand => new RelayCommand<MouseButtonEventArgs>(e => RemoveFields());

        public ICommand SearchCommand => new RelayCommand<object>(Search);
        private void Search(object sender)
        {
            string searchText = (sender as TextBox).Text;
            if (string.IsNullOrEmpty(searchText))
            {
                Fields = DMQueryInstance.GetInstance().Fields;
                return;
            }

            ObservableCollection<string> searchResult = new ObservableCollection<string>();
            foreach (var field in DMQueryInstance.GetInstance().Fields)
            {
                if (field.ToLower().Contains(searchText.ToLower()))
                {
                    searchResult.Add(field);
                }
            }
            Fields = searchResult;
        }

        //public ICommand SelectFieldsChangedCommand => new RelayCommand<object>(OnSelectFieldsChanged);
        //private void OnSelectFieldsChanged(object sender)
        //{
        //    var items = (sender as ListBox).Items;
        //    if (items.Count > 0)
        //    {
        //        string vars = string.Empty;
        //        foreach (var field in items)
        //        {
        //            vars += field.ToString() + ",";
        //        }
        //        vars = vars.Substring(0, vars.Length - 1);
        //        QueryString = string.Format(BaseQueryString, vars, string.Empty);
        //    }
        //}

        public ICommand RemoveFilterFieldCommand => new RelayCommand(RemoveFilterField);
        private void RemoveFilterField()
        {
            DMQueryInstance.GetInstance().RemoveFilterField();
            SetQueryString();
        }

        public ICommand ConfirmCommand => new RelayCommand(Confirm);
        private void Confirm()
        {
            DMQueryInstance.GetInstance().QueryString = QueryString;
            OpenDMQuerySetting = false;
            _readBkWorker.RunWorkerAsync();
        }

        public ICommand WindowClosingCommand => new RelayCommand(WindowClosing);
        private void WindowClosing()
        {
            MainWindowInstance.GetInstance().OnTextEditorPageClosing?.Invoke();
        }


        private void SetQueryString()
        {
            string vars = string.Empty;
            if (!(SelectedFields is null || SelectedFields.Count == 0))
            {
                vars = string.Join(",", SelectedFields);
                //foreach (var field in SelectedFields)
                //{
                //    vars += field + ",";
                //}
                //vars = vars.Substring(0, vars.Length - 1);
            }
            if (string.IsNullOrEmpty(vars))
            {
                vars = "*";
            }
            string filter = DMQueryInstance.GetInstance().GetFilterString().Trim();
            while (filter.EndsWith("And")) filter = filter.Substring(0, filter.Length - 3).Trim();
            while (filter.EndsWith("Or")) filter = filter.Substring(0, filter.Length - 2).Trim();
            if (!string.IsNullOrEmpty(filter)) filter = " Where " + filter;
            QueryString = string.Format(BaseQueryString, vars, filter);
        }

    }
}
