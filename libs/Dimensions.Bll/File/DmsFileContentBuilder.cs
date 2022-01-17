using Dimensions.Bll.Generic;
using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Dms)]
    public class DmsFileContentBuilder : IFileContentBuilder
    {

        internal DmsFileContentBuilder()
        {

        }

        private const string _setLoop = "" +
            "\n{0}    \"{1}\" [EvaluateemptyIterations = True]   loop\n" +
            "{{\n{2}\n" +
            "}} fields -\n" +
            "(\n    {3}    \"{4}\"    categorical[..]\n" +
            "    {{\n{5}\n" +
            "    }};\n) expand;\n\n";
        private const string _setNormal    = "{0}    \"{1}\" categorical[..]\n{{\n{2}\n}};";
        private const string _setDouble    = "{0}    \"{1}\"    double;";
        private const string _setLong      = "{0}    \"{1}\"    long;";
        private const string _setLoopFrame = "{0}    \"\"    [EvaluateemptyIterations = True] loop{{}} fields () expand grid;";
        private const string _setNotes     = "\n'*************************{0}*************************\n";

        private string _content;

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            ICodeList normalCats = null;
            ICodeList sideCats = null;
            //
            string _varName = string.Empty;
            string _varLabel = string.Empty;
            string _notes = string.Empty;
            string _sideName = string.Empty;
            //
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i].Key.ToString() == FileWriterKeys.Notes)
                {
                    _notes = contents[i].Value.ToString();
                }
                else if (contents[i].Key.ToString() == FileWriterKeys.DmsVariableName)
                {
                    _varName = contents[i].Value.ToString();
                }
                else if (contents[i].Key.ToString() == FileWriterKeys.DmsVariableLable)
                {
                    _varLabel = contents[i].Value.ToString();
                }
                else if (contents[i].Key.ToString() == FileWriterKeys.DmsTopCodeList)
                {
                    normalCats = (ICodeList)contents[i].Value;
                }
                else if (contents[i].Key.ToString() == FileWriterKeys.DmsSideCodeList)
                {
                    sideCats = (ICodeList)contents[i].Value;
                }
                else if (contents[i].Key.ToString() == FileWriterKeys.DmsSideName)
                {
                    _sideName = contents[i].Value.ToString();
                }
            }
            //
            string _sideCodes = string.Empty;
            string _topCodes = string.Empty;
            string _space = "      ";
            //
            switch (type)
            {
                case DmsFileContentType.Double:
                    _content = string.Format(_setDouble, _varName, _varLabel);
                    break;
                case DmsFileContentType.Long:
                    _content = string.Format(_setLong, _varName, _varLabel);
                    break;
                case DmsFileContentType.LoopFrame:
                    _content = string.Format(_setLoopFrame, _varName);
                    break;
                case DmsFileContentType.Normal:
                    if (sideCats != null)
                    {
                        for (int i = 0; i < sideCats.Count; i++)
                        {
                            _sideCodes += string.Format(
                                "    {0}{1}\"{2}\",\n",
                                sideCats[i].Name,
                                sideCats[i].Name.Length < 6 ? _space.Substring(sideCats[i].Name.Length) : " ",
                                sideCats[i].Label);
                        }
                    }
                    if (_sideCodes.Length > 2)
                    {
                        _sideCodes = _sideCodes.Substring(0, _sideCodes.Length - 2);
                    }
                    _content = string.Format(_setNormal, _varName, _varLabel, _sideCodes);
                    break;
                case DmsFileContentType.Loop:
                    if (normalCats != null)
                    {
                        for (int i = 0; i < normalCats.Count; i++)
                        {
                            _topCodes += string.Format(
                                "    {0}{1}\"{2}\",\n",
                                normalCats[i].Name,
                                normalCats[i].Name.Length < 6 ? _space.Substring(normalCats[i].Name.Length) : " ",
                                normalCats[i].Label);
                        }
                    }
                    if (sideCats != null)
                    {
                        for (int i = 0; i < sideCats.Count; i++)
                        {
                            _sideCodes += string.Format(
                                "        {0}{1}\"{2}\",\n",
                                sideCats[i].Name,
                                sideCats[i].Name.Length < 6 ? _space.Substring(sideCats[i].Name.Length) : " ",
                                sideCats[i].Label);
                        }
                    }
                    _content = string.Format(_setLoop, 
                        _varName, 
                        _varLabel, 
                        _topCodes.Length > 2 ? _topCodes.Substring(0, _topCodes.Length - 2) : _topCodes, 
                        _sideName, 
                        _varLabel, 
                        _sideCodes.Length > 2 ? _sideCodes.Substring(0, _sideCodes.Length - 2) : _sideCodes);
                    break;
                case DmsFileContentType.Note:
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
