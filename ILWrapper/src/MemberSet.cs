using System.Collections;
using System.Text;
using Mono.Collections.Generic;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper;

public delegate TTo TypeConvert<in TFrom, out TTo>(TFrom baseInstance);

public interface IMemberSet<T> : IList<T>
{
    public void CloneTo(IMemberSet<T> other, ParentInfo info);
    public void ReplaceContents(IMemberSet<T> replacement, ParentInfo info);

    public bool Matches(IMemberSet<T> other);
    
    public string ToString(Func<T, string> itemToString);
}

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

public class CollectionConvert<T, TFrom> : IList<T>
{
    private readonly IList<TFrom> _collection;
    public int Count => _collection.Count;
    public bool IsReadOnly => false;
    
    private readonly TypeConvert<TFrom, T> _fromBase;
    private readonly TypeConvert<T, TFrom> _toBase;
    
    public CollectionConvert(IList<TFrom> collection, TypeConvert<TFrom, T> fromBase, TypeConvert<T, TFrom> toBase)
    {
        _collection = collection;
        _fromBase = fromBase;
        _toBase = toBase;
    }
    
    public static CollectionConvert<TChild, TParent> Of<TChild, TParent>(IList<TParent> collection) where TParent : TChild
    {
        return new CollectionConvert<TChild, TParent>(
            collection,
            instance => instance,
            instance => (TParent)instance!);
    }
    
    
    
    public T this[int index]
    {
        get => _fromBase(_collection[index]);
        set => _collection[index] = _toBase(value);
    }
    
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collection).GetEnumerator();
    public void Add(T item) => _collection.Add(_toBase(item));
    public void Clear() => _collection.Clear();
    public bool Contains(T item) => _collection.Contains(_toBase(item));
    public bool Remove(T item) => _collection.Remove(_toBase(item));
    public int IndexOf(T item) => _collection.IndexOf(_toBase(item));
    public void Insert(int index, T item) => _collection.Insert(index, _toBase(item));
    public void RemoveAt(int index) => _collection.RemoveAt(index);
    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        
        if (arrayIndex + _collection.Count > array.Length)
            throw new ArgumentException("The number of elements in this MemberSet is greater than the available space in the array");
        
        for (var i = arrayIndex; i < _collection.Count; i++)
        {
            array[i] = _fromBase(_collection[i]);
        }
    }

    private struct Enumerator(CollectionConvert<T, TFrom> memberSet) : IEnumerator<T>
    {
        private CollectionConvert<T, TFrom>? _memberSet = memberSet;
        
        private int _index = 0;
        public T Current { get; private set; } = default!;
        object? IEnumerator.Current => Current;
        
        public bool MoveNext()
        {
            if (_index < 0)
                return false;
            if (_memberSet == null) throw new ObjectDisposedException("Cannot use disposed Enumerator");
            
            if (_index < _memberSet.Count) {
                Current = _memberSet[_index++];
                return true;
            }
            
            _index = -1;
            return false;
        }

        public void Reset()
        {
            _index = 0;
        }
        
        public void Dispose()
        {
            _memberSet = null;
        }
    }
}

public class MemberSet<T, TBase> : IMemberSet<T> where T : IMember<T, TBase>
{
    private readonly CollectionConvert<T, TBase> _collection;
    
    public int Count => _collection.Count;
    public bool IsReadOnly => false;
    
    public MemberSet(Collection<TBase> collection)
    {
        _collection = new CollectionConvert<T, TBase>(collection, T.FromBase, IMember<T, TBase>.ToBase);
    }
    public MemberSet(CollectionConvert<T, TBase> collection)
    {
        _collection = collection;
    }
    
    public static MemberSet<T, TBase> From<TFrom>(Collection<TFrom> collection) where TFrom : TBase
    {
        var toBase = new CollectionConvert<TBase, TFrom>(collection,
            instance => instance,
            instance => (TFrom?)instance ?? throw new InvalidOperationException($"Could not convert {instance} into {typeof(TFrom)}"));
        
        var toT = new CollectionConvert<T, TBase>(toBase,
            instance => T.FromBase(instance),
            instance => IMember<T, TBase>.ToBase(instance)
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

public static class Tmp
{
    public static int Indent;
}
