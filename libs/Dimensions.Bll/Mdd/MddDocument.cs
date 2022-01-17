using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using Dimensions.Bll.Generic;
using System.Diagnostics;

namespace Dimensions.Bll.Mdd
{
    [DebuggerDisplay("Count = {Fields.Count}")]
    public class MddDocument : IMddDocument
    {
        public MddDocument(string path)
        {
            _xmlMdm = XDocument.Load(path).Root.Element("{http://www.spss.com/mr/dm/metadatamodel/Arc 3/2000-02-04}metadata");
            _xmlDefinition = _xmlMdm.Element("definition");
            _xmlSystem = _xmlMdm.Element("system");
            _xmlDesign = _xmlMdm.Element("design");
            Fields = new MddVariableCollection();
            GetMddDocumentProperties();
            GetMddDocumentTemplate();
            GetAllLists();
            GetAllDefinitions();
            GetAllSystem();
            GetAllDesign();
            GetBlockFields();
            if (_languages != null && _languages.Length > 0)
            {
                Language = _languages[0];
            }
        }

        private string[] _languages;

        public event MddLoadEventHandler ReportLoadProgress;
        private void RaiseReportLoadProgress(string message, string progress)
        {
            ReportLoadProgress?.Invoke(message, progress);
        }

        //
        // Properties
        //
        public string Context { get; set; }

        public Property[] Properties { get; private set; } = null;

        public void SetProperty(Property property)
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

        public Property[] Templates { get; private set; }

        public void SetTemplates(Property property)
        {
            if (Templates is null)
            {
                Templates = new Property[1] { property };
            }
            else
            {
                Templates = Templates.Append(property).ToArray();
            }
        }

        public bool HasMultiLanguages
        {
            get
            {
                if (_languages.Length > 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string Language { get; private set; }
        public string[] Languages { get { return _languages; } }
        public IMddVariableCollection ListFields { get; private set; }
        public IMddVariableCollection Fields { get; private set; }
        public string[] NewVariables { get; private set; }
        public string[] FieldNames
        {
            get
            {
                if (Fields != null && Fields.Count > 0)
                {
                    string[] names = new string[1];
                    foreach (var field in Fields)
                    {
                        if (field.VariableType == VariableType.System && field.HasChildren)
                        {
                            foreach (var child in field.Children)
                            {
                                if (!string.IsNullOrEmpty(names[0]))
                                    names = names.Append(field.Name + "." + child.Name).ToArray();
                                else
                                    names[0] = field.Name + "." + child.Name;
                            }
                        }
                        if (field.VariableType == VariableType.Normal)
                        {
                            if (!string.IsNullOrEmpty(names[0]))
                                names = names.Append(field.Name).ToArray();
                            else
                                names[0] = field.Name;
                        }
                        if (field.VariableType == VariableType.Loop && field.HasChildren)
                        {
                            foreach (var child in field.Children)
                            {
                                if (field.UseList)
                                {
                                    ICodeList list = ListFields.Get(field.ListId).CodeList;
                                    foreach (var code in list)
                                    {
                                        if (!string.IsNullOrEmpty(names[0]))
                                            names = names.Append($"{field.Name}[{{{code.Name}}}].{child.Name}").ToArray();
                                        else
                                            names[0] = $"{field.Name}[{{{code.Name}}}].{child.Name}";
                                    }
                                }
                                if (field.CodeList != null && field.CodeList.Count > 0)
                                {
                                    foreach (var code in field.CodeList)
                                    {
                                        if (!string.IsNullOrEmpty(names[0]))
                                            names = names.Append($"{field.Name}[{{{code.Name}}}].{child.Name}").ToArray();
                                        else
                                            names[0] = $"{field.Name}[{{{code.Name}}}].{child.Name}";
                                    }
                                }
                            }
                        }
                    }
                    return names;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool HasLists
        {
            get
            {
                if (ListFields != null && ListFields.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SetLanguage(string language)
        {
            Language = language;
            foreach (var variabel in Fields)
                variabel.SetLanguage(language);
        }

        public string Record
        {
            get
            {
                string result = "Respondent.Serial";
                if (Fields != null && Fields["record"] != null) return "record";
                if (Fields != null && Fields["respid"] != null) return "respid";
                if (Fields != null && Fields["intnr"] != null) return "intnr";
                if (Fields != null && Fields["responseid"] != null) return "responseid";
                if (Fields != null && Fields["uid"] != null) return "uid";
                //if (Fields != null && Fields["realid"] != null) return "realid";
                return result;
            }
        }

        //
        // Find Functions
        //
        public IMddVariable Find(string name)
        {
            if (Fields != null && Fields.Count > 0)
            {
                IMddVariable result = null;
                foreach (var field in Fields.Data)
                {
                    if (name.ToLower() == field.Name.ToLower())
                    {
                        result = field;
                        break;
                    }
                    if (field.VariableType == VariableType.Loop && field.HasChildren)
                    {
                        for (int i = 0; i < field.Children.Length; i++)
                        {
                            if (name.ToLower() == field.Children[i].Name.ToLower())
                            {
                                result = field.Children[i];
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }


        public IMddVariable FindWithRegex(string varName)
        {
            string name = varName.Trim().ToLower();
            bool hasBaseStr = false;
            string pattern = @"";
            string tempStr = string.Empty;
            for (int i = 0; i < name.Length; i++)
            {
                string subStr = name.ToLower().Substring(i, 1);
                if (!hasBaseStr)
                {
                    if (subStr == " " || subStr == "-" || subStr == "_")
                        hasBaseStr = true;
                    else
                        pattern += subStr;
                }
                else
                {
                    if (subStr != " " && subStr != "-" && subStr != "_")
                        tempStr += subStr;
                    else
                    {
                        pattern += "[a-zA-Z_]*" + tempStr;
                        tempStr = string.Empty;
                    }
                }
                if (i == name.Length - 1)
                    pattern += "[a-zA-Z_]*" + tempStr;
            }
            //
            if (Fields != null && Fields.Count > 0)
            {
                IMddVariable result = null;
                foreach (var field in Fields.Data)
                {
                    if (Regex.Match(field.Name.ToLower(), pattern).Success &&
                        (field.ValueType == ValueType.Categorical || field.ValueType == ValueType.Long || field.ValueType == ValueType.Double))
                    {
                        result = field;
                        break;
                    }
                    if (field.VariableType == VariableType.Loop && field.HasChildren)
                    {
                        for (int i = 0; i < field.Children.Length; i++)
                        {
                            if (Regex.Match(field.Children[i].Name.ToLower(), pattern).Success &&
                                (field.Children[i].ValueType == ValueType.Categorical || field.Children[i].ValueType == ValueType.Long || field.Children[i].ValueType == ValueType.Double))
                            {
                                result = field.Children[i];
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public IMddVariableCollection FindAll(string key)
        {
            if (Fields == null || Fields.Count == 0)
            {
                return null;
            }
            else
            {
                IMddVariableCollection collection = new MddVariableCollection();

                foreach (var field in Fields)
                {
                    if (field.Name.ToLower().Contains(key.ToLower()))
                    {
                        collection.Add(field);
                    }
                    if (field.VariableType == VariableType.Loop && field.HasChildren)
                    {
                        for (int i = 0; i < field.Children.Length; i++)
                        {
                            if (field.Children[i].Name.ToLower().Contains(key.ToLower()))
                            {
                                collection.Add(field.Children[i]);
                            }
                        }
                    }
                }

                return collection;
            }
        }
        /// <summary>
        /// 获得完整的条件定义表达式，题号分隔符分为三种："="，":"， "："，码号分隔符分为两种："\"，"/"
        /// </summary>
        /// <param name="sourceString">原始字符串</param>
        /// <returns>完整的条件定义表达式</returns>
        public string GetFullDefinition(string source)
        {
            string result = string.Empty;
            DefinitionCollection definitions = GetDefinitionFromComplexString(source);
            if (definitions.Count > 0)
            {
                // 遍历所有子条件
                for (int i = 0; i < definitions.Count; i++)
                {
                    string varName = definitions[i].VariableName;
                    IMddVariable variable;
                    string codeLabel = "V";
                    // 查找对应变量
                    if (Find(varName) != null)
                    {
                        variable = Find(varName);
                        if (variable.CodeList != null) codeLabel = variable.CodeList.CommonPart;
                        varName = variable.Name;
                    }
                    else if (FindWithRegex(varName) != null)
                    {
                        variable = FindWithRegex(varName);
                        if (variable.CodeList != null) codeLabel = variable.CodeList.CommonPart;
                        varName = variable.Name;
                    }
                    result += definitions[i].GetSimpleExpression(codeLabel, varName);
                }
            }
            return result;
        }

        public void AddMergedVariable(string source, string label)
        {
            if (!source.Contains("+"))
                return;
            string[] vars = source.Split('+');
            string vName = source.Replace("+", "_");
            IMddVariable variable = new MddVariable();
            ICodeList cats = null;
            string listId = string.Empty;
            string assign = string.Empty;
            for (int i = 0; i < vars.Length; i++)
            {
                IMddVariable findVar = Find(vars[i].Replace(" ", "").Replace("-", "").Replace("_", "").Replace("+", ""));
                if (findVar != null)
                {
                    if (cats is null)
                        cats = findVar.CodeList;
                    else if (findVar.CodeList.Count > 0 && findVar.CodeList.Count < cats.Count)
                        cats = findVar.CodeList;
                    if (findVar.UseList)
                    {
                        listId = findVar.ListId;
                        if (ListFields.Contains(listId))
                        {
                            ICodeList listCodes = ListFields.Get(listId).CodeList;
                            if (string.IsNullOrEmpty(assign))
                                assign = $"For each cat in {vName}.Categories\n";
                            assign += $"    If {findVar.Name}.ContainsAny(\"{listCodes.CommonPart}\" + Right(cat.name, Len(cat.name) - 1))" +
                                $"    Then {vName} = {vName} + CCategorical(cat.name)\n";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(assign))
                            assign = $"For each cat in {vName}.Categories\n";
                        assign += $"    If {findVar.Name}.ContainsAny(\"{cats.CommonPart}\" + Right(cat.name, Len(cat.name) - 1))" +
                            $"    Then {vName} = {vName} + CCategorical(cat.name)\n";
                    }
                }
            }
            ICodeList finalCodes = null;
            if (cats != null)
            {
                finalCodes = new CodeList();
                string codeLabel = cats.CommonPart;
                for (int i = 0; i < cats.Count; i++)
                {
                    finalCodes.Add(new Categorical("V" + cats[i].Name.Substring(codeLabel.Length), cats[i].Label));
                }
            }
            if (!string.IsNullOrEmpty(assign))
            {
                assign += "Next";
            }
            variable.SetProperty(vName, label);
            if (finalCodes != null)
                variable.SetProperty(finalCodes);
            variable.SetProperty(listId);
            variable.SetProperty(VariableType.Normal);
            variable.SetProperty(ValueType.Categorical);
            variable.SetAssignment(assign);
            if (NewVariables is null)
            {
                NewVariables = new string[1];
                NewVariables[0] = vName;
            }
            else
            {
                NewVariables = NewVariables.Append(vName).ToArray();
            }
            Fields.Add(variable);
        }

        //
        //
        //

        private readonly XElement _xmlMdm;
        private readonly XElement _xmlSystem;
        private readonly XElement _xmlDesign;
        private readonly XElement _xmlDefinition;

        private IMddVariableCollection _mddDefinitions;

        //
        // Xml Functions
        //

        private ValueType GetValueType(XElement xElement)
        {
            ValueType valueType = ValueType.Default;
            if (xElement.HasAttributes && xElement.Attribute("type") != null)
            {
                return xElement.Attribute("type").Value switch
                {
                    "0" => ValueType.Info,
                    "1" => ValueType.Long,
                    "5" => ValueType.Date,
                    "6" => ValueType.Double,
                    "2" => ValueType.Text,
                    "3" => ValueType.Categorical,
                    "7" => ValueType.Boolean,
                    _ => ValueType.Default,
                };
            }
            return valueType;
        }

        private string GetVariableName(XElement xElement)
        {
            if (xElement.Attribute("name") == null)
            {
                return string.Empty;
            }
            return xElement.Attribute("name").Value;
        }

        private string GetVariableId(XElement xElement)
        {
            string id = string.Empty;
            if (xElement.HasAttributes && xElement.Attribute("id") != null)
            {
                id = xElement.Attribute("id").Value;
            }
            return id;
        }

        private string GetLabel(XElement xElement, ref string[] languages)
        {
            if (xElement.Descendants("labels") == null)
            {
                return string.Empty;
            }
            XElement labels = xElement.Element("labels");
            string finalLabel = string.Empty;
            foreach (var label in labels.Elements("text"))
            {
                string lang = label.LastAttribute.Value;
                if (languages == null) languages = new string[1];
                if (!languages.Contains(lang))
                {
                    if (string.IsNullOrEmpty(languages[languages.Length - 1])) languages[languages.Length - 1] = lang;
                    else languages = languages.Append(lang).ToArray();
                }
                if (string.IsNullOrEmpty(finalLabel)) finalLabel = label.Value;
            }
            return finalLabel;
        }

        private void GetLabels(XElement xElement, ref string[] languages, out string[] findLabels, out string[] findLanguages)
        {
            findLabels = new string[1];
            findLanguages = new string[1];
            //
            if (xElement.Descendants("labels") == null)
            {
                return;
            }
            //
            XElement labels = xElement.Element("labels");
            if (labels is null || labels.Elements("text") is null) return;

            foreach (var label in labels.Elements("text"))
            {
                string lang = label.LastAttribute.Value;
                if (languages == null) languages = new string[1];
                if (!languages.Contains(lang))
                {
                    if (string.IsNullOrEmpty(languages[languages.Length - 1])) languages[languages.Length - 1] = lang;
                    else languages = languages.Append(lang).ToArray();
                }
                if (!findLanguages.Contains(lang))
                {
                    if (string.IsNullOrEmpty(findLabels[findLabels.Length - 1])) findLabels[findLabels.Length - 1] = label.Value;
                    else findLabels = findLabels.Append(label.Value).ToArray();
                    //
                    if (string.IsNullOrEmpty(findLanguages[findLanguages.Length - 1])) findLanguages[findLanguages.Length - 1] = lang;
                    else findLanguages = findLanguages.Append(lang).ToArray();
                }
                if (label.Attribute("context") != null)
                {
                    string context = label.Attribute("context").Value;
                    if (context != Context) Context = context;
                }
            }

        }

        private void GetMddDocumentProperties()
        {
            if (_xmlMdm is null || _xmlMdm.Element("properties") is null) return;
            var properties = _xmlMdm.Element("properties");
            if (properties.Element("property") is null) return;
            foreach (var prop in properties.Elements("property"))
            {
                if (prop.Element("properties") != null) continue;
                string name = prop.Attribute("name") is null ? string.Empty : prop.Attribute("name").Value;
                string value = prop.Attribute("value") is null ? string.Empty : prop.Attribute("value").Value;
                SetProperty(new Property(name, value));
            }
        }

        private ValueRange GetVariableRange(XElement xElement)
        {
            ValueRange range = new ValueRange();
            string min = string.Empty;
            string max = string.Empty;
            if (xElement.Attribute("min") != null) min = xElement.Attribute("min").Value;
            if (xElement.Attribute("max") != null) max = xElement.Attribute("max").Value;
            range.SetValue(min, max);
            if (xElement.Attribute("rangeexp") != null) range.SetValue(xElement.Attribute("rangeexp").Value);
            return range;
        }

        private void GetMddDocumentTemplate()
        {
            if (_xmlMdm is null || _xmlMdm.Element("templates") is null) return;
            var templates = _xmlMdm.Element("templates");
            if (templates.Element("property") is null) return;
            foreach (var temp in templates.Elements("property"))
            {
                if (temp.Element("properties") != null) continue;
                string name = temp.Attribute("name") is null ? string.Empty : temp.Attribute("name").Value;
                string value = temp.Attribute("value") is null ? string.Empty : temp.Attribute("value").Value;
                if (Templates is null)
                    Templates = new Property[1] { new Property(name, value) };
                else
                    Templates = Templates.Append(new Property(name, value)).ToArray();
            }
        }

        private Property[] GetProperties(XElement xElement)
        {
            if (xElement.Element("properties") is null) return null;
            Property[] prop = null;
            XElement properties = xElement.Element("properties");
            if (properties.Element("property") is null) return null;
            foreach (var property in properties.Elements("property"))
            {
                string name = property.Attribute("name") is null ? string.Empty : property.Attribute("name").Value;
                string value = property.Attribute("value") is null ? string.Empty : property.Attribute("value").Value;
                if (prop is null)
                {
                    prop = new Property[1] { new Property(name, value) };
                }
                else
                {
                    prop = prop.Append(new Property(name, value)).ToArray();
                }
            }
            return prop;
        }

        private ICodeList GetCategoricals(XElement xElement)
        {
            if (xElement.Descendants("category") == null)
            {
                return null;
            }
            ICodeList categoricals = new CodeList();
            int index = 0;
            foreach (var cat in xElement.Descendants("category"))
            {
                string name = cat.Attribute("name") != null ? cat.Attribute("name").Value : string.Empty;
                string label = GetLabel(cat, ref _languages);
                ICategorical categorical = new Categorical(name);
                //
                GetLabels(cat, ref _languages, out string[] labels, out string[] langs);
                categorical.AddLabels(labels, langs);
                categorical.SetProperties(GetProperties(cat));
                categoricals.Add(categorical);
                index++;
            }
            return categoricals;
        }

        private void GetList(XElement xElement, out string listId)
        {
            listId = string.Empty;
            if (xElement.Element("categories") == null) return;
            if (xElement.Element("categories").Attribute("categoriesref") != null)
                listId = xElement.Element("categories").Attribute("categoriesref").Value;
            else
                GetList(xElement.Element("categories"), out listId);
        }

        private DefinitionCollection GetDefinitionFromComplexString(string source)
        {
            DefinitionCollection definitions = new DefinitionCollection();
            // 格式化字符串
            source = source.Replace("（", "(");
            source = source.Replace("）", ")");
            source = source.Replace("选择了", "=");
            source = source.Replace("选择", "=");
            source = source.Replace("选", "=");
            source = source.Replace("不选择", "!");
            source = source.Replace("不选", "!");
            source = source.Replace("≠", "!");
            source = source.Replace("不等于", "!");
            source = source.Replace("等于", "=");
            source = source.Replace("<>", "!");
            source = source.Replace("and", "&");
            source = source.Replace("And", "&");
            source = source.Replace("AND", "&");
            source = source.Replace("且", "&");
            source = source.Replace("Or", "|");
            source = source.Replace("or", "|");
            source = source.Replace("OR", "|");
            source = source.Replace("或", "|");
            source = source.Replace(",", "&");
            source = source.Replace("，", "&");
            source = source.Replace("-", "_");
            source = source.Replace("\n", "|");
            //
            string varName = string.Empty;
            string codes = string.Empty;
            DefinitionOuterLogic outerLogic = DefinitionOuterLogic.Null;
            DefinitionInnerLogic innerLogic = DefinitionInnerLogic.Null;
            // 用于判断是否遇到空格
            bool hasSpace = false;
            // 用于判断是否遇到内部逻辑符号
            bool hasInnerSymbol = false;
            // 用于判断括号情况
            int currentGroup = 0;
            bool hasLBkt = false;
            bool hasRBkt = false;
            //
            for (int i = 0; i < source.Length; i++)
            {
                string subStr = source.Substring(i, 1);
                if (subStr != " ")
                {
                    // 遇到左括号，设定变量值
                    if (subStr == "(")
                    {
                        currentGroup++;
                        hasLBkt = true;
                        varName = string.Empty;
                    }
                    // 遇到右括号，设定变量值
                    if (subStr == ")")
                    {
                        hasRBkt = true;
                    }
                    // 对于当前字符串为逻辑符号时，分两种情况讨论
                    // 1.如果下一个字符为数字，则视为码号分割
                    // 2.如果非第一种情况，视为外部逻辑符号
                    if (subStr == "&" || subStr == "|")
                    {
                        // 对于当前字符为&或|并且下一个字符为数字，视为码号定义，替换字符为/
                        if (i < source.Length - 1 && Regex.IsMatch(source.Substring(i + 1).Trim().Substring(0, 1), @"[0-9]"))
                        {
                            if (subStr == "&") subStr = "/";
                            if (subStr == "|") subStr = "/";
                        }
                        // 对于外部逻辑情况，将现有值写入结果，并清空当前临时数据，重设外部逻辑
                        else
                        {
                            Definition def = new Definition();
                            varName = GetCorrectVariableName(varName);
                            def.SetProperty(varName.Trim(), codes.Trim());
                            def.SetProperty(innerLogic);
                            def.SetProperty(outerLogic);
                            def.SetProperty(currentGroup);
                            if (hasLBkt && !hasRBkt)
                            {
                                def.SetProperty(DefinitionGroupLogic.Start);
                                def.SetProperty(true);
                                hasLBkt = false;
                            }
                            else if (hasRBkt && !hasLBkt)
                            {
                                def.SetProperty(DefinitionGroupLogic.End);
                                def.SetProperty(true);
                                hasRBkt = false;
                            }
                            else if (hasRBkt && hasLBkt)
                            {
                                def.SetProperty(DefinitionGroupLogic.Both);
                                def.SetProperty(true);
                                hasLBkt = false;
                                hasRBkt = false;
                            }
                            else
                            {
                                def.SetProperty(DefinitionGroupLogic.Null);
                                def.SetProperty(false);
                            }
                            definitions.Add(def);
                            // 清空临时数据，重新赋值
                            varName = string.Empty;
                            codes = string.Empty;
                            innerLogic = DefinitionInnerLogic.Null;
                            hasInnerSymbol = false;
                            // 写入外部逻辑
                            if (subStr == "&")
                                outerLogic = DefinitionOuterLogic.And;
                            else if (subStr == "|")
                                outerLogic = DefinitionOuterLogic.Or;
                            else
                                outerLogic = DefinitionOuterLogic.Null;
                        }
                    }
                    // 当遇到空格时，如果空格后的字符是字母，则清空变量名内容，重新开始赋值
                    if (hasSpace && !hasInnerSymbol && Regex.IsMatch(subStr, @"[A-Za-z]+"))
                        varName = string.Empty;
                    // 当遇到内部逻辑符号时，加入内部逻辑
                    if (subStr == "=" || subStr == "!")
                    {
                        if (subStr == "=")
                            innerLogic = DefinitionInnerLogic.Equal;
                        else
                            innerLogic = DefinitionInnerLogic.NotEqual;
                        hasInnerSymbol = true;
                    }
                    else
                    {
                        // 如果已经遇到内部逻辑符号，累加到codes上，否则，累加到变量名上
                        if (!hasInnerSymbol && subStr != "&" && subStr != "|" && subStr != "(" && subStr != ")")
                            varName += subStr;
                        else if (hasInnerSymbol && subStr != "&" && subStr != "|" && subStr != "(" && subStr != ")")
                            codes += subStr;
                    }
                    hasSpace = false;
                }
                else
                {
                    hasSpace = true;
                }
            }
            // 加入最后的结果
            Definition lastDef = new Definition();
            lastDef.SetProperty(varName.Trim(), codes.Trim());
            lastDef.SetProperty(innerLogic);
            lastDef.SetProperty(outerLogic);
            lastDef.SetProperty(currentGroup);
            if (hasLBkt && !hasRBkt)
            {
                lastDef.SetProperty(true);
                lastDef.SetProperty(DefinitionGroupLogic.Start);
            }
            else if (hasRBkt && !hasLBkt)
            {
                lastDef.SetProperty(true);
                lastDef.SetProperty(DefinitionGroupLogic.End);
            }
            else if (hasLBkt && hasRBkt)
            {
                lastDef.SetProperty(true);
                lastDef.SetProperty(DefinitionGroupLogic.Both);
            }
            else
            {
                lastDef.SetProperty(false);
                lastDef.SetProperty(DefinitionGroupLogic.Null);
            }
            definitions.Add(lastDef);
            //
            return definitions;
        }

        private string GetCorrectVariableName(string varName)
        {
            string result = varName;
            if (!Regex.IsMatch(result, @"[\u4E00-\u9FA5]+")) return result;
            int start = 0;
            while (!Regex.IsMatch(result.Substring(start, 1), @"[\u4E00-\u9FA5]"))
            {
                start++;
            }
            if (start == 0 || start == result.Length - 1) return result;
            string code = result.Substring(start).Trim();
            string name = result.Substring(0, start + 1).Trim();
            var find = Find(name);
            if (find is null)
            {
                find = FindWithRegex(name);
            }
            if (find is null ||
                find.VariableType != VariableType.Loop ||
                !find.HasChildren ||
                (!find.UseList && (find.CodeList is null || find.CodeList.Count == 0))) return result;
            ICodeList topCodes;
            if (find.UseList) topCodes = ListFields.Get(find.ListId).CodeList;
            else topCodes = find.CodeList;
            string resultCode = string.Empty;
            if (topCodes != null && topCodes.Count > 0)
            {
                foreach (var categorical in topCodes)
                {
                    if (categorical.Label.Contains(code))
                    {
                        resultCode = categorical.Name;
                        break;
                    }
                }
            }
            result = $"{find.Name}[{{{resultCode}}}].{find.Children[0].Name}";
            return result;
        }


        //
        //
        //

        private void GetAllDefinitions()
        {
            _mddDefinitions = new MddVariableCollection();
            if (_xmlDefinition != null)
            {
                foreach (var variable in _xmlDefinition.Elements("variable"))
                {
                    IMddVariable mddVariable = new MddVariable();
                    mddVariable.SetProperty(GetValueType(variable));
                    mddVariable.SetProperty(GetVariableName(variable), GetVariableId(variable));
                    mddVariable.SetProperty(VariableType.Normal);
                    mddVariable.SetProperty(GetCategoricals(variable));
                    mddVariable.SetProperty(GetProperties(variable));
                    mddVariable.SetProperty(GetVariableRange(variable));
                    //
                    GetLabels(variable, ref _languages, out string[] labels, out string[] languages);
                    mddVariable.SetLabels(labels, languages);
                    //
                    GetList(variable, out string listId);
                    mddVariable.SetProperty(listId);
                    //
                    if (_languages != null) mddVariable.SetLanguage(_languages[0]);
                    //
                    _mddDefinitions.Add(mddVariable);
                }
            }
        }

        private void GetAllSystem()
        {
            if (_xmlSystem is null) return;
            foreach (var field in _xmlSystem.Elements("class"))
            {
                IMddVariable sysVariable = new MddVariable();
                sysVariable.SetProperty(ValueType.Default);
                sysVariable.SetProperty(VariableType.System);
                sysVariable.SetProperty(GetVariableName(field), GetVariableId(field));
                sysVariable.SetProperty(GetProperties(field));
                var sides = field.Element("fields");
                if (sides is null || sides.Element("variable") is null) continue;
                foreach (var side in sides.Elements("variable"))
                {
                    if (side.Attribute("ref") is null || !_mddDefinitions.Contains(side.Attribute("ref").Value)) continue;
                    IMddVariable defVariable = _mddDefinitions.Get(side.Attribute("ref").Value);
                    sysVariable.SetProperty(defVariable.Copy());
                    sysVariable.Children[sysVariable.Children.Length - 1].SetParent(sysVariable);
                    _mddDefinitions.Remove(defVariable);
                }
                Fields.Add(sysVariable);
            }

        }

        //
        //
        //


        private void GetLoopDesign(XElement xElement, IMddVariableCollection mddDefinition, ref IMddVariable variable, string loopName)
        {
            variable.SetProperty(GetVariableName(xElement), GetVariableId(xElement));
            variable.SetProperty(ValueType.Categorical);
            variable.SetProperty(VariableType.Loop);
            variable.SetProperty(GetCategoricals(xElement));
            variable.SetProperty(GetProperties(xElement));
            //
            GetLabels(xElement, ref _languages, out string[] labels, out string[] languages);
            variable.SetLabels(labels, languages);
            //
            if (languages != null && languages.Length > 0) variable.SetLanguage(languages[0]);
            //
            GetList(xElement, out string listId);
            variable.SetProperty(listId);
            //
            var fields = xElement.Element("class").Element("fields");
            if (fields != null)
            {
                foreach (var slice in fields.Elements())
                {
                    if (slice.Name == "variable")
                    {
                        if (slice.Attribute("ref") != null && mddDefinition.Contains(slice.Attribute("ref").Value))
                        {
                            variable.SetProperty(mddDefinition.Get(slice.Attribute("ref").Value).Copy());
                            for (int i = 0; i < variable.Children.Length; i++)
                            {
                                variable.Children[i].SetParent(variable);
                            }
                            mddDefinition.Remove(mddDefinition.Get(slice.Attribute("ref").Value));
                        }
                    }
                    if (slice.Name == loopName)
                    {
                        IMddVariable child = new MddVariable();
                        GetLoopDesign(slice, mddDefinition, ref child, loopName);
                        variable.SetProperty(child);
                    }
                }
            }
        }

        private void GetAllDesign()
        {
            if (_xmlDesign != null)
            {
                var fields = _xmlDesign.Element("fields");
                if (fields is null || fields.Elements("variable") is null)
                {
                    return;
                }
                // 添加非loop类型变量
                foreach (var variable in fields.Elements("variable"))
                {
                    string reference = variable.Attribute("ref") != null ? variable.Attribute("ref").Value : string.Empty;
                    if (!string.IsNullOrEmpty(reference) && _mddDefinitions.Contains(reference))
                    {
                        IMddVariable findVariable = _mddDefinitions.Get(reference);
                        IMddVariable mddVariable = findVariable.Copy();
                        mddVariable.SetProperty(findVariable.ListId);
                        Fields.Add(mddVariable);
                        _mddDefinitions.Remove(_mddDefinitions.Get(reference));
                    }
                }
                // 添加loop类型变量
                string loopName = string.Empty;
                IEnumerable<XElement> loops = null;
                if (fields.Descendants("loop").Count() > 0)
                {
                    loops = fields.Descendants("loop");
                    loopName = "loop";
                }
                if (fields.Descendants("grid").Count() > 0)
                {
                    loops = fields.Descendants("grid");
                    loopName = "grid";
                }
                if (loops != null)
                {
                    foreach (var grid in loops)
                    {
                        IMddVariable loopVariable = new MddVariable();
                        GetLoopDesign(grid, _mddDefinitions, ref loopVariable, loopName);
                        Fields.Add(loopVariable);
                    }
                }
                //
                Fields.Merge(_mddDefinitions, MddMergeType.Behind);
            }
        }

        private void GetAllLists()
        {
            ListFields = new MddVariableCollection();
            if (_xmlDefinition != null)
            {
                if (_xmlDefinition.Element("categories") is null)
                {
                    return;
                }
                foreach (var list in _xmlDefinition.Elements("categories"))
                {
                    IMddVariable mddList = new MddVariable();
                    mddList.SetProperty(ValueType.Categorical);
                    mddList.SetProperty(VariableType.Normal);
                    mddList.SetProperty(GetVariableName(list), GetVariableId(list));
                    mddList.SetProperty(GetCategoricals(list));
                    ListFields.Add(mddList);
                }
            }
        }

        private void GetBlockFields()
        {
            if (_xmlDesign != null)
            {
                var fields = _xmlDesign.Element("fields");
                if (fields.Element("class") is null)
                {
                    return;
                }
                foreach (var block in fields.Elements("class"))
                {
                    IMddVariable variable = new MddVariable();
                    variable.SetProperty(GetVariableName(block), GetVariableId(block));
                    GetLabels(block, ref _languages, out string[] labels, out string[] langs);
                    variable.SetLabels(labels, langs);
                    variable.SetProperty(VariableType.Block);
                    variable.SetProperty(ValueType.Default);
                    if (block.Element("fields") != null && block.Element("fields").Element("variable") != null)
                    {
                        foreach (var child in block.Element("fields").Elements("variable"))
                        {
                            string reference = child.Attribute("ref") != null ? child.Attribute("ref").Value : string.Empty;
                            if (!string.IsNullOrEmpty(reference) && _mddDefinitions.Contains(reference))
                            {
                                IMddVariable findVariable = _mddDefinitions.Get(reference);
                                IMddVariable mddVariable = findVariable.Copy();
                                mddVariable.SetProperty(findVariable.ListId);
                                variable.SetProperty(mddVariable);
                            }
                        }
                    }
                    Fields.Add(variable);
                }
            }
        }
    }
}
