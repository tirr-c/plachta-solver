using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SophieSolver
{
    class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> list = new List<T>();
        private int waitingThreads = 0;
        public int Count => list.Count;
        public int MaxWaitingThreads { get; set; }

        public PriorityQueue(int maxWaitingThreads)
        {
            MaxWaitingThreads = maxWaitingThreads;
        }

        public void Enqueue(T item)
        {
            lock (list) {
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
        }

        public T Dequeue()
        {
            if (list.Count <= 0) throw new InvalidOperationException("No elements to dequeue");
            T ret;
            lock (list)
            {
                ret = list[0];
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
            }
            return ret;
        }

        public async Task<T> BlockingDequeueAsync()
        {
            bool waiting = false;
            while (true)
            {
                try
                {
                    if (waitingThreads >= MaxWaitingThreads)
                        throw new QueueExhaustedException();
                    T ret = Dequeue();
                    if (waiting) System.Threading.Interlocked.Decrement(ref waitingThreads);
                    return ret;
                }
                catch (InvalidOperationException)
                {
                    waiting = true;
                    System.Threading.Interlocked.Increment(ref waitingThreads);
                    await Task.Yield();
                }
            }
        }
    }

    [Serializable]
    public class QueueExhaustedException : Exception
    {
        public QueueExhaustedException() { }
        public QueueExhaustedException(string message) : base(message) { }
        public QueueExhaustedException(string message, Exception inner) : base(message, inner) { }
        protected QueueExhaustedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
