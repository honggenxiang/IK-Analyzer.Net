using Xunit;

namespace IKAnalyzer.Core.Test
{
    public class IKSegmenterTest
    {
        [Fact]
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
