using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core.Test
{
    public struct Person
    {
        public int Age { get; set; }

        public string[] _temp;
        public string[] Temp
        {
            get
            {
                if (_temp == null) _temp = new[] { "你好", "你妹啊" };
                return _temp;
            }
        }

        public string _name;
        public string Name
        {
            get
            {
                if (_name == null) _name = "坑爹";
                return _name;
            }
        }
    }
}
