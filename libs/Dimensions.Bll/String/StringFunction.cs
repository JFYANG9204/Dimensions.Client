using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dimensions.Bll.String
{
    internal static class StringFunction
    {
        /// <summary>
        /// 格式化文本，去掉[]、{}中间的内容
        /// </summary>
        /// <param name="str">需要处理的文本</param>
        /// <returns>处理后的文本</returns>
        public static string FormatString(string str)
        {
            string result = str;
            result = Regex.Replace(result, @"\[.*\]", "");
            result = Regex.Replace(result, @"\<.*\>", "");
            result = Regex.Replace(result, @"\{.*\}", "");
            result = result.Replace("$", "");
            result = result.Replace("?", "");
            result = result.Replace("\"", "");
            //
            if (str.Contains(": "))
            {
                string[] splitStr = Regex.Split(result, ": ", RegexOptions.IgnoreCase);
                if (splitStr.Length == 2)
                {
                    result = splitStr[1];
                }
                else
                {
                    result = splitStr[0];
                    for (int i = 1; i < splitStr.Length; i++)
                    {
                        if (splitStr[i].Length > result.Length)
                            result = splitStr[i];
                    }
                }
            }
            if (str.Contains(" - "))
            {
                result = Regex.Split(result, " - ", RegexOptions.IgnoreCase)[0].Trim();
            }
            //
            while (result.Length > 1 && !Regex.IsMatch(result.Substring(result.Length - 1), @"[\u4E00-\u9FA5\w]+"))
            {
                result = result.Substring(0, result.Length - 1);
            }
            //
            return result.Trim();
        }
        /// <summary>
        /// 针对合并为多选的Loop变量的码号，对比两个码号，返回不同的数字部分
        /// </summary>
        /// <param name="code1">第一个码号</param>
        /// <param name="code2">第二个码号</param>
        /// <param name="differentIndex">不同部分位置，1：不在结尾，2：在结尾</param>
        /// <returns>string[2]{第一个码号不同部分，第二个码号不同部分}</returns>
        public static string[] GetCodeDifferentPart(string code1, string code2, out int differentIndex)
        {
            string c1 = code1;
            string c2 = code2;
            //
            // 首先获取名称共有分段部分
            //
            int index = c1.Length - 1;
            //
            int lastIndex = index;
            //
            int firstLength = 0;
            int lastLength = 0;
            //
            bool hasLast = false;
            //
            while (index >= 0)
            {
                if (!hasLast && !Regex.IsMatch(c1.Substring(index, 1), @"[0-9]+"))
                {
                    while (!Regex.IsMatch(c1.Substring(index, 1), @"[0-9]+"))
                    {
                        lastLength++;
                        index--;
                    }
                    lastIndex = index + 1;
                    hasLast = true;
                }
                if (hasLast && !Regex.IsMatch(c1.Substring(index, 1), @"[0-9]+"))
                {
                    firstLength = index + 1;
                    break;
                }
                index--;
            }
            //
            string commonFirst = c1.Substring(0, firstLength);
            string commonLast = c1.Substring(lastIndex, lastLength);
            //
            string pattern = commonFirst + "([0-9]+)" + commonLast + "([0-9]+)";
            Match match1 = Regex.Match(c1, pattern);
            Match match2 = Regex.Match(c2, pattern);
            //
            if (match1.Groups[1].Value != match2.Groups[1].Value)
            {
                differentIndex = 1;
                c1 = match1.Groups[1].Value;
                c2 = match2.Groups[1].Value;
            }
            else
            {
                differentIndex = 2;
                c1 = match1.Groups[2].Value;
                c2 = match2.Groups[2].Value;
            }
            //
            string[] result = new string[2];
            result[0] = c1;
            result[1] = c2;
            return result;
        }
        /// <summary>
        /// 针对合并为多选的Loop变量，获取两个选项标签中不同的部分
        /// </summary>
        /// <param name="label1">第一个字符串</param>
        /// <param name="label2">第二个字符串</param>
        /// <returns>string[2]{第一个字符串中不同的部分，第二个字符串中不同的部分}</returns>
        public static string[] GetLabelDifferentPart(string label1, string label2)
        {
            string l1 = label1;
            string l2 = label2;
            //
            // 首先格式化字符串
            //
            if (l1.Contains(": ") && Regex.Split(l1, ": ").Length == 2)
            {
                l1 = Regex.Split(l1, ": ", RegexOptions.IgnoreCase)[1];
            }
            if (l2.Contains(": ") && Regex.Split(l2, ": ").Length == 2)
            {
                l2 = Regex.Split(l2, ": ", RegexOptions.IgnoreCase)[1];
            }
            //
            if (Regex.Split(l1, " - ").Length > 2)
            {
                string[] stringArray = Regex.Split(l1, " - ");
                l1 = stringArray[0] + " - " + stringArray[1];
            }
            //
            if (Regex.Split(l2, " - ").Length > 2)
            {
                string[] stringArray = Regex.Split(l2, " - ");
                l2 = stringArray[0] + " - " + stringArray[1];
            }
            //
            l1 = Regex.Replace(l1, @"\[.*\]", "");
            l1 = Regex.Replace(l1, @"\<.*\>", "");
            l1 = Regex.Replace(l1, @"\{.*\}", "");
            //
            l2 = Regex.Replace(l2, @"\[.*\]", "");
            l2 = Regex.Replace(l2, @"\<.*\>", "");
            l2 = Regex.Replace(l2, @"\{.*\}", "");
            //
            //
            int index1;
            int index2;
            if (l1.Substring(l1.Length - 1, 1) == l2.Substring(l2.Length - 1, 1))
            {
                index1 = l1.Length - 1;
                index2 = l2.Length - 1;
                while (index1 > 0 && index2 > 0 && l1.Substring(index1, 1) == l2.Substring(index2, 1))
                {
                    index1--;
                    index2--;
                }
                l1 = l1.Remove(index1 + 1, l1.Length - 1 - index1);
                l2 = l2.Remove(index2 + 1, l2.Length - 1 - index2);
            }
            if (l1.Substring(0, 1) == l2.Substring(0, 1))
            {
                int index = 0;
                while (index < l1.Length && index < l2.Length && l1.Substring(index, 1) == l2.Substring(index, 1))
                {
                    index++;
                }
                l1 = l1.Remove(0, index);
                l2 = l2.Remove(0, index);
            }
            //
            //
            string[] result = new string[2];
            result[0] = l1.Trim();
            result[1] = l2.Trim();
            return result;
        }
        /// <summary>
        /// 获取两个字符串从左边开始的相同字符，且保证结尾为非数字
        /// </summary>
        /// <param name="str1">第一个字符串</param>
        /// <param name="str2">第二个字符串</param>
        /// <returns>从左开始的相同字符串</returns>
        public static string GetCommonStringFromLeft(string str1, string str2)
        {
            string result = string.Empty;

            int length = str1.Length;
            if (str2.Length < length)
            {
                length = str2.Length;
            }
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (str1.Substring(i, 1) == str2.Substring(i, 1) && count == i)
                {
                    result += str1.Substring(i, 1);
                    count++;
                }
            }

            while (Regex.IsMatch(result.Substring(result.Length - 1), @"[0-9]"))
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
        /// <summary>
        /// 获取完整的码号定义字符，符号定义：码号分割："/"或"&"，范围分割："-"
        /// </summary>
        /// <param name="codes">原始字符</param>
        /// <param name="codeLabel">码号标签</param>
        /// <returns>码号标签+数字组成的字符串，中间以逗号分割</returns>
        public static string GetFormatedCodeDefinition(string codes, string codeLabel)
        {
            string result = string.Empty;
            codes = codes.Trim().Replace(" ", "").Replace(",", "/").Replace("&", "/");

            if (codes.Length > 1)
            {
                int startCode = 0;
                string tempCode = string.Empty;
                bool hasRange = false;
                for (int i = 0; i < codes.Length; i++)
                {
                    if (Regex.IsMatch(codes.Substring(i, 1), @"[0-9]"))
                    {
                        tempCode += codes.Substring(i, 1);
                    }
                    if (codes.Substring(i, 1) == "/" || i == codes.Length - 1)
                    {
                        if (hasRange)
                        {
                            int endCode;
                            try
                            {
                                endCode = Convert.ToInt32(tempCode);
                            }
                            catch (Exception)
                            {
                                endCode = 0;
                            }
                            if (endCode > startCode && endCode != 0 && startCode != 0)
                            {
                                for (int j = startCode; j <= endCode; j++)
                                {
                                    result += codeLabel + j.ToString() + ",";
                                }
                            }
                            tempCode = string.Empty;
                            hasRange = false;
                        }
                        else
                        {
                            result += codeLabel + tempCode + ",";
                            tempCode = string.Empty;
                        }
                    }
                    else if (codes.Substring(i, 1) == "_")
                    {
                        hasRange = true;
                        try
                        {
                            startCode = Convert.ToInt32(tempCode);
                        }
                        catch (Exception)
                        {
                            startCode = 0;
                        }
                        tempCode = string.Empty;
                    }
                }
                return result.Substring(0, result.Length - 1);
            }
            else
            {
                return codeLabel + codes;
            }
        }
        /// <summary>
        /// 从字符串右侧获取数字
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns></returns>
        public static string GetNumberFromRight(string source)
        {
            string result = string.Empty;
            bool hasNum = false;
            int index;
            string rightStr = string.Empty;
            if (!string.IsNullOrEmpty(source))
            {
                index = source.Length - 1;
                while (index >= 0)
                {
                    string subStr = source.Substring(index, 1);
                    if (Regex.Match(subStr, @"[0-9]+").Success)
                    {
                        if (!hasNum)
                        {
                            hasNum = true;
                        }
                        rightStr += subStr;
                    }
                    else
                    {
                        if (hasNum) break;
                    }
                    index--;
                }
            }
            for (int i = rightStr.Length - 1; i >= 0; i--)
            {
                result += rightStr.Substring(i, 1);
            }
            return result;
        }


    }
}
