namespace IKAnalyzer.Core
{
    /// <summary>
    /// 字符集识别工具
    /// </summary>
    /// <remarks>
    /// <see cref="http://www.unicode.org/Public/5.2.0/ucd/Blocks.txt"/>
    /// </remarks>
    public class CharacterUtil
    {
        /// <summary>
        /// 识别字符类型
        /// </summary>
        /// <param name="input">传入字符</param>
        /// <returns>字符类型</returns>
        public static CharType IdentifyCharType(char input)
        {
            if (input >= '0' && input <= '9')
                return CharType.CHAR_ARABIC;
            else if ((input >= 'a' && input <= 'z') || (input >= 'A' && input <= 'Z'))
                return CharType.CHAR_ENGLISH;
            else
            {   //中文utf-8字符 
                if ((input >= 0x4E00 && input <= 0x9FFF) //4E00..9FFF; CJK Unified Ideographs
                || (input >= 0xF900 && input <= 0xFAFF)//F900..FAFF; CJK Compatibility Ideographs
                || (input >= 0x3400 && input <= 0x4DBF)//3400..4DBF; CJK Unified Ideographs Extension A

                 )
                {
                    return CharType.CHAR_CHINESE;
                }
                else if (
              //全角数字字符和日韩字符 FF00..FFEF; Halfwidth and Fullwidth Forms
              (input >= 0xFF00 && input <= 0xFFEF)
               //韩文字符集    
               || (input >= 0xAC00 && input <= 0xD7AF)//AC00..D7AF; Hangul Syllables
               || (input >= 0x11000 && input <= 0x11FF)//1100..11FF; Hangul Jamo
               || (input >= 0x3130 && input <= 0x318F) //3130..318F; Hangul Compatibility Jamo
                                                       //日文字符集 
               || (input >= 0x3040 && input <= 0x309F)//平假名  3040..309F; Hiragana
               || (input >= 0x30A0 && input <= 0x30FF)//片假名  30A0..30FF; Katakana  
               || (input >= 0x31F0 && input <= 0x31FF)//31F0..31FF; Katakana Phonetic Extensions)
               )
                {
                    return CharType.CHAR_OTHER_CJK;
                }

                return CharType.CHAR_USELESS;
            }

        }
        /// <summary>
        /// 进行字符规则化(全角转半角，大写转小写处理)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static char Regularize(char input)
        {
            //全角字符  阿拉伯数目字、英文字母、标点符号、特殊符号
            // 0xFF01~0xFF5E
            if (input == 0x3000)
            {
                input = (char)0x20;
            }
            else if (input >= 0xFF00 && input <= 0xFF5E)
            {
                input = (char)(input - 65248);
            }


            if (input >= 'A' && input <= 'Z')
            {
                input = (char)(input + 32);
            }
            return input;
        }
    }
}
