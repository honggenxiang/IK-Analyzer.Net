using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 英文字符及阿拉伯数字子分词器
    /// </summary>
    /// <remarks>
    /// author:hgx
    /// description:
    /// createtime:20160714
    /// </remarks>
    public class LetterSegmenter : ISegmenter
    {
        /// <summary>
        /// 子分词器标签
        /// </summary>
        private const string SEGMENTER_NAME = "LETTER_SEGMENTER";
        /// <summary>
        /// 链接符号
        /// </summary>
        private static readonly char[] Letter_Connector = { '#', '&', '+', '-', '.', '@', '_' };
        /// <summary>
        /// 数字符号
        /// </summary>
        private static readonly char[] Num_Connector = { ',', '.' };

        /// <summary>
        /// 词元的开始位置
        /// 同时作为子分词器状态标识
        /// 当Start>-1时，标识当前的分词器正在处理字符
        /// </summary>
        private int start;
        /// <summary>
        /// 记录词元结束位置
        /// end 记录的是词元中最后一个出现Letter但非Sign_Connector的字符位置
        /// </summary>
        private int end;
        /// <summary>
        /// 字符起始位置
        /// </summary>
        private int englishStart;

        /// <summary>
        /// 字母结束位置
        /// </summary>
        private int englishEnd;

        /// <summary>
        /// 阿拉伯数字起始位置
        /// </summary>
        private int ArabicStart;
        /// <summary>
        /// 阿拉伯数字结束位置
        /// </summary>
        private int ArabicEnd;
        public void Analyze(AnalyzerContext context)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
