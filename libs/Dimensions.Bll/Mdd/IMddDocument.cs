using Dimensions.Bll.Generic;

namespace Dimensions.Bll.Mdd
{
    public interface IMddDocument
    {
        /// <summary>
        /// 上下文类型
        /// </summary>
        string Context { get; }
        /// <summary>
        /// 文档配置信息
        /// </summary>
        Property[] Properties { get; }
        /// <summary>
        /// 设定Property配置
        /// </summary>
        /// <param name="property"></param>
        void SetProperty(Property property);
        /// <summary>
        /// 模板配置信息
        /// </summary>
        Property[] Templates { get; }
        /// <summary>
        /// 设定模板配置
        /// </summary>
        /// <param name="property"></param>
        void SetTemplates(Property property);
        /// <summary>
        /// 是否包含List定义
        /// </summary>
        bool HasLists { get; }
        /// <summary>
        /// List变量定义
        /// </summary>
        IMddVariableCollection ListFields { get; }
        /// <summary>
        /// 是否包含多种语言
        /// </summary>
        bool HasMultiLanguages { get; }
        /// <summary>
        /// 当前语言
        /// </summary>
        string Language { get; }
        /// <summary>
        /// 所有语言
        /// </summary>
        string[] Languages { get; }
        /// <summary>
        /// 文档中所有Fields内容
        /// </summary>
        IMddVariableCollection Fields { get; }
        /// <summary>
        /// 问卷编号变量名
        /// </summary>
        string Record { get; }
        /// <summary>
        /// 新添加变量的变量名
        /// </summary>
        string[] NewVariables { get; }
        /// <summary>
        /// 获取所有Field名
        /// </summary>
        string[] FieldNames { get; }
        /// <summary>
        /// 变量查找方法，精确查找，不区分大小写，没有结果返回null
        /// </summary>
        /// <param name="name">查询内容</param>
        IMddVariable Find(string name);
        /// <summary>
        /// 查询所有包含关键字的变量名，不区分大小写，没有结果返回null
        /// </summary>
        /// <param name="key"></param>
        IMddVariableCollection FindAll(string key);
        /// <summary>
        /// 使用正则表达式查找
        /// </summary>
        /// <param name="pattern">模式字符串</param>
        /// <returns></returns>
        IMddVariable FindWithRegex(string name);
        /// <summary>
        /// 设定语言
        /// </summary>
        /// <param name="language">目标语言</param>
        void SetLanguage(string language);
        /// <summary>
        /// 获得完整的条件定义表达式，题号分隔符分为三种："="，":"， "："，码号分隔符分为两种："\"，"/"
        /// </summary>
        /// <param name="sourceString">原始字符串</param>
        /// <returns>完整的条件定义表达式</returns>
        string GetFullDefinition(string source);
        /// <summary>
        /// 添加多个Normal变量合并后的变量
        /// </summary>
        /// <param name="source">原始变量名字符，识别"+"</param>
        void AddMergedVariable(string source, string label);
        /// <summary>
        /// 报告mdd文件读取进度
        /// </summary>
        event MddLoadEventHandler ReportLoadProgress;
    }
}
