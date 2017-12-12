namespace IKAnalyzer.Core
{
    /// <summary>
    /// 词元类型
    /// </summary>
    public enum LexemeType
    {
        /// <summary>
        /// 未知
        /// </summary>
        TYPE_UNKNOWN = 0,
       /// <summary>
       /// 英文
       /// </summary>
        TYPE_ENGLISH = 1,
        /// <summary>
        /// 阿拉伯数字
        /// </summary>
        TYPE_ARABIC = 2,
        /// <summary>
        /// 英文数字混合
        /// </summary>
        TYPE_LETTER = 3,
        /// <summary>
        /// 中文词元
        /// </summary>
        TYPE_CNWORD = 4,
        /// <summary>
        /// 中文单字
        /// </summary>
        TYPE_CNCHAR = 5,
        /// <summary>
        /// 日韩文字
        /// </summary>
        TYPE_OTHER_CJK = 6,
        /// <summary>
        /// 中文数字
        /// </summary>
        TYPE_CNUM = 7,
        /// <summary>
        /// 中文量词
        /// </summary>
        TYPE_COUNT = 8,
        /// <summary>
        /// 中文数量词
        /// </summary>
        TYPE_CQUAN = 9
    }
}
