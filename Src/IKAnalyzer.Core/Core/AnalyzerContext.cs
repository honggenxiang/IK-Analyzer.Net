using IKAnalyzer.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 分词器上下文状态
    /// </summary>
    public class AnalyzerContext
    {/// <summary>
     /// 默认缓冲区大小
     /// </summary>
        private const int BUFF_SIZE = 4096;
        /// <summary>
        /// 缓冲区耗尽的临界值
        /// </summary>
        private const int BUFF_EXHAUST_CAITICAL = 100;

        /// <summary>
        /// 字符串读取缓冲
        /// </summary>
        public char[] segmentBuff { get; private set; }
        /// <summary>
        /// 字符类型数组
        /// </summary>
        private CharType[] charTypes;
        /// <summary>
        /// 记录Reader内已分析的字符串总长度
        /// 在分多段分析词元时，该变量累计当前的segmentBuff相对于reader其实位置的位移
        /// </summary>
        public int buffOffset { get; private set; }
        /// <summary>
        /// 当前缓冲区位置指针
        /// </summary>
        public int Cursor { get; private set; }
        /// <summary>
        /// 最近一次读取的，可处理的字符串长度
        /// </summary>
        private int available;
        /// <summary>
        /// 子分词器锁
        /// 该集合分控，说明有子分词器在占用segmentBuff
        /// </summary>
        private List<string> buffLocker;

        /// <summary>
        /// 原始分词结果集合，未经歧义处理
        /// </summary>
        public QuickSortSet OrgLexemes { get; private set; }
        /// <summary>
        /// LexemePath位置索引表
        /// </summary>
        private Dictionary<int, LexemePath> pathDict;
        /// <summary>
        /// 最终分词结果集
        /// </summary>
        private LinkedList<Lexeme> results;
        /// <summary>
        /// 分词器配置项
        /// </summary>
        private Configuration cfg;


        public AnalyzerContext(Configuration cfg)
        {
            this.cfg = cfg;
            segmentBuff = new char[BUFF_SIZE];
            charTypes = new CharType[BUFF_SIZE];
            buffLocker = new List<string>();
            OrgLexemes = new QuickSortSet();
            pathDict = new Dictionary<int, Core.LexemePath>();
            results = new LinkedList<Lexeme>();
        }

        public char CurrentChar
        {
            get
            {
                return segmentBuff[Cursor];
            }
        }

        public CharType CurrentCharType
        {
            get
            {
                return charTypes[Cursor];
            }
        }

        /// <summary>
        /// 根据context的上下文情况，填充segmentBuff
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>返回待分析的(有效的)字符长度</returns>
        public int FillBuff(StringReader reader)
        {
            int readCount = 0;
            if (buffOffset == 0)
            {
                //首次读取reader
                readCount = reader.Read(segmentBuff, 0, BUFF_SIZE);
            }
            else
            {
                int offset = available - Cursor;
                if (offset > 0)
                {//最近一次读取的>最近一次处理的，将未处理的字符串拷贝到segmentBuff头部
                    Array.Copy(segmentBuff, Cursor, segmentBuff, 0, offset);
                    readCount = offset;
                }
                //继续读取reader,以onceReadIn-OnceAnalyzed为起始位置，继续填充segmentBuff剩余的部分
                readCount += reader.Read(segmentBuff, offset, BUFF_SIZE - offset);
            }
            //记录最后一次从Reader中读入的可用字符长度
            available = readCount;
            //重置当前指针
            Cursor = 0;
            return readCount;
        }

        /// <summary>
        /// 初始化buff指针，处理第一个字符
        /// </summary>
        public void InitCursor()
        {
            Cursor = 0;
            segmentBuff[Cursor] = CharacterUtil.Regularize(segmentBuff[Cursor]);
            charTypes[Cursor] = CharacterUtil.IdentifyCharType(segmentBuff[Cursor]);
        }
        /// <summary>
        /// 指针+1
        /// 成功返回true；指针已经到buff尾部，不能前进，返回false
        /// 并处理当前字符
        /// </summary>
        /// <returns></returns>
        public bool MoveCursor()
        {
            if (Cursor < available - 1)
            {
                Cursor++;
                segmentBuff[Cursor] = CharacterUtil.Regularize(segmentBuff[Cursor]);
                charTypes[Cursor] = CharacterUtil.IdentifyCharType(segmentBuff[Cursor]);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 设置当前segmentBuff为锁定状态
        /// 加入占用segmentBuff的子分词器名称，表示占用segmentBuff
        /// </summary>
        /// <param name="segmenterName"></param>
        public void LockBuff(string segmenterName)
        {
            buffLocker.Add(segmenterName);
        }
        /// <summary>
        /// 移除制定的子分词器名，释放对segmentBuff的占用
        /// </summary>
        /// <param name="segmenterName"></param>
        public void UnlockBuff(string segmenterName)
        {

            buffLocker.Remove(segmenterName);
        }
        /// <summary>
        /// 只要buffLocker中存在segmenterName
        /// 则buffer被锁定
        /// </summary>
        /// <returns>缓冲区是否被锁定</returns>
        public bool IsBufferLocked()
        {
            return buffLocker.Count > 0;
        }
        /// <summary>
        /// 判断当前segmentBuff是否已经用完
        /// 当前执行cursor移至segmentBuff末端available-1
        /// </summary>
        /// <returns></returns>
        public bool IsBufferConsumed()
        {
            return Cursor == available - 1;
        }
        /// <summary>
        /// 判断segmentBuff是否需要读取新数据
        /// 
        /// 满足以下条件是,
        /// 1、available==BUFF_SIZE表示buffer满载
        /// 2、buffIndex《 available-1 &&buffIndex>available-BUFF_EXHAUST_CRITICAL表示当前指针处于临界区内
        /// 3、！context.isBufferLocked()表示没有segmenter在占用buffer
        /// 要终端当前循环(buffer要进行移位，并再读取数据的操作)
        /// </summary>
        /// <returns></returns>
        public bool NeedRefillBuffer()
        {
            return available == BUFF_SIZE
                && Cursor < available - 1
                && Cursor > available - BUFF_EXHAUST_CAITICAL
                && !IsBufferLocked();
        }
        /// <summary>
        /// 累计当前的segmentBuff相对于reader起始位置的位移
        /// </summary>
        public void MarkBufferOffset()
        {
            buffOffset += Cursor;
        }
        /// <summary>
        /// 向分词结果集添加词元
        /// </summary>
        /// <param name="lexeme"></param>
        public void AddLexeme(Lexeme lexeme)
        {
            OrgLexemes.AddLexeme(lexeme);
        }
        /// <summary>
        /// 添加分词结果路径
        /// 路径起始位置-->路径 映射表
        /// </summary>
        /// <param name="path"></param>
        public void AddLexemePath(LexemePath path)
        {
            //TODO:
        }
        /// <summary>
        /// 推送分词结果到结果集合
        /// 1、从buff又不遍历到cursor已处理位置
        /// 2、将dict中存在的分词结果推入results
        /// 3、将dict中不存在的CJDK字符已单字方式推入results
        /// </summary>

        public void OutputToResult()
        {

        }
    }


}
