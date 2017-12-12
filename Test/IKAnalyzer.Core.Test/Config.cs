using NetEscapades.Configuration.Yaml;
using Xunit;

namespace IKAnalyzer.Core.Test
{
    public class Config
    {
        [Fact]
        public void DefaultConfig()
        {
            bool result = string.IsNullOrEmpty("     ");
            result = "     ".Trim() == string.Empty;
            var p = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true, Path = "" });
            p.Load();
        }
       
    }
}
