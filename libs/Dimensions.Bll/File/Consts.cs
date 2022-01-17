
namespace Dimensions.Bll.File
{
    public static class FileType
    {
        public const string Top = "Top";
        public const string Mdd = "Mdd";
        public const string Edt = "Edt";
        public const string Dms = "Dms";
        public const string Tab = "Tab";
        public const string Bat = "Bat";
        public const string Log = "Log";
    }

    public static class MddFileContentType
    {
        public const string Text = "Text";
        public const string Title = "Title";
        public const string Axis = "Axis";
        public const string ResponseText = "ResponseText";
        public const string ListText = "ListText";
        public const string Average = "Average";
        public const string ContinueFactor = "ContinueFactor";
        public const string SplitFactor = "SplitFactor";
        public const string RebaseFunction = "RebaseFunction";
        public const string Note = "Note";
    }

    public static class DmsFileContentType
    {
        public const string Loop = "Loop";
        public const string Normal = "Normal";
        public const string Double = "Double";
        public const string Long = "Long";
        public const string LoopFrame = "LoopFrame";
        public const string Note = "Note";
    }

    public static class EdtFileContentType
    {
        public const string MergedLoop = "MergedLoop";
        public const string Pyramid = "Pyramid";
        public const string Text = "Text";
        public const string Note = "Note";
    }

    public static class TabFileContentType
    {
        public const string Normal = "Normal";
        public const string Grid = "Grid";
        public const string GridSlice = "GridSlice";
        public const string NormalWithFunction = "NormalWithFunction";
        public const string GridWithFunction = "GridWithFunction";
        public const string GridSliceWithFunction = "GridSliceWirthFunction";
        public const string Digit = "Digit";
        public const string Filter = "Filter";
        public const string NormalLabel = "NormalLabel";
        public const string GridLabel = "GridLabel";
        public const string DimVar = "DimVar";
        public const string Note = "Note";
    }

    public static class TopFileContentType
    {
        public const string SubTitle = "SubTitle";
        public const string NonSubTitle = "NonSubTitle";
    }

    public static class BatFileContentType
    {
        public const string SubTitle = "SubTitle";
        public const string Normal = "Normal";
        public const string Note = "Note";
    }

    public static class FileString
    {
        // 文件名
        public const string MddFileName = "MDD_Manipulation.mrs";
        public const string DmsFileName = "sbMetadata_dms.mrs";
        public const string EdtFileName = "sbOnNextCase.mrs";
        public const string TabFileName = "tab.mrs";
        public const string TopFileName = "sbMetadata.mrs";
        public const string BatFileName = "RunTables.bat";
        public const string LogFileName = "Logs.txt";
        // Mdd_Manipulation基础内容
        public const string MddBaseContent = "" +
            "'*****FILE VERSION=3, DATE LAST MODIFIED=2012/08/29, KO\n" +
            "\n'Use this file to amend/add axis expressions and analysis text\n" +
            "\n'CHANGE AS NEEDED: Existence options applied to FnCreateExistenceAxisSpec - set options as required\n" +
            "Dim iCategoryExistenceLabel,iDateDisplay,iVariableExistenceLabel,Context\n" +
            "Context=\"Main\"\n" +
            "iCategoryExistenceLabel=0\n" +
            "iDateDisplay=0\n" +
            "iVariableExistenceLabel=0\n" +
            "\n'This will remove any HTML tags included in labels of all fields and elements\n" +
            "sbStripHTMLFromFieldLabels(MDM,MDD_TYPE,LOCALE)\n" +
            "\n'Note: this will change all the text format of each category of all grids as defined by the CASE_OPTION in the job.ini\n" +
            "'If there is a need to treat the categories of an individual grid differently, you can use a loop such as:\n" +
            "'    dim cat\n" +
            "'    for each cat in mdm.fields[\"q3\"].categories\n" +
            "'      sbSetResponseText(MDM,\"q3\",MDD_TYPE,LOCALE,cat.name,fnCaseControl(cat.label,3))\n" +
            "'    next" +
            "sbAdjustCaseOfCategoriesOnUpperFields(mdm,mdm)\n" +
            "\n'*************** ADD MDD MANIPULATION HERE *********************\n" +
            "\n''**************************************************************\n" +
            "''EXAMPLES\n" +
            "\n''Edit/Add an Axis Expression\n" +
            "''sbSetAxisExpression(MDM,\"q13tab[..].slice\",\"{_\n" +
            "''_7CompletelySatisfied '(7) Completely Satisfied' [factor = 7],_\n" +
            "''_6 '(6)' [factor = 6],_\n" +
            "''_5 '(5)' [factor = 5],_\n" +
            "''_4 '(4)' [factor = 4],_\n" +
            "''_3 '(3)' [factor = 3],_\n" +
            "''_2 '(2)' [factor = 2],_\n" +
            "''_1NotAtAllSatisfied '(1) Not at all satisfied' [factor = 1],_\n" +
            "''mean() [IsFixed=True,Decimals=2],stddev(),stderr()}\")\n" +
            "\n''Edit/Add Title Text\n" +
            "''sbSetTitleText(MDM,\"Q26\",MDD_TYPE,LOCALE,\"(Q26) Where seen ads - Newspaper Campaign\")\n" +
            "\n''Edit/Add Response Text\n" +
            "''sbSetResponseText(MDM,\"Q26\",MDD_TYPE,LOCALE,\"StrAgr\",\"(5) Strongly Agree\")\n" +
            "\n'  <property>    ::=   CalculationScope=AllElements|PrecedingElements\n" +
            "'                    | CountsOnly=True|False\n" +
            "'                    | Decimals=DecimalPlaces\n" +
            "'                    | Factor=FactorValue\n" +
            "'                    | IsFixed=True|False\n" +
            "'                    | IsHidden=True|False\n" +
            "'                    | IsHiddenWhenColumn=True|False\n" +
            "'                    | IsHiddenWhenRow=True|False\n" +
            "'                    | IncludeInBase=True|False\n" +
            "'                    | IsUnweighted=True|False\n" +
            "'                    | Multiplier=MultiplierVariable\n" +
            "'                    | Weight=WeightVariable\n" +
            "''**************************************************************\n" +
            "Function mean_inc(Qes)\n" +
            "mean_inc = \", e99 '' text(),mean 'AVERAGE' mean(\"+Qes+\")[Decimals=2],stdv 'STANDARD DEVIATION' StdDev(\"+Qes+\")[Decimals=2],stde 'STANDARD ERROR' stderr(\"+Qes+\") [Decimals=2]\"\n" +
            "End Function\n" +
            "\n\n'	sbSetTitleText(MDM,TOPBREAK,MDD_TYPE,LOCALE,GLOBALLABEL)\n" +
            "'	MDM.fields[\"S9\"].elements[\"_15\"].Factor=35000\n" +
            "'	sbAddFactors(MDM,\"BE6[..].Slice\",MDD_TYPE,LOCALE,-5,1,\"\",False)\n" +
            "' sbAddAverageMentions(MDM,\"qb\",NULL,\"Average brand endorsement\",true,\"0\")\n" +
            "' sbAddAverageMentions(MDM,\"q3[..].slice\",\"None,DK\",\"Average brand endorsement\",FALSE,\"2\")\n" +
            "''**************************************************************\n\n";
    }

    public static class FileWriterKeys
    {
        public const string RecordName = "RecordName";
        public const string Notes = "Notes";
        // mdd
        public const string MddText = "MddText";
        public const string MddVariableName = "MddVariableName";
        public const string MddSideName = "MddSideName";
        public const string MddTitle = "MddTitle";
        public const string MddCodeName = "MddCodeName";
        public const string MddCodeLabel = "MddCodeLabel";
        public const string MddAxis = "MddAxis";
        public const string MddStartFactor = "MddStartFactor";
        public const string MddStep = "MddStep";
        public const string MddExclude = "MddExclude";
        public const string MddFactor = "MddFactor";
        public const string MddAverageLabel = "MddAverageLabel";
        // tab
        public const string TabTopName = "TabTopVariableName";
        public const string TabSideName = "TabSideVariableName";
        public const string TabBaseName = "TabBaseName";
        public const string TabTitle = "TabTitle";
        public const string TabNormalFilter = "TabNormalFilter";
        public const string TabLabel = "TabLabel";
        public const string TabFunction = "TabFunction";
        public const string TabSideAxis = "TabSideAxis";
        public const string TabHasMean = "TabHasMean";
        // top
        public const string TopVariableName = "TopVariableName";
        public const string TopTitle = "TopTitle";
        public const string TopLabelList = "TopLabelList";
        public const string TopDefinitionList = "TopDefinitionList";
        public const string TopDefinitions = "TopDefinitions";
        // dms
        public const string DmsVariableName = "DmsVariableName";
        public const string DmsVariableLable = "DmsVariableLabel";
        public const string DmsSideName = "DmsSideName";
        public const string DmsCodeLabel = "DmsCodeLabel";
        public const string DmsTopCodeList = "DmsTopCodeList";
        public const string DmsSideCodeList = "DmsSideCodeList";
        // edt
        public const string EdtSourceVariableName = "EdtSourceVariableName";
        public const string EdtTargetVariableName = "EdtTargetVariableName";
        public const string EdtTargetVariabelSideName = "EdtTargetVariableSideName";
        public const string EdtRefVarName = "EdtRefVarName";
        public const string EdtCodeLabel = "EdtCodeLabel";
        public const string EdtTopCodes = "EdtTopCodes";
        public const string EdtSideCodes = "EdtSideCodes";
        public const string EdtDefinition = "EdtDefinition";
        public const string EdtPyDatas = "EdtPyData";
        public const string EdtPyNames = "EdtPyName";
        public const string EdtText = "EdtText";
        // bat
        public const string BatSigIdResults = "BatSigIdResult";
        public const string BatSetSigResults = "BatSetSigResult";
        public const string BatAddSubTitle = "BatAddSubTitle";
        public const string BatTableName = "BatTableName";
        public const string BatHeaderName = "BatHeaderName";
        public const string BatHeaderSetting = "BatHeaderSetting";
        public const string BatHeaderItems = "BatHeaderItems";
        // log
        public const string LogText = "LogText";
        public const string LogVarName = "LogVarName";
        public const string LogStatus = "LogStatus";
    }


}
