
using Dimensions.Bll.Generic;
using Dimensions.Bll.Mdd;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Dimensions.Bll.Spec
{
    [DebuggerDisplay("Name = {Name}")]
    public class SpecItem : ISpecItem
    {
        public SpecItem()
        {
            Base = new ISpecItem.BaseItem[1];
        }

        public SpecItem(string[] highFactorKeys)
        {
            Base = new ISpecItem.BaseItem[1];
            HighFactorKeys = highFactorKeys;
        }
        /// <summary>
        /// Spec中的变量名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Spec中变量标题
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Base内容
        /// </summary>
        public ISpecItem.BaseItem[] Base { get; private set; }
        /// <summary>
        /// 是否包含Net
        /// </summary>
        public bool HasNet
        {
            get
            {
                if (Nets != null && Nets.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Net内容
        /// </summary>
        public NetContent Nets { get; private set; }
        /// <summary>
        /// Top Box / Bottom Box
        /// </summary>
        public string TopBottom { get; private set; }
        /// <summary>
        /// Mdd对象
        /// </summary>
        public IMddDocument Mdd { get; private set; }

        public Axis Axis
        {
            get
            {
                Axis _axis = new Axis();
                IMddVariable variable = Variable;
                bool _isLoop = false;
                bool _hasTotalBase = false;
                if (variable != null && variable.VariableType == VariableType.Loop && variable.HasChildren)
                {
                    variable = variable.Children[0];
                    _isLoop = true;
                }
                if (variable != null)
                {
                    string _side = "..";
                    _axis.Add(AxisElement.Text);
                    if (Nets != null && Nets.Count > 0)
                    {
                        string netStr = Nets.GetNetString(out string remain);
                        if (!string.IsNullOrEmpty(remain))
                            _side = $"_\n{netStr}    {remain}_\n    ";
                        else
                            _side = $"_\n{netStr}    ";
                    }
                    if (!string.IsNullOrEmpty(TopBottom))
                    {
                        _side = $"{GetTopBottomString(HighFactorKeys)},";
                    }
                    string _base = "Total Respondents";
                    if (Base != null && Base.Length > 0 && !string.IsNullOrEmpty(Base[0].Value))
                    {
                        int _totalIndex = 0;
                        _base = Base[0].Value;
                        for (int i = 0; i < Base.Length; i++)
                        {
                            if (Base[i].Value.ToLower().Contains("total") || Base[i].Value.ToLower().Contains("all"))
                            {
                                _totalIndex = i;
                                _hasTotalBase = true;
                                break;
                            }
                        }
                        if (_hasTotalBase && !_isLoop)
                        {
                            _base = Base[_totalIndex].Value;
                        }
                    }
                    if (_hasTotalBase && !_isLoop)
                        _axis.Add(AxisElement.Base, _base, "True");
                    else
                        _axis.Add(AxisElement.Base, _base);
                    _axis.Add(AxisElement.Text);
                    _axis.Add(AxisElement.MainSide, _side);
                    _axis.Add(AxisElement.Text);
                    _axis.Add(AxisElement.Sigma);
                    if (Mean)
                    {
                        if (variable.ValueType == ValueType.Long || variable.ValueType == ValueType.Double)
                        {
                            _axis.Add(AxisElement.Mean, variable.Name);
                        }
                        else
                        {
                            if (variable.CodeList != null &&
                                (variable.CodeList.IsContinuousFactor(out _, out _, HighFactorKeys) ||
                                variable.CodeList.IsSplitFactor(out _)))
                                _axis.Add(AxisElement.Mean);
                        }
                    }
                }
                return _axis;
            }
        }

        public bool IsSkipped
        {
            get
            {
                if (Variable is null && string.IsNullOrEmpty(Notes) && (NewVariableDefinition is null || NewVariableDefinition.Empty))
                    return true;
                else
                    return false;
            }
        }

        public IMddVariable Variable { get; private set; }

        public string Notes { get; private set; }

        public bool Mean { get; private set; }
        
        public NewVariableDefinition NewVariableDefinition { get; private set; }

        public string[] HighFactorKeys { get; private set; }

        public void AddNet(string key, string value)
        {
            if (Nets is null)
            {
                Nets = new NetContent();
            }
            if (!Nets.Contains(key))
            {
                Nets.Add(key, value);
            }
            Nets.SetVariable(Variable);
        }

        public void SetProperty(string name, string title)
        {
            Name = name;
            Title = title;
        }

        public void SetProperty(ISpecItem.BaseItem[] baseText)
        {
            Base = baseText;
        }

        public void SetProperty(NetContent nets)
        {
            Nets = nets;
            Nets.SetVariable(Variable);
        }

        public void SetProperty(string topbottom)
        {
            TopBottom = topbottom;
        }

        public void SetProperty(IMddVariable variable)
        {
            Variable = variable;
        }

        public void SetProperty(bool mean)
        {
            Mean = mean;
        }

        public void SetProperty(NewVariableDefinition newVariable)
        {
            NewVariableDefinition = newVariable;
        }

        public void SetNotes(string note)
        {
            Notes = note;
        }

        public void SetProperty(IMddDocument mdd)
        {
            Mdd = mdd;
        }

        public string GetTopBottomString(string[] highFactorKeys)
        {
            string tbStr = string.Empty;
            if (Variable is null)
                return string.Empty;
            ICodeList codes = Variable.CodeList;
            if (Variable.UseList) codes = Mdd.ListFields.Get(Variable.ListId).CodeList;
            if (Variable.VariableType == VariableType.Loop && Variable.HasChildren)
            {
                if (Variable.Children.Length == 1 &&
                    Variable.Children[0].VariableType == VariableType.Normal &&
                    Variable.Children[0].ValueType == ValueType.Categorical &&
                    Variable.Children[0].CodeList != null &&
                    Variable.Children[0].CodeList.Count > 0)
                {
                    if (!Variable.Children[0].UseList) codes = Variable.Children[0].CodeList;
                    else codes = Mdd.ListFields.Get(Variable.Children[0].ListId).CodeList;
                }
            }

            if (codes.IsContinuousFactor(out int step, out _, highFactorKeys))
            {
                // 移除异常码号，如：98、99、NA
                List<string> remove = new List<string>();
                foreach (var code in codes)
                {
                    if (!Regex.IsMatch(code.Name, @"[0-9]+") ||
                        (Regex.IsMatch(code.Name, @"[0-9]+") && int.Parse(Regex.Match(code.Name, @"[0-9]+").Value) > 20))
                        remove.Add(code.Name);
                }
                //
                List<string> exist = new List<string>();
                // top box
                if ((TopBottom.ToLower().Contains("tb") || TopBottom.ToLower().Contains("topbox")) &&
                    codes.Count > 2 && !exist.Contains("tb"))
                {
                    tbStr += "    tb    'Net.Top Box'          combine({";
                    if (step == -1)
                    {
                        tbStr += $"{codes[0].Name}}}),_\n";
                    }
                    else
                    {
                        for (int i = codes.Count - 1; i >= 0; i--)
                        {
                            if (!remove.Contains(codes[i].Name))
                            {
                                tbStr += $"{codes[i].Name}}}),_\n";
                                break;
                            }
                        }
                    }
                    exist.Add("tb");
                }
                // top 2 box
                if ((TopBottom.ToLower().Contains("t2b") || TopBottom.ToLower().Contains("top2")) &&
                    codes.Count > 2 && !exist.Contains("t2b"))
                {
                    tbStr += "    t2b    'Net.Top 2 Box'       combine({";
                    int count = 2;
                    int index = codes.Count - 1;
                    if (step == -1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = $"{tbStr.Substring(0, tbStr.Length - 1)}}}),_\n";
                    }
                    exist.Add("t2b");
                }
                // top 3 box
                if ((TopBottom.ToLower().Contains("t3b") || TopBottom.ToLower().Contains("top3")) &&
                    codes.Count > 3 && !exist.Contains("t3b"))
                {
                    tbStr += "    t3b    'Net.Top 3 Box'       combine({";
                    int count = 3;
                    int index = codes.Count - 1;
                    if (step == -1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name},{codes[2].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}),_\n";
                    }
                    exist.Add("t3b");
                }
                // top 4 box
                if ((TopBottom.ToLower().Contains("t4b") || TopBottom.ToLower().Contains("top4")) &&
                    codes.Count > 4 && !exist.Contains("t4b"))
                {
                    tbStr += "    t4b    'Net.Top 4 Box'       combine({";
                    int count = 4;
                    int index = codes.Count - 1;
                    if (step == -1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name},{codes[2].Name},{codes[3].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}),_\n";
                    }
                    exist.Add("t4b");
                }
                // bottom 2 box
                if ((TopBottom.ToLower().Contains("b2b") || TopBottom.ToLower().Contains("bottom2")) &&
                    codes.Count > 2 && !exist.Contains("b2b"))
                {
                    tbStr += "    b2b    'Net.Bottom 2 Box'    combine({";
                    int count = 2;
                    int index = codes.Count - 1;
                    if (step == 1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}),_\n";
                    }
                    exist.Add("b2b");
                }
                // bottom 3 box
                if ((TopBottom.ToLower().Contains("b3b") || TopBottom.ToLower().Contains("bottom3")) &&
                    codes.Count > 3 && !exist.Contains("b3b"))
                {
                    tbStr += "    b3b    'Net.Bottom 3 Box'    combine({";
                    int count = 3;
                    int index = codes.Count - 1;
                    if (step == 1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name},{codes[2].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}),_\n";
                    }
                    exist.Add("b3b");
                }
                // bottom 4 box
                if ((TopBottom.ToLower().Contains("b4b") || TopBottom.ToLower().Contains("bottom4")) &&
                    codes.Count > 4 && !exist.Contains("b4b"))
                {
                    tbStr += "    b4b    'Net.Bottom 4 Box'    combine({";
                    int count = 4;
                    int index = codes.Count - 1;
                    if (step == 1)
                    {
                        tbStr += $"{codes[0].Name},{codes[1].Name},{codes[2].Name},{codes[3].Name}}}),_\n";
                    }
                    else
                    {
                        while (count > 0 && index >= 0)
                        {
                            if (!remove.Contains(codes[index].Name))
                            {
                                tbStr += $"{codes[index].Name},";
                                count--;
                            }
                            index--;
                        }
                        tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}),_\n";
                    }
                    exist.Add("b4b");
                }
                // nps 默认为t2b - b7b
                if (TopBottom.ToLower().Contains("nps"))
                {
                    if (!exist.Contains("t2b"))
                    {
                        tbStr += "    t2b    'Net.Top 2 Box'       combine({";
                        int count = 2;
                        int index = codes.Count - 1;
                        if (step == -1)
                        {
                            tbStr += $"{codes[index - 1].Name},{codes[index].Name}}}),_\n";
                        }
                        else
                        {
                            while (count > 0 && index >= 0)
                            {
                                if (!remove.Contains(codes[index].Name))
                                {
                                    tbStr += $"{codes[index].Name},";
                                    count--;
                                }
                                index--;
                            }
                            tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}) [Ishidden=True],_\n";
                        }
                        exist.Add("t2b");
                    }
                    // b7b
                    if (codes.Count > 9 && !exist.Contains("b7b"))
                    {
                        tbStr += "    b7b    'Net.Bottom 7 Box'    combine({";
                        int count = 7;
                        int index = codes.Count - 1;
                        if (step == 1)
                        {
                            tbStr += $"{codes[0].Name},{codes[1].Name},{codes[2].Name},{codes[3].Name},{codes[4].Name},{codes[5].Name},{codes[6].Name}}}) [Ishidden=True],_\n";
                        }
                        else
                        {
                            while (count > 0 && index >= 0)
                            {
                                if (!remove.Contains(codes[index].Name))
                                {
                                    tbStr += $"{codes[index].Name},";
                                    count--;
                                }
                                index--;
                            }
                            tbStr = tbStr.Substring(0, tbStr.Length - 1) + "}) [Ishidden=True],_\n";
                        }
                        exist.Add("b7b");
                    }
                    // nps
                    tbStr += "    nps    'Nps(T2B - B7B)'      derived('t2b-b7b'),_\n";
                }
                tbStr += "    sigmahide subtotal() [Ishidden=True],_\n    e11 '' text(),_\n    ..";
                tbStr = "_\n" + tbStr;
            }
            if (string.IsNullOrEmpty(tbStr))
                return "..";
            return tbStr;
        }

    }
}
