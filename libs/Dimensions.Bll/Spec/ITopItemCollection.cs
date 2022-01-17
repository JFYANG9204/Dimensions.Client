
using System.Collections.Generic;

namespace Dimensions.Bll.Spec
{
    public interface ITopItemCollection
    {
        ITopItem this[int index] { get; }
        /// <summary>
        /// 表头名字
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 是否添加SubGroup
        /// </summary>
        bool AddSubTitle { get; }
        /// <summary>
        /// BAT文件SubTitle内容
        /// </summary>
        string SubTitle { get; }
        /// <summary>
        /// T检验字母设定
        /// </summary>
        string[] SigSettings { get; }
        /// <summary>
        /// T检验分组设定
        /// </summary>
        string[] SigTests { get; }
        /// <summary>
        /// 当前Collection的起始索引
        /// </summary>
        int Index { get; }
        /// <summary>
        /// ITopItem设定数据
        /// </summary>
        ITopItem[] Items { get; }
        /// <summary>
        /// 包含数据个数
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 问卷编号名称
        /// </summary>
        string Record { get; }
        /// <summary>
        /// 添加TopItem
        /// </summary>
        /// <param name="item">TopItem对象</param>
        /// <param name="group">Group信息</param>
        void Add(ITopItem item, string group);
        /// <summary>
        /// 表头分组
        /// </summary>
        Dictionary<string, string> Group { get; }
        /// <summary>
        /// 设定是否添加SubGroup
        /// </summary>
        /// <param name="subGroup"></param>
        void Set(bool subGroup);
        /// <summary>
        /// 设定问卷编号名称
        /// </summary>
        /// <param name="record">问卷编号名称</param>
        void Set(string record);
    }
}
