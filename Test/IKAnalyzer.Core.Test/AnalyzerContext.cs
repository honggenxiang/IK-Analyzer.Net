using System.Collections.Generic;
using System.IO;
using Xunit;

namespace IKAnalyzer.Core.Test
{
    public class AnalyzerContext
    {
        [Fact]
        public void ReaderCount()
        {

            LinkedList<Person> l = new LinkedList<Person>();
            var person = l.First.Value;


            char[] arr = new char[40];
            //30
            StringReader sr = new StringReader("123456789012345678901234567890");
            int readCount = sr.Read(arr, 0, 40);
        }
    }
}
