using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void ModifyCache()
        {
            MemoryCache cache = MemoryCache.Default;
            
            cache["person"] = new List<Person> { new Person() { Age = 2, Name = "小李" }, new Test.Person() { Age = 3, Name = "小芳" } };
            List<Person> p0 = (List<Person>)cache["person"];
            p0[0].Name = "xiaoli";
            List<Person> p1 = (List<Person>)cache["person"];
        }
    }
}
