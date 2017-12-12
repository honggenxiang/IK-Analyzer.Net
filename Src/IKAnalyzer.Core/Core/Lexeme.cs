using System;
using System.Text;

namespace IKAnalyzer.Core
{
    /// <summary>
    ///词元
    /// </summary>
    public class Lexeme : IComparable<Lexeme>
    {
        /// <summary>
        /// 词元的相对起始位置
        /// </summary>
        public int Begin { get; set; }
        /// <summary>
        ///词元的起始位移
        /// </summary>
        public int Offset { get; set; }

        private int _length;
        /// <summary>
        /// 词元的长度
        /// </summary>
        public int Length
        {
            get => _length;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Length<0");
                }
                _length = value;
            }
        }

        private string _lexemeText;
        /// <summary>
        /// 词元文本
        /// </summary>
        public string LexemeText
        {
            get
            {
                if (_lexemeText == null) return string.Empty;
                return _lexemeText;
            }
            set
            {
                if (value == null)
                {
                    _lexemeText = string.Empty;
                    Length = 0;
                }
                else
                {
                    _lexemeText = value;
                    Length = value.Length;
                }

            }

        }


        /// <summary>
        /// 词元类型
        /// </summary>
        public LexemeType LexemeType { get; set; }


        public Lexeme(int offset, int begin, int length, LexemeType lexemeType)
        {
            this.Offset = offset;
            this.Begin = begin;
            this.Length = length;
            this.LexemeType = lexemeType;
        }
        /// <summary>
        /// 获取词元类型标示字符串
        /// </summary>
        /// <returns></returns>
        public string GetLexemeTypeString()
        {
            switch (LexemeType)
            {

                case LexemeType.TYPE_ENGLISH:
                    return "ENGLISH";

                case LexemeType.TYPE_ARABIC:
                    return "ARABIC";

                case LexemeType.TYPE_LETTER:
                    return "LETTER";

                case LexemeType.TYPE_CNWORD:
                    return "CN_WORD";

                case LexemeType.TYPE_CNCHAR:
                    return "CN_CHAR";

                case LexemeType.TYPE_OTHER_CJK:
                    return "OTHER_CJK";

                case LexemeType.TYPE_COUNT:
                    return "COUNT";

                case LexemeType.TYPE_CNUM:
                    return "TYPE_CNUM";

                case LexemeType.TYPE_CQUAN:
                    return "TYPE_CQUAN";

                default:
                    return "UNKONW";
            }
        }

        /// <summary>
        /// 获取词元在文本中的起始位置
        /// </summary>
        /// <returns></returns>
        public int GetBeginPosition()
        {
            return Offset + Begin;
        }
        /// <summary>
        /// 获取词元在文本中的结束位置
        /// </summary>
        /// <returns></returns>
        public int GetEndPosition()
        {
            return Offset + Begin + Length;
        }

        /// <summary>
        /// 判断词元相等算法
        /// 起始位置偏移、起始位置、终止位置相同
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;

            if (obj is Lexeme other)
            {
                if (Offset == other.Offset && Begin == other.Begin && Length == other.Length)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 词元哈希编码算法
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int absBegin = GetBeginPosition();
            int absEnd = GetEndPosition();
            return (absBegin * 37) + (absEnd * 31) + ((absBegin * absEnd) % Length);
        }

        /// <summary>
        /// 词元在排序集合中的比较算法
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Lexeme other)
        {
            if (this.Begin < other.Begin)
            {
                return -1;
            }
            else if (Begin == other.Begin)
            {
                //词元长度优先
                if (Length > other.Length)
                {
                    return -1;
                }
                else if (Length == other.Length)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// 合并两个相邻的词元
        /// </summary>
        /// <param name="l"></param>
        /// <param name="lexemeType"></param>
        /// <returns>词元是否成功合并</returns>
        public bool Append(Lexeme l, LexemeType lexemeType)
        {
            if (l != null && GetEndPosition() == l.GetBeginPosition())
            {
                Length += l.Length;
                LexemeType = lexemeType;
                return true;
            }
            else
            {
                return false;
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetBeginPosition()).Append("-").Append(GetEndPosition()).Append(" : ").Append(LexemeText);
            sb.Append(" : \t").Append(GetLexemeTypeString());
            return sb.ToString();
        }
    }
}
