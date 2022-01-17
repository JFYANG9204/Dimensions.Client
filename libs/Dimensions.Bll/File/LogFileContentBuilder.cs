using System;
using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Log)]
    public class LogFileContentBuilder : IFileContentBuilder
    {
        internal LogFileContentBuilder()
        {

        }

        private string _content;

        public string Get()
        {
            return _content;
        }

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            //
            string logTime = DateTime.Now.ToString();
            string varName = string.Empty;
            string logStatus = string.Empty;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i].Key.ToString() == FileWriterKeys.LogVarName)
                {
                    varName = contents[i].Value.ToString();
                }
                if (contents[i].Key.ToString() == FileWriterKeys.LogStatus)
                {
                    logStatus = contents[i].Value.ToString();
                }
                if (contents[i].Key.ToString() == FileWriterKeys.LogText)
                {
                    _content = $"[Logger : {contents[i].Value}]";
                    return;
                }
            }
            //
            string space = "               ";
            if (varName.Length < space.Length)
            {
                space = space.Substring(varName.Length - 1);
            }
            _content = logTime + "    " + varName + space + logStatus;
        }
    }
}
