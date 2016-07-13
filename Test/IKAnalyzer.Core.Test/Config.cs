using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class Config
    {
        [TestMethod]
        public void DefaultConfig()
        {
            var p = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true, Path = ymlPath });
            p.Load();
        }
    }
}
