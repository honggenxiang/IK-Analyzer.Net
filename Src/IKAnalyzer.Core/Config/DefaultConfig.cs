using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetEscapades.Configuration.Yaml;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Config
{
    /// <summary>
    /// 默认配置
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class DefaultConfig : Configuration
    {
        /// <summary>
        /// 分词器默认字典路径
        /// </summary>
        private const string PATH_DIC_MAIN = "dic/main.dic";
        private const string PATH_DIC_QUANTIFIER = "dic/quantifier.dic";

        /// <summary>
        /// 分词器配置文件路径
        /// </summary>
        private const string FILE_NAME = "IKAnalyzer.cfg.yml";
        /// <summary>
        /// 配置属性--扩展字典
        /// </summary>
        private const string EXT_DICT = "ext.dict.dic";
        /// <summary>
        /// 配置属性--扩展停止字典
        /// </summary>
        private const string EXT_STOP = "ext_Stopwords";
        /// <summary>
        /// 基础路径
        /// </summary>

        private string base_path = AppDomain.CurrentDomain.BaseDirectory;

        private DefaultConfig()
        {
            //IK词库部分
            MainDictionary = Path.Combine(base_path, PATH_DIC_MAIN);
            QuantifierDictionary = Path.Combine(base_path, PATH_DIC_QUANTIFIER);
            //扩展词部分
            string ymlPath = Path.Combine(base_path, FILE_NAME);
            if (File.Exists(ymlPath))
            {
                var p = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true, Path = FILE_NAME, FileProvider = new PhysicalFileProvider(base_path) });
                p.Load();
                string ext_dict, ext_stopwords;
                if (!string.IsNullOrEmpty(ext_dict = p.Get("ext:ext_dict")))
                {
                    string[] extArray = ext_dict.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (extArray != null)
                    {
                        ExtDictionarys = (from c in extArray select Path.Combine(base_path, "dic/ext", c)).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(ext_stopwords = p.Get("ext:ext_stopwords")))
                {
                    string[] extStopArray = ext_stopwords.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (extStopArray != null)
                    {
                        ExtStopWordDictionarys = (from c in extStopArray select Path.Combine(base_path, "dic/ext", c)).ToList();
                    }
                }
            }
        }
        /// <summary>
        /// 返回配置实例
        /// </summary>
        /// <returns></returns>

        public static Configuration GetInstance()
        {
            return new DefaultConfig();
        }
        /// <summary>
        /// 主词库路径
        /// </summary>

        public string MainDictionary
        {
            get; private set;

        }
        /// <summary>
        /// 量词路径
        /// </summary>
        public string QuantifierDictionary
        {
            get; private set;
        }
        /// <summary>
        /// 是否智能分词
        /// </summary>
        public bool UseSmart
        {
            get; set;
        }
        /// <summary>
        /// 扩展词库
        /// </summary>
        public List<string> ExtDictionarys
        {
            get; private set;
        }
        /// <summary>
        /// 扩展停用词词库
        /// </summary>
        public List<string> ExtStopWordDictionarys
        {
            get; private set;
        }
    }
}
