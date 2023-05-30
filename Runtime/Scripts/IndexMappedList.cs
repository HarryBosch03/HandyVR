using System;
using System.Collections;
using System.Collections.Generic;

namespace HandyVR
{
    public class IndexMappedList<T>: System.Collections.Generic.ICollection<T>, System.Collections.Generic.IEnumerable<T>, System.Collections.IEnumerable
    {
        public List<T> list = new();

        public Func<int, int> mapIndex;

        public IndexMappedList()
        {
            mapIndex = Ring;
        }
        
        public int Ring(int i)
        {
            return (i % list.Count + list.Count) % list.Count;
        }

        private T this[int index]
        {
            get => list[mapIndex(index)];
            set => list[mapIndex(index)] = value;
        }

        public int IndexOf(T item) => list.IndexOf(item);
        public void Insert(int index, T item) => list.Insert(mapIndex(index), item);
        public void RemoveAt(int index) => list.RemoveAt(mapIndex(index));
        
        public int Count => list.Count;
        public bool IsReadOnly => throw null!;

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public void Add(T item) => list.Add(item);
        public void Clear() => list.Clear();
        public bool Contains(T item) => list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        public bool Remove(T item) => list.Remove(item);
    }
}