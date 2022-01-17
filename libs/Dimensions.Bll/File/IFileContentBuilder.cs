
using System.Collections.Generic;

namespace Dimensions.Bll.File
{
    public interface IFileContentBuilder
    {
        void Set(string type, params KeyValuePair<object, object>[] contents);
        string Get();
    }
}
