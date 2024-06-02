using System;
using System.Collections;
using System.Collections.Generic;
namespace Paltry.Collections
{
    internal class PriorityQueue<T> : IEnumerable<T>
    {
        public int Count { get; private set; }
        public int Capacity => items.Length;

        private T[] items;
        private Func<T, T, int> comparer;
        private readonly int minCapacity;
        /// <summary>
        /// 优先级比较方法可用于对该类型的成员比较
        /// </summary>
        /// <param name="minCapacity">最小容量,将始终确保不会小于该容量</param>
        /// <param name="priorityComparer">优先级比较方法</param>
        public PriorityQueue(int minCapacity, Func<T, T, int> priorityComparer)
        {
            if (minCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(minCapacity), "Capacity must be nonnegative");

            this.minCapacity = minCapacity;
            items = new T[minCapacity];
            comparer = priorityComparer;
        }
        /// <summary>
        /// 优先级比较方法可用于对该类型的成员比较
        /// </summary>
        /// <param name="priorityComparer">优先级比较方法</param>
        public PriorityQueue(Func<T, T, int> priorityComparer)
            : this(0, priorityComparer) { }
        /// <summary>
        /// 使用默认比较方法(需实现IComparable)比较优先级
        /// </summary>
        /// <param name="minCapacity">最小容量,将始终确保不会小于该容量</param>
        public PriorityQueue(int minCapacity)
            : this(minCapacity, Comparer<T>.Default.Compare) { }
        /// <summary>
        /// 使用默认比较方法(需实现IComparable)比较优先级
        /// </summary>
        public PriorityQueue()
            : this(0, Comparer<T>.Default.Compare) { }


        public void Enqueue(T item)
        {
            if (Count == items.Length)//扩容
            {
                int newCapacity = (items.Length == 0) ? 4 : items.Length * 2;
                T[] newArray = new T[newCapacity];
                Array.Copy(items, 0, newArray, 0, items.Length);
                items = newArray;
            }

            //向上调整
            items[Count++] = item;
            int curIndex = Count - 1;

            while (curIndex > 0)
            {
                int parent = (curIndex - 1) / 2;
                if (comparer(item, items[parent]) > 0)
                    items[curIndex] = items[parent];//并不需要Swap,找到位置后再赋值
                else break;
                curIndex = parent;
            }

            items[curIndex] = item;
        }

        public T Dequeue()
        {
            //向下调整
            T result = items[0];
            T item = items[Count - 1];
            int curIndex = 0;
            int left = curIndex * 2 + 1;
            while (left < Count)
            {
                int max;
                if (left + 1 >= Count || comparer(items[left], items[left + 1]) >= 0)
                    max = left;
                else
                    max = left + 1;

                if (comparer(items[max], item) > 0)
                    items[curIndex] = items[max];
                else break;

                curIndex = max;
                left = curIndex * 2 + 1;
            }

            items[curIndex] = item;
            Count--;
            if (Count < items.Length/4 && items.Length >= minCapacity*2)//缩容
            {
                T[] newArray = new T[items.Length / 2];
                Array.Copy(items, 0, newArray, 0, Count);
                items = newArray;
            }
            return result;
        }

        public bool TryDequeue(out T result)
        {
            if(Count == 0)
            {
                result = default(T);
                return false;
            }    
            result = Dequeue();
            return true;
        }
        public T Peek()
        {
            return items[0];
        }

        public void Clear()
        {
            items = new T[0];
            Count = 0;
        }

        public T Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
                if (predicate(items[i]))
                    return items[i];
            return default(T);
        }

        /// <summary>
        /// 查找第一个满足条件对象的引用,若未找到则返回第一个对象的引用
        /// <para>请确保此时优先队列至少有一个对象</para>
        /// </summary>
        public ref T FindRef(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
                if (predicate(items[i]))
                    return ref items[i];
            return ref items[0];
        }
        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < Count; i++)
                action(items[i]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return items[i];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
