using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Config
{
    /// <summary>
    /// ���ù�����ӿ�
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface Configuration
    {
        /// <summary>
        /// UseSmart��־λ
        /// UseSmart=true���ִ���ʹ�������зֲ���,=false��ʹ��ϸ�����з�
        /// </summary>
        bool UseSmart { get; set; }
        /// <summary>
        /// ��ȡ���ʵ�·��
        /// </summary>
        string MainDictionary { get; }
        /// <summary>
        /// ��ȡ���ʴʵ�·��
        /// </summary>
        string QuantifierDictionary { get; }
        /// <summary>
        /// ��ȡ��չ�ֵ�����·��
        /// </summary>
        List<string> ExtDictionarys { get; }
        /// <summary>
        /// ��ȡ��չֹͣ�ʵ�����·��
        /// </summary>
        List<string> ExtStopWordDictionarys { get; }
    }
}
