using Dimensions.Bll.Generic;
using Dimensions.Bll.String;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Dimensions.Bll.Spec
{
    [DebuggerDisplay("Name = {Name}")]
    public class NewVariableDefinition : IEnumerable
    {
        public NewVariableDefinition(string name, string label)
        {
            Name = name;
            Label = label;
            _defVariables = new IMddVariable[1];
            _labels = new string[1];
            _codes = new string[1];
            IsPyramid = false;
            TopList = new CodeList();
            SideList = new CodeList();
        }

        public NewVariableDefinition()
        {
            _defVariables = new IMddVariable[1];
            _labels = new string[1];
            _codes = new string[1];
            IsPyramid = false;
            TopList = new CodeList();
            SideList = new CodeList();
        }

        public IMddVariable this[int index]
        {
            get
            {
                if (_defVariables[0] != null && index < _defVariables.Length)
                    return _defVariables[index];
                else
                    return null;
            }
        }

        public string Name { get; private set; }
        public string Label { get; private set; }

        public ICodeList TopList { get; private set; }
        public ICodeList SideList { get; private set; }

        private IMddVariable[] _defVariables;
        private string[] _labels;
        private string[] _codes;

        public bool IsPyramid { get; private set; }

        public void AddVariable(IMddVariable variable, string code, string label)
        {
            if (_defVariables[0] is null)
            {
                _defVariables[0] = variable;
                _labels[0] = label;
                _codes[0] = code;
            }
            else
            {
                _defVariables = _defVariables.Append(variable).ToArray();
                _labels = _labels.Append(label).ToArray();
                _codes = _codes.Append(code).ToArray();
            }
        }

        public string GetAssignment()
        {
            string assign = string.Empty;
            //
            if (_defVariables is null || _defVariables[0] is null) return string.Empty;
            if (IsPyramid)
            {
                assign += $"For each cat in {Name}.Categories\n";
                for (int i = 0; i < _defVariables.Length; i++)
                {
                    IMddVariable variable = _defVariables[i];
                    string codePart = "cat.name";
                    if (variable.CodeList.CommonPart != "V")
                        codePart = $"\"{variable.CodeList.CommonPart}\" + Right(cat.name, Len(cat.name) - 1)";
                    assign += $"    If {variable.Name}.ContainsAny({codePart}) " +
                        $"Then {Name}[cat.name].Column = {Name}[cat.name].Column + {{{_defVariables[i].Name}}}\n";
                }
                assign += "Next\n";
            }
            //
            return assign;
        }

        public void SetProperty(string name, string label)
        {
            Name = name;
            Label = label;
        }

        public void SetProperty(string name, string label, NewVariableCodeListType type)
        {
            switch (type)
            {
                case NewVariableCodeListType.Top:
                    if (!TopList.Contains(name))
                    {
                        TopList.Add(new Categorical(name, label));
                    }
                    break;
                case NewVariableCodeListType.Side:
                    if (!SideList.Contains(name))
                    {
                        SideList.Add(new Categorical(name, label));
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetProperty(ICodeList codes, NewVariableCodeListType type)
        {
            switch (type)
            {
                case NewVariableCodeListType.Top:
                    if (codes.CommonPart == "V")
                    {
                        TopList = codes;
                    }
                    else
                    {
                        TopList = new CodeList();
                        foreach (var code in codes)
                        {
                            TopList.Add(new Categorical("V" + code.Name.Substring(codes.CommonPart.Length), StringFunction.FormatString(code.Label)));
                        }
                    }
                    break;
                case NewVariableCodeListType.Side:
                    if (codes.CommonPart == "V")
                    {
                        SideList = codes;
                    }
                    else
                    {
                        SideList = new CodeList();
                        foreach (var code in codes)
                        {
                            SideList.Add(new Categorical("V" + code.Name.Substring(codes.CommonPart.Length), StringFunction.FormatString(code.Label)));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetProperty(bool isPyramid)
        {
            IsPyramid = isPyramid;
        }

        public IEnumerator GetEnumerator()
        {
            if (_defVariables[0] != null)
            {
                for (int i = 0; i < _defVariables.Length; i++)
                {
                    yield return _defVariables[i];
                }
            }
        }

        public bool Empty
        {
            get
            {
                if (_defVariables[0] is null && _defVariables.Length == 1)
                    return true;
                return false;
            }
        }
    }

    public enum NewVariableCodeListType
    {
        Top,
        Side
    }
}
