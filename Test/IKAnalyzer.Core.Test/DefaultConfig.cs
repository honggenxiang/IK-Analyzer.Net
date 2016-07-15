using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class DefaultConfig
    {
        [TestMethod]
        public void Constrator()
        {
            var cfg = IKAnalyzer.Config.DefaultConfig.GetInstance();
            Assert.IsNotNull(cfg);
            Assert.IsNotNull(cfg.ExtDictionarys);
            Assert.IsNotNull(cfg.ExtStopWordDictionarys);
            Assert.IsNotNull(cfg.MainDictionary);
            Assert.IsNotNull(cfg.QuantifierDictionary);
        }
    }
}
