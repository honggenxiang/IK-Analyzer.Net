using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class AnalyzerContext
    {
        [TestMethod]
        public void ReaderCount()
        {
            char[] arr = new char[40];
            //30
            StringReader sr = new StringReader("123456789012345678901234567890");
            int readCount = sr.Read(arr, 0, 40);
        }
    }
}
