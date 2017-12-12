using Xunit;

namespace IKAnalyzer.Core.Test
{
    public class DefaultConfig
    {
        [Fact]
        public void Constrator()
        {
            var cfg = IKAnalyzer.Config.DefaultConfig.GetInstance();
            Assert.NotNull(cfg);
            Assert.NotNull(cfg.ExtDictionarys);
            Assert.NotNull(cfg.ExtStopWordDictionarys);
            Assert.NotNull(cfg.MainDictionary);
            Assert.NotNull(cfg.QuantifierDictionary);
        }
    }
}
