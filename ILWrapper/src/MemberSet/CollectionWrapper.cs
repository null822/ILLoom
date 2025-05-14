using System.Collections;
using Mono.Collections.Generic;

namespace ILWrapper.MemberSet;

public class CollectionWrapper<T> : IList<T>
{
    private readonly Collection<T> _collection;
    public int Count => _collection.Count;
    public bool IsReadOnly => false;

    public CollectionWrapper(Collection<T> collection)
    {
        _collection = collection;
    }
    
    public T this[int index]
    {
        get => _collection[index];
        set => _collection[index] = value;
    }
    
    public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collection).GetEnumerator();
    public void Add(T item) => _collection.Add(item);
    public void Clear() => _collection.Clear();
    public bool Contains(T item) => _collection.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
    public bool Remove(T item) => _collection.Remove(item);
    public int IndexOf(T item) => _collection.IndexOf(item);
    public void Insert(int index, T item) => _collection.Insert(index, item);
    public void RemoveAt(int index) => _collection.RemoveAt(index);
}