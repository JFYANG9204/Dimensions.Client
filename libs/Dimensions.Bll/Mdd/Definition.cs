
using Dimensions.Bll.Generic;
using Dimensions.Bll.String;

namespace Dimensions.Bll.Mdd
{
    public class Definition : IDefinition
    {
        public Definition()
        {
            HasGroup = false;
        }

        public string VariableName { get; private set; }

        public string Codes { get; private set; }

        public ICodeList CodeList { get; private set; }

        public DefinitionOuterLogic OuterLogic { get; private set; }

        public DefinitionInnerLogic InnerLogic { get; private set; }

        public DefinitionGroupLogic GroupLogic { get; private set; }

        public int Group { get; private set; }
        public bool HasGroup { get; private set; }

        public string GetSimpleExpression(string codeLabel, string varName)
        {
            string result = string.Empty;
            if (OuterLogic == DefinitionOuterLogic.And)
                result += " And ";
            else if (OuterLogic == DefinitionOuterLogic.Or)
                result += " Or ";
            if (GroupLogic == DefinitionGroupLogic.Start || GroupLogic == DefinitionGroupLogic.Both)
                result += "(";
            switch (InnerLogic)
            {
                case DefinitionInnerLogic.Equal:
                    result += $"{varName}.ContainsAny({{{StringFunction.GetFormatedCodeDefinition(Codes, codeLabel)}}})";
                    break;
                case DefinitionInnerLogic.NotEqual:
                    result += $"Not {varName}.ContainsAny({{{StringFunction.GetFormatedCodeDefinition(Codes, codeLabel)}}})";
                    break;
                case DefinitionInnerLogic.Null:
                    break;
                default:
                    break;
            }
            if (GroupLogic == DefinitionGroupLogic.End || GroupLogic == DefinitionGroupLogic.Both)
                result += ")";
            return result;
        }

        public void SetProperty(ICodeList list)
        {
            CodeList = list;
        }

        public void SetProperty(string name, string codes)
        {
            VariableName = name;
            Codes = codes;
        }

        public void SetProperty(DefinitionOuterLogic outerLogic)
        {
            OuterLogic = outerLogic;
        }

        public void SetProperty(DefinitionInnerLogic innerLogic)
        {
            InnerLogic = innerLogic;
        }

        public void SetProperty(DefinitionGroupLogic groupLogic)
        {
            GroupLogic = groupLogic;
        }

        public void SetProperty(int group)
        {
            Group = group;
        }

        public void SetProperty(bool hasGroup)
        {
            HasGroup = hasGroup;
        }
    }
}
