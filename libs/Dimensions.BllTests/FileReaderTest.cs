using Dimensions.Bll.FileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dimensions.BllTests
{
    [TestClass]
    public class FileReaderTest
    {
        [TestMethod]
        public void Reader()
        {
            string path = @"F:\Program\C#\testSpec\77403614_R19.mdd";
            MddFileReader reader = new MddFileReader();
            reader.Load(path);
            Console.ReadLine();
        }
    }
}
