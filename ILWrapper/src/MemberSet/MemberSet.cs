using System.Collections;
using System.Text;
using Mono.Collections.Generic;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.MemberSet;

public delegate TTo TypeConvert<in TFrom, out TTo>(TFrom baseInstance);

public class MemberSet<T, TBase> : IMemberSet<T> where T : IMember<T, TBase>
{
    private readonly CollectionConvert<T, TBase> _collection;
    
    public int Count => _collection.Count;
    public bool IsReadOnly => false;
    
    public MemberSet(IList<TBase> collection)
    {
        _collection = new CollectionConvert<T, TBase>(collection, T.FromBase, IMember<T, TBase>.ToBase);
    }
    public MemberSet(CollectionConvert<T, TBase> collection)
    {
        _collection = collection;
    }
    
    public static MemberSet<T, TBase> From<TFrom>(Collection<TFrom> collection) where TFrom : class, TBase
    {
        var toBase = new CollectionConvert<TBase, TFrom>(collection,
            TBase (instance) => instance,
            instance => instance as TFrom ?? throw new InvalidOperationException($"Could not convert {instance} into {typeof(TFrom)}"));
        
        var toT = new CollectionConvert<T, TBase>(toBase,
            T.FromBase,
            IMember<T, TBase>.ToBase
        );
        
        return new MemberSet<T, TBase>(toT);
    }
    
    public T this[int index]
    {
        get => _collection[index];
        set => _collection[index] = value;
    }

    public void Add(T item) => _collection.Add(item);
    public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
    public bool Remove(T item) => _collection.Remove(item);
    public void RemoveAt(int index) => _collection.RemoveAt(index);
    public int IndexOf(T item) => _collection.IndexOf(item);
    public bool Contains(T item) => _collection.Contains(item);
    public void Insert(int index, T item) => _collection.Insert(index, item);
    public void Clear() => _collection.Clear();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
    
    public void CloneTo(IMemberSet<T> target, ParentInfo info)
    {
        foreach (var item in this)
        {
            if (item is Type { FullName: "<Module>" })
                continue;
            
            var clone = item.Clone(info);
            
            target.Add(clone);
        }
    }
    
    public void ReplaceContents(IMemberSet<T> replacement, ParentInfo info)
    {
        Clear();
        replacement.CloneTo(this, info);
    }
    
    public bool Matches(IMemberSet<T> other)
    {
        if (Count != other.Count)
            return false;

        for (var i = 0; i < Count; i++)
        {
            if (this[i].FullName != other[i].FullName)
                return false;
        }

        return true;
    }
    
    public override string ToString()
    {
        var s = new StringBuilder();
        foreach (var baseElement in _collection)
        {
            s.Append(baseElement);
            s.Append(", ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
    
    public string ToString(Func<T, string> itemToString)
    {
        var s = new StringBuilder();
        foreach (var baseElement in _collection)
        {
            s.Append(itemToString.Invoke(baseElement));
            s.Append(", ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
}
