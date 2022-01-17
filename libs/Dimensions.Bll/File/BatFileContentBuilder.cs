using Dimensions.Bll.Spec;
using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Bat)]
    class BatFileContentBuilder : IFileContentBuilder
    {
        internal BatFileContentBuilder()
        {

        }

        private const string _setBaseCT = "mrScriptCL Run.mrs /d:TOPBREAK=\"\\\"{0}\\\"\"  /d:vcout1=\"\\\"True\\\"\"  /d:vcout2=\"\\\"False\\\"\" /d:OUTPUTNAME=\"\\\"{1}-CT\\\"\"    /d:SIGTEST1=\"\\\"False\\\"\" /d:varsig=\"\\\"10\\\"\" /d:GLOBALFILTER=\"\\\"{2} ^> 0\\\"\" /d:GLOBALLABEL=\"\\\"Total\\\"\" /d:SIGCOLUMNIDS=\"\\\"%sigid%\\\"\" /d:SIGCOLUMNS=\"\\\"%sigtest%\\\"\" >> MasterLog.txt\n";
        private const string _setBaseNT = "mrScriptCL Run.mrs /d:TOPBREAK=\"\\\"{0}\\\"\"  /d:vcout1=\"\\\"False\\\"\" /d:vcout2=\"\\\"True\\\"\"  /d:OUTPUTNAME=\"\\\"{1}-NT\\\"\"    /d:SIGTEST1=\"\\\"False\\\"\" /d:varsig=\"\\\"10\\\"\" /d:GLOBALFILTER=\"\\\"{2} ^> 0\\\"\" /d:GLOBALLABEL=\"\\\"Total\\\"\" /d:SIGCOLUMNIDS=\"\\\"%sigid%\\\"\" /d:SIGCOLUMNS=\"\\\"%sigtest%\\\"\" >> MasterLog.txt\n";
        private const string _setBaseWT = "mrScriptCL Run.mrs /d:TOPBREAK=\"\\\"{0}\\\"\"  /d:vcout1=\"\\\"False\\\"\" /d:vcout2=\"\\\"True\\\"\"  /d:OUTPUTNAME=\"\\\"{1}-WT\\\"\"    /d:SIGTEST1=\"\\\"True\\\"\"  /d:varsig=\"\\\"10\\\"\" /d:GLOBALFILTER=\"\\\"{2} ^> 0\\\"\" /d:GLOBALLABEL=\"\\\"Total\\\"\" /d:SIGCOLUMNIDS=\"\\\"%sigid%\\\"\" /d:SIGCOLUMNS=\"\\\"%sigtest%\\\"\" >> MasterLog.txt\n";
        private const string _setMultWT = "mrScriptCL Run.mrs /d:TOPBREAK=\"\\\"{0}\\\"\"  /d:vcout1=\"\\\"False\\\"\" /d:vcout2=\"\\\"True\\\"\"  /d:OUTPUTNAME=\"\\\"{1}-WT{2}\\\"\" /d:SIGTEST1=\"\\\"True\\\"\"  /d:varsig=\"\\\"10\\\"\" /d:GLOBALFILTER=\"\\\"{3} ^> 0\\\"\" /d:GLOBALLABEL=\"\\\"Total\\\"\" /d:SIGCOLUMNIDS=\"\\\"%sigid%\\\"\" /d:SIGCOLUMNS=\"\\\"%sigtest%\\\"\" >> MasterLog.txt\n";

        //private const string _setBaseSigId = "set sigid=.abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ\n";
        private const string _setSigTest = "set sigtest={0}\n";
        private const string _setSigId = "set sigid={0}\n";
        private const string _setBatNote = "\n:::::::::::::::::::::::::::::::::::::::::::  {0}   \n";
        private string _content;

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            bool _addSubTitle = false;
            if (type == BatFileContentType.SubTitle)
            {
                _addSubTitle = true;
            }

            string _tableName = string.Empty;
            string _note = string.Empty;
            //
            ITopItemCollection _topItems = null;
            //
            for (int i = 0; i < contents.Length; i++)
            {
                switch (contents[i].Key.ToString())
                {
                    case FileWriterKeys.BatTableName:
                        _tableName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.BatHeaderItems:
                        if (contents[i].Value.GetType() == typeof(TopItemCollection))
                            _topItems = (TopItemCollection)contents[i].Value;
                        break;
                    case FileWriterKeys.Notes:
                        _note = contents[i].Value.ToString();
                        break;
                    default:
                        break;
                }
            }
            //
            if (type == BatFileContentType.Note)
            {
                _content = string.Format(_setBatNote, _note);
            }
            //
            if (_topItems != null)
            {
                string _headerName;
                string _recordName = _topItems.Record;
                if (_addSubTitle)
                {
                    _content += "\n\n" + _topItems.SubTitle + "\n\n";
                    string index = _topItems.Index > 10 ? _topItems.Index.ToString() : "0" + _topItems.Index.ToString();
                    _headerName = $"%Table_{index}%";
                }
                else
                {
                    _headerName = _topItems.Name;
                }
                //
                _content += string.Format(_setBaseCT, _headerName, _tableName, _recordName);
                _content += string.Format(_setBaseNT, _headerName, _tableName, _recordName);
                //
                if (_topItems.SigTests != null && _topItems.SigTests != null && _topItems.SigTests.Length == _topItems.SigSettings.Length)
                {
                    if (_topItems.SigTests.Length > 1)
                    {
                        for (int i = 0; i < _topItems.SigTests.Length; i++)
                        {
                            _content += string.Format(_setSigId, _topItems.SigSettings[i]);
                            _content += string.Format(_setSigTest, _topItems.SigTests[i]);
                            _content += string.Format(_setMultWT, _headerName, _tableName, (i + 1).ToString(), _recordName);
                        }
                    }
                    else
                    {
                        _content += string.Format(_setSigId, _topItems.SigSettings[0]);
                        _content += string.Format(_setSigTest, _topItems.SigTests[0]);
                        _content += string.Format(_setBaseWT, _headerName, _tableName, _recordName);
                    }
                }
            }
        }

        public string Get()
        {
            return _content;
        }
    }
}
