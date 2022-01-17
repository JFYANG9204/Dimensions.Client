using System;

namespace Dimensions.Bll.File
{
    [AttributeUsage(AttributeTargets.Class |
        AttributeTargets.Method,
        AllowMultiple = true)]
    internal class WriterInfoAttribute : Attribute
    {
        public WriterInfoAttribute(string file)
        {
            File = file;
        }

        public string File { get; }
    }
}