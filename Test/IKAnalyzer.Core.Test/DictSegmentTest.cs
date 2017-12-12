using IKAnalyzer.Dic;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IKAnalyzer.Core.Test
{
    public class DictSegmentTest
    {
        [Fact]
        public void FillSegment()
        {
            DictSegment dictSegment = new DictSegment((char)0);
            List<string> list = new List<string>() { "北京", "北京市", "北京高中", "北京市学科网" };
            foreach (var str in list)
            {
                dictSegment.FillSegment(str.ToArray());
            }

        }

        [Fact]
        public void PropertyTest()
        {
            Person Person = new Person();

            string temp = Person._name;
        }
    }
}
