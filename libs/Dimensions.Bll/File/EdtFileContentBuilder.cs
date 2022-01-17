using Dimensions.Bll.Generic;
using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Edt)]
    class EdtFileContentBuilder : IFileContentBuilder
    {
        internal EdtFileContentBuilder()
        {

        }

        private string _content;

        private const string _setNote = "\n'*************************{0}*************************\n";

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            string _notes = string.Empty;
            ICodeList _topCodes = null;
            ICodeList _sideCodes = null;
            ICodeList.MergedLoopDefinition _mergedefinition = new ICodeList.MergedLoopDefinition();
            //
            string _targetName = string.Empty;
            string _sideName = string.Empty;
            string _refName = string.Empty;
            string _codeLabel = string.Empty;
            string _sourceName = string.Empty;
            string _pyAssignment = string.Empty;
            string _text = string.Empty;
            //
            for (int i = 0; i < contents.Length; i++)
            {
                switch (contents[i].Key.ToString())
                {
                    case FileWriterKeys.Notes:
                        _notes = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtTargetVariableName:
                        _targetName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtTargetVariabelSideName:
                        _sideName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtTopCodes:
                        if (contents[i].Value.GetType() == typeof(CodeList))
                            _topCodes = (CodeList)contents[i].Value;
                        break;
                    case FileWriterKeys.EdtSideCodes:
                        if (contents[i].Value.GetType() == typeof(CodeList))
                            _sideCodes = (CodeList)contents[i].Value;
                        break;
                    case FileWriterKeys.EdtCodeLabel:
                        _codeLabel = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtSourceVariableName:
                        _sourceName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtRefVarName:
                        _refName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.EdtDefinition:
                        if (contents[i].Value.GetType() == typeof(ICodeList.MergedLoopDefinition))
                            _mergedefinition = (ICodeList.MergedLoopDefinition)contents[i].Value;
                        break;
                    case FileWriterKeys.EdtPyDatas:
                        if (contents[i].Value.GetType() == typeof(string))
                            _pyAssignment = (string)contents[i].Value;
                        break;
                    case FileWriterKeys.EdtText:
                        if (contents[i].Value.GetType() == typeof(string))
                            _text = (string)contents[i].Value;
                        break;
                    default:
                        break;
                }
            }
            //
            switch (type)
            {
                case EdtFileContentType.Note:
                    _content = string.Format(_setNote, _notes);
                    break;
                case EdtFileContentType.MergedLoop:
                    //
                    string topLoopName = "cat";
                    string sideLoopName = "cat_1";
                    //
                    bool hasTopArray = false;
                    bool hasSideArray = false;
                    //
                    if (!(_topCodes is null) && !_topCodes.IsContinuousCodes())
                    {
                        topLoopName = "int_Top_" + _refName;
                        _content += string.Format("\nDim str_Top_{0}_Code[], {1}\n", _refName, topLoopName);
                        for (int i = 0; i < _topCodes.Count; i++)
                        {
                            string strIndex;
                            if (i < 9) strIndex = "0" + (i + 1).ToString();
                            else strIndex = (i + 1).ToString();
                            _content += string.Format("str_Top_{0}_Code[{1}] = \"{2}\"\n", _refName, strIndex, _topCodes[i].Name);
                        }
                        hasTopArray = true;
                    }
                    if (!(_sideCodes is null) && !_sideCodes.IsContinuousCodes())
                    {
                        sideLoopName = "int_Side_" + _refName;
                        _content += string.Format("\nDim str_Side_{0}_Code[], {1}\n", _refName, sideLoopName);
                        for (int i = 0; i < _sideCodes.Count; i++)
                        {
                            string strIndex;
                            if (i < 9) strIndex = "0" + (i + 1).ToString();
                            else strIndex = (i + 1).ToString();
                            _content += string.Format("str_Side_{0}_Code[{1}], \"{2}\"\n", _refName, strIndex, _sideCodes[i].Name);
                        }
                        hasSideArray = true;
                    }
                    //
                    string codePart = "\"" + _mergedefinition.StartLabel + "\"";
                    string fullTargetName = _targetName;
                    if (hasTopArray)
                    {
                        _content += string.Format("\nFor {0} = 1 To UBound({1})\n", 
                            topLoopName, "str_Top_" + _refName + "_Code");
                        if (_mergedefinition.Type == ICodeList.MergedLoopDefinitionType.TopFirst)
                            codePart += " + str_Top_" + _refName + "_Code + \"" + _mergedefinition.InnerLabel + "\"";
                        fullTargetName += "[\"" + _codeLabel + "\" + str_Top_" + _refName + "_Code[" + topLoopName + "]]." + _sideName;
                    }
                    else
                    {
                        _content += string.Format("For each cat in {0}.Categories\n", _targetName);
                        if (_mergedefinition.Type == ICodeList.MergedLoopDefinitionType.TopFirst)
                            codePart += " + Right(cat.name, Len(cat.name) - 1) + " + "\"" + _mergedefinition.InnerLabel + "\"";
                        fullTargetName += "[cat.name]." + _sideName;
                    }
                    //
                    string fullTargetCode;
                    if (hasSideArray)
                    {
                        _content += string.Format("    For {0} = 1 To UBound({1})\n",
                            sideLoopName, "str_Side_" + _refName + "_Code");
                        if (_mergedefinition.Type == ICodeList.MergedLoopDefinitionType.SideFirst)
                            codePart += " + str_Side_" + _refName + "_Code\"" + _mergedefinition.InnerLabel + "\"";
                        fullTargetCode = "CCategorical(\"" + _codeLabel + "\" + str_Side_" + _refName + "_Code[" + sideLoopName + "])";
                    }
                    else
                    {
                        _content += string.Format("    For each cat_1 in {0}[cat.name].Column.Categories\n", _targetName);
                        if (_mergedefinition.Type == ICodeList.MergedLoopDefinitionType.SideFirst)
                            codePart += " + Right(cat_1.name, Len(cat_1.name) - 1) + " + "\"" + _mergedefinition.InnerLabel + "\"";
                        fullTargetCode = "CCategorical(cat_1.name)";
                    }
                    //
                    if (_mergedefinition.Type == ICodeList.MergedLoopDefinitionType.TopFirst)
                    {
                        if (hasSideArray) codePart += " + str_Side_" + _refName + "_Code[" + sideLoopName + "]";
                        else codePart += " + Right(cat_1.name, Len(cat_1.name) - 1)";
                    }
                    else
                    {
                        if (hasTopArray) codePart += " + str_Top_" + _refName + "_Code[" + topLoopName + "]";
                        else codePart += " + Right(cat.name, Len(cat.name) - 1)";
                    }
                    //
                    _content += string.Format("        If {0}.ContainsAny({1})    Then {2} = {2} + {3}\n",
                        _sourceName, codePart, fullTargetName, fullTargetCode);
                    _content += "    Next\nNext\n";
                    break;
                case EdtFileContentType.Pyramid:
                    _content += _pyAssignment;
                    break;
                case EdtFileContentType.Text:
                    _content += _text;
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
