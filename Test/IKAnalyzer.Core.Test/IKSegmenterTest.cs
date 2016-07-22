using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class IKSegmenterTest
    {
        [TestMethod]
        public void Segment()
        {
            IKSegmenter iKSegmenter = new IKSegmenter(new System.IO.StringReader("unit100"), true);
            Lexeme l = iKSegmenter.Next();
            string result = string.Empty;
            while (l != null)
            {
                result += l.LexemeText + " - ";
                l = iKSegmenter.Next();
            }
        }
    }
}
