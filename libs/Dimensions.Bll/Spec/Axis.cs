
using System;
using System.Diagnostics;
using System.Linq;

namespace Dimensions.Bll.Spec
{
    [DebuggerDisplay("Value = {Value}")]
    public class Axis
    {
        public Axis()
        {
        }

        public AxisItem this[int index]
        {
            get
            {
                if (Items is null || index >= Items.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return Items[index];
            }
        }

        public AxisItem[] this[AxisElement type]
        {
            get
            {
                if (Items is null)
                    return null;
                AxisItem[] result = new AxisItem[1];
                int count = 0;
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].Type == type)
                    {
                        if (count == 0)
                            result[0] = Items[i];
                        else
                            result = result.Append(Items[i]).ToArray();
                        count++;
                    }
                }
                if (count == 0)
                    return null;
                else
                    return result;
            }
        }

        private bool _firstBase = false;
        private int _text_version = 0;
        private int _base_version = 0;
        private int _dev_version = 0;

        public AxisItem[] Items { get; private set; }

        public bool Empty
        {
            get
            {
                if (Items is null || Items.Length == 0)
                    return true;
                return false;
            }
        }

        public void Add(AxisItem element)
        {
            if (Items is null)
            {
                Items = new AxisItem[1];
                Items[0] = element;
            }
            else
            {
                Items = Items.Append(element).ToArray();
            }
        }

        public void Add(AxisElement type, params string[] values)
        {
            switch (type)
            {
                case AxisElement.Text:
                    string _txt = string.Empty;
                    if (values != null && values.Length >=1 && values[0] != null)
                        _txt = values[0];
                    Add(new AxisItem(AxisElement.Text, $"e{++_text_version} '{_txt}' text()"));
                    break;
                case AxisElement.Base:
                    string _baseTxt = "Total Respondents";
                    string _baseFlt = string.Empty;
                    if (values != null)
                    {
                        if (values.Length >= 1 && values[0] != null)
                            _baseTxt = values[0];
                        if (values.Length >= 2 && values[1] != null)
                            _baseFlt = $"'{values[1]}'";
                    }
                    if (!_firstBase)
                    {
                        Add(new AxisItem(AxisElement.Base, $"Base 'Base : {_baseTxt}' base({_baseFlt})"));
                        _firstBase = true;
                    }
                    else
                    {
                        Add(new AxisItem(AxisElement.Base, $"bs_{++_base_version} 'Base : {_baseTxt}' base({_baseFlt})"));
                    }
                    break;
                case AxisElement.MainSide:
                    string _side = "..";
                    if (values != null && values.Length >= 1 && values[0] != null)
                        _side = values[0];
                    Add(new AxisItem(AxisElement.MainSide, _side));
                    break;
                case AxisElement.Sigma:
                    Add(new AxisItem(AxisElement.Sigma, "sigma 'Sigma' subtotal()"));
                    break;
                case AxisElement.Derived:
                    string _flt = string.Empty;
                    string _label = string.Empty;
                    if (values != null && values.Length >= 1 && values[0] != null)
                        _label = values[0];
                    if (values != null && values.Length >= 2 && values[1] != null)
                        _flt = values[1];
                    Add(new AxisItem(AxisElement.Derived, $"dev{++_dev_version} '{_label}' derived({_flt})"));
                    break;
                case AxisElement.Mean:
                    string _meanVar = string.Empty;
                    if (values != null && values.Length >= 1 && values[0] != null)
                        _meanVar = values[0];
                    Add(new AxisItem(AxisElement.Mean, $"\" + mean_inc(\"{_meanVar}\") + \""));
                    break;
                default:
                    break;
            }
        }

        public string Value
        {
            get
            {
                string result = string.Empty;
                if (Items is null)
                {
                    return string.Empty;
                }
                result += "{";
                bool hasNet = false;
                for (int i = 0; i < Items.Length; i++)
                {
                    if (i == 0 || Items[i].Type == AxisElement.Mean)
                        result += Items[i].Value;
                    else if (Items[i].Type == AxisElement.MainSide)
                    {
                        if (Items[i].Value != "..")
                            hasNet = true;
                        result += "," + Items[i].Value;
                    }
                    else
                    {
                        if (hasNet)
                        {
                            result += Items[i].Value;
                            hasNet = false;
                        }
                        else
                        {
                            result += "," + Items[i].Value;
                        }
                    }
                }
                result += "}";
                return result;
            }
        }
        [DebuggerDisplay("Value = {Value}")]
        public struct AxisItem
        {
            public AxisItem(AxisElement type, string value)
            {
                Value = value;
                Type = type;
            }
            public string Value { get; set; }
            public AxisElement Type { get; set; }

        }
    }
}
