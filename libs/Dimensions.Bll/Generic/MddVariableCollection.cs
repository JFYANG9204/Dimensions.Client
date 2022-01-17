using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dimensions.Bll.Generic
{
    [DebuggerDisplay("Count = {Count}")]
    class MddVariableCollection : IMddVariableCollection, IEnumerable, IEnumerable<IMddVariable>
    {
        private int _version;

        public MddVariableCollection()
        {
            _version = 0;
            Data = new List<IMddVariable>();
        }

        public IMddVariable this[string name]
        {
            get
            {
                IMddVariable mddVariable = null;
                foreach (var variable in Data)
                {
                    if (variable.Name.ToLower() == name.ToLower())
                    {
                        mddVariable = variable;
                    }
                }
                return mddVariable;
            }
        }

        public IMddVariable this[int index]
        {
            get
            {
                if (index >= Data.Count)
                {
                    return null;
                }
                else
                {
                    return Data[index];
                }
            }
        }

        public List<IMddVariable> Data { get; private set; }

        public int Count
        {
            get
            {
                if (Data == null)
                {
                    return 0;
                }
                else
                {
                    return Data.Count;
                }
            }
        }

        public void Add(IMddVariable variable)
        {
            Data.Add(variable);
            _version++;
        }

        public bool Contains(string id)
        {
            bool result = false;
            foreach (var variable in Data)
            {
                if (variable.Id == id)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public IMddVariable Get(string id)
        {
            IMddVariable findVariable = null;
            foreach (var variable in Data)
            {
                if (variable.Id == id)
                {
                    findVariable = variable;
                    break;
                }
            }
            return findVariable;
        }

        public void Remove(IMddVariable variable)
        {
            if (Data is null || !Data.Contains(variable)) return;
            Data.Remove(variable);
        }

        public void RemoveAt(int index)
        {
            if (Data is null || index < 0 || index >= Data.Count) return;
            Data.RemoveAt(index);
        }

        public void Merge(IMddVariableCollection variables, MddMergeType type)
        {
            List<IMddVariable> result = new List<IMddVariable>();
            switch (type)
            {
                case MddMergeType.Front:
                    foreach (var item in variables)
                    {
                        result.Add(item);
                    }
                    foreach (var item in Data)
                    {
                        result.Add(item);
                    }
                    break;
                case MddMergeType.Behind:
                    foreach (var item in Data)
                    {
                        result.Add(item);
                    }
                    foreach (var item in variables)
                    {
                        result.Add(item);
                    }
                    break;
                default:
                    break;
            }
            Data = result;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<IMddVariable> IEnumerable<IMddVariable>.GetEnumerator()
        {
            return new Enumerator(this);
        }


        public struct Enumerator : IEnumerator<IMddVariable>
        {
            private MddVariableCollection list;
            private int index;
            private IMddVariable current;
            private int version;

            public object Current
            {
                get
                {
                    return current;
                }
            }

            IMddVariable IEnumerator<IMddVariable>.Current
            {
                get { return current; }
            }


            internal Enumerator(MddVariableCollection collection)
            {
                list = collection;
                version = collection._version;
                index = 0;
                current = default;
            }

            public bool MoveNext()
            {
                MddVariableCollection localList = list;
                if (version == localList._version && index < localList.Count)
                {
                    current = list[index];
                    index++;
                    return true;
                }
                index = list.Count + 1;
                current = default;
                return false;
            }

            public void Reset()
            {
                index = 0;
                current = default;
            }

            public void Dispose()
            {
            }
        }

    }
}
