using IKAnalyzer.Dic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 中文-日韩文字子分词器
    /// </summary>
    public class CJKSegmenter : ISegmenter
    {
        /// <summary>
        /// 子分词器标签
        /// </summary>
        private const string SEGMENTER_NAME = "CJK_SEGMENTER";
        /// <summary>
        /// 待处理的分词hit队列
        /// </summary>
        private LinkedList<Hit> tempHits;

        public CJKSegmenter()
        {
            tempHits = new LinkedList<Hit>();
        }

        public void Analyze(AnalyzerContext context)
        {
            if (context.CurrentCharType != CharType.CHAR_USELESS)
            {
                //优先处理tempHits中的hit
                if (tempHits.Count > 0)
                {
                    //处理词段队列
                    Hit[] tempArray = tempHits.ToArray();
                    foreach (var hit in tempArray)
                    {
                        var h = Dictionary.GetSingleton().MatchWithHit(context.SegmentBuff, context.Cursor, hit);
                        if (h.IsMatch())
                        {//输出当前的词
                            Lexeme newLexeme = new Lexeme(context.BuffOffset, h.Begin, context.Cursor - h.Begin + 1, LexemeType.TYPE_CNWORD);
                            context.AddLexeme(newLexeme);

                            if (!h.IsPrefix())
                            {//不是词前缀，hit不需要继续匹配，移除
                                tempHits.Remove(hit);
                            }
                        }
                        else if (h.IsUnMatch())
                        {//hit不是词，移除
                            tempHits.Remove(hit);
                        }
                    }
                }

                //*******************************
                //再对当前指针位置的字符进行单字匹配
                Hit singleCharHit = Dictionary.GetSingleton().MatchInMainDict(context.SegmentBuff, context.Cursor, 1);
                if (singleCharHit.IsMatch())
                {//首字成词
                    //输出当前的词
                    Lexeme newLexeme = new Lexeme(context.BuffOffset, context.Cursor, 1, LexemeType.TYPE_CNWORD);
                    //同时也是词前缀
                    if (singleCharHit.IsPrefix())
                    {
                        //前缀匹配则放入hit列表
                        tempHits.AddLast(singleCharHit);
                    }
                }
                else if (singleCharHit.IsPrefix())
                {//首字为词前缀
                 //前缀匹配则放入hit列表
                    tempHits.AddLast(singleCharHit);
                }
            }
        }

        public void Reset()
        {
            //队列清空
            tempHits.Clear();
        }
    }
}
