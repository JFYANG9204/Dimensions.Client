using Dimensions.Bll.Generic;
using Dimensions.Bll.Mdd;
using Dimensions.Bll.String;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dimensions.Bll.Spec
{
    public delegate void SpecLoadEventHandler(string section, double progress);

    public class SpecDocument
    {
        public SpecDocument(
            bool subgroup = false, 
            SpecRemoveHeaderRowType specRemoveHeaderRowType = SpecRemoveHeaderRowType.Auto, 
            SpecAnalysisType specAnalysisType = SpecAnalysisType.Normal,
            HeaderAnalysisType headerAnalysisType = HeaderAnalysisType.Normal,
            int headerRow = 0
            )
        {
            SubGroup = subgroup;
            _removeHeaderRowType = specRemoveHeaderRowType;
            _headerRowCount = headerRow;
            _specSheetKeys = _defaultSpecSheetKeys;
            _headerSheetKeys = _defaultHeaderSheetKeys;
            _netKeys = _defaultNetKeys;
            _topbottomKeys = _defaultTopBottomKeys;
            _averageKeys = _defaultAverageKeys;
            _highFactorKeys = _defaultHighFactorKeys;

            _specAnalysisType = specAnalysisType;
            _headerAnalysisType = headerAnalysisType;
            _fixedLabelKeys = _defaultFixedLabelKeys;
            _fixedNameKeys = _defaultFixedNameKeys;
            _fixedBaseKeys = _defaultFixedBaseKeys;
            _fixedTopBottomKeys = _defaultFixedTBKeys;
            _fixedMeanKeys = _defaultFixedMeanKeys;
            _fixedAvgKeys = _defaultAverageKeys;
            _fixedSummaryKeys = _defaultFixedSummaryKeys;
            _fixedValueKeys = _defaultFixedValueKeys;
            _fixedRemarkKeys = _defaultFixedRemarkKeys;
        }

        private string[] _specSheetKeys;
        private string[] _headerSheetKeys;
        private string[] _netKeys;
        private string[] _topbottomKeys;
        private string[] _averageKeys;
        private string[] _highFactorKeys;

        private readonly string[] _defaultSpecSheetKeys = { "spec", "表头" };
        private readonly string[] _defaultHeaderSheetKeys = { "header", "topbreak", "banner", "表侧" };
        private readonly string[] _defaultHighFactorKeys = { "increase", "enhance", "improve", "raise", "heighten",
            "增加", "提高", "首选", "非常相符", "非常了解", "唯一考虑", "首先考虑", "唯一", "第一选择", "第一" };
        private readonly string[] _defaultNetKeys = { "net", "nets" };
        private readonly string[] _defaultTopBottomKeys = { "t2b", "top2", "t3b", "top3", "b2b", "bottom2",
            "b3b", "bottom3", "nps", "topbox", "tb" };
        private readonly string[] _defaultAverageKeys = { "average", "平均提及", "均值", "mean" };

        private string[] _fixedLabelKeys;
        private string[] _fixedNameKeys;
        private string[] _fixedBaseKeys;
        private string[] _fixedTopBottomKeys;
        private string[] _fixedMeanKeys;
        private string[] _fixedAvgKeys;
        private string[] _fixedSummaryKeys;
        private string[] _fixedValueKeys;
        private string[] _fixedRemarkKeys;

        private readonly string[] _defaultFixedLabelKeys = { "名字", "name", "title" };
        private readonly string[] _defaultFixedNameKeys = { "题号", "question", "qn" };
        private readonly string[] _defaultFixedBaseKeys = { "base", "基数" };
        private readonly string[] _defaultFixedTBKeys = { "top", "bottom", "top/bottom" };
        private readonly string[] _defaultFixedMeanKeys = { "mean", "std", "mean/std" };
        private readonly string[] _defaultFixedAvgKeys = { "average", "平均提及" };
        private readonly string[] _defaultFixedSummaryKeys = { "summary" };
        private readonly string[] _defaultFixedValueKeys = { "value", "赋值" };
        private readonly string[] _defaultFixedRemarkKeys = { "note", "remark" };

        public string[] HeaderSheetNames { get; private set; }
        public string[] SpecSheetNames { get; private set; }

        public event SpecLoadEventHandler OnReportLoadProgress;

        public void SetKeys(SpecKeys keys, string[] value)
        {
            switch (keys)
            {
                case SpecKeys.SpecSheetKeys:
                    _specSheetKeys = value;
                    break;
                case SpecKeys.SpecHeaderKeys:
                    _headerSheetKeys = value;
                    break;
                case SpecKeys.HighFactorKeys:
                    _highFactorKeys = value;
                    break;
                case SpecKeys.NetKeys:
                    _netKeys = value;
                    break;
                case SpecKeys.TopBottomKeys:
                    _topbottomKeys = value;
                    break;
                case SpecKeys.AverageKeys:
                    _averageKeys = value;
                    break;
                case SpecKeys.FixedLabelKeys:
                    _fixedLabelKeys = value;
                    break;
                case SpecKeys.FixedNameKeys:
                    _fixedNameKeys = value;
                    break;
                case SpecKeys.FixedBaseKeys:
                    _fixedBaseKeys = value;
                    break;
                case SpecKeys.FixedMeanKeys:
                    _fixedMeanKeys = value;
                    break;
                case SpecKeys.FixedAverageKeys:
                    _fixedAvgKeys = value;
                    break;
                case SpecKeys.FixedTopBottomKeys:
                    _fixedTopBottomKeys = value;
                    break;
                case SpecKeys.FixedSummaryKeys:
                    _fixedSummaryKeys = value;
                    break;
                case SpecKeys.FixedRemarkKeys:
                    _fixedRemarkKeys = value;
                    break;
                case SpecKeys.FixedValueKeys:
                    _fixedValueKeys = value;
                    break;
                default:
                    break;
            }
        }

        internal SheetType SetSheetName(string sheetName)
        {
            for (int i = 0; i < _specSheetKeys.Length; i++)
            {
                if (sheetName.ToLower().Contains(_specSheetKeys[i].ToLower()))
                {
                    if (SpecSheetNames is null)
                    {
                        SpecSheetNames = new string[1];
                    }
                    if (string.IsNullOrEmpty(SpecSheetNames[SpecSheetNames.Length - 1]))
                    {
                        SpecSheetNames[SpecSheetNames.Length - 1] = sheetName;
                        return SheetType.Spec;
                    }
                    else
                    {
                        SpecSheetNames = SpecSheetNames.Append(sheetName).ToArray();
                        return SheetType.Spec;
                    }
                }
            }
            //
            for (int i = 0; i < _headerSheetKeys.Length; i++)
            {
                if (sheetName.Replace(" ", "").Replace("_", "").ToLower().Contains(_headerSheetKeys[i].ToLower()))
                {
                    if (HeaderSheetNames is null)
                    {
                        HeaderSheetNames = new string[1];
                    }
                    if (string.IsNullOrEmpty(HeaderSheetNames[HeaderSheetNames.Length - 1]))
                    {
                        HeaderSheetNames[HeaderSheetNames.Length - 1] = sheetName;
                        return SheetType.Header;
                    }
                    else
                    {
                        HeaderSheetNames = HeaderSheetNames.Append(sheetName).ToArray();
                        return SheetType.Header;
                    }
                }
            }
            //
            return SheetType.Default;
        }

        public DataSet SpecRawData { get; private set; }
        public DataSet SpecCurrentData { get; private set; }

        public NewVariableDefinition[] NewVariables { get; private set; }
        public ISpecItem[] SpecItems { get; private set; }
        public ITopItemCollection[] Top { get; private set; }
        public bool SubGroup { get; private set; }

        public string Path { get; private set; }

        private readonly SpecRemoveHeaderRowType _removeHeaderRowType;
        private readonly SpecAnalysisType _specAnalysisType;
        private readonly HeaderAnalysisType _headerAnalysisType;
        private readonly int _headerRowCount;

        public void Load(string path)
        {
            SpecRawData = new DataSet("RawData");
            Application app = new Application()
            {
                Visible = false,
                DisplayAlerts = false
            };
            try
            {
                Path = path;
                Workbook workbook = app.Workbooks.Open(path);
                //
                foreach (Worksheet sheet in workbook.Sheets)
                {
                    bool usedSheet = false;
                    if (StringArrayFunction.StringHasKey(sheet.Name.Replace(" ", "").Replace("_", "").Replace("-", ""), _specSheetKeys))
                    {
                        usedSheet = true;
                        if (SpecSheetNames is null)
                            SpecSheetNames = new string[1];
                        if (string.IsNullOrEmpty(SpecSheetNames[0]))
                            SpecSheetNames[0] = sheet.Name;
                        else
                            SpecSheetNames = SpecSheetNames.Append(sheet.Name).ToArray();
                    }
                    if (StringArrayFunction.StringHasKey(sheet.Name.Replace(" ", "").Replace("_", "").Replace("-", ""), _headerSheetKeys))
                    {
                        usedSheet = true;
                        if (HeaderSheetNames is null)
                            HeaderSheetNames = new string[1];
                        if (string.IsNullOrEmpty(HeaderSheetNames[0]))
                            HeaderSheetNames[0] = sheet.Name;
                        else
                            HeaderSheetNames = HeaderSheetNames.Append(sheet.Name).ToArray();
                    }
                    if (!usedSheet) continue;
                    //
                    System.Data.DataTable dt = new System.Data.DataTable(sheet.Name);
                    //
                    int allRows = sheet.UsedRange.Rows.Count;
                    int allCols = sheet.UsedRange.Columns.Count;
                    //
                    for (int i = 1; i <= allCols; i++)
                        dt.Columns.Add(GetCharactorColumnName(i));
                    //
                    int allCells = allRows * allCols;
                    int cellCount = 0;
                    //
                    for (int row = 1; row <= allRows; row++)
                    {
                        if (row > dt.Rows.Count)
                        {
                            dt.Rows.Add();
                        }
                        //
                        for (int col = 1; col <= allCols; col++)
                        {
                            dt.Rows[row - 1][col - 1] = sheet.Cells[row, col].Value;
                            OnReportLoadProgress?.Invoke("读取原始Sheet: " + sheet.Name, Math.Round((double)(++cellCount) / allCells * 100, 0));
                        }
                    }
                    SpecRawData.Tables.Add(dt);
                }
                //
                workbook.Close();
                workbook = null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                app.Quit();
                app = null;
            }
            //
            if (StringArrayFunction.HasCommonItem(HeaderSheetNames, SpecSheetNames, out string[] names))
            {
                string message = string.Join(",", names);
                throw new Exception($"有Sheet同时被归入Header和Spec，相关Sheet名: {message}");
            }
            FormatDataSet(SpecRawData, _specAnalysisType, _headerAnalysisType);
            //
            SpecCurrentData = new DataSet("CurrentData");
            int allSheetsCount = SpecRawData.Tables.Count;
            int sheetCount = 0;
            if (HeaderSheetNames != null)
            {
                for (int i = 0; i < HeaderSheetNames.Length; i++)
                {
                    if (_headerAnalysisType == HeaderAnalysisType.Normal)
                        SpecCurrentData.Tables.Add(AnalysisTopDataTable(SpecRawData.Tables[HeaderSheetNames[i]]));
                    else
                        SpecCurrentData.Tables.Add(AnalysisTopDataTableWithHorizenList(SpecRawData.Tables[HeaderSheetNames[i]]));
                    OnReportLoadProgress?.Invoke("自动解析Sheet内容: " + HeaderSheetNames[i], Math.Round((double)(++sheetCount) / allSheetsCount * 100, 0));
                }
            }
            if (SpecSheetNames != null)
            {
                for (int j = 0; j < SpecSheetNames.Length; j++)
                {
                    if (_specAnalysisType == SpecAnalysisType.Normal)
                        SpecCurrentData.Tables.Add(AnalysisSpecDataTable(SpecRawData.Tables[SpecSheetNames[j]]));
                    else
                        SpecCurrentData.Tables.Add(AnalysisSpecSheetWithFixedHeader(SpecRawData.Tables[SpecSheetNames[j]]));
                    OnReportLoadProgress?.Invoke("自动解析Sheet内容: " + SpecSheetNames[j], Math.Round((double)(++sheetCount) / allSheetsCount * 100, 0));
                }
            }
        }

        private List<string> _existVarId;

        public void LoadSpecItem(IMddDocument mdd)
        {
            _existVarId = new List<string>();
            SpecItems = new ISpecItem[1];
            NewVariables = new NewVariableDefinition[1];
            for (int i = 0; i < SpecSheetNames.Length; i++)
            {
                System.Data.DataTable spec = SpecCurrentData.Tables[SpecSheetNames[i]];
                ISpecItem item;
                for (int row = 0; row < spec.Rows.Count; row++)
                {
                    string varStr = spec.Rows[row][ColumnNames.SpecQuestionName].ToString();
                    string basStr = spec.Rows[row][ColumnNames.SpecBaseName].ToString();
                    string ttlStr = spec.Rows[row][ColumnNames.SpecTitleName].ToString();
                    string netStr = spec.Rows[row][ColumnNames.SpecNetName].ToString();
                    string tbbStr = spec.Rows[row][ColumnNames.SpecTopBottomColumn].ToString();
                    string meanStr = spec.Rows[row][ColumnNames.SpecMeanName].ToString();
                    string rmkStr = spec.Rows[row][ColumnNames.SpecRemarkColumn].ToString();
                    //
                    //
                    if (!string.IsNullOrEmpty(varStr) && !string.IsNullOrEmpty(ttlStr))
                    {
                        item = new SpecItem(_highFactorKeys);
                        item.SetProperty(mdd);
                        ttlStr = varStr + "." + ttlStr;
                        //
                        // 查询变量部分
                        //
                        string varName = varStr;
                        bool isPyramid = false;
                        //
                        if (varName.Contains("+"))
                        {
                            mdd.AddMergedVariable(varName, ttlStr);
                            varName = varName.Replace("+", "_");
                        }
                        //
                        if (!string.IsNullOrEmpty(rmkStr) && AnalysisPyramidDefinition(rmkStr, out _, out _))
                        {
                            NewVariableDefinition newVariable = new NewVariableDefinition();
                            newVariable.SetProperty(true);
                            string[] pyDef = rmkStr.Split('\n');
                            ICodeList topList = new CodeList();
                            for (int j = 0; j < pyDef.Length; j++)
                            {
                                string[] splitDef = null;
                                if (Regex.Match(pyDef[j], @"[a-zA-Z]+\d*\s*:\s*[\u4E00-\u9FA5\w]+").Success)
                                    splitDef = pyDef[j].Split(':');
                                if (Regex.Match(pyDef[j], @"[a-zA-Z]+\d*\s*.\s*[\u4E00-\u9FA5\w]+").Success)
                                    splitDef = pyDef[j].Split('.');
                                if (splitDef != null && splitDef.Length == 2)
                                {
                                    string vName = splitDef[0];
                                    string vLabel = splitDef[1];
                                    if (vName.Contains("+") && mdd.Find(vName.Replace("+", "_")) is null)
                                    {
                                        mdd.AddMergedVariable(vName, vLabel);
                                    }
                                    IMddVariable findVar = mdd.Find(vName);
                                    newVariable.SetProperty($"Brand_Pyramid_{varStr.Replace(" ", "").Replace("-", "_").Replace("+", "_")}", ttlStr);
                                    if (findVar != null)
                                    {
                                        if (topList.Count == 0) topList = findVar.CodeList;
                                        else if (topList.Count > findVar.CodeList.Count) topList = findVar.CodeList;
                                        newVariable.AddVariable(findVar, vName.Replace("+", "_").Replace("-", "_"), vLabel);
                                    }
                                    newVariable.SetProperty(topList, NewVariableCodeListType.Top);
                                    newVariable.SetProperty(vName.Replace("+", "_").Replace("-", "_"), vLabel, NewVariableCodeListType.Side);
                                }
                            }
                            item.SetProperty(newVariable);
                            isPyramid = true;
                        }
                        //
                        if (!isPyramid)
                        {
                            IMddVariable findVariable = mdd.Find(varName);
                            if (findVariable is null)
                            {
                                IMddVariableCollection vars = mdd.FindAll(varName);
                                bool hasLoop = false;
                                if (vars.Count > 0)
                                {
                                    foreach (var variable in vars)
                                    {
                                        if (variable.VariableType == VariableType.Loop && !_existVarId.Contains(variable.Id))
                                        {
                                            findVariable = variable;
                                            _existVarId.Add(variable.Id);
                                            hasLoop = true;
                                        }
                                    }
                                }
                                if (!hasLoop)
                                {
                                    findVariable = mdd.FindWithRegex(varName);
                                    if (findVariable != null)
                                        _existVarId.Add(findVariable.Id);
                                    else
                                    {
                                        item = new SpecItem(_highFactorKeys);
                                        item.SetProperty(mdd);
                                        item.SetProperty(varName, ttlStr);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                _existVarId.Add(findVariable.Id);
                            }
                            item.SetProperty(findVariable);
                        }
                        //
                        item.SetProperty(varStr, ttlStr);
                        if (!string.IsNullOrWhiteSpace(basStr))
                        {
                            if (basStr.Contains("\n"))
                            {
                                string[] _bsArray = StringArrayFunction.RemoveArraySerialNumber(basStr.Split('\n'));
                                ISpecItem.BaseItem[] bases = new ISpecItem.BaseItem[_bsArray.Length];
                                for (int j = 0; j < _bsArray.Length; j++)
                                {
                                    bases[j] = new ISpecItem.BaseItem(_bsArray[j], mdd.GetFullDefinition(_bsArray[j]));
                                }
                                item.SetProperty(bases);
                            }
                            else
                                item.SetProperty(new ISpecItem.BaseItem[1] { new ISpecItem.BaseItem(basStr, string.Empty) });
                        }
                        if (!string.IsNullOrEmpty(tbbStr))
                            item.SetProperty(tbbStr);
                        if (!string.IsNullOrEmpty(netStr))
                        {
                            string[] nets = netStr.Split('\n');
                            for (int j = 0; j < nets.Length; j++)
                            {
                                string[] ntItem = null;
                                if (Regex.Match(nets[j], @"([\u4E00-\u9FA5\w\s_\-\:]+(\s*=\s*)[0-9\/\-.]+)").Success)
                                    ntItem = nets[j].Split('=');
                                if (Regex.Match(nets[j], @"([\u4E00-\u9FA5\w\s_\-\:]+\s*:\s*[0-9\-\/.]+)").Success)
                                    ntItem = nets[j].Split(':');
                                if (Regex.Match(nets[j], @"([\u4E00-\u9FA5\w\s_\-\:]+\s*：\s*[0-9\-\/.]+)").Success)
                                    ntItem = nets[j].Split('：');
                                if (ntItem != null && ntItem.Length == 2)
                                    item.AddNet(ntItem[0], ntItem[1]);
                            }
                        }
                        if (!string.IsNullOrEmpty(meanStr))
                            item.SetProperty(true);
                        else
                            item.SetProperty(false);
                        //
                        // 添加内容
                        //
                        if (SpecItems[0] is null)
                            SpecItems[0] = item;
                        else
                            SpecItems = SpecItems.Append(item).ToArray();
                    }
                    else if (DataTableRowNotEmptyCellCount(spec, row, out string onlyContent) == 1)
                    {
                        item = new SpecItem(_highFactorKeys);
                        item.SetProperty(mdd);
                        item.SetNotes(onlyContent);
                        //
                        // 添加内容
                        //
                        if (SpecItems[0] is null)
                            SpecItems[0] = item;
                        else
                            SpecItems = SpecItems.Append(item).ToArray();
                    }
                }
            }
        }

        public void LoadTopItem(IMddDocument mdd, bool subGroup = false)
        {
            SubGroup = subGroup;
            Top = new ITopItemCollection[1];
            int index = 1;
            int count = 0;
            int codeCount = 0;
            bool hasFirstHeader = false;
            string group = string.Empty;
            if (HeaderSheetNames is null)
            {
                return;
            }
            bool addTotal = false;
            for (int i = 0; i < HeaderSheetNames.Length; i++)
            {
                System.Data.DataTable topSheet = SpecCurrentData.Tables[HeaderSheetNames[i]];
                ITopItemCollection collection = new TopItemCollection($"TopBreak_{++count}", index, SubGroup);
                collection.Set(mdd.Record);
                for (int row = 0; row < topSheet.Rows.Count; row++)
                {
                    string tbStr = topSheet.Rows[row][ColumnNames.TopHeaderNames].ToString();
                    string gpStr = topSheet.Rows[row][ColumnNames.TopGroupName].ToString();
                    string dpStr = topSheet.Rows[row][ColumnNames.TopDescriptionName].ToString();
                    string dfStr = topSheet.Rows[row][ColumnNames.TopDefinitionName].ToString();
                    if (!addTotal)
                    {
                        // 添加Total
                        ITopItem total = new TopItem();
                        total.SetProperty("T", "Total");
                        total.SetProperty($"{mdd.Record} > 0");
                        collection.Add(total, string.Empty);
                        addTotal = true;
                    }
                    //
                    if (!string.IsNullOrEmpty(tbStr) && hasFirstHeader)
                    {
                        codeCount = 0;
                        group = string.Empty;
                        if (Top[0] is null)
                            Top[0] = collection;
                        else
                            Top = Top.Append(collection).ToArray();
                        collection = new TopItemCollection($"TopBreak_{++count}", ++index, SubGroup);
                        collection.Set(mdd.Record);
                        addTotal = false;
                    }
                    if (!string.IsNullOrEmpty(dfStr) && !string.IsNullOrEmpty(dpStr))
                    {
                        if (!string.IsNullOrEmpty(gpStr))
                        {
                            group = gpStr;
                        }
                        if (!hasFirstHeader)
                        {
                            hasFirstHeader = true;
                        }
                        ITopItem topItem = new TopItem();
                        codeCount++;
                        string code = codeCount > 9 ? $"_{codeCount}" : $"_0{codeCount}";
                        topItem.SetProperty(code, dpStr);
                        topItem.SetProperty(mdd.GetFullDefinition(dfStr));
                        collection.Add(topItem, group);
                    }
                    if (row == topSheet.Rows.Count - 1)
                    {
                        if (Top[0] is null)
                            Top[0] = collection;
                        else
                            Top = Top.Append(collection).ToArray();
                    }
                }
            }
        }
        /// <summary>
        /// 按列用原始数据替换现有数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="sourceCl">原始数据列名</param>
        /// <param name="targetCl">现有数据列名</param>
        public void SetCurrentColumnData(string tableName, string sourceCl, string targetCl)
        {
            if (!SpecRawData.Tables.Contains(tableName)) return;
            if (!SpecCurrentData.Tables.Contains(tableName)) return;
            if (!SpecRawData.Tables[tableName].Columns.Contains(sourceCl)) return;
            if (!SpecCurrentData.Tables[tableName].Columns.Contains(targetCl)) return;

            for (int i = 0; i < SpecCurrentData.Tables[tableName].Rows.Count; i++)
            {
                if (i < SpecRawData.Tables[tableName].Rows.Count)
                    SpecCurrentData.Tables[tableName].Rows[i][targetCl] = SpecRawData.Tables[tableName].Rows[i][sourceCl].ToString();
                else
                    SpecCurrentData.Tables[tableName].Rows[i][targetCl] = string.Empty;
            }
        }
        /// <summary>
        /// 修改现有数据内容
        /// </summary>
        /// <param name="tableName">数据表名称</param>
        /// <param name="row">行号</param>
        /// <param name="col">列号</param>
        /// <param name="value">更新值</param>
        public void SetCurrentData(string tableName, int row, int col, string value)
        {
            if (!SpecCurrentData.Tables.Contains(tableName)) return;
            if (row >= SpecCurrentData.Tables[tableName].Rows.Count) return;
            if (col >= SpecCurrentData.Tables[tableName].Columns.Count) return;
            SpecCurrentData.Tables[tableName].Rows[row][col] = value;
        }

        public void Clear()
        {
            if (!(SpecRawData is null))
            {
                SpecRawData.Clear();
                SpecRawData = null;
            }
            if (!(SpecCurrentData is null))
            {
                SpecCurrentData.Clear();
                SpecCurrentData = null;
            }
            HeaderSheetNames = null;
            SpecSheetNames = null;
        }

        //
        //
        //

        private void FormatDataSet(DataSet ds, SpecAnalysisType fixType, HeaderAnalysisType headerType)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                System.Data.DataTable dt = ds.Tables[i];
                //
                if (fixType == SpecAnalysisType.Fixed && SpecSheetNames.Contains(dt.TableName)) continue;
                if (headerType == HeaderAnalysisType.Horizen && HeaderSheetNames.Contains(dt.TableName)) continue;
                int col = dt.Columns.Count - 1;
                int cellCount = 0;
                while (col >= 0)
                {
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[row][col].ToString()))
                            cellCount++;
                    }
                    if (cellCount < 2 || IsSigIdColumn(dt, col))
                        dt.Columns.RemoveAt(col);
                    col--;
                }
                //
                int totalRows = dt.Rows.Count - 1;
                cellCount = 0;
                while (totalRows >= 0)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[totalRows][j].ToString()))
                            cellCount++;
                    }
                    if (cellCount == 0)
                        dt.Rows.RemoveAt(totalRows);
                    totalRows--;
                }
                // 移除表头及以上内容
                if (_removeHeaderRowType == SpecRemoveHeaderRowType.Auto || HeaderSheetNames.Contains(dt.TableName))
                {
                    int lrow = 0;
                    int lCol = dt.Columns.Count - 1;
                    int srow = -1;
                    List<int> remove = new List<int>();
                    while (lrow < dt.Rows.Count)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[lrow][lCol].ToString()) &&
                            !string.IsNullOrEmpty(dt.Rows[lrow][lCol - 1].ToString()))
                        {
                            if (HeaderSheetNames != null && 
                                HeaderSheetNames.Contains(dt.TableName) && 
                                DataTableRowNotEmptyCellCount(dt, lrow, out _) == 1)
                            {
                                srow = lrow + 1;
                                if (remove.Count >= 1) remove.RemoveAt(remove.Count - 1);
                            }
                            else
                            {
                                remove.Add(lrow);
                            }
                            break;
                        }
                        remove.Add(lrow);
                        lrow++;
                    }
                    if (srow > -1)
                    {
                        DataTableRemoveSameRows(dt, srow);
                    }
                    //
                    int index = remove.Count - 1;
                    if (remove.Count > 0)
                    {
                        while (index >= 0)
                        {
                            dt.Rows.RemoveAt(remove[index]);
                            index--;
                        }
                    }
                }
                else
                {
                    if (_headerRowCount == 0) continue;
                    int index = _headerRowCount - 1;
                    while (index >= 0)
                    {
                        dt.Rows.RemoveAt(index);
                        index--;
                    }
                }
                DataTableRemoveEmptyRows(dt);
            }
        }

        private bool IsSigIdColumn(System.Data.DataTable dt, int col)
        {
            int count = 0;
            int notEmptyCount = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][col].ToString().Length == 1 && Regex.IsMatch(dt.Rows[i][col].ToString(), @"[a-zA-Z]"))
                {
                    count++;
                }
                if (!string.IsNullOrEmpty(dt.Rows[i][col].ToString()))
                {
                    notEmptyCount++;
                }
            }
            if (count >= dt.Rows.Count / 2 || count >= (notEmptyCount * 3 / 4))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetCharactorColumnName(int index)
        {
            string result = string.Empty;

            if (index <= 26) return Convert.ToChar(64 + index).ToString();
            if (index > 26 && index <= 26 * 27) return Convert.ToChar(64 + (index - 1) / 26).ToString() + Convert.ToChar(64 + (index % 26 == 0 ? 26 : index % 26)).ToString();

            return result;
        }

        private int DataTableRowNotEmptyCellCount(System.Data.DataTable dt, int row, out string onlyContent)
        {
            int count = 0;
            //
            onlyContent = string.Empty;
            int colIndex = -1;
            //
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                if (!string.IsNullOrEmpty(dt.Rows[row][col].ToString()))
                {
                    count++;
                    colIndex = col;
                }
            }
            if (count == 1 && colIndex > -1)
                onlyContent = dt.Rows[row][colIndex].ToString();
            return count;
        }

        private int DataTableColumnMaxCommonCellContent(System.Data.DataTable dt, int col)
        {
            int count = 0;
            Dictionary<string, int> countDic = new Dictionary<string, int>();
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                string cellContent = dt.Rows[row][col].ToString();
                if (!string.IsNullOrEmpty(cellContent))
                {
                    if (!countDic.ContainsKey(cellContent))
                        countDic.Add(cellContent, 1);
                    else
                        countDic[cellContent]++;
                }
            }
            foreach (var key in countDic.Keys)
            {
                if (countDic[key] > count)
                    count = countDic[key];
            }
            return count;
        }

        private string[] GetNetContent(string remark)
        {
            // string[0]: Top/Bottom
            // string[1]: Net
            // string[2]: net / value
            string[] netResult = new string[3];
            remark = remark.Replace("\\", "/");
            string pattern;
            if (remark.Contains("="))
                pattern = @"([\u4E00-\u9FA5\w\s_\-\:]+(\s*=\s*)[0-9\/\-.]+)";
            else
                pattern = @"([\u4E00-\u9FA5\w\s_\-\:]+(\s*(:|：){1})\s*[0-9\-\/.]+)";
            while (remark.Contains(" / "))
                remark = remark.Replace(" / ", "/");
            while (remark.Contains(" /"))
                remark = remark.Replace(" /", "/");
            while (remark.Contains("/ "))
                remark = remark.Replace("/ ", "/");
            if (StringArrayFunction.StringHasKey(remark, _topbottomKeys))
                netResult[0] = StringArrayFunction.GetCommonPartInArray(remark, _topbottomKeys);
            //
            bool hasMultiCode = false;
            MatchCollection mathes = Regex.Matches(remark, pattern);
            if (mathes.Count > 0)
            {
                netResult[1] = string.Empty;
                for (int i = 0; i < mathes.Count; i++)
                {
                    if (Regex.IsMatch(mathes[i].Value, @".*[0-9]+\s*/\s*[0-9]+.*") || Regex.IsMatch(mathes[i].Value, @".*[0-9]+\s*-\s*[0-9]+.*"))
                        hasMultiCode = true;
                    if (i == 0 || (!string.IsNullOrEmpty(mathes[i].Value) && mathes[i].Value.Substring(mathes[i].Value.Length - 1) == "\n"))
                        netResult[1] += mathes[i].Value;
                    else
                        netResult[1] += "\n" + mathes[i].Value;
                }
                if (hasMultiCode)
                    netResult[2] = "net";
                else
                    netResult[2] = "value";
            }
            //
            return netResult;
        }

        private bool AnalysisPyramidDefinition(string source, out string formattedString, out CodeList codes)
        {
            bool checkResult = false;
            formattedString = string.Empty;
            codes = new CodeList();
            //
            if (!source.Contains("\n"))
            {
                return false;
            }
            string[] setting = source.Split('\n');
            for (int i = 0; i < setting.Length; i++)
            {
                if (Regex.IsMatch(setting[i], @"[a-zA-Z]+\d*\s*(:|\.){1}\s*[\u4E00-\u9FA5\w]+"))
                {
                    formattedString += setting[i] + "\n";
                    int index = -1;
                    if (setting[i].Contains(":"))
                        index = setting[i].IndexOf(':');
                    else if (setting[i].Contains("."))
                        index = setting[i].IndexOf('.');
                    if (index > -1)
                    {
                        string name = setting[i].Substring(0, index).Replace("-", "_").Replace("+", "_");
                        string label = StringFunction.FormatString(setting[i].Substring(index + 1));
                        codes.Add(new Categorical(name, label));
                    }
                    checkResult = true;
                }
            }
            if (formattedString.Length > 0)
                formattedString = formattedString.Substring(0, formattedString.Length - 1);
            //
            return checkResult;
        }

        private System.Data.DataTable AnalysisTopDataTable(System.Data.DataTable dt)
        {
            System.Data.DataTable rt = new System.Data.DataTable(dt.TableName);
            //
            rt.Columns.Add(ColumnNames.TopHeaderNames);
            rt.Columns.Add(ColumnNames.TopGroupName);
            rt.Columns.Add(ColumnNames.TopDescriptionName);
            rt.Columns.Add(ColumnNames.TopDefinitionName);
            //
            int[,] matrix = new int[dt.Columns.Count, 2];
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                matrix[col, 0] = 0;
                matrix[col, 1] = 0;
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    string cellContent = dt.Rows[row][col].ToString();
                    if (!string.IsNullOrEmpty(cellContent))
                    {
                        if (cellContent.Length > 1)
                            matrix[col, 0]++;
                        if (Regex.Match(cellContent, @"[A-Za-z]+[a-zA-Z0-9_.\-]*\s*=\s*[0-9]+[0-9&/\\\s\-]*").Success)
                            matrix[col, 1]++;
                    }
                }
            }
            //
            int groupIndex = 0;
            int descriptionIndex = 0;
            int filterIndex = 0;
            int headerIndex = -1;
            // matrix第1列最大值对应filter列
            // matrix第0列最大值对应description列
            // matrix第0列第二大值对应group列
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (matrix[i, 1] > matrix[filterIndex, 1])
                    filterIndex = i;
            }
            //
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i != filterIndex)
                {
                    if (matrix[i, 0] > matrix[descriptionIndex, 0])
                    {
                        groupIndex = descriptionIndex;
                        descriptionIndex = i;
                    }
                }
            }
            //
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i != filterIndex && i != descriptionIndex && i != groupIndex && matrix[i, 0] > 1)
                {
                    if (headerIndex == -1)
                        headerIndex = i;
                    else if (matrix[i, 0] > matrix[headerIndex, 0])
                        headerIndex = i;
                }
            }
            //
            int rowCount = 0;
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                rt.Rows.Add();
                if (DataTableRowNotEmptyCellCount(dt, row, out string content) == 1)
                    rt.Rows[rowCount][ColumnNames.TopHeaderNames] = content;
                else if (headerIndex != -1)
                    rt.Rows[rowCount][ColumnNames.TopHeaderNames] = dt.Rows[row][headerIndex].ToString();
                if (groupIndex > -1 && rt.Rows[rowCount][ColumnNames.TopHeaderNames].ToString() != dt.Rows[row][groupIndex].ToString())
                    rt.Rows[rowCount][ColumnNames.TopGroupName] = dt.Rows[row][groupIndex].ToString();
                if (descriptionIndex > -1)
                    rt.Rows[rowCount][ColumnNames.TopDescriptionName] = dt.Rows[row][descriptionIndex].ToString();
                if (filterIndex > -1)
                    rt.Rows[rowCount][ColumnNames.TopDefinitionName] = dt.Rows[row][filterIndex].ToString();
                if (!string.IsNullOrEmpty(rt.Rows[rowCount][ColumnNames.TopGroupName].ToString()) &&
                    !string.IsNullOrEmpty(rt.Rows[rowCount][ColumnNames.TopDefinitionName].ToString()) &&
                    string.IsNullOrEmpty(rt.Rows[rowCount][ColumnNames.TopDescriptionName].ToString()))
                    rt.Rows[rowCount][ColumnNames.TopDescriptionName] = rt.Rows[rowCount][ColumnNames.TopGroupName].ToString();
                rowCount++;
            }
            //
            DataTableRemoveEmptyRows(rt);
            return rt;
        }

        private System.Data.DataTable AnalysisSpecDataTable(System.Data.DataTable dt)
        {
            System.Data.DataTable rt = new System.Data.DataTable(dt.TableName);
            //
            rt.Columns.Add(ColumnNames.SpecQuestionName);
            rt.Columns.Add(ColumnNames.SpecTitleName);
            rt.Columns.Add(ColumnNames.SpecBaseName);
            rt.Columns.Add(ColumnNames.SpecMeanName);
            rt.Columns.Add(ColumnNames.SpecNetName);
            rt.Columns.Add(ColumnNames.SpecTopBottomColumn);
            rt.Columns.Add(ColumnNames.SpecValueColumn);
            rt.Columns.Add(ColumnNames.SpecRemarkColumn);
            //
            // matrix 列判别矩阵
            // 0 - 题号，由正则表达式判别
            // 1 - 最大相同字符的个数
            // 2 - net
            // 3 - average
            // 4 - 非空单元格个数
            // 5 - Pyramid
            // 6 - 纯数字个数，用来排除序号列
            int[,] matrix = new int[dt.Columns.Count, 7];
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                matrix[col, 1] = DataTableColumnMaxCommonCellContent(dt, col);
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    string cellContent = dt.Rows[row][col].ToString();
                    if (!string.IsNullOrEmpty(cellContent))
                    {
                        matrix[col, 4]++;
                        if (cellContent.Length < 6 && Regex.Match(cellContent, @"[A-Za-z]{1,4}[A-Za-z0-9.\s_\-]{0,5}").Success)
                            matrix[col, 0]++;
                        if (StringArrayFunction.StringHasKey(cellContent, _netKeys) || StringArrayFunction.StringHasKey(cellContent, _topbottomKeys))
                            matrix[col, 2]++;
                        if (StringArrayFunction.StringHasKey(cellContent, _averageKeys))
                            matrix[col, 3]++;
                        if (AnalysisPyramidDefinition(cellContent, out _, out _))
                            matrix[col, 5]++;
                        if (string.IsNullOrEmpty(Regex.Replace(cellContent, @"[0-9]+", "")))
                            matrix[col, 6]++;
                    }
                }
            }
            // 首先查找出题号列
            int varCl = matrix.SliceArray(0).MaxValueIndex();
            int idxCl = matrix.SliceArray(6).MaxValueIndex();
            List<int> exclude = new List<int> { varCl, idxCl };
            int netCl = matrix.SliceArray(2).MaxValueIndex(exclude.ToArray());
            int avgCl = matrix.SliceArray(3).MaxValueIndex(exclude.ToArray());
            int ttlCl = matrix.SliceArray(4).MaxValueIndex(exclude.ToArray());
            //
            int pyCl = -1;
            bool hasPyCl = false;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (matrix[i, 5] > 0)
                    hasPyCl = true;
            }
            if (hasPyCl)
            {
                pyCl = matrix.SliceArray(5).MaxValueIndex(exclude.ToArray());
            }
            //
            if (!exclude.Contains(netCl))
                exclude.Add(netCl);
            if (!exclude.Contains(avgCl))
                exclude.Add(avgCl);
            if (!exclude.Contains(ttlCl))
                exclude.Add(ttlCl);
            if (hasPyCl && !exclude.Contains(pyCl))
                exclude.Add(pyCl);
            int bsCl = matrix.SliceArray(1).MaxValueIndex(exclude.ToArray());
            //
            int rowCount = 0;
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                rt.Rows.Add();
                if (DataTableRowNotEmptyCellCount(dt, row, out string content) == 1)
                {
                    rt.Rows[rowCount][ColumnNames.SpecTitleName] = content;
                }
                else
                {
                    if (varCl > -1)
                        rt.Rows[rowCount][ColumnNames.SpecQuestionName] = dt.Rows[row][varCl].ToString();
                    if (ttlCl > -1)
                        rt.Rows[rowCount][ColumnNames.SpecTitleName] = dt.Rows[row][ttlCl].ToString();
                    if (bsCl > -1)
                        rt.Rows[rowCount][ColumnNames.SpecBaseName] = dt.Rows[row][bsCl].ToString();
                    if (!string.IsNullOrEmpty(dt.Rows[row][netCl].ToString()))
                    {
                        string[] netResult = GetNetContent(dt.Rows[row][netCl].ToString());
                        if (netResult[2] == "net")
                            rt.Rows[rowCount][ColumnNames.SpecNetName] = netResult[1];
                        else if (netResult[2] == "value")
                            rt.Rows[rowCount][ColumnNames.SpecValueColumn] = netResult[1];
                        rt.Rows[rowCount][ColumnNames.SpecTopBottomColumn] = netResult[0];
                    }
                    if (avgCl > -1 && StringArrayFunction.StringHasKey(dt.Rows[row][avgCl].ToString(), _averageKeys))
                        rt.Rows[rowCount][ColumnNames.SpecMeanName] = "Y";
                    if (hasPyCl)
                    {
                        AnalysisPyramidDefinition(dt.Rows[row][pyCl].ToString(), out string formattedString, out _);
                        rt.Rows[rowCount][ColumnNames.SpecRemarkColumn] = formattedString;
                    }
                }
                rowCount++;
            }
            //
            DataTableRemoveEmptyRows(rt);
            return rt;
        }

        private System.Data.DataTable AnalysisSpecSheetWithFixedHeader(System.Data.DataTable dt)
        {
            System.Data.DataTable rt = new System.Data.DataTable(dt.TableName);
            //
            rt.Columns.Add(ColumnNames.SpecQuestionName);
            rt.Columns.Add(ColumnNames.SpecTitleName);
            rt.Columns.Add(ColumnNames.SpecBaseName);
            rt.Columns.Add(ColumnNames.SpecMeanName);
            rt.Columns.Add(ColumnNames.SpecNetName);
            rt.Columns.Add(ColumnNames.SpecTopBottomColumn);
            rt.Columns.Add(ColumnNames.SpecValueColumn);
            rt.Columns.Add(ColumnNames.SpecRemarkColumn);
            //
            int ttlCol = -1;
            int varCol = -1;
            int basCol = -1;
            int tbbCol = -1;
            int msdCol = -1;
            int avgCol = -1;
            int sumCol = -1;
            int valCol = -1;
            int rmkCol = -1;
            //
            int headerRow = -1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string content = dt.Rows[i][j].ToString().ToLower();
                    if (StringArrayFunction.StringHasKey(content, _fixedLabelKeys)) ttlCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedNameKeys)) varCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedBaseKeys)) basCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedMeanKeys)) msdCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedAvgKeys)) avgCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedTopBottomKeys)) tbbCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedValueKeys)) valCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedSummaryKeys)) sumCol = j;
                    if (StringArrayFunction.StringHasKey(content, _fixedRemarkKeys)) rmkCol = j;
                }
                if (ttlCol > -1 && varCol > -1 && ttlCol != varCol)
                {
                    headerRow = i;
                    break;
                }
            }
            //
            if (ttlCol == -1 || varCol == -1 || headerRow == -1) return rt;
            int rowCount = 0;
            string pyDef = string.Empty;
            for (int row = headerRow + 1; row < dt.Rows.Count; row++)
            {
                rt.Rows.Add();
                if (DataTableRowNotEmptyCellCount(dt, row, out string content) == 1)
                {
                    rt.Rows[rowCount][ColumnNames.SpecTitleName] = content;
                    pyDef = string.Empty;
                }
                else
                {
                    if (varCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecQuestionName] = dt.Rows[row][varCol].ToString();
                    if (ttlCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecTitleName] = dt.Rows[row][ttlCol].ToString();
                    if (basCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecBaseName] = dt.Rows[row][basCol].ToString();
                    if (avgCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecMeanName] = dt.Rows[row][avgCol].ToString();
                    if (msdCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecMeanName] = dt.Rows[row][msdCol].ToString();
                    if (tbbCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecTopBottomColumn] = dt.Rows[row][tbbCol].ToString();
                    if (valCol > -1)
                        rt.Rows[rowCount][ColumnNames.SpecValueColumn] = dt.Rows[row][valCol].ToString();
                    if (sumCol > -1 && varCol > -1 && ttlCol > -1 && !string.IsNullOrEmpty(dt.Rows[row][sumCol].ToString()))
                    {
                        pyDef += $"{dt.Rows[row][varCol]}.{dt.Rows[row][ttlCol]}\n";
                    }
                    if (rmkCol > -1)
                    {
                        string varContent = dt.Rows[row][varCol].ToString().ToLower();
                        if (varContent.Contains("summary"))
                        {
                            rt.Rows[rowCount][ColumnNames.SpecRemarkColumn] = pyDef;
                            pyDef = string.Empty;
                        }
                        else
                        {
                            rt.Rows[rowCount][ColumnNames.SpecRemarkColumn] = dt.Rows[row][rmkCol].ToString();
                        }
                    }
                }
                rowCount++;
            }
            DataTableRemoveEmptyRows(rt);
            return rt;
        }

        private System.Data.DataTable AnalysisTopDataTableWithHorizenList(System.Data.DataTable dt)
        {
            System.Data.DataTable rt = new System.Data.DataTable(dt.TableName);
            //
            rt.Columns.Add(ColumnNames.TopHeaderNames);
            rt.Columns.Add(ColumnNames.TopGroupName);
            rt.Columns.Add(ColumnNames.TopDescriptionName);
            rt.Columns.Add(ColumnNames.TopDefinitionName);
            //
            List<int> defRows = new List<int>();
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                int notEmptyCount = 0;
                int defCount = 0;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string cellContent = dt.Rows[i][j].ToString();
                    if (Regex.Match(cellContent, @"[A-Za-z]+\S*\s*=\s*[0-9]+[0-9&/\\-]*\S*\s*").Success) defCount++;
                    if (!string.IsNullOrEmpty(cellContent)) notEmptyCount++;
                }
                if (defCount > 0 && defCount >= (notEmptyCount / 2))
                {
                    defRows.Add(i);
                }
            }
            //
            int rowCount = 0;
            int headerCount = 0;
            foreach (int row in defRows)
            {
                bool hasGroup = false;
                if (row < 1) continue;
                if (row > 1 && DataTableRowNotEmptyCellCount(dt, row - 2, out _) > 1) hasGroup = true;
                rt.Rows.Add();
                rt.Rows[rowCount][ColumnNames.TopHeaderNames] = $"TopBreak_{++headerCount}";
                rowCount++;
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    if (string.IsNullOrEmpty(dt.Rows[row][col].ToString())) continue;
                    rt.Rows.Add();
                    if (hasGroup) rt.Rows[rowCount][ColumnNames.TopGroupName] = dt.Rows[row - 2][col].ToString();
                    rt.Rows[rowCount][ColumnNames.TopDescriptionName] = dt.Rows[row - 1][col].ToString();
                    rt.Rows[rowCount][ColumnNames.TopDefinitionName] = dt.Rows[row][col].ToString();
                    rowCount++;
                }
            }
            //
            return rt;
        }


        private void DataTableRemoveEmptyRows(System.Data.DataTable dt)
        {
            int row = dt.Rows.Count - 1;
            while (row >= 0)
            {
                if (DataTableRowNotEmptyCellCount(dt, row, out _) == 0)
                {
                    dt.Rows.RemoveAt(row);
                }
                row--;
            }
        }

        private void DataTableRemoveSameRows(System.Data.DataTable dt, int row)
        {
            if (row < 0) return;
            int cr = dt.Rows.Count - 1;
            bool hasSameRow = false;
            while (cr > row)
            {
                if (dt.Rows[row][0].ToString() == dt.Rows[cr][0].ToString())
                {
                    bool isSame = true;
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        if (dt.Rows[row][col].ToString() != dt.Rows[cr][col].ToString())
                        {
                            isSame = false;
                            break;
                        }
                    }
                    if (isSame)
                    {
                        hasSameRow = true;
                        dt.Rows.RemoveAt(cr);
                    }
                }
                cr--;
            }
            if (hasSameRow)
            {
                dt.Rows.RemoveAt(row);
            }
        }

    }


    public enum SpecKeys
    {
        SpecSheetKeys,
        SpecHeaderKeys,
        HighFactorKeys,
        NetKeys,
        TopBottomKeys,
        AverageKeys,

        FixedLabelKeys,
        FixedNameKeys,
        FixedBaseKeys,
        FixedTopBottomKeys,
        FixedMeanKeys,
        FixedAverageKeys,
        FixedSummaryKeys,
        FixedValueKeys,
        FixedRemarkKeys
    }

}

