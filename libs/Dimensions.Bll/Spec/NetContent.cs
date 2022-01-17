using Dimensions.Bll.Generic;
using Dimensions.Bll.String;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dimensions.Bll.Spec
{
    public class NetContent : IEnumerable
    {
        public NetContent()
        {
            _items = new NetItem[1];
        }

        public NetItem? this[int index]
        {
            get
            {
                if (_items is null)
                    return null;
                if (index < _items.Length)
                    return _items[index];
                else
                    return null;
            }
        }

        public IMddVariable Variable { get; private set; }

        private NetItem[] _items;

        public int Count
        {
            get
            {
                if (_items is null)
                    return 0;
                else
                    return _items.Length;
            }
        }

        public void Add(NetItem item)
        {
            if (_items[0].Empty)
                _items[0] = item;
            else
                _items = _items.Append(item).ToArray();
        }

        public void Add(string label, string codes)
        {
            if (_items[0].Empty)
                _items[0] = new NetItem() { Codes = codes, Label = label };
            else
                _items = _items.Append(new NetItem() { Label = label, Codes = codes }).ToArray();
        }

        public bool Contains(string key)
        {
            bool result = false;
            if (_items != null)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    if (!_items[i].Empty && _items[i].Label.ToLower() == key.ToLower())
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public void SetVariable(IMddVariable variable)
        {
            Variable = variable;
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                yield return _items[i];
            }
        }

        public struct NetItem
        {
            public string Label;
            public string Codes;
            public bool Empty { get
                {
                    if (string.IsNullOrEmpty(Label) || string.IsNullOrEmpty(Codes))
                        return true;
                    else
                        return false;
                }
            }
        }

        public string GetNetString(out string remain)
        {
            string netRst = string.Empty;
            remain = string.Empty;
            //
            string[] codes = null;
            if (Variable != null && Variable.CodeList.Count > 0)
            {
                codes = new string[Variable.CodeList.Count];
                for (int i = 0; i < Variable.CodeList.Count; i++)
                {
                    codes[i] = Variable.CodeList[i].Name;
                }
            }
            //
            for (int i = 0; i < _items.Length; i++)
            {
                string ntCount;
                if (i < 9)
                    ntCount = "0" + (i + 1).ToString();
                else
                    ntCount = (i + 1).ToString();
                //
                string label = FormatNetLabel(_items[i].Label);
                string space = "                       ";
                if (label.Length > space.Length)
                    space = "    ";
                else
                    space = space.Substring(label.Length);
                //
                string codeLabel = "V";
                if (Variable != null)
                    codeLabel = Variable.CodeList.CommonPart;
                if (string.IsNullOrEmpty(codeLabel))
                    codeLabel = "V";
                //
                netRst += "    nt" + ntCount + "    'Net." + label + "'" + space + "net({";
                foreach (string code in _items[i].Codes.Split('/'))
                {
                    if (Variable != null)
                    {
                        if (Variable.CodeList.Count > 0 && Variable.CodeList.GetCodeNameFromNumber(code, out string checkCode))
                        {
                            netRst += checkCode + ",";
                            if (codes != null && codes.Length > 0)
                            {
                                codes = StringArrayFunction.RemoveItemByNumber(checkCode, codes);
                            }
                        }
                        else
                        {
                            netRst += codeLabel + code + ",";
                            if (codes != null && codes.Length > 0)
                            {
                                codes = StringArrayFunction.RemoveItemByNumber(code, codes);
                            }
                        }
                    }
                    else
                    {
                        netRst += codeLabel + code + ",";
                    }
                }
                netRst = netRst.Substring(0, netRst.Length - 1) + "}),_\n";
            }
            //
            if (codes != null && codes.Length > 0)
            {
                for (int i = 0; i < codes.Length; i++)
                {
                    remain += codes[i] + ",";
                }
            }
            //
            return netRst;
        }

        private string FormatNetLabel(string label)
        {
            string formatStr = label;
            if (formatStr.Contains(": "))
            {
                string[] strArray = Regex.Split(formatStr, ": ");
                int index = strArray.Length - 1;
                formatStr = strArray[index];
                while (index >= 0)
                {
                    if (formatStr.Length < strArray[index].Length)
                    {
                        formatStr = strArray[index];
                    }
                    index--;
                }
            }
            return formatStr.Trim();
        }
    }
}
