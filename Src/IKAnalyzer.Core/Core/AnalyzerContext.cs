using IKAnalyzer.Config;
using IKAnalyzer.Dic;
using System;
using System.Collections.Concurrent;
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
        public char[] SegmentBuff { get; private set; }
        /// <summary>
        /// 字符类型数组
        /// </summary>
        private CharType[] charTypes;
        /// <summary>
        /// 记录Reader内已分析的字符串总长度
        /// 在分多段分析词元时，该变量累计当前的segmentBuff相对于reader其实位置的位移
        /// </summary>
        public int BuffOffset { get; private set; }
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
            SegmentBuff = new char[BUFF_SIZE];
            charTypes = new CharType[BUFF_SIZE];
            buffLocker = new List<string>();
            OrgLexemes = new QuickSortSet();
            pathDict = new Dictionary<int, LexemePath>();
            results = new LinkedList<Lexeme>();
        }

        public char CurrentChar
        {
            get
            {
                return SegmentBuff[Cursor];
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
        public int FillBuffer(StringReader reader)
        {
            int readCount = 0;
            if (BuffOffset == 0)
            {
                //首次读取reader
                readCount = reader.Read(SegmentBuff, 0, BUFF_SIZE);
            }
            else
            {
                int offset = available - Cursor;
                if (offset > 0)
                {//最近一次读取的>最近一次处理的，将未处理的字符串拷贝到segmentBuff头部
                    Array.Copy(SegmentBuff, Cursor, SegmentBuff, 0, offset);
                    readCount = offset;
                }
                //继续读取reader,以onceReadIn-OnceAnalyzed为起始位置，继续填充segmentBuff剩余的部分
                readCount += reader.Read(SegmentBuff, offset, BUFF_SIZE - offset);
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
            SegmentBuff[Cursor] = CharacterUtil.Regularize(SegmentBuff[Cursor]);
            charTypes[Cursor] = CharacterUtil.IdentifyCharType(SegmentBuff[Cursor]);
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
                SegmentBuff[Cursor] = CharacterUtil.Regularize(SegmentBuff[Cursor]);
                charTypes[Cursor] = CharacterUtil.IdentifyCharType(SegmentBuff[Cursor]);
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
        public void UnlockBuffer(string segmenterName)
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
            BuffOffset += Cursor;
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
            if (path != null)
            {
                if (pathDict.ContainsKey(path.PathBegin))
                    pathDict[path.PathBegin] = path;
                else
                    pathDict.Add(path.PathBegin, path);
            }
        }
        /// <summary>
        /// 推送分词结果到结果集合
        /// 1、从buff头部遍历到cursor已处理位置
        /// 2、将dict中存在的分词结果推入results
        /// 3、将dict中不存在的CJDK字符已单字方式推入results
        /// </summary>

        public void OutputToResult()
        {
            int index = 0;
            while (index <= Cursor)
            {
                //跳过F非CJK字符
                if (charTypes[index] == CharType.CHAR_USELESS)
                {
                    index++;
                    continue;
                }
                //从pathDict找到对应index位置的LexemePath
                LexemePath path;
                pathDict.TryGetValue(index, out path);
                if (path != null)
                {
                    //输出LexemePath中的lexeme到results集合
                    Lexeme l = path.PollFirst();
                    while (l != null)
                    {
                        results.AddLast(l);
                        //将index移至lexeme后
                        index = l.Begin + l.Length;
                        l = path.PollFirst();
                        if (l != null)
                        {
                            //输出path内部，词元间遗漏的单字
                            for (; index < l.Begin; index++)
                            {
                                OutputStringCJK(index);
                            }
                        }
                    }
                }
                else
                {//pathDict中找不到index对应的LexemePath
                    //单字输出
                    OutputStringCJK(index);
                    index++;
                }
            }
            //清空当前的Dict
            pathDict.Clear();
        }
        /// <summary>
        /// 对CJK字符进行单字输出
        /// </summary>
        private void OutputStringCJK(int index)
        {
            if (charTypes[index] == CharType.CHAR_CHINESE)
            {
                Lexeme singleCharLexeme = new Lexeme(BuffOffset, index, 1, LexemeType.TYPE_CNCHAR);
                results.AddLast(singleCharLexeme);
            }
            else if (charTypes[index] == CharType.CHAR_OTHER_CJK)
            {
                Lexeme singleCharLexeme = new Lexeme(BuffOffset, index, 1, LexemeType.TYPE_OTHER_CJK);
                results.AddLast(singleCharLexeme);
            }
        }
        /// <summary>
        /// 返回lexeme
        /// 同时处理合并
        /// </summary>
        /// <returns></returns>
        public Lexeme GetNextLexeme()
        {
            Lexeme result = results.FirstOrDefault();
            //删除第一个节点
            if (results != null && results.Count > 0) results.RemoveFirst();
            while (result != null)
            {
                
                //数量词合并
                Compound(result);
                if (Dictionary.GetSingleton().IsStopWord(SegmentBuff, result.Begin, result.Length))
                {
                    //是停止词继续去列表的下一个
                    result = results.FirstOrDefault();
                    //删除第一个节点
                    if (results!=null&&results.Count > 0) results.RemoveFirst();
                }
                else
                {//不是停用词，生成Lexeme的次元文本，输出
                    result.LexemeText = new string(SegmentBuff, result.Begin, result.Length);
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 组合词元
        /// </summary>
        private void Compound(Lexeme result)
        {
            if (!cfg.UseSmart)
            {
                return;
            }

            //数量词合并处理
            if (results.Count > 0)
            {
                if (result.LexemeType == LexemeType.TYPE_ARABIC)
                {
                    Lexeme nextLexeme = results.First.Value;
                    bool appendOK = false;
                    if (LexemeType.TYPE_CNUM == nextLexeme.LexemeType)
                    {//合并英文数次+中文数词
                        appendOK = result.Append(nextLexeme, LexemeType.TYPE_CNUM);

                    }
                    else if (LexemeType.TYPE_COUNT == nextLexeme.LexemeType)
                    {
                        //合并英文数次+中文量词
                        appendOK = result.Append(nextLexeme, LexemeType.TYPE_CQUAN);
                    }
                    if (appendOK)
                    {
                        //弹出
                        results.RemoveFirst();
                    }
                }

                //可能存在第二轮合并
                if (LexemeType.TYPE_CNUM == result.LexemeType && results.Count > 0)
                {
                    Lexeme nextLexeme = results.First.Value;
                    bool appendOK = false;
                    if (LexemeType.TYPE_COUNT == nextLexeme.LexemeType)
                    {
                        //合并中文数次+中文量词
                        appendOK = result.Append(nextLexeme, LexemeType.TYPE_CQUAN);
                        if (appendOK)
                        {
                            //弹出
                            results.RemoveFirst();
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 重置分词上下文状态
        /// </summary>
        public void Reset()
        {
            buffLocker.Clear();
            OrgLexemes = new Core.QuickSortSet();
            available = 0;
            BuffOffset = 0;
            charTypes = new CharType[BUFF_SIZE];
            Cursor = 0;
            results.Clear();
            SegmentBuff = new char[BUFF_SIZE];
            pathDict.Clear();
        }
    }


}
