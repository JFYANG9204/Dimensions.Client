
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dimensions.Bll.String
{
    internal static class StringArrayFunction
    {
        /// <summary>
        /// 判断字符串是否包含关键字数组中的关键字
        /// </summary>
        /// <param name="content">目标字符串</param>
        /// <param name="array">关键字数组</param>
        /// <returns></returns>
        internal static bool StringHasKey(string content, string[] array)
        {
            bool result = false;
            //
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (content.ToLower().Contains(array[i].ToLower()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            //
            return result;
        }
        /// <summary>
        /// 判断字符串数组中是否包含关键字
        /// </summary>
        /// <param name="content">内容字段</param>
        /// <param name="array"></param>
        /// <returns></returns>
        internal static bool StringInArray(string content, string[] array)
        {
            bool result = false;
            //
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].ToLower().Contains(content.ToLower()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            //
            return result;
        }

        /// <summary>
        /// 二维数组行切片
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="array">源数据数组</param>
        /// <param name="rowIndex">二维数组第二个索引</param>
        /// <returns>一维数组</returns>
        internal static T[] SliceArray<T>(this T[,] array, int rowIndex)
        {
            T[] result = new T[array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                result[i] = array[i, rowIndex];
            }
            return result;
        }
        /// <summary>
        /// 二维数组切片
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="array">元数据数组</param>
        /// <param name="colIndex">二维数组第一个索引</param>
        /// <returns>一维数组</returns>
        public static T[] SliceArrayColumn<T>(this T[,] array, int colIndex)
        {
            T[] result = new T[array.GetLength(1)];
            for (int i = 0; i < array.GetLength(1); i++)
            {
                result[i] = array[colIndex, i];
            }
            return result;
        }
        /// <summary>
        /// 获取整型数组中的最大值
        /// </summary>
        /// <param name="array">元数据数组</param>
        /// <returns>最大值</returns>
        public static int GetMax(this int[] array)
        {
            if (array.Length > 0)
            {
                int max = array[0];
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[i] > max)
                        max = array[i];
                }
                return max;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取整形数组中最大值的索引
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <returns>最大值索引</returns>
        public static int MaxValueIndex(this int[] array)
        {
            int index = 0;
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > array[index])
                    index = i;
            }
            return index;
        }
        /// <summary>
        /// 获取整形数组中最大值的索引
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="exclude">需要排除的索引</param>
        /// <returns>最大值索引</returns>
        public static int MaxValueIndex(this int[] array, int[] exclude)
        {
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (!exclude.Contains(i))
                {
                    if (index == -1 && array[i] > 0)
                        index = i;
                    if (index > -1 && array[i] > array[index])
                        index = i;
                }
            }
            return index;
        }
        /// <summary>
        /// 获取字符串中包含的数组中的部分，以","分隔
        /// </summary>
        /// <param name="source">原字符串</param>
        /// <param name="array">关键字数组</param>
        /// <returns></returns>
        internal static string GetCommonPartInArray(string source, string[] array)
        {
            string common = string.Empty;
            foreach (var item in array)
            {
                if (source.Replace(" ", "").ToLower().Contains(item.ToLower()))
                    common += item + ",";
            }
            if (common.Length > 1)
            {
                common = common.Substring(0, common.Length - 1);
            }
            return common;
        }
        /// <summary>
        /// 移除数字字符串数组中的数字
        /// </summary>
        /// <param name="remove">需要移除的数字</param>
        /// <param name="array">原数组</param>
        /// <returns>修改后的数组</returns>
        internal static string[] RemoveItemByNumber(string remove, string[] array)
        {
            ArrayList remain = new ArrayList(array);
            for (int i = 0; i < array.Length; i++)
            {
                if (Regex.Replace(array[i], @"^[0-9]+", "") == Regex.Replace(remove, @"^[0-9]+", ""))
                {
                    remain.RemoveAt(i);
                }
            }
            return (string[])remain.ToArray(typeof(string));
        }
        /// <summary>
        /// 移除字符串数组各项头部的数字序号
        /// </summary>
        /// <param name="array">原始数组</param>
        /// <returns>修改后的数组</returns>
        internal static string[] RemoveArraySerialNumber(string[] array)
        {
            if (array is null || array.Length < 1)
                return array;
            string[] result = array;
            for (int i = 0; i < result.Length; i++)
            {
                while (Regex.IsMatch(result[i].Substring(0, 1), @"[0-9.]+"))
                {
                    result[i] = result[i].Substring(1);
                }
                result[i] = result[i].Trim();
            }
            return result;
        }

        /// <summary>
        /// 判断字符串数组的内容是否包含相似内容
        /// </summary>
        /// <param name="array">原数组</param>
        /// <returns></returns>
        internal static bool StringArrayHasSimilarContent(string[] array)
        {
            if (array.Length <= 1) return false;
            bool similar = false;
            string common = array[0];
            bool different = false;
            for (int i = 1; i < array.Length; i++)
            {
                string temp = string.Empty;
                int count = 0;
                for (int j = 0; j < array[i].Length; j++)
                {
                    if (j < common.Length && common.Substring(j, 1) == array[i].Substring(j, 1) && count == j)
                    {
                        temp += common.Substring(j, 1);
                        if (!different) different = true;
                        count++;
                    }
                }
                if (!string.IsNullOrEmpty(temp) && temp.Length < common.Length) common = temp;
            }
            if (different && (common.Length >= array[0].Length - 3 || array.Contains(common))) similar = true;
            return similar;
        }

        internal static int MaxLengthOfStringArray(string[] array)
        {
            int len = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length > len)
                    len = array[i].Length;
            }
            return len;
        }

        internal static bool HasCommonItem(string[] array1, string[] array2, out string[] items)
        {
            items = new string[1];
            if (array1 is null || array2 is null) return false;
            bool common = false;
            for (int i = 0; i < array1.Length; i++)
            {
                for (int j = 0; j < array2.Length; j++)
                {
                    if (array1[i] == array2[j])
                    {
                        common = true;
                        if (string.IsNullOrEmpty(items[0]))
                        {
                            items[0] = array1[i];
                        }
                        else
                        {
                            if (!items.Contains(array1[i]))
                                items = items.Append(array1[i]).ToArray();
                        }
                    }
                }
            }
            return common;
        }
    }
}
