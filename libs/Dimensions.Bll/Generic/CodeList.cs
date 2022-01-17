using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Dimensions.Bll.String;

namespace Dimensions.Bll.Generic
{
    [DebuggerDisplay("Count = {Count}")]
    public class CodeList : ICodeList, IEnumerable<ICategorical>
    {
        public CodeList()
        {
            _codes = new ICategorical[1];
            _codes[0] = null;
            _version = 0;
            _languages = new string[1];
        }

        public ICategorical this[int index]
        {
            get
            {
                if (_codes != null && index < _codes.Length)
                    return _codes[index];
                else
                    return null;
            }
        }

        private ICategorical[] _codes;
        private int _version;
        private string[] _languages;

        public int Count
        {
            get
            {
                if (_codes[0] != null)
                    return _codes.Length;
                else
                    return 0;
            }
        }

        public string Language { get; private set; }

        public void Add(ICategorical categorical)
        {
            if (_codes[0] == null)
                _codes[0] = categorical;
            else
                if (!Contains(categorical.Name))
                    _codes = _codes.Append(categorical).ToArray();
            _version++;
        }

        public void SetLanguage(string language)
        {
            Language = language;
            if (string.IsNullOrEmpty(_languages[0]))
                _languages[0] = language;
            else
                if (!_languages.Contains(language))
                    _languages = _languages.Append(language).ToArray();
            for (int i = 0; i < _codes.Length; i++)
                if (_codes[i] != null)
                    _codes[i].SetLabel(language);
        }

        public ICodeList Copy()
        {
            ICodeList copyList = new CodeList();
            if (Count > 0)
                for (int i = 0; i < _codes.Length; i++)
                    copyList.Add(new Categorical(_codes[i].Name, _codes[i].Label));
            return copyList;
        }

        public struct Enumerator : IEnumerator<ICategorical>
        {
            private readonly CodeList _categoricals;
            private int index;
            private readonly int version;

            internal Enumerator(CodeList categoricals)
            {
                _categoricals = categoricals;
                index = 0;
                version = categoricals._version;
                Current = default;
            }

            public ICategorical Current { get; private set; }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                CodeList localList = _categoricals;
                if (version == localList._version && index < localList.Count)
                {
                    Current = _categoricals[index];
                    index++;
                    return true;
                }
                index = _categoricals.Count + 1;
                Current = default;
                return false;
            }

            public void Reset()
            {
                index = 0;
                Current = default;
            }
        }

        public bool Formated { get; private set; } = false;

        public void FormatCodeLabels()
        {
            if (Formated) return;
            if (_codes is null || _codes.Length == 0) return;
            string first = _codes[0].Label;
            if (string.IsNullOrEmpty(first) || (!first.Contains(" - ") && !first.Contains(": "))) return;
            LabelType labelType;
            if (first.Contains(" - ") && !first.Contains(": ")) 
                labelType = LabelType.EnDash;
            else if (first.Contains(": ") && !first.Contains(" - ")) 
                labelType = LabelType.Colon;
            else 
                labelType = LabelType.Both;
            List<string[]> splitLabel = new List<string[]>();
            int count = 0;
            switch (labelType)
            {
                case LabelType.Colon:
                    splitLabel.Clear();
                    count = Regex.Split(first, ": ").Length;
                    for (int i = 0; i < count; i++)
                    {
                        splitLabel.Add(new string[1]);
                    }
                    for (int i = 0; i < _codes.Length; i++)
                    {
                        string[] split = Regex.Split(_codes[i].Label, ": ");
                        for (int j = 0; j < split.Length; j++)
                        {
                            if (j >= count) break;
                            if (string.IsNullOrEmpty(splitLabel[j][0]))
                                splitLabel[j][0] = split[j];
                            else
                                splitLabel[j] = splitLabel[j].Append(split[j]).ToArray();
                        }
                    }
                    break;
                case LabelType.EnDash:
                    splitLabel.Clear();
                    count = Regex.Split(first, " - ").Length;
                    for (int i = 0; i < count; i++)
                    {
                        splitLabel.Add(new string[1]);
                    }
                    for (int i = 0; i < _codes.Length; i++)
                    {
                        string[] split = Regex.Split(_codes[i].Label, " - ");
                        for (int j = 0; j < split.Length; j++)
                        {
                            if (j >= count) break;
                            if (string.IsNullOrEmpty(splitLabel[j][0]))
                                splitLabel[j][0] = split[j];
                            else
                                splitLabel[j] = splitLabel[j].Append(split[j]).ToArray();
                        }
                    }
                    break;
                case LabelType.Both:
                    splitLabel.Clear();
                    count = 0;
                    string[] colonArray = Regex.Split(first, ": ");
                    for (int i = 0; i < colonArray.Length; i++)
                    {
                        if (!colonArray[i].Contains(" - "))
                            count++;
                        else
                            count += Regex.Split(colonArray[i], " - ").Length;
                    }
                    if (count == 0) break;
                    for (int i = 0; i < count; i++)
                    {
                        splitLabel.Add(new string[1]);
                    }
                    for (int i = 0; i < _codes.Length; i++)
                    {
                        string[] split = Regex.Split(_codes[i].Label, ": ");
                        int index = 0;
                        for (int j = 0; j < split.Length; j++)
                        {
                            if (index >= count) break;
                            if (!split[j].Contains(" - "))
                            {
                                if (string.IsNullOrEmpty(splitLabel[index][0]))
                                    splitLabel[index][0] = split[j];
                                else
                                    splitLabel[index] = splitLabel[index].Append(split[j]).ToArray();
                                index++;
                            }
                            else
                            {
                                string[] subSplit = Regex.Split(split[j], " - ");
                                for (int k = 0; k < subSplit.Length; k++)
                                {
                                    if (string.IsNullOrEmpty(splitLabel[index][0]))
                                        splitLabel[index][0] = subSplit[k];
                                    else
                                        splitLabel[index] = splitLabel[index].Append(subSplit[k]).ToArray();
                                    index++;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            if (count == 0) return;
            List<string[]> resultArray = new List<string[]>();
            foreach (var part in splitLabel)
            {
                if (!StringArrayFunction.StringArrayHasSimilarContent(part))
                {
                    resultArray.Add(part);
                }
            }
            if (resultArray.Count == 0) return;
            if (resultArray.Count == 1)
            {
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (i < resultArray[0].Length)
                        _codes[i].ResetLabel(_codes[i].Label, resultArray[0][i]);
                }
            }
            else
            {
                int resultIndex = 0;
                int max = StringArrayFunction.MaxLengthOfStringArray(resultArray[0]);
                for (int i = 1; i < resultArray.Count; i++)
                {
                    int tmpMax = StringArrayFunction.MaxLengthOfStringArray(resultArray[i]);
                    if (tmpMax > max)
                    {
                        max = tmpMax;
                        resultIndex = i;
                    }
                }
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (i < resultArray[resultIndex].Length)
                        _codes[i].ResetLabel(_codes[i].Label, resultArray[resultIndex][i]);
                }
            }
            Formated = true;
        }

        private enum LabelType
        {
            Colon,
            EnDash,
            Both
        }

        public string GetCodes(string start, string end)
        {
            string cats = string.Empty;
            if (!Contains(start) || !Contains(end)) return cats;
            bool hasBegan = false;
            bool hasEnded = false;
            Sort();
            foreach (var cat in _codes)
            {
                if (cat.Name.Substring(CommonPart.Length) == start && !hasBegan)
                {
                    cats += cat.Name.Substring(CommonPart.Length);
                    hasBegan = true;
                }
                if (!hasEnded && hasBegan)
                {
                    cats += "/" + cat.Name.Substring(CommonPart.Length);
                }
                if (cat.Name.Substring(CommonPart.Length) == end && !hasEnded)
                {
                    cats += "/" + cat.Name.Substring(CommonPart.Length);
                    hasEnded = true;
                }
            }
            return cats;
        }

        public bool IsContinuousCodes()
        {
            if (_codes != null)
            {
                bool result = false;
                //
                int[] _intCodeList = new int[1];
                _intCodeList[0] = int.MaxValue;
                //
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (!Regex.IsMatch(_codes[i].Name, @"[0-9]+") || (Regex.Match(_codes[i].Name, @"[0-9]+").Success && Regex.Matches(_codes[i].Name, @"[0-9]+").Count > 1))
                    {
                        continue;
                    }
                    int _intCode = int.Parse(Regex.Match(_codes[i].Name, @"[0-9]+").Value);
                    if (_intCodeList[0] == int.MaxValue)
                    {
                        _intCodeList[0] = _intCode;
                    }
                    else
                    {
                        _intCodeList = _intCodeList.Append(_intCode).ToArray();
                    }
                }
                if (_intCodeList.Length > 1)
                {
                    Array.Sort(_intCodeList);
                    for (int i = 1; i < _intCodeList.Length; i++)
                    {
                        if (Math.Abs(_intCodeList[i] - _intCodeList[i - 1]) != 1)
                        {
                            result = false;
                            break;
                        }
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool IsContinuousFactor(out int factorStep, out string exclude, string[] highFactorLabel = null)
        {
            factorStep = 0;
            exclude = string.Empty;
            //
            if (_codes != null && _codes.Length > 0 && _codes[0] != null)
            {
                bool result = false;
                //
                int[] _intCodes = new int[1];
                _intCodes[0] = int.MaxValue;
                int nonIntCode = 0;
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (!Regex.IsMatch(_codes[i].Label.Replace("?", "-").Replace("X", "-").Replace(",", ""), @"([0-9]+)|(-[0-9]+)"))
                    {
                        nonIntCode++;
                        continue;
                    }
                    int _intCodeLabel = int.Parse(Regex.Match(_codes[i].Label.Replace("?", "-").Replace("X", "-").Replace(",", ""), @"([0-9]+)|(-[0-9]+)").Value);
                    if (_intCodes[0] == int.MaxValue)
                    {
                        _intCodes[0] = _intCodeLabel;
                    }
                    else
                    {
                        _intCodes = _intCodes.Append(_intCodeLabel).ToArray();
                    }
                }
                if (nonIntCode > 2)
                {
                    result = false;
                }
                //
                if (_intCodes.Length > 1)
                {
                    int step = 0;
                    for (int i = 1; i < _intCodes.Length; i++)
                    {
                        if (step == 0)
                        {
                            step = _intCodes[i] - _intCodes[i - 1];
                        }
                        else
                        {
                            if (step != (_intCodes[i] - _intCodes[i - 1]))
                            {
                                step = 0;
                                break;
                            }
                        }
                    }

                    if (step == 0)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                        if (step > 0)
                            factorStep = 1;
                        else
                            factorStep = -1;
                    }
                }
                //
                if (!result && highFactorLabel != null)
                {
                    if (StringArrayFunction.StringHasKey(_codes[0].Label, highFactorLabel))
                    {
                        result = true;
                        factorStep = -1;
                    }
                    else
                    {
                        int index = _codes.Length - 1;
                        while (index > 0)
                        {
                            if (StringArrayFunction.StringHasKey(_codes[index].Label, highFactorLabel))
                            {
                                result = true;
                                factorStep = 1;
                                break;
                            }
                            else
                            {
                                exclude += _codes[index].Name + ",";
                            }
                            index--;
                        }
                        if (exclude.EndsWith(","))
                        {
                            exclude = exclude.Substring(exclude.Length - 1);
                        }
                    }
                }
                //
                return result;
            }
            else
            {
                return false;
            }
        }
        public bool IsSplitFactor(out Dictionary<string, double> factors)
        {
            factors = new Dictionary<string, double>();
            bool result = false;
            int hasFactorCount = 0;
            //
            if (_codes != null && _codes.Length > 1)
            {
                for (int i = 0; i < _codes.Length; i++)
                {
                    string label = StringFunction.FormatString(_codes[i].Label).Replace(",", "");
                    if (Regex.IsMatch(label, @"[0-9]+"))
                    {
                        MatchCollection matches = Regex.Matches(label, @"[0-9.]+");
                        if (matches.Count == 1)
                        {
                            string factor = matches[0].Value;
                            while (factor.EndsWith(".")) factor = factor.Substring(factor.Length - 1);
                            if (factor == "." || string.IsNullOrEmpty(factor)) continue;
                            factors.Add(_codes[i].Name, double.Parse(factor));
                            hasFactorCount++;
                        }
                        if (matches.Count == 2)
                        {
                            string factor1 = matches[0].Value;
                            string factor2 = matches[1].Value;
                            //
                            while (factor1.EndsWith(".")) factor1 = factor1.Substring(factor1.Length - 1);
                            while (factor2.EndsWith(".")) factor2 = factor2.Substring(factor2.Length - 1);
                            if (factor1 == "." || factor2 == "." || string.IsNullOrEmpty(factor1) || string.IsNullOrEmpty(factor2)) continue;
                            //
                            factors.Add(_codes[i].Name, (double.Parse(factor2) + double.Parse(factor1)) / 2);
                            hasFactorCount++;
                        }
                    }
                }
                if (_codes.Length - hasFactorCount > 2)
                {
                    return false;
                }
                if (factors.Count > 1)
                {
                    result = true;
                }
            }
            //
            return result;
        }
        /// <summary>
        /// 判断是否包含指定码号
        /// </summary>
        /// <param name="name">码号名，不区分大小写</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            bool result = false;
            if (_codes != null && _codes.Length > 0 && _codes[0] != null)
            {
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (_codes[i].Name.ToLower() == name.ToLower())
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 针对码号数字部分进行正序排序
        /// </summary>
        public void Sort()
        {
            if (_codes is null || _codes[0] is null || _codes.Length == 0) return;
            List<ICategorical> nonIntCodes = new List<ICategorical>();
            Dictionary<int, ICategorical> hasIntCodes = new Dictionary<int, ICategorical>();
            for (int i = 0; i < _codes.Length; i++)
            {
                if (!string.IsNullOrEmpty(GetIntFromLeft(_codes[i].Name)))
                    hasIntCodes.Add(int.Parse(GetIntFromLeft(_codes[i].Name)), _codes[i]);
                else
                    nonIntCodes.Add(_codes[i]);
            }
            if (hasIntCodes.Count > 1)
            {
                List<int> keys = hasIntCodes.Keys.ToList();
                keys.Sort();
                ICategorical[] sortedList = new ICategorical[keys.Count];
                for (int i = 0; i < keys.Count; i++)
                {
                    sortedList[i] = hasIntCodes[keys[i]];
                }
                _codes = sortedList;
                if (nonIntCodes.Count > 0)
                {
                    for (int i = 0; i < nonIntCodes.Count; i++)
                    {
                        _codes = _codes.Append(nonIntCodes[i]).ToArray();
                    }
                }
            }
        }

        private string GetIntFromLeft(string code)
        {
            string result = string.Empty;
            bool hasInt = false;
            for (int i = 0; i < code.Length; i++)
            {
                if (!hasInt && !Regex.IsMatch(code.Substring(i, 1), @"[0-9]+")) continue;
                if (Regex.IsMatch(code.Substring(i, 1), @"[0-9]+"))
                {
                    result += code.Substring(i, 1);
                    hasInt = true;
                }
                if (hasInt && !Regex.IsMatch(code.Substring(i, 1), @"[0-9]+")) break;
            }
            return result;
        }
        /// <summary>
        /// 移除单个Categorical类型变量
        /// </summary>
        /// <param name="categorical">需要移除的Categorical类型变量</param>
        public void Remove(ICategorical categorical)
        {
            int index = -1;
            if (_codes != null)
            {
                for (int i = 0; i < _codes.Length; i++)
                {
                    if (categorical.Name == _codes[i].Name)
                    {
                        index = i;
                        break;
                    }
                }
                if (index > -1)
                {
                    if (_codes.Length == 1)
                    {
                        _codes[0] = null;
                    }
                    else
                    {
                        ICategorical[] _newCodes = new ICategorical[_codes.Length - 1];
                        int count = 0;
                        for (int i = 0; i < _codes.Length; i++)
                        {
                            if (i == index)
                                continue;
                            _newCodes[count] = _codes[i];
                            count++;
                        }
                        _codes = _newCodes;
                    }
                }
            }
        }

        public string CommonPart
        {
            get
            {
                if (_codes != null && _codes.Length > 1)
                {
                    string _common = _codes[0].Name;
                    for (int i = 1; i < _codes.Length; i++)
                    {
                        int length = _common.Length;
                        string tempCommon = string.Empty;
                        if (_codes[i].Name.Length < length) length = _codes[i].Name.Length;
                        for (int j = 0; j < length; j++)
                        {
                            if (_codes[i].Name.Substring(j, 1) == _common.Substring(j, 1))
                                tempCommon += _common.Substring(j, 1);
                            else
                            {
                                _common = tempCommon;
                                break;
                            }
                        }
                    }
                    if (_common == _codes[0].Name)
                    {
                        while (_common.Length > 0 && Regex.IsMatch(_common.Substring(_common.Length - 1), @"[0-9]+"))
                        {
                            _common = _common.Substring(0, _common.Length - 1);
                        }
                    }
                    return _common;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsMergedLoop(string codeLabel, out ICodeList.MergedLoopDefinition definition)
        {
            definition = new ICodeList.MergedLoopDefinition();
            bool result = false;
            //
            if (_codes != null && _codes.Length > 1)
            {
                result = true;
                if (!string.IsNullOrEmpty(CommonPart))
                {
                    string pattern = CommonPart + @"[0-9]+[a-zA-Z_]+[0-9]+";
                    for (int i = 0; i < _codes.Length; i++)
                    {
                        if (!Regex.IsMatch(_codes[i].Name, pattern))
                        {
                            result = false;
                            break;
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }
            //
            if (result)
            {
                ICodeList part1 = new CodeList();
                ICodeList part2 = new CodeList();
                //
                ICategorical baseCode = _codes[0];
                //
                string checkString = StringFunction.GetCommonStringFromLeft(baseCode.Name, _codes[1].Name);
                string startLabel = checkString;

                int lengthPart1 = 0;
                int lengthPart2 = 0;

                int diffIndex1 = 1;
                int diffIndex2 = 1;
                //
                for (int i = 1; i < _codes.Length; i++)
                {
                    string commonString = StringFunction.GetCommonStringFromLeft(baseCode.Name, _codes[i].Name);
                    // 如果相同字段长度小于初始长度，startLabel更新为最新值
                    if (commonString.Length < startLabel.Length) startLabel = commonString;
                    // 如果从左开始的字符串相同，视为同一级码号
                    if (commonString == checkString)
                    {
                        // 分别获取码号和标签不同的部分
                        string[] diffCode = StringFunction.GetCodeDifferentPart(baseCode.Name, _codes[i].Name, out diffIndex1);
                        string[] diffLabel = StringFunction.GetLabelDifferentPart(baseCode.Label, _codes[i].Label);
                        // 如果没有字典中没有基准码号，则加入
                        if (!part1.Contains(diffCode[0]) && diffCode[0].Length > 0)
                        {
                            part1.Add(new Categorical(codeLabel + diffCode[0], diffLabel[0]));
                            // 记录字符最大长度
                            if (diffLabel[0].Length > lengthPart1) lengthPart1 = diffLabel[0].Length;
                        }
                        if (!part1.Contains(diffCode[1]) && diffCode[1].Length > 0)
                        {
                            // 加入新的码号
                            part1.Add(new Categorical(codeLabel + diffCode[1], diffLabel[1]));
                            // 记录最大字符长度
                            if (diffLabel[1].Length > lengthPart1) lengthPart1 = diffLabel[1].Length;
                        }
                    }
                    // 如果相同字符串不同, 视为另一级码号
                    else
                    {
                        break;
                    }
                }
                part1.Sort();
                //
                int l1 = part1.Count;
                if (l1 <= 0) return result;
                for (int i = l1; i < _codes.Length; i += l1)
                {
                    string commonString = StringFunction.GetCommonStringFromLeft(baseCode.Name, _codes[i].Name);
                    if (commonString.Length < startLabel.Length) startLabel = commonString;
                    // 分别获取码号和标签不同的部分
                    string[] diffCode = StringFunction.GetCodeDifferentPart(baseCode.Name, _codes[i].Name, out diffIndex2);
                    string[] diffLabel = StringFunction.GetLabelDifferentPart(baseCode.Label, _codes[i].Label);
                    //
                    if (!part2.Contains(diffCode[0]) && diffCode[0].Length > 0)
                    {
                        part2.Add(new Categorical(codeLabel + diffCode[0], diffLabel[0]));
                        if (diffLabel[0].Length > lengthPart2) lengthPart2 = diffLabel[0].Length;
                    }
                    if (!part2.Contains(diffCode[1]) && diffCode[0].Length > 0)
                    {
                        part2.Add(new Categorical(codeLabel + diffCode[1], diffLabel[1]));
                        if (diffLabel[1].Length > lengthPart2) lengthPart2 = diffLabel[1].Length;
                    }
                }
                part2.Sort();
                //
                definition.StartLabel = startLabel;
                string checkCode = baseCode.Name.Remove(0, startLabel.Length);
                string endLabel = Regex.Match(checkCode, @"[^0-9]+").Success ? Regex.Match(checkCode, @"[^0-9]+").Value : string.Empty;
                definition.InnerLabel = endLabel;
                if (lengthPart1 > lengthPart2 || lengthPart2 == 0)
                {
                    definition.TopList = part2;
                    definition.SideList = part1;
                    if (diffIndex1 == 2 && diffIndex2 == 1) definition.Type = ICodeList.MergedLoopDefinitionType.TopFirst;
                    else definition.Type = ICodeList.MergedLoopDefinitionType.SideFirst;
                }
                else
                {
                    definition.TopList = part1;
                    definition.SideList = part2;
                    if (diffIndex1 == 2 && diffIndex2 == 1) definition.Type = ICodeList.MergedLoopDefinitionType.SideFirst;
                    else definition.Type = ICodeList.MergedLoopDefinitionType.TopFirst;
                }
            }
            //
            return result;
        }

        public bool GetCodeNameFromNumber(string number, out string code)
        {
            code = string.Empty;
            bool result = false;
            foreach (var cat in _codes)
            {
                string num = StringFunction.GetNumberFromRight(cat.Name);
                if (!string.IsNullOrEmpty(num) && number == num)
                {
                    result = true;
                    code = cat.Name;
                    break;
                }
            }
            return result;
        }

        public IEnumerator<ICategorical> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

    }
}
