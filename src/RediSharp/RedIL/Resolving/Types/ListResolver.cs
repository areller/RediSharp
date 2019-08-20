using System.Collections;
using System.Collections.Generic;

namespace RediSharp.RedIL.Resolving.Types
{
    class ListProxy<T> : IList<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(T item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        public T this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
    
    class ListResolver<T> : TypeResolver<List<T>>
    {
        public ListResolver()
        {
            Proxy<ListProxy<T>>();
        }
    }

    class ListInterfaceResolver<T> : TypeResolver<IList<T>>
    {
        public ListInterfaceResolver()
        {
            Proxy<ListProxy<T>>();
        }
    }

    class CollectionInterfaceResolver<T> : TypeResolver<ICollection<T>>
    {
        public CollectionInterfaceResolver()
        {
            Proxy<ListProxy<T>>();
        }
    }
}