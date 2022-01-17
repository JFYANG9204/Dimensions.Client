using Dimensions.Bll.Mdd;
using System.Xml.Linq;
using System.Reflection;
using System;
using Dimensions.Bll.Generic;

namespace Dimensions.Bll.FileReader
{
    public class MddFileReader
    {
        public MddFileReader()
        {
        }

        public string Path { get; private set; }

        private MddDocument _mdd;

        public void Load(string _path)
        {
            Path = _path;
            _mdd = new MddDocument(_path);
            LoadContent();
        }

        public bool ShowUseful { get; set; }

        public string Content { get { return _content; } }

        private string _content = string.Empty;

        public void Clear()
        {
            _content = string.Empty;
        }

        private void LoadContent()
        {
            if (_mdd is null) return;
            Type mddType = _mdd.GetType();
            FieldInfo fieldInfo = mddType.GetField("_xmlMdm", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            XElement xml = (XElement)fieldInfo.GetValue(_mdd);
            _content = $"Metadata({_mdd.Language}, {_mdd.Context}, Label";
            if (xml.Attribute("systemvariable") != null && xml.Attribute("systemvariable").Value == "0") _content += ", SystemVariables = false";
            _content += ")\n\n";
            if (_mdd.Properties != null)
            {
                string properties = string.Empty;
                foreach (var prop in _mdd.Properties)
                {
                    properties += $"        {prop.Name} = \"{prop.Value}\"\n";
                }
                _content += $"HDATA \"\"\n    [\n{properties}\n    ]\n";
            }
            if (_mdd.Templates != null)
            {
                string templates = string.Empty;
                foreach (var temp in _mdd.Templates)
                {
                    templates += $"        {temp.Name} = \"{temp.Value}\"\n";
                }
                _content += $"    templates(\n{templates}\n    );\n";
            }
            _content += "\n";
            if (_mdd.ListFields.Count > 0)
            {
                string listTxt = string.Empty;
                foreach (var list in _mdd.ListFields)
                {
                    string codes = string.Empty;
                    foreach (var code in list.CodeList)
                        codes += $"        {code.Name}    \"{code.Label}\",\n";
                    if (!string.IsNullOrEmpty(codes)) codes = codes.Substring(0, codes.Length - 2);
                    listTxt += $"    {list.Name} - define\n    {{\n{codes}\n    }};\n";
                }
                _content += listTxt;
            }
            // Design
            FieldInfo designInfo = mddType.GetField("_xmlDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            XElement design = (XElement)designInfo.GetValue(_mdd);
            foreach (var node in design.Element("fields").Elements())
            {
                if (node.Name == "variable")
                {
                    if (node.Attribute("ref") is null) continue;
                    string id = node.Attribute("ref").Value;
                    _content += GetNormalVariableText(_mdd.Fields.Get(id), 4);
                }
                if (node.Name == "loop" || node.Name == "grid")
                {
                    string _loop = string.Empty;
                    GetLoopVariableText(_mdd.Fields[node.Attribute("name").Value], 4, ref _loop);
                    _content += "\n" + _loop + "\n";
                }
                if (node.Name == "class")
                {
                    if (node.Attribute("name") is null) continue;
                    string name = node.Attribute("name").Value;
                    _content += GetBlockText(_mdd.Fields[name], 4);
                }
            }
            //
            _content += "\nEnd Metadata";
        }

        private string GetPropertieText(Property[] properties, int indent)
        {
            if (properties is null) return string.Empty;
            string space = string.Empty;
            for (int i = 0; i < indent; i++) space += " ";
            string res = space + "[\n";
            foreach (var prop in properties)
            {
                if (prop.Name.ToUpper() == "VISIBLE") continue;
                res += $"{space}    {prop.Name} = \"{prop.Value}\",\n";
            }
            res = res.Substring(0, res.Length - 2) + "\n" + space + "]";
            return res;
        }

        private bool HasOnlyVisibleProperty(Property[] properties)
        {
            if (properties is null || properties.Length > 1) return false;
            else return properties[0].Name.ToUpper() == "VISIBLE";
        }

        private string GetNormalVariableText(IMddVariable variable, int indent)
        {
            if (variable is null) return string.Empty;
            string res = string.Empty;
            string space = string.Empty;
            for (int i = 0; i < indent; i++) space += " ";
            res += $"\n{space}{variable.Name}    \"{variable.Label}\"";
            if (variable.Properties != null && !HasOnlyVisibleProperty(variable.Properties) && !ShowUseful) 
                res += $"\n{GetPropertieText(variable.Properties, indent + 4)}\n";
            else res += "\n";
            switch (variable.ValueType)
            {
                case ValueType.Long:
                    if (!string.IsNullOrEmpty(variable.Range.Max) || !string.IsNullOrEmpty(variable.Range.Min))
                        res += $"{space}long {variable.Range.Get()};\n";
                    else
                        res += $"{space}long;\n";
                    break;
                case ValueType.Double:
                    if (!string.IsNullOrEmpty(variable.Range.Max) || !string.IsNullOrEmpty(variable.Range.Min))
                        res += $"{space}double {variable.Range.Get()};\n";
                    else
                        res += $"{space}double;\n";
                    break;
                case ValueType.Categorical:
                    res += $"{space}categorical";
                    if (!string.IsNullOrEmpty(variable.Range.Max) || !string.IsNullOrEmpty(variable.Range.Min)) res += $" {variable.Range.Get()}";
                    res += "\n";
                    if (variable.ValueType == ValueType.Categorical)
                    {
                        string side = string.Empty;
                        if (variable.UseList)
                        {
                            string listName = _mdd.ListFields.Get(variable.ListId).Name;
                            side += $"{space}    use {listName}";
                        }
                        if (variable.CodeList.Count > 0)
                        {
                            if (variable.UseList) side += ",\n";
                            side += GetCodeListText(variable.CodeList, indent);
                            side = side.Substring(0, side.Length - 2);
                        }
                        res += $"{space}{{\n{side}\n{space}}};\n";
                    }
                    break;
                case ValueType.Text:
                    if (!string.IsNullOrEmpty(variable.Range.Max) || !string.IsNullOrEmpty(variable.Range.Min))
                        res += $"{space}text {variable.Range.Get()};\n";
                    else
                        res += $"{space}text;\n";
                    break;
                case ValueType.Info:
                    res += $"{space}info;\n";
                    break;
                case ValueType.Boolean:
                    res += $"{space}boolean;\n";
                    break;
                case ValueType.Date:
                    res += $"{space}date;\n";
                    break;
                case ValueType.Default:
                    break;
                default:
                    break;
            }
            return res;
        }

        private void GetLoopVariableText(IMddVariable variable, int indent, ref string res)
        {
            if (variable is null) return;
            string space = string.Empty;
            for (int i = 0; i < indent; i++) space += " ";
            res += $"\n{space}{variable.Name}    \"{variable.Label}\"    loop\n";
            string top = string.Empty;
            if (variable.UseList)
            {
                top += $"{space}    use {_mdd.ListFields.Get(variable.ListId).Name}";
            }
            if (variable.CodeList.Count > 0)
            {
                if (variable.UseList) top += ",\n";
                top += GetCodeListText(variable.CodeList, indent);
                top = top.Substring(0, top.Length - 2);
            }
            res += $"{space}{{\n{top}\n{space}}} fields -\n{space}(\n";
            if (variable.HasChildren)
            {
                foreach (var child in variable.Children)
                {
                    if (child.HasChildren)
                    {
                        GetLoopVariableText(child, indent + 4, ref res);
                    }
                    else
                    {
                        res += GetNormalVariableText(child, indent + 4);
                    }
                }
            }
            res += $"\n{space}) expand;\n";
        }

        private string GetBlockText(IMddVariable variable, int indent)
        {
            if (variable is null) return string.Empty;
            string space = string.Empty;
            for (int i = 0; i < indent; i++) space += " ";
            string res = $"\n{space}{variable.Name}    \"{variable.Label}\"    block fields\n{space}(\n";
            if (variable.HasChildren)
            {
                foreach (var child in variable.Children)
                {
                    res += GetNormalVariableText(child, indent + 4);
                }
            }
            res += $"\n{space});\n";
            return res;
        }

        private string GetCodeListText(ICodeList codes, int indent)
        {
            string res = string.Empty;
            string space = string.Empty;
            for (int i = 0; i < indent; i++) space += " ";
            foreach (var code in codes)
            {
                string properties = GetPropertieText(code.Properties, indent + 4);
                if (code.Name.ToLower() == "other")
                    res += $"{space}    - \"{code.Label}\" other";
                else if (code.Name.ToLower() == "na")
                    res += $"{space}    - \"{code.Label}\" NA";
                else if (code.Name.ToLower() == "dk")
                    res += $"{space}    - \"{code.Label}\" DK";
                else
                    res += $"{space}    {code.Name}    \"{code.Label}\"";
                if (code.Properties != null && !HasOnlyVisibleProperty(code.Properties) && !ShowUseful)
                    res += $"\n{GetPropertieText(code.Properties, indent + 8)}";
                res += ",\n";
            }
            return res;
        }

    }
}
