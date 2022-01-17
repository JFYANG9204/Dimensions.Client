using System;
using System.Collections.Generic;
using System.Linq;

namespace Dimensions.Bll.Spec
{
    public class TopItemCollection : ITopItemCollection
    {
        public TopItemCollection(string name, int index, bool subGroup = false)
        {
            Name = name;
            Index = index;
            AddSubTitle = subGroup;
            Items = new ITopItem[1];
            Group = new Dictionary<string, string>();
            _version = 0;
        }

        public ITopItem this[int index]
        {
            get
            {
                if (Items is null || index >= Items.Length)
                {
                    return null;
                }
                return Items[index];
            }
        }

        public string Name { get; }

        private int _version;
        public int Index { get; }

        public string Record { get; private set; }
        public bool AddSubTitle { get; private set; }

        public string SubTitle
        {
            get
            {
                string result = string.Empty;
                if (Group is null || Group.Count == 0)
                {
                    return result;
                }
                int count = 0;
                string tabIndex = Index.ToString();
                if (Index < 10) tabIndex = $"0{Index}";
                result += $"set Table_{tabIndex}=";
                foreach (var key in Group.Keys)
                {
                    result += $"{Name}{{base() [Ishidden=True],{Group[key]}}}";
                    if (!string.IsNullOrEmpty(key))
                    {
                        result += $" as b{count}'{key}'";
                        count++;
                    }
                    result += "+";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
        }

        public string[] SigSettings
        {
            get
            {
                string[] result = new string[1];
                if (Group is null || Group.Count <= 1)
                {
                    result[0] = ".abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ";
                    return result;
                }
                // 字母AscII码列表，不包含大小写的O
                int[] ascII = new int[50];
                int index = 0;
                for (int i = 97; i < 123; i++)
                {
                    if (i != 111)
                    {
                        ascII[index] = i;
                        index++;
                    }
                }
                for (int i = 65; i < 91; i++)
                {
                    if (i != 79)
                    {
                        ascII[index] = i;
                        index++;
                    }
                }

                int current = 0;
                string temp = ".";
                string _point = "";
                foreach (var key in Group.Keys)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    //
                    int count = Group[key].Split(',').Length;
                    if ((current + count) >= ascII.Length)
                    {
                        if (string.IsNullOrEmpty(result[0]))
                            result[0] = temp;
                        else
                            result = result.Append(temp).ToArray();
                        temp = ".";
                        for (int j = 0; j < current; j++)
                        {
                            _point += ".";
                        }
                        temp += _point;
                        current = 0;
                    }
                    for (int i = current; i < current + count; i++)
                    {
                        temp += $"{Convert.ToChar(ascII[i])}";
                    }
                    current += count;
                }
                if (string.IsNullOrEmpty(result[0]))
                    result[0] = temp;
                else
                    result = result.Append(temp).ToArray();
                return result;
            }
        }

        public string[] SigTests
        {
            get
            {
                string[] result = new string[1];
                if (Group is null || Group.Count <= 1)
                {
                    result[0] = string.Empty;
                    return result;
                }
                // 字母AscII码列表，不包含大小写的O
                int[] ascII = new int[50];
                int index = 0;
                for (int i = 97; i < 123; i++)
                {
                    if (i != 111)
                    {
                        ascII[index] = i;
                        index++;
                    }
                }
                for (int i = 65; i < 91; i++)
                {
                    if (i != 79)
                    {
                        ascII[index] = i;
                        index++;
                    }
                }

                int current = 0;
                string temp = string.Empty;
                foreach (var key in Group.Keys)
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    //
                    int count = Group[key].Split(',').Length;
                    if ((current + count) >= ascII.Length)
                    {
                        if (string.IsNullOrEmpty(result[0]))
                            result[0] = temp.Substring(0, temp.Length - 1);
                        else
                            result = result.Append(temp.Substring(0, temp.Length - 1)).ToArray();
                        temp = string.Empty;
                        current = 0;
                    }
                    for (int i = current; i < current + count; i++)
                    {
                        temp += $"{Convert.ToChar(ascII[i])}/";
                    }
                    temp = temp.Substring(0, temp.Length - 1) + ",";
                    current += count;
                }
                if (string.IsNullOrEmpty(result[0]))
                    result[0] = temp.Substring(0, temp.Length - 1);
                else
                    result = result.Append(temp.Substring(0, temp.Length - 1)).ToArray();
                return result;
            }
        }

        public int Count
        {
            get
            {
                if (Items is null || Items[0] is null)
                {
                    return 0;
                }
                return Items.Length;
            }
        }

        public ITopItem[] Items { get; private set; }

        public Dictionary<string, string> Group { get; private set; }

        public void Add(ITopItem item, string group)
        {
            if (_version < Items.Length)
                Items[_version] = item;
            else
                Items = Items.Append(item).ToArray();
            if (Group.ContainsKey(group))
                Group[group] += "," + item.Code;
            else
                Group.Add(group, item.Code);
            _version++;
        }

        public void Set(bool subGroup)
        {
            AddSubTitle = subGroup;
        }

        public void Set(string record)
        {
            Record = record;
        }
    }
}
