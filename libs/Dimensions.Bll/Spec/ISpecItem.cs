using Dimensions.Bll.Generic;
using Dimensions.Bll.Mdd;

namespace Dimensions.Bll.Spec
{
    public interface ISpecItem
    {
        /// <summary>
        /// Spec中的变量名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Spec中变量标题
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Base内容
        /// </summary>
        BaseItem[] Base { get; }
        /// <summary>
        /// 是否包含Net
        /// </summary>
        bool HasNet { get; }
        /// <summary>
        /// Net内容
        /// </summary>
        NetContent Nets { get; }
        /// <summary>
        /// Top Box / Bottom Box
        /// </summary>
        string TopBottom { get; }
        /// <summary>
        /// Mdd对象
        /// </summary>
        IMddDocument Mdd { get; }
        /// <summary>
        /// 对应题目Mdd变量
        /// </summary>
        IMddVariable Variable { get; }
        /// <summary>
        /// 变量轴表达式
        /// </summary>
        Axis Axis { get; }
        /// <summary>
        /// 高分标签
        /// </summary>
        string[] HighFactorKeys { get; }
        bool Mean { get; }
        NewVariableDefinition NewVariableDefinition { get; }
        bool IsSkipped { get; }
        string Notes { get; }
        void SetProperty(string name, string title);
        void SetProperty(BaseItem[] baseText);
        void SetProperty(NetContent nets);
        void SetProperty(string topbottom);
        void SetProperty(IMddVariable variable);
        void SetProperty(bool mean);
        void SetProperty(NewVariableDefinition newVariableDefinition);
        void SetProperty(IMddDocument mdd);
        void AddNet(string key, string value);
        void SetNotes(string note);
        public struct BaseItem
        {
            public BaseItem(string value, string expression)
            {
                Value = value;
                Expression = expression;
            }
            public string Value { get; set; }
            public string Expression { get; set; }
        }
    }
}
