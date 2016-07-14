using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Collections.Generic;

namespace IKAnalyzer.Core.Test
{
    [TestClass]
    public class CharacterUtilTest
    {
        [TestMethod]
        public void FullWidth()
        {
            char begin = (char)65281;
            char end = (char)0xFF5E;
            string beginStr = begin.ToString();
        }
        [TestMethod]
        public void CharType()
        {
            char[] cnArr = "而毅線".ToCharArray();
            foreach (var cn in cnArr)
            {
                Assert.AreEqual(IKAnalyzer.Core.CharType.CHAR_CHINESE, CharacterUtil.IdentifyCharType(cn));
            }

            char[] arabicArr = "0123456789".ToCharArray();
            foreach (var arabic in arabicArr)
            {
                Assert.AreEqual(IKAnalyzer.Core.CharType.CHAR_ARABIC, CharacterUtil.IdentifyCharType(arabic));
            }

            char[] enArr = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            foreach (var en in enArr)
            {
                Assert.AreEqual(IKAnalyzer.Core.CharType.CHAR_ENGLISH, CharacterUtil.IdentifyCharType(en));
            }


            char[] enCJK = "ののをするサービスをま일본어번역및일본어사전다운로드서비스".ToCharArray();
            foreach (var cjk in enCJK)
            {
                Assert.AreEqual(IKAnalyzer.Core.CharType.CHAR_OTHER_CJK, CharacterUtil.IdentifyCharType(cjk));
            }

            char[] uselessArr = "แปลภาษาญี่ปุ่นและภาษาญี่ปุ่นП".ToCharArray();
            foreach (var useless in uselessArr)
            {
                Assert.AreEqual(IKAnalyzer.Core.CharType.CHAR_USELESS, CharacterUtil.IdentifyCharType(useless));
            }
        }
        [TestMethod]
        public void Full2Half()
        {
            string full = "１２３４５６７８９０ｑｗｅｒｔｙｕｉｏｐａｓｄｆｇｈｊｋｌｚｘｃｖｂｎｍＱＷＥＲＴＹＵＩＯＰＡＳＤＦＧＨＪＫＬＭＮＢＶＣＸＺ，．／；＇［］＼＝－（＊＆＾％＄＃＠～！　";
            string half = "1234567890qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklmnbvcxz,./;'[]\\=-(*&^%$#@~! ";
            for (int i=0;i<full.Length;i++)
            {
               char temp=IKAnalyzer.Core.CharacterUtil.Regularize(full[i]);
                char expect = half[i];
                Assert.AreEqual(expect, temp);
            }
         
        }
    }
}
