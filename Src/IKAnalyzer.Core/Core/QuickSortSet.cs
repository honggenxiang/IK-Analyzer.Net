using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKAnalyzer.Core
{
    /// <summary>
    /// IK分词器专用的Lexeme快速排序集合
    /// </summary>
    public class QuickSortSet
    {
        /// <summary>
        /// 链表头
        /// </summary>
        public Cell Head { get; private set; }
        /// <summary>
        /// 链表尾
        /// </summary>
        private Cell tail;
        /// <summary>
        /// 链表的实际大小
        /// </summary>
        public int Size { get; private set; }
        public QuickSortSet()
        {
            Size = 0;
        }
        /// <summary>
        /// 向链表集合添加词元
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public bool AddLexeme(Lexeme lexeme)
        {
            Cell newCell = new Cell(lexeme);
            if (Size == 0)
            {
                Head = newCell;
                tail = newCell;
                Size++;
                return true;
            }
            else
            {
                if (tail.CompareTo(newCell) == 0)//词元与尾部词元相同，不放入集合
                {
                    return false;
                }
                else if (tail.CompareTo(newCell) < 0)//词元插入链表尾部
                {//词元接入链表尾部
                    tail.Next = newCell;
                    newCell.Pre = tail;
                    tail = newCell;
                    Size++;
                    return true;
                }
                else if (Head.CompareTo(newCell) > 0)//词元接入链表头部
                {
                    Head.Pre = newCell;
                    newCell.Next = Head;
                    Head = newCell;
                    Size++;
                    return true;
                }
                else
                {//从尾部上溯
                    Cell index = tail;
                    while (index != null && index.CompareTo(newCell) > 0)
                    {
                        index = index.Pre;
                    }
                    if (index.CompareTo(newCell) == 0)//词元与集合中的词元重复，不放入集合
                    {
                        return false;
                    }
                    else if (index.CompareTo(newCell) < 0)//词元插入链表中的某个位置
                    {
                        newCell.Pre = index;
                        newCell.Next = index.Next;
                        index.Next.Pre = newCell;
                        index.Next = newCell;
                        Size++;
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 返回链表头部元素
        /// </summary>
        /// <returns></returns>
        public Lexeme PeekFirst()
        {
            if (Head != null) return Head.Lexeme;
            return null;
        }
        /// <summary>
        /// 返回链表尾部元素
        /// </summary>
        /// <returns></returns>
        public Lexeme PeekLast()
        {
            if (tail != null) return tail.Lexeme;
            return null;
        }
        /// <summary>
        /// 去除链表集合的最后一个元素
        /// </summary>
        /// <returns></returns>
        public Lexeme PollLast()
        {
            if (Size == 1)
            {
                Lexeme last = Head.Lexeme;
                Head = null;
                tail = null;
                Size--;
                return last;
            }
            else if (Size > 1)
            {
                Lexeme last = tail.Lexeme;
                tail = tail.Pre;
                Size--;
                return last;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 取出链表集合的第一个元素
        /// </summary>
        /// <returns></returns>
        public Lexeme PollFirst()
        {
            if (Size == 1)
            {
                Lexeme first = Head.Lexeme;
                Head = null;
                tail = null;
                Size--;
                return first;
            }
            else if (Size > 1)
            {
                Lexeme first = Head.Lexeme;
                Head = Head.Next;
                Size--;
                return first;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 判断集合是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Size == 0;
        }

    }

    /// <summary>
    /// QuickSortSet集合单元
    /// </summary>
    public class Cell : IComparable<Cell>
    {
        public Cell Pre { get; set; }

        public Cell Next { get; set; }

        public Lexeme Lexeme { get; private set; }

        public Cell(Lexeme lexeme)
        {
            if (lexeme == null)
            {
                throw new ArgumentNullException("lexeme不能为空!!!");
            }
            Lexeme = lexeme;
        }

        public int CompareTo(Cell other)
        {
            return Lexeme.CompareTo(other.Lexeme);
        }
    }
}
