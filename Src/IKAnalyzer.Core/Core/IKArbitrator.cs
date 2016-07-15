using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// 歧义裁决器
    /// </summary>
    public class IKArbitrator
    {
        /// <summary>
        /// 分词歧义处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="useSmart"></param>
        public void Process(AnalyzerContext context, bool useSmart)
        {
            QuickSortSet orgLexemes = context.OrgLexemes;
            Lexeme orgLexeme = orgLexemes.PollFirst();

            LexemePath crossPath = new LexemePath();
            while (orgLexeme != null)
            {
                if (!crossPath.AddCrossLexeme(orgLexeme))
                {//找到与crossPath不相交的下一个crossPaht
                    if (crossPath.Size == 1 || !useSmart)
                    {
                        //croseePath没有歧义或者不做歧义处理
                        //直接输出当前crossPath
                        context.AddLexemePath(crossPath);
                    }
                    else
                    {
                        //对当前的crossPath进行歧义处理
                        Cell headCell = crossPath.Head;

                        LexemePath judgeResult = Judge(headCell, crossPath.PayloadLength);
                        //输出歧义处理结果 judgeResult
                        context.AddLexemePath(judgeResult);
                    }
                    //把orgLexeme加入新得crossPath中
                    crossPath = new LexemePath();
                    crossPath.AddCrossLexeme(orgLexeme);

                }
                orgLexeme = orgLexemes.PollFirst();
            }

            //处理最后的path
            if (crossPath.Size == 1 || !useSmart)
            {//crosssPath没有歧义 或者不做歧义处理
                //直接输出当前crossPath
                context.AddLexemePath(crossPath);
            }
            else
            {//对当前的crossPath进行歧义处理
                Cell headCell = crossPath.Head;
                LexemePath judgeReuslt = Judge(headCell, crossPath.GetPathLength());
                //输出歧义处理结果judgeResult
                context.AddLexemePath(judgeReuslt);
            }

        }

        private LexemePath Judge(Cell lexemeCell, int fullTextLength)
        {
            //候选路径集合
            SortedSet<LexemePath> pathOptions = new SortedSet<LexemePath>();
            //候选结果路径
            LexemePath option = new LexemePath();

            //对crossPath进行一次遍历，同时返回本次遍历中有冲突的Lexeme栈
            Stack<Cell> lexemeStack = ForwardPath(lexemeCell, option);

            //当前词元链并非最理想的，加入候选路径集合
            pathOptions.Add(option.Copy());

            //存在歧义词，处理
            Cell c = null;
            while (lexemeStack.Count > 0)
            {
                c = lexemeStack.Pop();
                //回滚词元链
                BackPath(c.Lexeme, option);
                //从歧义词位置开始，递归，生成可选方案
                ForwardPath(c, option);
                pathOptions.Add(option.Copy());
            }
            //返回集合中的最优方案
            return pathOptions.First();

        }

        /// <summary>
        /// 向前遍历，添加词元，构造一个无歧义词元组合
        /// </summary>
        /// <param name="lexemeCell"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private Stack<Cell> ForwardPath(Cell lexemeCell, LexemePath option)
        {
            Stack<Cell> conflictStack = new Stack<Cell>();
            Cell c = lexemeCell;
            //迭代遍历Lexeme链表
            while (c != null && c.Lexeme != null)
            {
                if (!option.AddNotCrossLexeme(c.Lexeme))
                {//词元交叉，添加失败则加入lexemeStack栈
                    conflictStack.Push(c);
                }
                c = c.Next;
            }
            return conflictStack;
        }
        /// <summary>
        /// 回滚词元链，知道它能够接受指定的词元
        /// </summary>
        private void BackPath(Lexeme l, LexemePath option)
        {
            while (option.CheckCross(l))
            {
                option.RemoveTail();
            }
        }
    }
}
