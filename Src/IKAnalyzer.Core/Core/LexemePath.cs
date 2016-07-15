using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// Lexeme链(路径)
    /// </summary>
    public class LexemePath : QuickSortSet, IComparable<LexemePath>
    {/// <summary>
     /// 起始位置
     /// </summary>
        public int PathBegin { get; private set; }
        /// <summary>
        /// 结束
        /// </summary>
        public int PathEnd { get; private set; }
        /// <summary>
        /// 词元链的有效字符长度
        /// </summary>
        public int PayloadLength { get; private set; }

        public LexemePath()
        {
            PathBegin = -1;
            PathEnd = -1;
            PayloadLength = 0;
        }
        /// <summary>
        /// 向LexemePath追加相交的Lexeme
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public bool AddCrossLexeme(Lexeme lexeme)
        {
            if (IsEmpty())
            {
                AddLexeme(lexeme);
                PathBegin = lexeme.Begin;
                PathEnd = lexeme.Begin + lexeme.Length;
                PayloadLength += lexeme.Length;
                return true;
            }
            else if (CheckCross(lexeme))
            {
                AddLexeme(lexeme);
                if (lexeme.Begin + lexeme.Length > PathEnd)
                {
                    PathEnd = lexeme.Begin + lexeme.Length;
                }
                PayloadLength = PathEnd - PathBegin;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 向LexemePath追加不想交的Lexeme
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public bool AddNotCrossLexeme(Lexeme lexeme)
        {
            if (IsEmpty())
            {
                AddLexeme(lexeme);
                PathBegin = lexeme.Begin;
                PathEnd = lexeme.Begin + lexeme.Length;
                PayloadLength += lexeme.Length;
                return true;
            }
            else if (CheckCross(lexeme))
            {
                return false;
            }
            else
            {
                AddLexeme(lexeme);
                PayloadLength += lexeme.Length;
                Lexeme head = PeekFirst();
                PathBegin = head.Begin;
                Lexeme tail = PeekLast();
                PathEnd = tail.Begin + tail.Length;
                return true;
            }
        }

        /// <summary>
        /// 移除尾部的Lexeme
        /// </summary>
        /// <returns></returns>
        public Lexeme RemoveTail()
        {
            Lexeme tail = PollLast();
            if (IsEmpty())
            {
                PathBegin = -1;
                PathEnd = -1;
                PayloadLength = 0;
            }
            else
            {
                PayloadLength -= tail.Length;
                Lexeme newTail = PeekLast();
                PathEnd = newTail.Begin + newTail.Length;
            }
            return tail;
        }
        /// <summary>
        /// 检测词元位置交叉(有歧义的切分)
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public bool CheckCross(Lexeme lexeme)
        {
            return (lexeme.Begin >= PathBegin && lexeme.Begin <= PathEnd) || (PathBegin >= lexeme.Begin && PathBegin < (lexeme.Begin + lexeme.Length));
        }
        /// <summary>
        /// 获取LexemePath的路径长度
        /// </summary>
        /// <returns></returns>
        public int GetPathLength()
        {
            return PathEnd - PathBegin;
        }
        /// <summary>
        /// X权重(词元长度积)
        /// </summary>
        /// <returns></returns>
        public int GetXWeight()
        {
            int product = 1;
            Cell c = Head;
            while (c != null && c.Lexeme != null)
            {
                product *= c.Lexeme.Length;
                c = c.Next;
            }
            return product;
        }
        /// <summary>
        /// 词元位置权重
        /// </summary>
        /// <returns></returns>
        public int GetPWeight()
        {
            int pWeight = 0;
            int p = 0;
            Cell c = Head;
            while (c != null && c.Lexeme != null)
            {
                p++;
                pWeight += p * c.Lexeme.Length;
                c = c.Next;
            }
            return pWeight;
        }

        public LexemePath Copy()
        {
            LexemePath theCopy = new LexemePath();
            theCopy.PathBegin = PathBegin;
            theCopy.PathEnd = PathEnd;
            theCopy.PayloadLength = PayloadLength;
            Cell c = Head;
            while (c != null && c.Lexeme != null)
            {
                theCopy.AddLexeme(c.Lexeme);
                c = c.Next;
            }
            return theCopy;
        }

        public int CompareTo(LexemePath other)
        {
            //比较有效文本长度
            if (PayloadLength > other.PayloadLength)
            {
                return -1;
            }
            else if (PayloadLength < other.PayloadLength)
            {
                return 1;
            }
            else
            {//比较词元个数，越少越好
                if (Size < other.Size)
                {
                    return -1;
                }
                else if (Size > other.Size)
                {
                    return 1;
                }
                else
                {
                    //路径跨度越大越好
                    if (GetPathLength() > other.GetPathLength())
                    {
                        return -1;
                    }
                    else if (GetPathLength() < other.GetPathLength())
                    {
                        return 1;
                    }
                    else
                    {//根据统计学结论，逆向切分概率高于正向切分，因此位置越靠后的优先
                        if (PathEnd > other.PathEnd)
                        {
                            return -1;
                        }
                        else if (PathEnd < other.PathEnd)
                        {
                            return 1;
                        }
                        else
                        {//词元越平均越好
                            if (GetXWeight() > other.GetXWeight())
                            {
                                return -1;
                            }
                            else if (GetXWeight() < other.GetXWeight())
                            {
                                return 1;

                            }
                            else
                            {
                                if (GetPWeight() > other.GetPWeight())
                                {
                                    return -1;
                                }
                                else if (GetPWeight() < other.GetPWeight())
                                {
                                    return 1;
                                }
                            }
                        }

                    }
                }

            }
            return 0;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("pathBegin  : ").Append(PathBegin).Append("\r\n");
            sb.Append("pathEnd  : ").Append(PathEnd).Append("\r\n");
            sb.Append("payloadLength  : ").Append(PayloadLength).Append("\r\n");
            Cell head = this.Head;
            while (head != null)
            {
                sb.Append("lexeme : ").Append(head.Lexeme).Append("\r\n");
                head = head.Next;
            }
            return sb.ToString();
        }
    }
}
