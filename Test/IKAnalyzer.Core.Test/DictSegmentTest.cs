using IKAnalyzer.Dic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class DictSegmentTest
    {
        [TestMethod]
        public void FillSegment()
        {
            DictSegment dictSegment = new DictSegment((char)0);
            List<string> list = new List<string>() { "北京", "北京市", "北京高中", "北京市学科网" };
            foreach (var str in list)
            {
                dictSegment.FillSegment(str.ToArray());
            }

        }

        [TestMethod]
        public void PropertyTest()
        {
            Person Person = new Person();

            string temp = Person._name;
        }
    }
}
