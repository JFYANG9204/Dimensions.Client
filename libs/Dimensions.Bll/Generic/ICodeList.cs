using System.Collections.Generic;

namespace Dimensions.Bll.Generic
{
    public interface ICodeList : IEnumerable<ICategorical>
    {
        /// <summary>
        /// 根据数值索引获取对应位置Categorical值
        /// </summary>
        /// <param name="index">数组索引</param>
        /// <returns></returns>
        ICategorical this[int index] { get; }
        /// <summary>
        /// 添加Categorical类型变量
        /// </summary>
        /// <param name="categorical"></param>
        void Add(ICategorical categorical);
        /// <summary>
        /// 已存储的Categorical类型变量数量
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 判断是否包含指定码号
        /// </summary>
        /// <param name="name">码号名，不区分大小写</param>
        /// <returns></returns>
        bool Contains(string name);
        /// <summary>
        /// 移除单个Categorical类型变量
        /// </summary>
        /// <param name="categorical">需要移除的Categorical类型变量</param>
        void Remove(ICategorical categorical);
        /// <summary>
        /// 针对码号数字部分进行正序排序
        /// </summary>
        void Sort();
        /// <summary>
        /// 码号的共同部分
        /// </summary>
        string CommonPart { get; }
        /// <summary>
        /// 当前语言
        /// </summary>
        string Language { get; }
        /// <summary>
        /// 复制当前数据到新的ICodeList中
        /// </summary>
        ICodeList Copy();
        /// <summary>
        /// 设定语言
        /// </summary>
        void SetLanguage(string language);
        /// <summary>
        /// 判断码号列表数字部分是否连续
        /// </summary>
        /// <returns></returns>
        bool IsContinuousCodes();
        /// <summary>
        /// 判断码号列表标签是否包含连续的数字部分
        /// </summary>
        /// <param name="factorStep">连续数字的间隔</param>
        /// <param name="highFactorLabel">高分值关键字</param>
        /// <returns></returns>
        bool IsContinuousFactor(out int factorStep, out string exclude, string[] highFactorLabel = null);
        /// <summary>
        /// 判断是否包含"xxxx-xxxx"类型的数值
        /// </summary>
        /// <param name="factors">码号对应的赋值列表</param>
        /// <returns></returns>
        bool IsSplitFactor(out Dictionary<string, double> factors);
        /// <summary>
        /// 判断是否为合并为多选的Loop类型变量
        /// </summary>
        /// <param name="topCodeList">结果top部分码号列表</param>
        /// <param name="sideCodeList">结果side部分码号列表</param>
        /// <returns></returns>
        bool IsMergedLoop(string codeLabel, out MergedLoopDefinition definition);
        /// <summary>
        /// 根据数字码号获取完整码号
        /// </summary>
        /// <param name="number">数字码号</param>
        /// <returns></returns>
        bool GetCodeNameFromNumber(string number, out string code);
        /// <summary>
        /// 格式化码号标签
        /// </summary>
        void FormatCodeLabels();
        /// <summary>
        /// 获取码号字符串
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        string GetCodes(string start, string end);
        public struct MergedLoopDefinition
        {
            public string StartLabel;
            public string InnerLabel;
            public ICodeList TopList;
            public ICodeList SideList;
            public MergedLoopDefinitionType Type;
        }

        public enum MergedLoopDefinitionType
        {
            TopFirst,
            SideFirst
        }
    }
}
