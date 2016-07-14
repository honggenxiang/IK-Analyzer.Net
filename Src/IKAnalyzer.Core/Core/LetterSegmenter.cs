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
        private int arabicStart;
        /// <summary>
        /// 阿拉伯数字结束位置
        /// </summary>
        private int arabicEnd;

        public LetterSegmenter()
        {
            Array.Sort(Letter_Connector);
            Array.Sort(Num_Connector);
            start = -1;
            end = -1;
            englishStart = -1;
            englishEnd = -1;
            arabicStart = -1;
            arabicEnd = -1;
        }
        public void Analyze(AnalyzerContext context)
        {
            bool bufferLockFlag = false;
            //处理英文字母
            bufferLockFlag = ProcessEnglishLetter(context) || bufferLockFlag;
            //处理阿拉伯字母
            bufferLockFlag = ProcessArabicLetter(context) || bufferLockFlag;
            //处理混合字母(这个要最后处理,可以通过QuickSortSet排除重复)
            bufferLockFlag = ProcessMixLetter(context) || bufferLockFlag;

            //判断是否锁定缓冲区
            if (bufferLockFlag)
            {
                context.LockBuff(SEGMENTER_NAME);
            }
            else
            {//对缓冲区解锁
                context.UnlockBuff(SEGMENTER_NAME);
            }
        }
        /// <summary>
        /// 处理纯英文字母输出
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ProcessEnglishLetter(AnalyzerContext context)
        {
            bool needLock = false;
            if (englishStart == -1)//当前的分词器尚未开始处理英文字母
            {
                if (context.CurrentCharType == CharType.CHAR_ENGLISH)
                {
                    //记录起始指针的位置，表明分词器进入处理状态
                    englishStart = context.Cursor;
                    englishEnd = englishStart;
                }
            }
            else
            {//当前的分词器正在处理英文字符
                if (context.CurrentCharType == CharType.CHAR_ENGLISH)
                {
                    //记录当前指针的位置为结束位置
                    englishEnd = context.Cursor;
                }
                else
                {
                    //遇到非English字符，输出次元
                    Lexeme newLexeme = new Lexeme(context.buffOffset, englishStart, englishEnd - englishStart + 1, LexemeType.TYPE_ENGLISH);
                    context.AddLexeme(newLexeme);
                    englishStart = -1;
                    englishEnd = -1;
                }

            }
            //判断缓冲区是否已经读完
            if (context.IsBufferConsumed())
            {
                if (englishStart != -1 && englishEnd != -1)
                {
                    //缓冲区读完，输出次元
                    Lexeme newLexeme = new Lexeme(context.buffOffset, englishStart, englishEnd - englishStart + 1, LexemeType.TYPE_ENGLISH);
                    context.AddLexeme(newLexeme);
                    englishStart = -1;
                    englishEnd = -1;
                }
            }
            //判断是否锁定缓冲区
            if (englishStart == -1 && englishEnd == -1)
            {
                //对缓冲区解锁
                needLock = false;
            }
            else
            {
                needLock = true;
            }
            return needLock;
        }

        /// <summary>
        /// 处理阿拉伯数字输出
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ProcessArabicLetter(AnalyzerContext context)
        {
            bool needLock = false;
            if (arabicStart == -1)//当前的分词器尚未开始处理数字字符
            {
                if (context.CurrentCharType == CharType.CHAR_ARABIC)
                {
                    //记录起始指针的位置，表明分词器进入处理状态
                    arabicStart = context.Cursor;
                    arabicEnd = arabicStart;
                }
            }
            else
            {//当前的分词器正在处理数字字符
                if (context.CurrentCharType == CharType.CHAR_ARABIC)
                {
                    //记录当前指针的位置为结束位置
                    arabicEnd = context.Cursor;
                }
                else if (context.CurrentCharType == CharType.CHAR_USELESS && IsNumConnector(context.CurrentChar))
                {
                    //不输出数字，但不标记结束
                }
                else
                {//遇到非Arabic字符，输出词元
                    Lexeme newLexeme = new Lexeme(context.buffOffset, arabicStart, arabicEnd - arabicStart + 1, LexemeType.TYPE_ARABIC);
                    context.AddLexeme(newLexeme);
                    arabicStart = -1;
                    arabicEnd = -1;
                }
            }
            //判断缓冲区是否已经读完
            if (context.IsBufferConsumed())
            {
                if (arabicStart == -1 && arabicEnd == -1)
                {
                    Lexeme newLexeme = new Lexeme(context.buffOffset, arabicStart, arabicEnd - arabicStart + 1, LexemeType.TYPE_ARABIC);
                    context.AddLexeme(newLexeme);
                    arabicStart = -1;
                    arabicEnd = -1;
                }
            }

            //判断是否锁定缓冲区
            if (arabicStart == -1 && arabicEnd == -1)
            {
                //对缓冲区解锁
                needLock = false;
            }
            else
            {
                needLock = true;
            }
            return needLock;
        }
        /// <summary>
        /// 处理数字字母混合输出
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ProcessMixLetter(AnalyzerContext context)
        {
            bool needLock = false;
            if (start == -1)//当前的分词器尚未开始处理字符
            {
                if (context.CurrentCharType == CharType.CHAR_ARABIC || context.CurrentCharType == CharType.CHAR_CHINESE)
                {//记录起始指针的位置，表明分词器进入处理状态
                    start = context.Cursor;
                    end = start;
                }
            }
            else//当前的分词器正在处理字符
            {
                if (context.CurrentCharType == CharType.CHAR_ARABIC || context.CurrentCharType == CharType.CHAR_ENGLISH)
                {
                    //记录下可能的结束位置
                    end = context.Cursor;
                }
                else if (context.CurrentCharType == CharType.CHAR_USELESS && IsLetterConnector(context.CurrentChar))
                { //记录下可能的结束位置
                    end = context.Cursor;
                }
                else
                {//遇到非Letter字符，输出词元
                    Lexeme newLexeme = new Lexeme(context.buffOffset, start, end - start + 1, LexemeType.TYPE_LETTER);
                    start = -1;
                    end = -1;
                }

            }
            //判断缓冲区是否已经读完
            if (context.IsBufferConsumed())
            {
                if (start != -1 && end != -1)
                {
                    //缓冲已读完，输出词元
                    Lexeme newLexeme = new Lexeme(context.buffOffset, start, end - start + 1, LexemeType.TYPE_LETTER);
                    start = -1;
                    end = -1;
                }
            }

            if (start == -1 && end == -1)
            {
                needLock = false;
            }
            else
            {
                needLock = true;
            }
            return needLock;
        }
        /// <summary>
        /// 判断是否是字母链接符号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsLetterConnector(char input)
        {
            int index = Array.BinarySearch(Letter_Connector, input);
            return index >= 0;
        }
        /// <summary>
        /// 判断是否是数字链接符号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsNumConnector(char input)
        {
            int index = Array.BinarySearch(Num_Connector, input);
            return index >= 0;
        }

        public void Reset()
        {
            start = -1;
            end = -1;
            arabicStart = -1;
            arabicEnd = -1;
            englishStart = -1;
            englishEnd = -1;
        }
    }
}
