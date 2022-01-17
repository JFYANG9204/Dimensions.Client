using System;
using System.IO;
using System.Text;

namespace Dimensions.Client
{
    static class ErrorLogger
    {
        static readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");
        public static void WirteLine(string content)
        {
            if (!File.Exists(_path))
            {
                FileStream fs = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
            }
            string _text = $"[{DateTime.Now}] {content}";
            using (StreamWriter sw = new StreamWriter(_path, true, Encoding.Default))
            {
                sw.WriteLine(_text);
            }
        }
    }
}
