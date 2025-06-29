using System.Collections;
using System.Text;
using Mono.Collections.Generic;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.MemberSet;

public delegate TTo TypeConvert<in TFrom, out TTo>(TFrom baseInstance);

public class MemberSet<T, TBase> : IMemberSet<T> where T : IMember<T, TBase>
{
    public CollectionConvert<T, TBase> Base { get; private set; }
    
    public int Count => Base.Count;
    public bool IsReadOnly => false;
    
    public MemberSet(IList<TBase> collection)
    {
        Base = new CollectionConvert<T, TBase>(collection, T.FromBase, IMember<T, TBase>.ToBase);
    }
    public MemberSet(CollectionConvert<T, TBase> @base)
    {
        Base = @base;
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
        get => Base[index];
        set => Base[index] = value;
    }

    public void Add(T item) => Base.Add(item);
    public void CopyTo(T[] array, int arrayIndex) => Base.CopyTo(array, arrayIndex);
    public bool Remove(T item) => Base.Remove(item);
    public void RemoveAt(int index) => Base.RemoveAt(index);
    public int IndexOf(T item) => Base.IndexOf(item);
    public bool Contains(T item) => Base.Contains(item);
    public void Insert(int index, T item) => Base.Insert(index, item);
    public void Clear() => Base.Clear();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator() => Base.GetEnumerator();
    
    public void CloneTo(IList<T> target, ParentInfo info)
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
    
    public void ReplaceContents(IList<T> replacement)
    {
        Clear();
        foreach (var element in replacement)
        {
            Add(element);
        }
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
        foreach (var baseElement in Base)
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
        foreach (var baseElement in Base)
        {
            s.Append(itemToString.Invoke(baseElement));
            s.Append(", ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
}
