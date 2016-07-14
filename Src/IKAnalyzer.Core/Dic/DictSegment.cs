using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IKAnalyzer.Dic
{
    /// <summary>
    /// 词典树分段，表示词典树的一个分支
    /// </summary>
    public class DictSegment : IComparable<DictSegment>
    {
        #region 类型属性
        /// <summary>
        /// 公共字典表，存储汉字
        /// </summary>
        private static Dictionary<char, char> charDict = new Dictionary<char, char>();

        /// <summary>
        /// 数组大小上限
        /// </summary>
        private static readonly int ARRAY_LENGTH_LIMIT = 3;
        #endregion

        #region 实例属性
        /// <summary>
        /// 同步锁
        /// </summary>
        private readonly object objLock = new object();


        private Dictionary<char, DictSegment> childrenDict;
        /// <summary>
        /// 字典存储结构
        /// </summary>
        public Dictionary<char, DictSegment> ChildrenDict
        {
            get
            {
                if (childrenDict != null)
                    return childrenDict;
                Dictionary<char, DictSegment> dict = new Dictionary<char, DictSegment>();
                Interlocked.CompareExchange(ref childrenDict, dict, null);
                return childrenDict;
            }
        }

        private DictSegment[] childrenArray;
        /// <summary>
        /// 数组方式存储结构
        /// </summary>
        public DictSegment[] ChildrenArray
        {
            get
            {
                if (childrenArray != null)
                    return childrenArray;
                DictSegment[] arr = new DictSegment[ARRAY_LENGTH_LIMIT];
                Interlocked.CompareExchange(ref childrenArray, arr, null);
                return childrenArray;
            }
        }
        /// <summary>
        /// 当前节点上存储的字符
        /// </summary>
        public char NodeChar { get; private set; }



        /// <summary>
        /// 当前节点存储的Segment数目
        /// </summary>
        private int storeSize = 0;
        /// <summary>
        /// 当前DictSegment状态，默认0,1表示从根节点到当前节点的路径表示一个词
        /// </summary>
        private int nodeState = 0;
        #endregion

        public DictSegment(char nodeChar)
        {
            this.NodeChar = nodeChar;
        }

        /// <summary>
        /// 判断是否有下一个节点
        /// </summary>
        /// <returns></returns>
        public bool HasNextNode()
        {
            return storeSize > 0;
        }
        /// <summary>
        /// 加载填充词典片段
        /// </summary>
        public void FillSegment(char[] charArray)
        {
            FillSegment(charArray, 0, charArray.Length, 1);
        }
        /// <summary>
        /// 屏蔽词典中的一个词
        /// </summary>
        public void DisableSegment(char[] charArray)
        {
            FillSegment(charArray, 0, charArray.Length, 0);
        }
        /// <summary>
        /// 加载填充词典片段
        /// </summary>
        private void FillSegment(char[] charArray, int begin, int length, int enabled)
        {
            lock (objLock)
            {
                //获取字典表中的汉子对象
                char beginChar = charArray[begin];
                char keyChar;
                if (!charDict.TryGetValue(beginChar, out keyChar))
                {
                    charDict.Add(beginChar, beginChar);
                    keyChar = beginChar;
                }
                //搜索当前节点的存储，查询对应keychar的keychar，如果没有则创建
                DictSegment ds = LookforSegment(keyChar, enabled);
                if (ds != null)
                {
                    //处理keyChar对应的segment
                    if (length > 1)
                    {
                        //次元还没有完全加入词典数
                        ds.FillSegment(charArray, begin + 1, length - 1, enabled);
                    }
                    else if (length == 1)
                    {
                        //已经是次元的最后一个char，设置的当前节点状态为enabled
                        //enabled=1表明一个完整的词，enabled=0表示从词典中屏蔽当前词
                        ds.nodeState = enabled;
                    }
                }
            }
        }
        /// <summary>
        /// 查找本节点下对应的KeyChar的Segmetn
        /// </summary>
        /// <param name="keyChar"></param>
        /// <param name="create">=1如果没找到，则创建新的Segment；=0如果没找到，不创建，返回null</param>
        /// <returns></returns>
        private DictSegment LookforSegment(char keyChar, int create)
        {
            DictSegment ds = null;
            if (storeSize <= ARRAY_LENGTH_LIMIT)
            {//获取数组容器，如果数组未创建则创建数组
                DictSegment[] segmentArray = ChildrenArray;
                //搜寻数组
                DictSegment keySegment = new DictSegment(keyChar);
                int position = Array.BinarySearch(segmentArray, 0, this.storeSize, keySegment);
                if (position >= 0)
                {
                    ds = segmentArray[position];
                }
                //遍历数组后没有找到对应的segment
                if (ds == null && create == 1)
                {
                    ds = keySegment;
                    if (this.storeSize < ARRAY_LENGTH_LIMIT)
                    {
                        //数组容器未满，使用数组存储
                        segmentArray[storeSize] = ds;
                        //segment数目+1
                        this.storeSize++;
                        Array.Sort(segmentArray, 0, this.storeSize);
                    }
                    else
                    {   //数组容器已满，切换字典存储
                        //获取字典容易，如果字典未创建，则创建字典
                        Dictionary<char, DictSegment> segmentDict = ChildrenDict;
                        //将数组中的segment迁移到dict中
                        Migrate(childrenArray, segmentDict);
                        //存储新的segment
                        segmentDict.Add(keyChar, ds);
                        //segment数据+1，必须在释放数组前执行storeSize++,确保极端情况下，不会取到空的数组
                        storeSize++;
                        //释放当前的数组引用
                        childrenArray = null;
                    }
                }

            }
            else
            {
                //获取dict容器，如果dict未创建，则创建dict
                Dictionary<char, DictSegment> segmentDict = ChildrenDict;
                //搜索Dict
                if (!segmentDict.TryGetValue(keyChar, out ds) && create == 1)
                {//构造新的segment
                    ds = new DictSegment(keyChar);
                    segmentDict.Add(keyChar, ds);
                    //当前节点存储segment数目+1
                    storeSize++;
                }
            }
            return ds;
        }
        /// <summary>
        /// 将数组中的segment迁移到Dict中
        /// </summary>
        /// <param name="segmentArray"></param>
        /// <param name="segmentDict"></param>
        private void Migrate(DictSegment[] segmentArray, Dictionary<char, DictSegment> segmentDict)
        {
            foreach (var segment in segmentArray)
            {
                if (segment != null)
                {
                    segmentDict.Add(segment.NodeChar, segment);
                }
            }
        }
        /// <summary>
        /// 实现IComparable
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DictSegment other)
        {
            return NodeChar.CompareTo(other.NodeChar);
        }
    }
}
