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
        /// ���ȼ��ȽϷ��������ڶԸ����͵ĳ�Ա�Ƚ�
        /// </summary>
        /// <param name="minCapacity">��С����,��ʼ��ȷ������С�ڸ�����</param>
        /// <param name="priorityComparer">���ȼ��ȽϷ���</param>
        public PriorityQueue(int minCapacity, Func<T, T, int> priorityComparer)
        {
            if (minCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(minCapacity), "Capacity must be nonnegative");

            this.minCapacity = minCapacity;
            items = new T[minCapacity];
            comparer = priorityComparer;
        }
        /// <summary>
        /// ���ȼ��ȽϷ��������ڶԸ����͵ĳ�Ա�Ƚ�
        /// </summary>
        /// <param name="priorityComparer">���ȼ��ȽϷ���</param>
        public PriorityQueue(Func<T, T, int> priorityComparer)
            : this(0, priorityComparer) { }
        /// <summary>
        /// ʹ��Ĭ�ϱȽϷ���(��ʵ��IComparable)�Ƚ����ȼ�
        /// </summary>
        /// <param name="minCapacity">��С����,��ʼ��ȷ������С�ڸ�����</param>
        public PriorityQueue(int minCapacity)
            : this(minCapacity, Comparer<T>.Default.Compare) { }
        /// <summary>
        /// ʹ��Ĭ�ϱȽϷ���(��ʵ��IComparable)�Ƚ����ȼ�
        /// </summary>
        public PriorityQueue()
            : this(0, Comparer<T>.Default.Compare) { }


        public void Enqueue(T item)
        {
            if (Count == items.Length)//����
            {
                int newCapacity = (items.Length == 0) ? 4 : items.Length * 2;
                T[] newArray = new T[newCapacity];
                Array.Copy(items, 0, newArray, 0, items.Length);
                items = newArray;
            }

            //���ϵ���
            items[Count++] = item;
            int curIndex = Count - 1;

            while (curIndex > 0)
            {
                int parent = (curIndex - 1) / 2;
                if (comparer(item, items[parent]) > 0)
                    items[curIndex] = items[parent];//������ҪSwap,�ҵ�λ�ú��ٸ�ֵ
                else break;
                curIndex = parent;
            }

            items[curIndex] = item;
        }

        public T Dequeue()
        {
            //���µ���
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
            if (Count < items.Length/4 && items.Length >= minCapacity*2)//����
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
        /// ���ҵ�һ�������������������,��δ�ҵ��򷵻ص�һ�����������
        /// <para>��ȷ����ʱ���ȶ���������һ������</para>
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
