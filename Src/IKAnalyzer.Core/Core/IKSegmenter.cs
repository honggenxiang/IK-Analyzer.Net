using IKAnalyzer.Config;
using IKAnalyzer.Dic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// Ik分词器主类
    /// </summary>
    public class IKSegmenter
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private readonly object objLock = new object();
        /// <summary>
        /// 字符串reader
        /// </summary>
        private StringReader input;
        /// <summary>
        /// 分词器配置项
        /// </summary>
        private Configuration cfg;
        /// <summary>
        /// 分词器上下文
        /// </summary>
        private AnalyzerContext context;
        /// <summary>
        /// 分词处理器列表
        /// </summary>
        private List<ISegmenter> segmenters;

        //分词歧义裁决器
        private IKArbitrator arbitrator;
        /// <summary>
        /// Ik分词器构造函数
        /// </summary>
        public IKSegmenter(StringReader input, bool useSmart)
        {
            this.input = input;
            cfg = DefaultConfig.GetInstance();
            cfg.UseSmart = useSmart;
            Init();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            //初始化词典单例
            Dictionary.Initial(cfg);
            //初始化分词上下文
            context = new AnalyzerContext(cfg);
            //加载子分词器
            segmenters = LoadSegmenters();
            //加载歧义裁决器
            arbitrator = new IKArbitrator();
        }
        /// <summary>
        /// 初始化词典，加载子分词器实现
        /// </summary>
        /// <returns></returns>
        private List<ISegmenter> LoadSegmenters()
        {
            List<ISegmenter> segmenters = new List<ISegmenter>(4);
            //处理字母的子分词器
            segmenters.Add(new LetterSegmenter());
            //处理中文数量词的子分词器
            segmenters.Add(new CN_QuantifierSegmenter());
            //处理中文词的子分词器
            segmenters.Add(new CJKSegmenter());

            return segmenters;
        }
        /// <summary>
        /// 分词，获取下一个词元
        /// </summary>
        /// <returns></returns>
        public Lexeme Next()
        {
            lock (objLock)
            {
                Lexeme l = null;
                while ((l = context.GetNextLexeme()) == null)
                {
                    /*****
                    从readrer中读取数据，填充buffer
                    如果reader是分词读入buffer的，那么buffer要进行移位处理
                    移位处理上次读入的但未处理的数据 
                    */
                    int available = context.FillBuffer(input);
                    if (available <= 0)
                    {
                        //reader已经读完
                        context.Reset();
                        return null;
                    }
                    else
                    {
                        //初始化指针
                        context.InitCursor();
                        do
                        {
                            //遍历子分词器
                            foreach (var segmenter in segmenters)
                            {
                                segmenter.Analyze(context);
                            }
                            //字符缓冲区接近读完，需要读入新的字符
                            if (context.NeedRefillBuffer())
                            {
                                break;
                            }
                            //向前移动指针
                        } while (context.MoveCursor());
                        //重置子分词器
                        foreach (var segmenter in segmenters)
                        {
                            segmenter.Reset();
                        }
                    }
                    //对分词进行歧义处理
                    arbitrator.Process(context, cfg.UseSmart);
                    //将分词结果输出到结果集，并处理未切分的单个CJK字符
                    context.OutputToResult();
                    //记录本次分词的缓冲区位移
                    context.MarkBufferOffset();
                }
                return l;
            }
        }
        /// <summary>
        /// 重置分词器到初始状态
        /// </summary>
        /// <param name="input"></param>
        public void Reset(StringReader input)
        {
            lock (objLock)
            {
                this.input = input;
                context.Reset();
                foreach (var segmenter in segmenters)
                {
                    segmenter.Reset();
                }
            }
        }
    }
}
