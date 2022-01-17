
using Dimensions.Bll.Generic;

namespace Dimensions.Bll.Mdd
{
    public interface IDefinition
    {
        string VariableName { get; }
        string Codes { get; }
        ICodeList CodeList { get; }
        DefinitionOuterLogic OuterLogic { get; }
        DefinitionInnerLogic InnerLogic { get; }
        DefinitionGroupLogic GroupLogic { get; }
        int Group { get; }
        bool HasGroup { get; }
        string GetSimpleExpression(string codeLabel, string varName);
        void SetProperty(ICodeList list);
        void SetProperty(string name, string codes);
        void SetProperty(DefinitionOuterLogic outerLogic);
        void SetProperty(DefinitionInnerLogic innerLogic);
        void SetProperty(DefinitionGroupLogic groupLogic);
        void SetProperty(int group);
        void SetProperty(bool hasGroup);
    }
}
