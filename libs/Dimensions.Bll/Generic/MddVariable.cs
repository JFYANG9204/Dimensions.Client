using System.Diagnostics;
using System.Linq;

namespace Dimensions.Bll.Generic
{
    [DebuggerDisplay("Name = {Name}")]
    class MddVariable : IMddVariable
    {
        public MddVariable()
        {
            _languages = new string[1];
            _labels = new string[1];
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Label
        {
            get
            {
                if (!string.IsNullOrEmpty(Language) && _languages != null && _languages.Contains(Language))
                {
                    int index = -1;
                    for (int i = 0; i < _languages.Length; i++)
                    {
                        if (_languages[i] == Language)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index > -1 && _labels != null && _labels.Length > index)
                    {
                        return _labels[index];
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                return string.Empty;
            }
        }
        public ICodeList CodeList { get; private set; }
        public bool UseList
        {
            get
            {
                if (!string.IsNullOrEmpty(ListId))
                    return true;
                else
                    return false;
            }
        }
        public string ListId { get; private set; }
        public string Language { get; private set; }

        private string[] _languages;
        private string[] _labels;

        public bool HasChildren
        {
            get
            {
                if (Children != null && Children.Length > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public IMddVariable[] Children { get; private set; }
        public IMddVariable Parent { get; private set; }
        public VariableType VariableType { get; private set; }
        public ValueType ValueType { get; private set; }
        public Property[] Properties { get; private set; }
        public string Validation { get; set; }
        public string Assignment { get; private set; }

        public void SetProperty(string listId)
        {
            ListId = listId;
        }

        public void SetProperty(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public void SetProperty(ICategorical categorical)
        {
            CodeList.Add(categorical);
        }

        public void SetProperty(ICodeList codeList)
        {
            CodeList = codeList;
        }

        public void SetProperty(IMddVariable children)
        {
            if (Children != null)
            {
                Children = Children.Append(children).ToArray();
            }
            else
            {
                Children = new IMddVariable[1];
                Children[0] = children;
            }
        }

        public void SetProperty(VariableType variableType)
        {
            VariableType = variableType;
        }

        public void SetProperty(ValueType valueType)
        {
            ValueType = valueType;
        }

        public void SetProperty(Property property)
        {
            if (Properties is null)
                Properties = new Property[1] { property };
            else
                Properties = Properties.Append(property).ToArray();
        }

        public void SetProperty(Property[] properties)
        {
            Properties = properties;
        }

        public void SetProperty(ValueRange range)
        {
            Range = range;
        }

        public void SetLanguage(string language)
        {
            Language = language;
            if (string.IsNullOrEmpty(_languages[0]))
                _languages[0] = language;
            else
                if (!_languages.Contains(language))
                    _languages = _languages.Append(language).ToArray();
            if (CodeList != null)
                CodeList.SetLanguage(language);
            if (Children != null && Children.Length > 0)
                foreach (var child in Children)
                    child.SetLanguage(language);
        }

        public void SetLabels(string label, string language)
        {
            if (_labels is null) _labels = new string[1];
            if (_languages is null) _languages = new string[1];
            //
            if (_languages.Contains(language))
            {
                int index = -1;
                for (int i = 0; i < _languages.Length; i++)
                {
                    if (_languages[i] == language)
                    {
                        index = i;
                        break;
                    }
                }
                if (index > -1) _labels[index] = label;
            }
            else
            {
                if (string.IsNullOrEmpty(_languages[_languages.Length - 1]))
                {
                    _languages[_languages.Length - 1] = language;
                    _labels[_labels.Length - 1] = label;
                }
                else
                {
                    _languages = _languages.Append(language).ToArray();
                    _labels = _labels.Append(label).ToArray();
                }
            }
        }

        public void SetParent(IMddVariable parent)
        {
            Parent = parent;
        }

        public void SetLabels(string[] labels, string[] languages)
        {
            _languages = languages;
            _labels = labels;
        }

        public void SetAssignment(string assignment)
        {
            Assignment = assignment;
        }

        public string[] GetLabels()
        {
            return _labels;
        }

        public string[] GetLanguages()
        {
            return _languages;
        }

        public IMddVariable Copy()
        {
            IMddVariable copyVariable = new MddVariable();
            copyVariable.SetProperty(CodeList.Copy());
            copyVariable.SetProperty(ListId);
            copyVariable.SetProperty(Name, Id);
            copyVariable.SetLanguage(Language);
            if (HasChildren)
            {
                IMddVariable[] copyChildren = new IMddVariable[Children.Length];
                Children.CopyTo(copyChildren, 0);
                for (int i = 0; i < copyChildren.Length; i++)
                    copyVariable.SetProperty(copyChildren[i]);
            }
            copyVariable.SetLabels(_labels, _languages);
            copyVariable.SetProperty(VariableType);
            copyVariable.SetProperty(ValueType);
            copyVariable.SetProperty(Range);
            copyVariable.SetProperty(Properties);
            return copyVariable;
        }

        public ValueRange Range { get; private set; }

    }
}
