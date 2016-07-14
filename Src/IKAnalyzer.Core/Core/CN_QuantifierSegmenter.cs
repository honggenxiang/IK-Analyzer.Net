using IKAnalyzer.Dic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 中文数量词子分词器
    /// </summary>
    public class CN_QuantifierSegmenter : ISegmenter
    {
        /// <summary>
        /// 字分词器标签
        /// </summary>
        private const string SEGMENTER_NAME = "QUAN_SEGMENTER";
        /// <summary>
        /// 中文数词
        /// </summary>
        private static string Chn_Num = "一二两三四五六七八九十零壹贰叁肆伍陆柒捌玖拾百千万亿拾佰仟萬億兆卅廿";
        private static List<char> ChnNumberChars = new List<char>();

        static CN_QuantifierSegmenter()
        {
            char[] ca = Chn_Num.ToCharArray();
            foreach (var nchar in ca)
            {
                ChnNumberChars.Add(nchar);
            }
        }
        /// <summary>
        /// 词元的开始位置，同时作为字分词器状态标识 当start>-1时，标识当前的分词器正在处理字符
        /// </summary>
        private int nStart;
        /// <summary>
        /// 记录次元结束位置 end记录的是词元中最后一个出现的合理的数词结束
        /// </summary>
        public int nEnd;
        /// <summary>
        /// 待处理的量词hit队列
        /// </summary>
        private LinkedList<Hit> countHits;
        public CN_QuantifierSegmenter()
        {
            nStart = -1;
            nEnd = -1;
            countHits = new LinkedList<Hit>();
        }

        public void Analyze(AnalyzerContext context)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 处理数词
        /// </summary>
        /// <param name="context"></param>
        private void ProcessCNumber(AnalyzerContext context)
        {
            if (nStart == -1 && nEnd == -1)//初始状态
            {
                if (context.CurrentCharType == CharType.CHAR_CHINESE && ChnNumberChars.Contains(context.CurrentChar))
                {
                    //记录数词的起始、结束位置
                    nStart = context.Cursor;
                    nEnd = nStart;
                }
            }
            else//正在处理状态
            {
                if (context.CurrentCharType == CharType.CHAR_CHINESE && ChnNumberChars.Contains(context.CurrentChar))
                {//记录数词的结束位置
                    nEnd = context.Cursor;
                }
                else
                { //输出数词
                    OutputNumLexeme(context);
                    nStart = -1;
                    nEnd = -1;
                }
            }
            //缓冲区已经用完，还有未输出的数词
            if (context.IsBufferConsumed())
            {
                if (nStart != -1 && nEnd != -1)
                {
                    //输出数词
                    OutputNumLexeme(context);
                    nStart = -1;
                    nEnd = -1;
                }
            }
        }
        /// <summary>
        /// 处理中文量词
        /// </summary>
        /// <param name="context"></param>
        private void ProcessCount(AnalyzerContext context)
        {
            //判断是否需要扫描量词
            if (!NeedCountScan(context))
            {
                return;
            }
            if (context.CurrentCharType == CharType.CHAR_CHINESE)
            {
                //优先处理countHits中的Hit
                if (countHits.Count > 0)
                {
                    Hit[] tmpArray = countHits.ToArray();
                    foreach (var hit in tmpArray)
                    {
                        var h = Dictionary.GetSingleton().MatchWithHit(context.SegmentBuff, context.Cursor, hit);
                        if (h.IsMatch())
                        {
                            //输出当前的词
                            Lexeme newLexeme = new Lexeme(context.buffOffset, hit.Begin, context.Cursor - hit.Begin + 1, LexemeType.TYPE_COUNT);
                            context.AddLexeme(newLexeme);

                            if (!h.IsPrefix())
                            {//不是前缀，hit不需要继续匹配，移除
                                countHits.Remove(hit);
                            }
                        }
                        else if (hit.IsUnMatch())
                        {//hit不是词，移除
                            countHits.Remove(hit);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 判断是否需要扫描量词
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool NeedCountScan(AnalyzerContext context)
        {
            if ((nStart != -1 && nEnd != -1) || countHits.Count != 0)
            {//正在处理中文数词，或者正在处理量词
                return true;
            }
            else
            {
                //找到一个相邻的数词
                if (!context.OrgLexemes.IsEmpty())
                {
                    Lexeme l = context.OrgLexemes.PeekLast();
                    if (LexemeType.TYPE_CNUM == l.LexemeType || LexemeType.TYPE_ARABIC == l.LexemeType)
                    {
                        if (l.Begin + l.Length == context.Cursor)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 添加数词词元到结果集
        /// </summary>
        /// <param name="context"></param>
        private void OutputNumLexeme(AnalyzerContext context)
        {
            if (nStart > -1 && nEnd > -1)
            {
                //输出数词
                Lexeme newLexeme = new Lexeme(context.buffOffset, nStart, nEnd - nStart + 1, LexemeType.TYPE_CNUM);
                context.AddLexeme(newLexeme);
            }
        }

        public void Reset()
        {
            nStart = -1;
            nEnd = -1;
            countHits.Clear();
        }
    }
}
