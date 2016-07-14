using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetEscapades.Configuration.Yaml;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class Config
    {
        [TestMethod]
        public void DefaultConfig()
        {
            bool result = string.IsNullOrEmpty("     ");
            result = "     ".Trim() == string.Empty;
            var p = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true, Path = "" });
            p.Load();
        }
        [TestMethod]
        public void Bound()
        {
            Temp temp = new Temp();

        }
    }
}
