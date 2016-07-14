using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 字符类型
    /// </summary>
    /// <remarks>
    /// author:hgx
    /// desctription:
    /// createtime:20160713
    /// </remarks>
    [Flags]
    public enum CharType
    {
        /// <summary>
        /// 未识别字符
        /// </summary>
        CHAR_USELESS = 0x00000000,
        /// <summary>
        /// 阿拉伯数字
        /// </summary>
        CHAR_ARABIC = 0x00000001,
        /// <summary>
        /// 英文
        /// </summary>
        CHAR_ENGLISH = 0x00000002,
        /// <summary>
        /// 中文
        /// </summary>
        CHAR_CHINESE = 0x00000004,
        /// <summary>
        /// 其他日中韩字符
        /// </summary>
        CHAR_OTHER_CJK = 0x00000008
    }
}
