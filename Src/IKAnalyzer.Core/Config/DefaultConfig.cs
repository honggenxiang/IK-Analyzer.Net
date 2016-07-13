using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetEscapades.Configuration.Yaml;
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
        private const string FILE_NAME = "IKAnalyzer.cfg..yml";
        /// <summary>
        /// 配置属性--扩展字典
        /// </summary>
        private const string EXT_DICT = "ext.dict.dic";
        /// <summary>
        /// 配置属性--扩展停止字典
        /// </summary>
        private const string EXT_STOP = "ext_Stopwords";

        private string base_path = AppDomain.CurrentDomain.BaseDirectory;

        private DefaultConfig()
        {
            string ymlPath = Path.Combine(base_path, FILE_NAME);
            if (File.Exists(ymlPath))
            {
                var p = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true, Path = ymlPath });
                p.Load();
            }
        }

        public string ExtDictionarys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ExtStopWordDictionarys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MainDictionary
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QuantifierDictionary
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool UseSmart
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
