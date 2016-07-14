using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Config
{
    /// <summary>
    /// 配置管理类接口
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface Configuration
    {
        /// <summary>
        /// UseSmart标志位
        /// UseSmart=true，分词器使用智能切分策略,=false则使用细粒度切分
        /// </summary>
        bool UseSmart { get; set; }
        /// <summary>
        /// 获取主词典路径
        /// </summary>
        string MainDictionary { get; }
        /// <summary>
        /// 获取量词词典路径
        /// </summary>
        string QuantifierDictionary { get; }
        /// <summary>
        /// 获取扩展字典配置路径
        /// </summary>
        string ExtDictionarys { get; }
        /// <summary>
        /// 获取扩展停止词典配置路径
        /// </summary>
        string ExtStopWordDictionarys { get; }
    }
}
