using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Mdd)]
    public class MddFileContentBuilder : IFileContentBuilder
    {
        private const string _setTitleLabel = "    sbSetTitleText(MDM,\"{0}\",MDD_TYPE,LOCALE,\"{1}\")";
        private const string _setAxis = "    sbSetAxisExpression(MDM,\"{0}\",\"{1}\")";
        private const string _setResponseText = "    sbSetResponseText(MDM,\"{0}\",MDD_TYPE,LOCALE,\"{1}\",\"{2}\")";
        private const string _setListText = "    sbSetDefineResponseText(MDM,\"{0}\",MDD_TYPE,LOCALE,\"{1}\",\"{2}\")";
        private const string _setAverage = "    sbAddAverageMentions(MDM,\"{0}\",Null,\"{1}\",True,\"2\")";
        private const string _setContinueFactor = "    sbAddFactors(MDM,\"{0}\",MDD_TYPE,LOCALE,{1},{2},\"{3}\",True)";
        private const string _setSplitFactor = "    MDM.fields[\"{0}\"].elements[\"{1}\"].Factor = {2}";
        private const string _setRebaseFunction = "" +
            "Function fnRebaseOnTotal(baseText, strBase, strSide, booMean)\n" +
            "    Dim _axis\n" +
            "    _axis = \"{e1 '' text(),base 'Base : \" + strBase + \"' base('\" + baseText + \"'),e2 '' text(),\" + strSide + \",e3 '' text(),sigma 'Sigma' subtotal()\"\n" +
            "    If booMean = True Then _axis = _axis + mean_inc(\"\")\n" +
            "    _axis = _axis + \"}\"\n" +
            "    fnRebaseOnTotal = _axis\n" +
            "End Function\n";
        private const string _setNotes = "\n'*************************{0}*************************\n";

        internal MddFileContentBuilder()
        {
        }

        private string _content;
        /// <summary>
        /// 添加内容
        /// </summary>
        /// <param name="type">内容类型</param>
        /// <param name="contents">文本内容</param>
        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            string _notes = string.Empty;
            string _varName = string.Empty;
            string _varTitle = string.Empty;
            string _axis = string.Empty;
            //
            string _codeName = string.Empty;
            string _codeLabel = string.Empty;
            //
            string _factor = string.Empty;
            string _startFactor = string.Empty;
            string _stepFactor = string.Empty;
            string _removeCode = string.Empty;
            //
            string _avgLabel = string.Empty;
            //
            string _text = string.Empty;
            //
            for (int i = 0; i < contents.Length; i++)
            {
                switch (contents[i].Key.ToString())
                {
                    case FileWriterKeys.MddText:
                        _text = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.Notes:
                        _notes = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddVariableName:
                        _varName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddTitle:
                        _varTitle = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddAxis:
                        _axis = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddCodeName:
                        _codeName = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddCodeLabel:
                        _codeLabel = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddFactor:
                        _factor = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddStartFactor:
                        _startFactor = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddStep:
                        _stepFactor = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddAverageLabel:
                        _avgLabel = contents[i].Value.ToString();
                        break;
                    case FileWriterKeys.MddExclude:
                        _removeCode = contents[i].Value.ToString();
                        break;
                    default:
                        break;
                }
            }
            //
            switch (type)
            {
                case MddFileContentType.Text:
                    _content = _text;
                    break;
                case MddFileContentType.Title:
                    _content = string.Format(_setTitleLabel, _varName, _varTitle);
                    break;
                case MddFileContentType.Axis:
                    _content = string.Format(_setAxis, _varName, _axis);
                    break;
                case MddFileContentType.ResponseText:
                    _content = string.Format(_setResponseText, _varName, _codeName, _codeLabel);
                    break;
                case MddFileContentType.ListText:
                    _content = string.Format(_setListText, _varName, _codeName, _codeLabel);
                    break;
                case MddFileContentType.Average:
                    _content = string.Format(_setAverage, _varName, _avgLabel);
                    break;
                case MddFileContentType.ContinueFactor:
                    _content = string.Format(_setContinueFactor, _varName, _startFactor, _stepFactor, _removeCode);
                    break;
                case MddFileContentType.SplitFactor:
                    _content = string.Format(_setSplitFactor, _varName, _codeName, _factor);
                    break;
                case MddFileContentType.RebaseFunction:
                    _content = _setRebaseFunction;
                    break;
                case MddFileContentType.Note:
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
