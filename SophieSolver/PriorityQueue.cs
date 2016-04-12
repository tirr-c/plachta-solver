using System;
using System.Collections.Generic;

namespace SophieSolver
{
    class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> list;

        public PriorityQueue()
        {
            list = new List<T>();
        }

        public void Enqueue(T item)
        {
            list.Add(item);
            int current = list.Count - 1;
            int parent = (current - 1) / 2;
            while (parent >= 0 && list[parent].CompareTo(list[current]) < 0)
            {
                T tmp = list[parent];
                list[parent] = list[current];
                list[current] = tmp;

                current = parent;
                parent = (current - 1) / 2;
            }
        }

        public T Dequeue()
        {
            if (list.Count <= 0) throw new InvalidOperationException("No elements to dequeue");
            T ret = list[0];
            list[0] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            int current = 0;
            int leftChild = current * 2 + 1;
            while (leftChild < list.Count)
            {
                int idxMax = current;
                if (list[current].CompareTo(list[leftChild]) < 0) idxMax = leftChild;
                leftChild++;
                if (leftChild < list.Count && list[current].CompareTo(list[leftChild]) < 0) idxMax = leftChild;
                if (idxMax == current) break;

                T tmp = list[current];
                list[current] = list[idxMax];
                list[idxMax] = tmp;

                current = idxMax;
                leftChild = current * 2 + 1;
            }
            return ret;
        }
    }
}
