using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using MDMLib;

namespace Dimensions.Client.Views
{
    /// <summary>
    /// TextEditorView.xaml 的交互逻辑
    /// </summary>
    public partial class TextEditorView : UserControl
    {
        public TextEditorView()
        {
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(TextEditorView).Assembly.GetManifestResourceStream("Dimensions.Client.CustomHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("Mdd Highlighting", new string[] { ".mrs", ".dms", ".mdd" }, customHighlighting);

            _openMddFileWorker.DoWork += OnOpenMddFileWorkerRun;
            _saveMddFileWorker.DoWork += OnSaveMddFileWorkerRun;

            _openMddFileWorker.RunWorkerCompleted += OnOpenMddFileWorkerCompleted;

            InitializeComponent();
            PrcRing.Visibility = Visibility.Hidden;
            SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);

            SearchPanel.Install(Editor.TextArea);
        }

        private readonly Document _document = new Document();

        private string _path;
        private string _tempPath;
        private string _savePath;

        private string _script;
        private string _metadata;
        private string _code;

        private bool _isMddOpened = false;

        private readonly BackgroundWorker _openMddFileWorker = new BackgroundWorker();
        private readonly BackgroundWorker _saveMddFileWorker = new BackgroundWorker();

        private void OnOpenMddFileWorkerRun(object sender, DoWorkEventArgs e)
        {
            _document.Open(_path);
            _script = _document.Script;
            _metadata = _script.Substring(0, _script.IndexOf("End Metadata") + 12);
            if (_script.Contains("End Routing"))
            {
                _code = _script.Substring(_script.IndexOf("End Metadata") + 14);
            }
            _tempPath = Path.Combine(Path.GetDirectoryName(_path), Path.GetFileNameWithoutExtension(_path) + ".tmp");
            CreateTempFile(_tempPath, _metadata);
            _isMddOpened = true;
        }

        private void OnOpenMddFileWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Editor.Load(_tempPath);
        }

        private void OnSaveMddFileWorkerRun(object sender, DoWorkEventArgs e)
        {
            //string data = _metadata;
            //if (!string.IsNullOrEmpty(_code))
            //    data += "\r\n" + _code;
            _document.Script = _script;
            if (!string.IsNullOrEmpty(_savePath))
                _document.Save(_savePath);
            else
                _document.Save();
            //_document.Close();
        }

        private void CreateTempFile(string path, string data)
        {
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                byte[] text = Encoding.Default.GetBytes(data);
                fs.Write(text, 0, text.Length);
                fs.Flush();
            }
        }

        public void Load(string path)
        {
            _path = path;
            if (Path.GetExtension(_path) == ".mdd")
            {
                _openMddFileWorker.RunWorkerAsync();
            }
            else
            {
                Editor.Load(_path);
            }
            Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(path));
            //SearchPanel.Install(Editor);
        }

        public void SetFontSize(int fontSize)
        {
            Editor.FontSize = fontSize;
        }

        public void Save(string savePath = null)
        {
            string path = _path;
            if (!string.IsNullOrEmpty(savePath)) 
            {
                path = savePath;
                _savePath = savePath;
            }
            if (Path.GetExtension(_path) == ".mdd")
            {
                _saveMddFileWorker.RunWorkerAsync();
            }
            else
            {
                Editor.Save(path);
            }
        }

        public void DeleteTempFile()
        {
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
        }

        public void Close()
        {
            if (_isMddOpened)
            {
                _document.Close();
            }
        }

        private void OnEditorTextChanged(object sender, EventArgs e)
        {
            _metadata = Editor.Text;
            if (!string.IsNullOrEmpty(_code))
                _script = _metadata + "\r\n" + _code;
            else
                _script = _metadata;
        }
    }
}
