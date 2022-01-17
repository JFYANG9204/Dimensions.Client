
namespace Dimensions.Bll.Generic
{
    public interface IMddVariable
    {
        /// <summary>
        /// 变量ID
        /// </summary>
        string Id { get; }
        /// <summary>
        /// 变量名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 变量标签
        /// </summary>
        string Label { get; }
        /// <summary>
        /// 针对Categorical类型变量， 非Loop类型变量为表侧码号列表， Loop类型为表头码号列表
        /// </summary>
        ICodeList CodeList { get; }
        /// <summary>
        /// 变量是否引用List
        /// </summary>
        bool UseList { get; }
        /// <summary>
        /// 引用的List Id
        /// </summary>
        string ListId { get; }
        /// <summary>
        /// 语言列表
        /// </summary>
        string Language { get; }
        /// <summary>
        /// 是否有子对象
        /// </summary>
        bool HasChildren { get; }
        /// <summary>
        /// 表侧子对象
        /// </summary>
        IMddVariable[] Children { get; }
        /// <summary>
        /// 父对象
        /// </summary>
        IMddVariable Parent { get; }
        /// <summary>
        /// 变量类型 -- Normal, Loop
        /// </summary>
        VariableType VariableType { get; }
        /// <summary>
        /// 数据值类型，Loop类型变量为表头类型，Normal类型变量为表侧类型
        /// </summary>
        ValueType ValueType { get; }
        /// <summary>
        /// 变量属性
        /// </summary>
        Property[] Properties { get; }
        /// <summary>
        /// 判定有效内容的正则表达式
        /// </summary>
        string Validation { get; }
        /// <summary>
        /// 新建变量的赋值语句，非新增变量为null
        /// </summary>
        string Assignment { get; }

        void SetProperty(string listId);
        void SetProperty(ICodeList categoricals);
        void SetProperty(string name, string id);
        void SetProperty(ICategorical categorical);
        void SetProperty(IMddVariable children);
        void SetProperty(VariableType variableType);
        void SetProperty(ValueType valueType);
        void SetProperty(Property property);
        void SetProperty(Property[] properties);
        void SetProperty(ValueRange range);

        void SetLanguage(string language);
        void SetLabels(string label, string language);
        void SetLabels(string[] labels, string[] languages);
        void SetAssignment(string assignment);

        void SetParent(IMddVariable parent);

        string[] GetLabels();
        string[] GetLanguages();

        IMddVariable Copy();

        ValueRange Range { get; }
    }

}
