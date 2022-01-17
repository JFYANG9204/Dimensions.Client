using Dimensions.Bll.Generic;
using Dimensions.Bll.Spec;
using Dimensions.Bll.String;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Dimensions.Bll.Mdd;

namespace Dimensions.Bll.File
{
    public class FileWriter
    {
        public FileWriter(string path, bool subGroup = false)
        {
            _path = Path.Combine(path, "Files");
            _subGroup = subGroup;
            //
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            //
            InitializeFile(Path.Combine(_path, FileString.MddFileName));
            InitializeFile(Path.Combine(_path, FileString.DmsFileName));
            InitializeFile(Path.Combine(_path, FileString.TabFileName));
            InitializeFile(Path.Combine(_path, FileString.EdtFileName));
            InitializeFile(Path.Combine(_path, FileString.BatFileName));
            InitializeFile(Path.Combine(_path, FileString.TopFileName));
            InitializeFile(Path.Combine(_path, FileString.LogFileName));
        }

        private readonly string _path;
        private readonly bool _subGroup;

        public void Write(IFileContentBuilder fileContentBuilder)
        {
            Type type = fileContentBuilder.GetType();
            foreach (var attribute in type.GetCustomAttributes(false))
            {
                if (attribute is WriterInfoAttribute info)
                {
                    string fullPath = string.Empty;
                    switch (info.File)
                    {
                        case FileType.Mdd:
                            fullPath = Path.Combine(_path, FileString.MddFileName);
                            break;
                        case FileType.Tab:
                            fullPath = Path.Combine(_path, FileString.TabFileName);
                            break;
                        case FileType.Dms:
                            fullPath = Path.Combine(_path, FileString.DmsFileName);
                            break;
                        case FileType.Edt:
                            fullPath = Path.Combine(_path, FileString.EdtFileName);
                            break;
                        case FileType.Top:
                            fullPath = Path.Combine(_path, FileString.TopFileName);
                            break;
                        case FileType.Bat:
                            fullPath = Path.Combine(_path, FileString.BatFileName);
                            break;
                        case FileType.Log:
                            fullPath = Path.Combine(_path, FileString.LogFileName);
                            break;
                        default:
                            break;
                    }
                    if (!string.IsNullOrEmpty(fullPath))
                    {
                        using StreamWriter sw = new StreamWriter(fullPath, true, Encoding.Default);
                        sw.WriteLine(fileContentBuilder.Get());
                    }
                }
            }
        }

        private void InitializeFile(string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            if (Path.GetFileName(path) == FileString.MddFileName)
            {
                byte[] data = Encoding.Default.GetBytes(FileString.MddBaseContent);
                fs.Write(data, 0, data.Length);
                fs.Flush();
            }
            fs.Close();
        }

        private readonly IFileContentBuilder mddContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Mdd);
        private readonly IFileContentBuilder tabContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Tab);
        private readonly IFileContentBuilder dmsContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Dms);
        private readonly IFileContentBuilder edtContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Edt);
        private readonly IFileContentBuilder topContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Top);
        private readonly IFileContentBuilder batContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Bat);
        private readonly IFileContentBuilder logContent = FileContentBuilderFactory.CreateContentBuilder(FileType.Log);

        public void WriteAll(IMddDocument mdd)
        {
            if (mdd is null || mdd.NewVariables is null || mdd.NewVariables[0] is null) return;
            for (int i = 0; i < mdd.NewVariables.Length; i++)
            {
                IMddVariable newVar = mdd.Fields[mdd.NewVariables[i]];
                if (newVar is null) continue;
                ICodeList _finalList = new CodeList();
                if (newVar.UseList)
                {
                    _finalList = mdd.ListFields.Get(newVar.ListId).CodeList;
                }
                if (newVar.CodeList != null && newVar.CodeList.Count > 0)
                {
                    foreach (var code in newVar.CodeList)
                    {
                        _finalList.Add(code);
                    }
                }
                dmsContent.Set(DmsFileContentType.Normal,
                    new KeyValuePair<object, object>(FileWriterKeys.DmsVariableName, newVar.Name),
                    new KeyValuePair<object, object>(FileWriterKeys.DmsSideCodeList, _finalList));
                // Edt赋值语句
                edtContent.Set(
                    EdtFileContentType.Note,
                    new KeyValuePair<object, object>(FileWriterKeys.Notes, newVar.Name));
                Write(edtContent);
                edtContent.Set(
                    EdtFileContentType.Pyramid,
                    new KeyValuePair<object, object>(FileWriterKeys.EdtPyDatas, newVar.Assignment));
                Write(edtContent);
            }
        }
        /// <summary>
        /// 写入所有主Spec内容
        /// </summary>
        /// <param name="specItems"></param>
        public void WriteAll(ISpecItem[] specItems, IMddDocument _mdd)
        {
            if (specItems is null || specItems[0] is null) return;
            //
            List<string> exist = new List<string>();
            bool hasRebaseFunction = false;
            // 写入mdd基础文本
            mddContent.Set(FileWriterKeys.MddText, new KeyValuePair<object, object>(MddFileContentType.Text, FileString.MddBaseContent));
            Write(mddContent);
            //
            logContent.Set(string.Empty,
                new KeyValuePair<object, object>(FileWriterKeys.LogText, "开始执行"));
            Write(logContent);
            for (int i = 0; i < specItems.Length; i++)
            {
                if (specItems[i].IsSkipped)
                {
                    logContent.Set(string.Empty,
                        new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                        new KeyValuePair<object, object>(FileWriterKeys.LogStatus, "匹配失败,已跳过"));
                    Write(logContent);
                    continue;
                }
                // 写入注释
                if (!string.IsNullOrEmpty(specItems[i].Notes))
                {
                    mddContent.Set(MddFileContentType.Note, new KeyValuePair<object, object>(FileWriterKeys.Notes, specItems[i].Notes));
                    Write(mddContent);
                    tabContent.Set(TabFileContentType.Note, new KeyValuePair<object, object>(FileWriterKeys.Notes, specItems[i].Notes));
                    Write(tabContent);
                    continue;
                }
                //
                if (specItems[i].NewVariableDefinition != null && !specItems[i].NewVariableDefinition.Empty)
                {
                    NewVariableDefinition newVar = specItems[i].NewVariableDefinition;
                    //
                    string _axis = $"{{e1 '' text(),base 'Base : {specItems[i].Base[0].Value}' base(),..,e2 '' text(),sigma 'Sigma' subtotal()}}";
                    mddContent.Set(
                        MddFileContentType.Axis,
                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, newVar.Name),
                        new KeyValuePair<object, object>(FileWriterKeys.MddAxis, _axis));
                    Write(mddContent);
                    // DMS
                    dmsContent.Set(
                        DmsFileContentType.Loop,
                        new KeyValuePair<object, object>(FileWriterKeys.DmsVariableName, newVar.Name),
                        new KeyValuePair<object, object>(FileWriterKeys.DmsVariableLable, specItems[i].Title),
                        new KeyValuePair<object, object>(FileWriterKeys.DmsSideName, "Column"),
                        new KeyValuePair<object, object>(FileWriterKeys.DmsTopCodeList, newVar.TopList),
                        new KeyValuePair<object, object>(FileWriterKeys.DmsSideCodeList, newVar.SideList));
                    Write(dmsContent);
                    // Edt赋值语句
                    edtContent.Set(
                        EdtFileContentType.Note,
                        new KeyValuePair<object, object>(FileWriterKeys.Notes, newVar.Name));
                    Write(edtContent);
                    edtContent.Set(
                        EdtFileContentType.Pyramid,
                        new KeyValuePair<object, object>(FileWriterKeys.EdtPyDatas, newVar.GetAssignment()));
                    Write(edtContent);
                    // Tab
                    tabContent.Set(
                        TabFileContentType.Grid,
                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, $"{newVar.Name}[..].Column"),
                        new KeyValuePair<object, object>(FileWriterKeys.TabTopName, newVar.Name),
                        new KeyValuePair<object, object>(FileWriterKeys.TabBaseName, string.Empty));
                    Write(tabContent);
                    // 标题追加标签
                    tabContent.Set(
                        TabFileContentType.GridLabel,
                        new KeyValuePair<object, object>(FileWriterKeys.TabLabel, " - Summary"));
                    Write(tabContent);
                    // Grid Slice
                    tabContent.Set(
                        TabFileContentType.GridSlice,
                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, $"{newVar.Name}[..].Column"));
                    Write(tabContent);
                    // Logger
                    if (newVar.IsPyramid)
                    {
                        logContent.Set(string.Empty,
                            new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                            new KeyValuePair<object, object>(FileWriterKeys.LogStatus, $"匹配为品牌金字塔变量，已新建变量：{newVar.Name}"));
                        Write(logContent);
                    }
                    else
                    {
                        logContent.Set(string.Empty,
                            new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                            new KeyValuePair<object, object>(FileWriterKeys.LogStatus, $"匹配为品牌新增变量，已新建变量：{newVar.Name}"));
                        Write(logContent);
                    }
                    continue;
                }
                //
                IMddVariable variable = specItems[i].Variable;
                // 判断是否为重复变量
                if (exist.Contains(variable.Id))
                {
                    logContent.Set(string.Empty,
                        new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                        new KeyValuePair<object, object>(FileWriterKeys.LogStatus, "匹配到重复变量，已跳过"));
                    Write(logContent);
                    continue;
                }
                // Normal变量
                if (variable.VariableType == VariableType.Normal)
                {
                    string varName = variable.Name;
                    ICodeList codes = variable.CodeList;
                    // 分为数值型变量和非数值型变量两种情况
                    if (variable.ValueType == ValueType.Categorical)
                    {
                        // 首先检查是否为合并为多选的loop变量
                        if (variable.CodeList != null && variable.CodeList.Count > 0 && variable.CodeList.IsMergedLoop("V", out ICodeList.MergedLoopDefinition definition))
                        {
                            // Dms创建变量
                            string _newVarName = "DP_" + varName;
                            dmsContent.Set(
                                DmsFileContentType.Loop,
                                new KeyValuePair<object, object>(FileWriterKeys.DmsVariableName, _newVarName),
                                new KeyValuePair<object, object>(FileWriterKeys.DmsVariableLable, specItems[i].Title),
                                new KeyValuePair<object, object>(FileWriterKeys.DmsSideName, "Column"),
                                new KeyValuePair<object, object>(FileWriterKeys.DmsTopCodeList, definition.TopList),
                                new KeyValuePair<object, object>(FileWriterKeys.DmsSideCodeList, definition.SideList));
                            Write(dmsContent);
                            // Edt赋值语句
                            edtContent.Set(
                                EdtFileContentType.Note,
                                new KeyValuePair<object, object>(FileWriterKeys.Notes, varName));
                            Write(edtContent);
                            edtContent.Set(
                                EdtFileContentType.MergedLoop,
                                new KeyValuePair<object, object>(FileWriterKeys.EdtDefinition, definition),
                                new KeyValuePair<object, object>(FileWriterKeys.EdtSourceVariableName, varName),
                                new KeyValuePair<object, object>(FileWriterKeys.EdtTargetVariableName, _newVarName),
                                new KeyValuePair<object, object>(FileWriterKeys.EdtTargetVariabelSideName, "Column"));
                            Write(edtContent);
                            // Mdd
                            // 注释
                            mddContent.Set(
                                MddFileContentType.Note,
                                new KeyValuePair<object, object>(FileWriterKeys.Notes, _newVarName));
                            Write(mddContent);
                            // Axis
                            string _axis = $"{{e1 '' text(),base 'Base : {specItems[i].Base[0].Value}' base(),..,e2 '' text(),sigma 'Sigma' subtotal()}}";
                            mddContent.Set(
                                MddFileContentType.Axis,
                                new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, _newVarName),
                                new KeyValuePair<object, object>(FileWriterKeys.MddAxis, _axis));
                            Write(mddContent);
                            // Tab
                            tabContent.Set(
                                TabFileContentType.Grid,
                                new KeyValuePair<object, object>(FileWriterKeys.TabSideName, $"{_newVarName}[..].Column"),
                                new KeyValuePair<object, object>(FileWriterKeys.TabTopName, _newVarName),
                                new KeyValuePair<object, object>(FileWriterKeys.TabBaseName, string.Empty));
                            Write(tabContent);
                            // 标题追加标签
                            tabContent.Set(
                                TabFileContentType.GridLabel,
                                new KeyValuePair<object, object>(FileWriterKeys.TabLabel, " - Summary"));
                            Write(tabContent);
                            // Grid Slice
                            tabContent.Set(
                                TabFileContentType.GridSlice,
                                new KeyValuePair<object, object>(FileWriterKeys.TabSideName, $"{_newVarName}[..].Column"));
                            Write(tabContent);
                            // Log
                            logContent.Set(string.Empty,
                                new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                                new KeyValuePair<object, object>(FileWriterKeys.LogStatus, $"匹配为合并为多选的循环变量，已创建新变量：{_newVarName}"));
                            Write(logContent);
                            continue;
                        }
                        // Mdd文件写入
                        if (!exist.Contains(variable.Id))
                        {
                            // 注释
                            mddContent.Set(
                                MddFileContentType.Note,
                                new KeyValuePair<object, object>(FileWriterKeys.Notes, varName));
                            Write(mddContent);
                            // 判断是否为Loop变量的Side自变量
                            if (variable.Parent != null)
                            {
                                IMddVariable parent = variable.Parent;
                                varName = parent.Name + "[..]." + variable.Name;
                                if (!exist.Contains(parent.Id))
                                {
                                    // 标题
                                    mddContent.Set(
                                        MddFileContentType.Title,
                                        new KeyValuePair<object, object>(FileWriterKeys.MddTitle, specItems[i].Title),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, parent.Name));
                                    // 码号标签
                                    if (parent.CodeList != null && parent.CodeList.Count > 0)
                                    {
                                        parent.CodeList.FormatCodeLabels();
                                        for (int j = 0; j < parent.CodeList.Count; j++)
                                        {
                                            mddContent.Set(
                                                MddFileContentType.ResponseText,
                                                new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, parent.Name),
                                                new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, parent.CodeList[j].Name),
                                                new KeyValuePair<object, object>(FileWriterKeys.MddCodeLabel, StringFunction.FormatString(parent.CodeList[j].Label)));
                                            Write(mddContent);
                                        }
                                    }
                                    exist.Add(parent.Id);
                                }
                            }
                            // 标题
                            mddContent.Set(
                                MddFileContentType.Title,
                                new KeyValuePair<object, object>(FileWriterKeys.MddTitle, specItems[i].Title),
                                new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName));
                            Write(mddContent);
                            // Axis
                            mddContent.Set(
                                MddFileContentType.Axis,
                                new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName),
                                new KeyValuePair<object, object>(FileWriterKeys.MddAxis, specItems[i].Axis.Value));
                            Write(mddContent);
                            // 码号标签
                            if (codes != null && codes.Count > 0)
                            {
                                codes.FormatCodeLabels();
                                for (int j = 0; j < codes.Count; j++)
                                {
                                    mddContent.Set(
                                        MddFileContentType.ResponseText,
                                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, codes[j].Name),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddCodeLabel, StringFunction.FormatString(codes[j].Label)));
                                    Write(mddContent);
                                }
                            }
                            // Factor
                            if (specItems[i].Mean && variable.ValueType == ValueType.Categorical && (codes != null || variable.UseList))
                            {
                                if (variable.UseList) codes = _mdd.ListFields.Get(variable.ListId).CodeList;
                                if (codes.Count > 0 && codes.IsContinuousFactor(out int step, out string exclude, specItems[i].HighFactorKeys))
                                {
                                    int start = 1;
                                    string _exclude = "Null";
                                    if (step == -1)
                                    {
                                        if (!string.IsNullOrEmpty(exclude))
                                        {
                                            start = codes.Count - exclude.Split(',').Length;
                                            _exclude = "\"" + exclude + "\"";
                                        }
                                    }
                                    mddContent.Set(
                                        MddFileContentType.ContinueFactor,
                                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddStartFactor, start),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddStep, step),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddExclude, _exclude));
                                    Write(mddContent);
                                }
                                else if (codes.Count > 0 && codes.IsSplitFactor(out Dictionary<string, double> facts))
                                {
                                    foreach (var fact in facts)
                                    {
                                        mddContent.Set(
                                            MddFileContentType.SplitFactor,
                                            new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, fact.Key),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddFactor, fact.Value.ToString()));
                                        Write(mddContent);
                                    }
                                }
                                else
                                {
                                    string _avgLabel = "平均提及个数";
                                    if (variable.Language != "zh-CN")
                                        _avgLabel = "Average Metion No.";
                                    mddContent.Set(
                                        MddFileContentType.Average,
                                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, varName),
                                        new KeyValuePair<object, object>(FileWriterKeys.MddAverageLabel, _avgLabel));
                                    Write(mddContent);
                                }
                            }
                        }
                        // tab
                        tabContent.Set(TabFileContentType.Normal,
                            new KeyValuePair<object, object>(FileWriterKeys.TabSideName, varName),
                            new KeyValuePair<object, object>(FileWriterKeys.TabBaseName, string.Empty));
                        Write(tabContent);
                        if (specItems[i].Base.Length > 1)
                        {
                            for (int j = 1; j < specItems[i].Base.Length; j++)
                            {
                                // tab
                                tabContent.Set(
                                    TabFileContentType.Normal,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabSideName, varName),
                                    new KeyValuePair<object, object>(FileWriterKeys.TabBaseName, specItems[i].Base[j].Value));
                                Write(tabContent);
                                // Filter
                                tabContent.Set(
                                    TabFileContentType.Filter,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabNormalFilter, specItems[i].Base[j].Expression));
                                Write(tabContent);
                                // Label
                                tabContent.Set(
                                    TabFileContentType.NormalLabel,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabLabel, $" - {specItems[i].Base[j].Value}"));
                                Write(tabContent);
                            }
                        }
                        logContent.Set(string.Empty,
                            new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                            new KeyValuePair<object, object>(FileWriterKeys.LogStatus, "写入成功"));
                        Write(logContent);
                        exist.Add(variable.Id);
                        continue;
                    }
                    // 数值型变量，直接创建Table
                    if (variable.ValueType == ValueType.Long || variable.ValueType == ValueType.Double)
                    {
                        tabContent.Set(
                            TabFileContentType.Digit,
                            new KeyValuePair<object, object>(FileWriterKeys.TabSideName, varName));
                        Write(tabContent);
                        //
                        string _nameWithAxis = $"{varName}.code_{varName}{{e1 '' text(),base 'Base : {specItems[i].Base[0].Value}' base()," +
                            $"e2 '' text(),..,e3 '' text()\" + mean_inc(\"{varName}\") + \"}}";
                        tabContent.Set(
                            TabFileContentType.Normal,
                            new KeyValuePair<object, object>(FileWriterKeys.TabSideName, _nameWithAxis),
                            new KeyValuePair<object, object>(FileWriterKeys.TabTitle, $"\"{specItems[i].Title}\""));
                        Write(tabContent);
                        // Log
                        logContent.Set(string.Empty,
                            new KeyValuePair<object, object>(FileWriterKeys.LogVarName, specItems[i].Name),
                            new KeyValuePair<object, object>(FileWriterKeys.LogStatus, "写入成功"));
                        Write(logContent);
                        exist.Add(variable.Id);
                        continue;
                    }
                }
                // Loop 类型
                if (variable.VariableType == VariableType.Loop)
                {
                    // Mdd部分
                    if (!exist.Contains(variable.Id))
                    {
                        // 注释
                        mddContent.Set(MddFileContentType.Note,
                            new KeyValuePair<object, object>(FileWriterKeys.Notes, specItems[i].Name));
                        Write(mddContent);
                        // Title
                        mddContent.Set(MddFileContentType.Title,
                            new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, variable.Name),
                            new KeyValuePair<object, object>(FileWriterKeys.MddTitle, specItems[i].Title));
                        Write(mddContent);
                        // Loop层码号标签
                        ICodeList topCodes = variable.CodeList;
                        if (topCodes != null && topCodes.Count > 0)
                        {
                            topCodes.FormatCodeLabels();
                            for (int j = 0; j < topCodes.Count; j++)
                            {
                                mddContent.Set(MddFileContentType.ResponseText,
                                    new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, variable.Name),
                                    new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, topCodes[j].Name),
                                    new KeyValuePair<object, object>(FileWriterKeys.MddCodeLabel, StringFunction.FormatString(topCodes[j].Label)));
                                Write(mddContent);
                            }
                        }
                        // Side部分
                        if (variable.HasChildren)
                        {
                            for (int j = 0; j < variable.Children.Length; j++)
                            {
                                IMddVariable side = variable.Children[j];
                                string fullName = variable.Name + "[..]." + side.Name;
                                // 注释
                                mddContent.Set(MddFileContentType.Note,
                                    new KeyValuePair<object, object>(FileWriterKeys.Notes, fullName));
                                Write(mddContent);
                                // Title
                                mddContent.Set(MddFileContentType.Title,
                                    new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                    new KeyValuePair<object, object>(FileWriterKeys.MddTitle, specItems[i].Title));
                                Write(mddContent);
                                // Axis
                                mddContent.Set(
                                    MddFileContentType.Axis,
                                    new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                    new KeyValuePair<object, object>(FileWriterKeys.MddAxis, specItems[i].Axis.Value));
                                Write(mddContent);
                                // 码号标签
                                ICodeList sideCodes = variable.Children[j].CodeList;
                                if (sideCodes != null && sideCodes.Count > 0)
                                {
                                    sideCodes.FormatCodeLabels();
                                    for (int k = 0; k < sideCodes.Count; k++)
                                    {
                                        mddContent.Set(MddFileContentType.ResponseText,
                                            new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, sideCodes[k].Name),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddCodeLabel, StringFunction.FormatString(sideCodes[k].Label)));
                                        Write(mddContent);
                                    }
                                }
                                // Factor
                                if (specItems[i].Mean && side.ValueType == ValueType.Categorical && (sideCodes != null || side.UseList))
                                {
                                    if (side.UseList) sideCodes = _mdd.ListFields.Get(side.ListId).CodeList;
                                    if (sideCodes.Count > 0 && sideCodes.IsContinuousFactor(out int step, out string exclude, specItems[i].HighFactorKeys))
                                    {
                                        int start = 1;
                                        string _exclude = string.Empty;
                                        if (step == -1)
                                        {
                                            if (!string.IsNullOrEmpty(exclude))
                                            {
                                                start = sideCodes.Count - exclude.Split(',').Length;
                                                _exclude = exclude;
                                            }
                                            else
                                            {
                                                start = sideCodes.Count;
                                            }
                                        }
                                        mddContent.Set(
                                            MddFileContentType.ContinueFactor,
                                            new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddStartFactor, start),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddStep, step),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddExclude, _exclude));
                                        Write(mddContent);
                                    }
                                    else if (sideCodes.Count > 0 && sideCodes.IsSplitFactor(out Dictionary<string, double> facts))
                                    {
                                        foreach (var fact in facts)
                                        {
                                            mddContent.Set(
                                                MddFileContentType.SplitFactor,
                                                new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                                new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, fact.Key),
                                                new KeyValuePair<object, object>(FileWriterKeys.MddFactor, fact.Value.ToString()));
                                            Write(mddContent);
                                        }
                                    }
                                    else
                                    {
                                        string _avgLabel = "平均提及个数";
                                        if (variable.Language != "zh-CN")
                                            _avgLabel = "Average Metion No.";
                                        mddContent.Set(
                                            MddFileContentType.Average,
                                            new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, fullName),
                                            new KeyValuePair<object, object>(FileWriterKeys.MddAverageLabel, _avgLabel));
                                        Write(mddContent);
                                    }
                                }
                                // Tab
                                // 判断Loop类型变量是否为多Base，并且其中一个为Total
                                int totalIndex = -1;
                                if (specItems[i].Base.Length > 1)
                                {
                                    for (int k = 0; k < specItems[i].Base.Length; k++)
                                    {
                                        if (specItems[i].Base[k].Value.ToLower().Contains("total"))
                                        {
                                            totalIndex = k;
                                            break;
                                        }
                                    }
                                }
                                // 如果MDD文件中没有添加过Function，则添加
                                if (totalIndex > -1 && !hasRebaseFunction)
                                {
                                    // 注释
                                    mddContent.Set(MddFileContentType.Note,
                                        new KeyValuePair<object, object>(FileWriterKeys.Notes, "Function"));
                                    Write(mddContent);
                                    // Function
                                    mddContent.Set(MddFileContentType.RebaseFunction);
                                    Write(mddContent);
                                    hasRebaseFunction = true;
                                }
                                if (totalIndex > -1)
                                {
                                    // DMS文件写入循环变量框架
                                    dmsContent.Set(DmsFileContentType.LoopFrame,
                                        new KeyValuePair<object, object>(FileWriterKeys.DmsVariableName, variable.Name));
                                    Write(dmsContent);
                                }
                                // 按顺序写入Tab
                                if (totalIndex == 0)
                                {
                                    string _axis = specItems[i].Axis[AxisElement.MainSide][0].Value;
                                    while (_axis.EndsWith(","))
                                    {
                                        _axis = _axis.Substring(0, _axis.Length - 1);
                                    }
                                    string _hasMean = "False";
                                    if (specItems[i].Mean)
                                    {
                                        _hasMean = "True";
                                    }
                                    string _sideName = "..";
                                    if (_axis != "..")
                                    {
                                        _sideName = "strAxis_" + variable.Name;
                                        tabContent.Set(TabFileContentType.DimVar,
                                            new KeyValuePair<object, object>(FileWriterKeys.TabSideName, _sideName),
                                            new KeyValuePair<object, object>(FileWriterKeys.TabSideAxis, _axis),
                                            new KeyValuePair<object, object>(FileWriterKeys.TabHasMean, _hasMean));
                                        Write(tabContent);
                                    }
                                    tabContent.Set(TabFileContentType.GridWithFunction,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName),
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideAxis, _sideName),
                                        new KeyValuePair<object, object>(FileWriterKeys.TabHasMean, _hasMean),
                                        new KeyValuePair<object, object>(FileWriterKeys.TabTopName, variable.Name));
                                    Write(tabContent);
                                    tabContent.Set(TabFileContentType.GridLabel,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabLabel, " - Summary"));
                                    Write(tabContent);
                                    tabContent.Set(TabFileContentType.GridSliceWithFunction,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName),
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideAxis, _sideName));
                                    Write(tabContent);
                                }
                                // Summary
                                tabContent.Set(TabFileContentType.Grid,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName),
                                    new KeyValuePair<object, object>(FileWriterKeys.TabTopName, variable.Name),
                                    new KeyValuePair<object, object>(FileWriterKeys.TabBaseName, string.Empty));
                                Write(tabContent);
                                // 标题追加标签
                                tabContent.Set(TabFileContentType.GridLabel,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabLabel, " - Summary"));
                                Write(tabContent);
                                // GridSlice
                                tabContent.Set(TabFileContentType.GridSlice,
                                    new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName));
                                Write(tabContent);
                                //
                                if (totalIndex > 0)
                                {
                                    tabContent.Set(TabFileContentType.GridWithFunction,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName),
                                        new KeyValuePair<object, object>(FileWriterKeys.TabTopName, variable.Name));
                                    Write(tabContent);
                                    tabContent.Set(TabFileContentType.GridLabel,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabLabel, " - Summary"));
                                    Write(tabContent);
                                    tabContent.Set(TabFileContentType.GridSliceWithFunction,
                                        new KeyValuePair<object, object>(FileWriterKeys.TabSideName, fullName));
                                    Write(tabContent);
                                }

                            }
                        }
                    }
                    exist.Add(variable.Id);
                }
            }
            logContent.Set(string.Empty,
                new KeyValuePair<object, object>(FileWriterKeys.LogText, "执行完成"));
            Write(logContent);
        }
        /// <summary>
        /// 写入所有表头项和bat脚本内容
        /// </summary>
        /// <param name="topItems"></param>
        public void WriteAll(ITopItemCollection[] topItems)
        {
            if (topItems is null || topItems[0] is null) return;
            for (int i = 0; i < topItems.Length; i++)
            {
                batContent.Set(BatFileContentType.Note,
                    new KeyValuePair<object, object>(FileWriterKeys.Notes, topItems[i].Name));
                Write(batContent);
                if (_subGroup)
                {
                    topContent.Set(TopFileContentType.NonSubTitle,
                        new KeyValuePair<object, object>(FileWriterKeys.TopDefinitions, topItems[i]));
                    batContent.Set(BatFileContentType.SubTitle,
                        new KeyValuePair<object, object>(FileWriterKeys.BatHeaderItems, topItems[i]),
                        new KeyValuePair<object, object>(FileWriterKeys.BatTableName, $"Table_{i + 1}"));
                }
                else
                {
                    topContent.Set(TopFileContentType.SubTitle,
                        new KeyValuePair<object, object>(FileWriterKeys.TopDefinitions, topItems[i]));
                    batContent.Set(BatFileContentType.Normal,
                        new KeyValuePair<object, object>(FileWriterKeys.BatHeaderItems, topItems[i]),
                        new KeyValuePair<object, object>(FileWriterKeys.BatTableName, $"Table_{i + 1}"));
                }
                Write(topContent);
                Write(batContent);
            }
        }
        /// <summary>
        /// 写入所有list在MDD_Manipulation文件中的标签修改语句
        /// </summary>
        /// <param name="mdd"></param>
        public void WriteAllList(IMddDocument mdd)
        {
            if (mdd.ListFields.Count == 0) return;
            foreach (var list in mdd.ListFields)
            {
                if (list.CodeList.Count == 0) continue;
                mddContent.Set(MddFileContentType.Note, new KeyValuePair<object, object>(FileWriterKeys.Notes, list.Name));
                Write(mddContent);
                foreach (var code in list.CodeList)
                {
                    mddContent.Set(MddFileContentType.ListText,
                        new KeyValuePair<object, object>(FileWriterKeys.MddVariableName, list.Name),
                        new KeyValuePair<object, object>(FileWriterKeys.MddCodeName, code.Name),
                        new KeyValuePair<object, object>(FileWriterKeys.MddCodeLabel, code.Label));
                    Write(mddContent);
                }
            }
        }

    }
}
