using System.Diagnostics;
using System.Linq;

namespace Dimensions.Bll.Generic
{
    [DebuggerDisplay("Name = {Name}")]
    class Categorical : ICategorical
    {
        public Categorical()
        {
            _labels = new string[1];
            _languages = new string[1];
        }

        public Categorical(string name)
        {
            Name = name;
            _labels = new string[1];
            _languages = new string[1];
        }

        public Categorical(string name, string label)
        {
            Name = name;
            Label = label;
            _labels = new string[1];
            _languages = new string[1];
        }

        public string Name { get; internal set; }

        public string Label { get; internal set; }

        private string[] _labels;
        private string[] _languages;

        public void AddLabel(string label, string language)
        {
            if (string.IsNullOrEmpty(_labels[0]))
            {
                _languages[0] = language;
                _labels[0] = label;
            }
            else
            {
                if (!_languages.Contains(language))
                {
                    _languages.Append(language);
                    _labels.Append(label);
                }
                else
                {
                    int index = 0;
                    while (index < _languages.Length)
                    {
                        if (_languages[index] == language)
                            break;
                        else
                            index++;
                    }
                    if (index < _languages.Length) _labels[index] = label;
                }
            }
        }

        public void AddLabels(string[] labels, string[] languages)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = labels[i];
            }
            //
            _labels = labels;
            _languages = languages;
            //
            if (languages.Length >= 1)
            {
                Label = labels[0];
            }
        }

        public void SetLabel(string language)
        {
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
                if (index > -1)
                {
                    Label = _labels[index];
                }
            }
        }

        public void ResetLabel(string oldLabel, string newLabel)
        {
            int index = 0;
            while (index < _labels.Length)
            {
                if (_labels[index] == oldLabel)
                    break;
                else
                    index++;
            }
            if (index < _labels.Length)
            {
                _labels[index] = newLabel;
            }
            Label = newLabel;
        }

        public Property[] Properties { get; private set; } = null;

        public void SetProperties(Property property)
        {
            if (Properties is null)
            {
                Properties = new Property[1] { property };
            }
            else
            {
                Properties = Properties.Append(property).ToArray();
            }
        }

        public void SetProperties(Property[] properties)
        {
            Properties = properties;
        }
    }
}
