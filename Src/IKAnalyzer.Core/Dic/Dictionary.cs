using IKAnalyzer.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IKAnalyzer.Dic
{
    /// <summary>
    /// 词典管理,单例模式
    /// </summary>
    public class Dictionary
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object objLock = new object();
        /// <summary>
        /// 词典单例实例
        /// </summary>
        private static Dictionary singleton;
        /// <summary>
        /// 主词典对象
        /// </summary>
        private DictSegment mainDict;
        /// <summary>
        /// 停止词词典
        /// </summary>
        private DictSegment stopWordDict;
        /// <summary>
        /// 量词
        /// </summary>
        private DictSegment quantifierDict;

        /// <summary>
        /// 配置对象
        /// </summary>
        private Configuration cfg;

        private Dictionary(Configuration cfg)
        {
            this.cfg = cfg;
            LoadMainDict();
            LoadStopWordDict();
            LoadQuantifierDict();
        }

        /// <summary>
        /// 词典初始化
        /// 由于IK Analyzer的词典采用Dictionary类的静态方法进行词典初始化
        /// 只有当Dictionary类被实际调用时，才会开始载入词典，
        /// 这将延长首次分词操作的时间
        /// 该方法提供了一个在应用加载阶段就初始化字典的手段
        /// </summary>
        /// <param name="cfg">配置</param>
        /// <returns>字典</returns>
        public static Dictionary Initial(Configuration cfg)
        {
            //双检锁
            if (singleton != null) return singleton;
            Dictionary dictionary = new Dictionary(cfg);
            Interlocked.CompareExchange(ref singleton, dictionary, null);
            return singleton;
        }
        /// <summary>
        /// 批量加载新词条
        /// </summary>
        /// <param name="words"></param>
        public void AddWords(List<string> words)
        {
            if (words != null)
            {
                foreach (var word in words)
                {
                    if (word != null)
                    {
                        //批量加载词条到主内存词典中
                        GetSingleton().mainDict.FillSegment(word.Trim().ToLower().ToCharArray());
                    }
                }
            }
        }

        /// <summary>
        /// 批量移除(频闭)词条
        /// </summary>
        /// <param name="words"></param>

        public void DisableWords(List<string> words)
        {
            if (words != null)
            {
                foreach (var word in words)
                {
                    if (word != null)
                    {
                        //批量屏蔽词条
                        GetSingleton().mainDict.DisableSegment(word.Trim().ToLower().ToCharArray());
                    }
                }
            }
        }
        /// <summary>
        /// 获取词典单例
        /// </summary>
        /// <returns></returns>
        public static Dictionary GetSingleton()
        {
            if (singleton == null)
            {
                throw new InvalidOperationException("词典尚未初始化，请先调用Initial方法");
            }
            return singleton;
        }
        /// <summary>
        /// 检索匹配主词典
        /// </summary>
        /// <param name="charArray"></param>
        /// <param name="begin"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Hit MatchInMainDict(char[] charArray)
        {
            return GetSingleton().mainDict.Match(charArray);
        }
        /// <summary>
        /// 检索匹配主词典
        /// </summary>
        /// <param name="charArray"></param>
        /// <param name="begin"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Hit MatchInMainDict(char[] charArray, int begin, int length)
        {
            return GetSingleton().mainDict.Match(charArray, begin, length);
        }

        /// <summary>
        /// 检索匹配量词词典
        /// </summary>
        /// <returns></returns>
        public Hit MatchInQuantifierDict(char[] charArray, int begin, int length)
        {
            return GetSingleton().quantifierDict.Match(charArray, begin, length);
        }

        /// <summary>
        /// 从匹配的Hit中取出DictSegment，继续向下匹配
        /// </summary>
        /// <returns></returns>
        public Hit MatchWithHit(char[] charArray, int currentIndex, Hit matchedHit)

        {
            DictSegment ds = matchedHit.MatchedDictSegment;
            return ds.Match(charArray, currentIndex, 1, matchedHit);
        }
        /// <summary>
        /// 判断是否是停止词
        /// </summary>
        /// <param name="charArray"></param>
        /// <param name="begin"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool IsStopWord(char[] charArray, int begin, int length)
        {
            return GetSingleton().stopWordDict.Match(charArray, begin, length).IsMatch();
        }

        /// <summary>
        /// 加载主词典及扩展词典
        /// </summary>
        private void LoadMainDict()
        {
            //建立一个主词典实例
            mainDict = new DictSegment((char)0);
            //读取量词词典文件
            if (!File.Exists(cfg.MainDictionary))
            {
                throw new InvalidOperationException("未发现主词库词典!!!");
            }
            string[] theWord = File.ReadAllLines(cfg.MainDictionary, Encoding.UTF8);
            foreach (var word in theWord)
            {
                if (IsValidWord(word))
                {
                    mainDict.FillSegment(word.Trim().ToLower().ToCharArray());
                }
            }

            //加载扩展词典
            LoadExtDict();
        }
        /// <summary>
        /// 加载用户配置的扩展词典到主词库
        /// </summary>
        public void LoadExtDict()
        {
            //加载扩展词典位置
            List<string> extDictFiles = cfg.ExtDictionarys;
            if (extDictFiles != null)
            {
                foreach (var extDictFile in extDictFiles)
                {
                    Debug.Print("加载扩展词典:" + extDictFile);
                    //读取量词词典文件
                    if (File.Exists(extDictFile))
                    {
                        string[] theWord = File.ReadAllLines(extDictFile, Encoding.UTF8);
                        foreach (var word in theWord)
                        {
                            if (IsValidWord(word))
                            {
                                mainDict.FillSegment(word.Trim().ToLower().ToCharArray());
                            }
                        }
                    }
                    else
                    {
                        Debug.Print("扩展词典不存在:" + extDictFile);
                    }
                }
            }
            else
            {
                Debug.Print("不存在扩展词典");
            }
        }
        /// <summary>
        /// 加载用户扩展的停止词词典
        /// </summary>
        public void LoadStopWordDict()
        {

            //建立一个停用词词典实例
            stopWordDict = new Dic.DictSegment((char)0);
            List<string> extStopWordFiles = cfg.ExtDictionarys;
            if (extStopWordFiles != null)
            {
                foreach (var extStopWordFile in extStopWordFiles)
                {
                    if (File.Exists(extStopWordFile))
                    {
                        Debug.Print("加载扩展停用词词典:" + extStopWordFile);
                        string[] theWord = File.ReadAllLines(extStopWordFile, Encoding.UTF8);
                        foreach (var word in theWord)
                        {
                            if (IsValidWord(word))
                            {
                                stopWordDict.FillSegment(word.Trim().ToLower().ToCharArray());
                            }
                        }
                    }
                    else
                    {
                        Debug.Print("扩展停用词词典未找到:" + extStopWordFile);
                    }
                }
            }
            else
            {
                Debug.Print("不存在扩展停用词词典");
            }
        }
        /// <summary>
        /// 加载量词词典
        /// </summary>
        public void LoadQuantifierDict()
        {
            //建立一个量词词典实例
            quantifierDict = new DictSegment((char)0);
            //读取量词词典文件
            if (!File.Exists(cfg.QuantifierDictionary))
            {
                throw new InvalidOperationException("未发现量词词典!!!");
            }
            string[] theWord = File.ReadAllLines(cfg.QuantifierDictionary, Encoding.UTF8);
            foreach (var word in theWord)
            {
                if (IsValidWord(word))
                {
                    quantifierDict.FillSegment(word.Trim().ToLower().ToCharArray());
                }
            }
        }
        /// <summary>
        /// 是否有效的关键字
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool IsValidWord(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                string realWord = word.Trim();
                //#，/ 为词库行注释符号
                if (!string.IsNullOrEmpty(realWord) && !realWord.StartsWith("#") && !realWord.StartsWith("/"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
