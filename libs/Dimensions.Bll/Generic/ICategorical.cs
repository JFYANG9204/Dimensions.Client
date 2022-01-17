
namespace Dimensions.Bll.Generic
{
    public interface ICategorical
    {
        /// <summary>
        /// Categorical类型变量名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Categorical类型变量标签
        /// </summary>
        string Label { get; }
        /// <summary>
        /// 添加语言标签
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="language">语言</param>
        void AddLabel(string label, string language);
        /// <summary>
        /// 设定多语言标签，并将第一个设定为默认值
        /// </summary>
        /// <param name="labels">标签列表</param>
        /// <param name="languages">语言列表</param>
        void AddLabels(string[] labels, string[] languages);
        /// <summary>
        /// 设定多语言标签
        /// </summary>
        /// <param name="language">语言</param>
        void SetLabel(string language);
        /// <summary>
        /// 重新设定标签
        /// </summary>
        /// <param name="oldLabel">原标签</param>
        /// <param name="newLabel">新标签</param>
        void ResetLabel(string oldLabel, string newLabel);
        /// <summary>
        /// Code包含属性
        /// </summary>
        Property[] Properties { get; }
        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="property"></param>
        void SetProperties(Property property);
        /// <summary>
        /// 添加所有属性
        /// </summary>
        /// <param name="properties"></param>
        void SetProperties(Property[] properties);
    }
}
