
namespace Dimensions.Bll
{
    public enum VariableType
    {
        Normal,
        Loop,
        Block,
        System
    }

    public enum ValueType
    {
        Long,
        Double,
        Categorical,
        Text,
        Info,
        Boolean,
        Date,
        Default
    }

    public enum MddMergeType
    {
        Front,
        Behind
    }

    internal enum SheetType
    {
        Header,
        Spec,
        Default
    }

    public enum AxisElement
    {
        Text,
        Base,
        MainSide,
        Sigma,
        Derived,
        Mean
    }

    public enum SpecRemoveHeaderRowType
    {
        Auto,
        Custom
    }

    public enum SpecAnalysisType
    {
        Normal,
        Fixed
    }

    public enum HeaderAnalysisType
    {
        Horizen,
        Normal
    }

}
