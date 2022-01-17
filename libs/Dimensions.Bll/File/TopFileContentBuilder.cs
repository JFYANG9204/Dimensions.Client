using System.Collections.Generic;
using Dimensions.Bll.Spec;

namespace Dimensions.Bll.File
{
    [WriterInfo(FileType.Top)]
    class TopFileContentBuilder : IFileContentBuilder
    {
        internal TopFileContentBuilder()
        {

        }

        private const string _setBaseFrame = "\n{0}    \"\"    categorical[..]\n{{\n{1}}} axis(\"{{ base '总体' base()[Ishidden = True],..}}\");\n";
        private const string _setBaseDefinition = "    {0}{1}\"{2}\"{3}expression(\"{4}\"),\n";
        private const string _setSpace = "                                                            ";
        private const string _setVarSpace = "     ";

        private string _content;

        public void Set(string type, params KeyValuePair<object, object>[] contents)
        {
            _content = string.Empty;
            ITopItemCollection _items = null;
            bool _subTitle = false;
            //
            switch (type)
            {
                case TopFileContentType.SubTitle:
                    _subTitle = true;
                    break;
                case TopFileContentType.NonSubTitle:
                    _subTitle = false;
                    break;
                default:
                    break;
            }
            //
            for (int i = 0; i < contents.Length; i++)
            {
                switch (contents[i].Key.ToString())
                {
                    case FileWriterKeys.TopDefinitions:
                        if (contents[i].Value.GetType() == typeof(TopItemCollection))
                            _items = (TopItemCollection)contents[i].Value;
                        break;
                    default:
                        break;
                }
            }
            //
            string _varName = _items != null ? _items.Name : "TopBreak";
            string _definitions = string.Empty;
            if (_items != null)
            {
                List<string> _subs = new List<string>();
                if (_subTitle && _items.Group != null && _items.Group.Count > 0)
                {
                    foreach (var key in _items.Group.Keys)
                    {
                        int count = _items.Group[key].Split(',').Length;
                        for (int j = 0; j < count; j++)
                        {
                            _subs.Add(key);
                        }
                    }
                }
                for (int i = 0; i < _items.Count; i++)
                {
                    string space = _setSpace;
                    string label = _items[i].Label;
                    if (_subTitle && _subs.Count > i && !string.IsNullOrEmpty(_subs[i]))
                    {
                        label = _subs[i] + " - " + _items[i].Label;
                    }
                    if (label.Length < _setSpace.Length) space = space.Substring(label.Length);
                    else space = "    ";
                    _definitions += string.Format(_setBaseDefinition,
                        _items[i].Code,
                        _setVarSpace.Substring(_items[i].Code.Length),
                        label,
                        space,
                        _items[i].Definition);
                }
                _definitions = _definitions.Substring(0, _definitions.Length - 2) + "\n";
            }
            _content = string.Format(_setBaseFrame, _varName, _definitions);
        }

        public string Get()
        {
            return _content;
        }
    }
}
