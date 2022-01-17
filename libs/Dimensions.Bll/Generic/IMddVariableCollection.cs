using System.Collections.Generic;

namespace Dimensions.Bll.Generic
{
    public interface IMddVariableCollection : IEnumerable<IMddVariable>
    {
        /// <summary>
        /// 获取具体变量数据
        /// </summary>
        /// <param name="name">变量名，不区分大小写</param>
        /// <returns>变量数据</returns>
        public IMddVariable this[string name] { get; }
        public IMddVariable this[int index] { get; }
        /// <summary>
        /// 数据集合
        /// </summary>
        public List<IMddVariable> Data { get; }
        /// <summary>
        /// 向数据集合中添加变量数据
        /// </summary>
        /// <param name="variable">变量对象</param>
        public void Add(IMddVariable variable);
        /// <summary>
        /// 判断数据集中是否包含目标ID号的变量对象
        /// </summary>
        /// <param name="id">查询ID值</param>
        /// <returns></returns>
        public bool Contains(string id);
        /// <summary>
        /// 当前数据集合中的对象数量
        /// </summary>
        public int Count { get; }
        /// <summary>
        /// 通过ID号获得变量数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMddVariable Get(string id);
        /// <summary>
        /// 移除指定MddVariable
        /// </summary>
        /// <param name="variable">变量</param>
        public void Remove(IMddVariable variable);
        /// <summary>
        /// 移除指定位置的Variable
        /// </summary>
        /// <param name="index">索引</param>
        public void RemoveAt(int index);
        /// <summary>
        /// 合并变量集合
        /// </summary>
        /// <param name="variables">合并集合</param>
        /// <param name="type">插入在前面或后面</param>
        public void Merge(IMddVariableCollection variables, MddMergeType type);
    }
}
