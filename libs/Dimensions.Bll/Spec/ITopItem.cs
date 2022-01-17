
namespace Dimensions.Bll.Spec
{
    public interface ITopItem
    {
        /// <summary>
        /// 码号
        /// </summary>
        string Code { get; }
        /// <summary>
        /// 标签
        /// </summary>
        string Label { get; }
        /// <summary>
        /// 条件定义
        /// </summary>
        string Definition { get; }
        /// <summary>
        /// 设定码号和标签
        /// </summary>
        /// <param name="code">码号</param>
        /// <param name="label">标签</param>
        void SetProperty(string code, string label);
        /// <summary>
        /// 设定条件定义字符串
        /// </summary>
        /// <param name="definition">条件字符串</param>
        void SetProperty(string definition);
    }
}
