using System.Collections;
using System.Linq;

namespace Dimensions.Bll.Mdd
{
    public class DefinitionCollection : IDefinitionCollection, IEnumerable
    {
        public IDefinition this[int index]
        {
            get
            {
                if (_definitions != null)
                {
                    if (_definitions.Length > index)
                        return _definitions[index];
                    else
                        return null;
                }
                else
                {
                    return null;
                }
            }
        }

        private IDefinition[] _definitions;

        public int Count
        {
            get
            {
                if (_definitions is null)
                    return 0;
                else
                    return _definitions.Length;
            }
        }

        public void Add(IDefinition definition)
        {
            if (_definitions is null)
                _definitions = new IDefinition[1];
            if (_definitions[0] is null)
                _definitions[0] = definition;
            else
                _definitions = _definitions.Append(definition).ToArray();
        }

        public bool Exists(string varName)
        {
            bool result = false;
            if (_definitions is null)
            {
                return false;
            }
            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].VariableName.ToLower() == varName.ToLower())
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            if (_definitions != null)
            {
                for (int i = 0; i < _definitions.Length; i++)
                {
                    yield return _definitions[i];
                }
            }
            else
            {
                yield return null;
            }
        }
    }
}
