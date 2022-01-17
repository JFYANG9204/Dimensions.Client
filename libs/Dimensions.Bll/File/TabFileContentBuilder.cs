using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Tab)]
    class TabFileContentBuilder : IFileContentBuilder
    {
        private const string _setNormalTab = "fnAddTable(TableDoc,\"{0}\",banner,{1},\"{2}\")";
        private const string _setGridTab = "fnAddGrid(TableDoc,\"{0}\",\"{1}\",NULL,\"{2}\")";
        private const string _setGridWithFunction = "fnAddGrid(TableDoc,\"{0}\" + fnRebaseOnTotal(\"True\", \"Total Respondents\", {1}, {2}),\"{3}\",NULL,\"{4}\")";
        private const string _setGridSlice = "fnAddGridSliceTables(TableDoc,\"{0}\",banner,NULL,\"\",NULL,\"\")";
        private const string _setGridSliceWithFunction = "fnAddGridSliceTables(TableDoc,\"{0}\",banner,NULL,\"\",NULL,fnRebaseOnTotal(\"True\", \"Total Respondents\", {1}, {2}))";
        private const string _setDigit = "TableDoc.Coding.CreateCategorizedVariable(\"{0}\",\".code_{0}\")";
        private const string _setFilter = "    .Item[\"T\" + ctext(.count)].Filters.AddNew(\"f\"+ ctext(.count),\"{0}\",\"\")";
        private const string _setNormalLabel = "    .Item[\"T\" + ctext(.count)].Side.SubAxes[0].Label =.Item[\"T\" + ctext(.count)].Side.SubAxes[0].Label + \"{0}\"";
        private const string _setGridLabel = "    .Item[\"TG\" + ctext(.count)].Side.SubAxes[0].Label =.Item[\"TG\" + ctext(.count)].Side.SubAxes[0].Label + \"{0}\"";
        private const string _setNormalWirthFunction = "fnAddTable(TableDoc,\"{0}\" + {1},banner,Null,\"\")";
        private const string _setNotes = "\n'*************************{0}*************************\n";
        private const string _setDimVar = "\nDim {0}\n{0} = \"{1}\"\n";

        internal TabFileContentBuilder()
        {
        }

        private string _content;

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            string _notes = string.Empty;
            //
            string _varName = string.Empty;
            string _baseLabel = string.Empty;
            string _function = string.Empty;
            string _varTopName = string.Empty;
            string _filter = string.Empty;
            string _tabLabel = string.Empty;
            string _tabSide = "..";
            string _tabTitle = "NULL";
            string _hasMean = "False";
            //
            for (int i = 0; i < contents.Length; i++)
            {
                switch (contents[i].Key.ToString())
                {
                    case FileWriterKeys.Notes:
                        _notes = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabSideName:
                        _varName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabBaseName:
                        _baseLabel = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabFunction:
                        _function = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabTopName:
                        _varTopName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabNormalFilter:
                        _filter = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabLabel:
                        _tabLabel = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabTitle:
                        _tabTitle = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabSideAxis:
                        _tabSide = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.TabHasMean:
                        _hasMean = contents[i].Value.ToString();
                        break;
                    default:
                        break;
                }
            }
            //
            switch (type)
            {
                case TabFileContentType.Normal:
                    _content = string.Format(_setNormalTab, _varName, _tabTitle, _baseLabel);
                    break;
                case TabFileContentType.NormalWithFunction:
                    _content = string.Format(_setNormalWirthFunction, _varName, _function);
                    break;
                case TabFileContentType.Grid:
                    _content = string.Format(_setGridTab, _varName, _varTopName, _baseLabel);
                    break;
                case TabFileContentType.GridWithFunction:
                    _content = string.Format(_setGridWithFunction,
                        _varName,
                        _tabSide == ".." ? "\"..\"" : _tabSide,
                        _hasMean,
                        _varTopName,
                        _baseLabel);
                    break;
                case TabFileContentType.GridSlice:
                    _content = string.Format(_setGridSlice, _varName);
                    break;
                case TabFileContentType.GridSliceWithFunction:
                    _content = string.Format(_setGridSliceWithFunction, _varName,
                        _tabSide == ".." ? "\"..\"" : _tabSide,
                        _hasMean);
                    break;
                case TabFileContentType.NormalLabel:
                    _content = string.Format(_setNormalLabel, _tabLabel);
                    break;
                case TabFileContentType.GridLabel:
                    _content = string.Format(_setGridLabel, _tabLabel);
                    break;
                case TabFileContentType.Digit:
                    _content = string.Format(_setDigit, _varName);
                    break;
                case TabFileContentType.Filter:
                    _content = string.Format(_setFilter, _filter);
                    break;
                case TabFileContentType.DimVar:
                    _content = string.Format(_setDimVar, _varName, _tabSide);
                    break;
                case TabFileContentType.Note:
                    _content = string.Format(_setNotes, _notes);
                    break;
                default:
                    break;
            }
        }

        public string Get()
        {
            return _content;
        }
    }
}
