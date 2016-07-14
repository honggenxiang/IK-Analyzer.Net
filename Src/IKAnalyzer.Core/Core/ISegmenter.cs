using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 子分词器接口
    /// </summary>
    /// <remarks>
    /// author:hgx
    /// 
    /// </remarks>
    public interface ISegmenter
    {
        void Analyze(AnalyzerContext context);
        /// <summary>
        /// 重置子分析器状态
        /// </summary>
        void Reset();
    }
}
