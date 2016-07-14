using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Dic
{
    /// <summary>
    /// 表示一次词典匹配的命中
    /// </summary>
    public class Hit
    {
        #region 属性
        /// <summary>
        /// 改HIT当前状态，默认未匹配
        /// </summary>
        private HitType hitState = HitType.MATCH;
        /// <summary>
        /// 记录词典匹配过程中，当前匹配到的词典分支节点
        /// </summary>
        public DictSegment MatchedDictSegment { get; set; }
        /// <summary>
        /// 词端的开始位置
        /// </summary>
        public int begin { get; set; }
        /// <summary>
        /// 词段的结束位置
        /// </summary>
        public int end { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 判断是否完全匹配
        /// </summary>
        /// <returns></returns>
        public bool IsMatch()
        {
            return (hitState & HitType.MATCH) > 0;
        }
        /// <summary>
        /// 设置完全匹配
        /// </summary>
        public void SetMatch()
        {
            hitState |= HitType.MATCH;
        }
        /// <summary>
        /// 判断是否是词的前缀
        /// </summary>
        /// <returns></returns>
        public bool IsPrefix()
        {
            return (hitState & HitType.PREFIX) > 0;
        }
        /// <summary>
        /// 设置前缀匹配
        /// </summary>
        public void SetPrefix()
        {
            hitState |= HitType.PREFIX;
        }
        /// <summary>
        /// 判断是否不匹配
        /// </summary>
        /// <returns></returns>
        public bool IsUnMatch()
        {
            return hitState == HitType.UNMATCH;
        }
        /// <summary>
        /// 设置不匹配
        /// </summary>
        public void SetUnMatch()
        {
            hitState = HitType.UNMATCH;
        }
        #endregion
    }
    /// <summary>
    /// 匹配类型
    /// </summary>
    public enum HitType
    {/// <summary>
     /// Hit不匹配
     /// </summary>
        UNMATCH = 0x00000000,
        /// <summary>
        /// Hit匹配
        /// </summary>
        MATCH = 0x00000001,
        /// <summary>
        /// Hit前缀匹配
        /// </summary>
        PREFIX = 0x00000002
    }
}
