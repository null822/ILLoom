using System.Collections;

namespace ILWrapper.MemberSet;

public class CollectionConvert<T, TBase> : IList<T>
{
    private readonly IList<TBase> _collection;
    public int Count => _collection.Count;
    public bool IsReadOnly => false;
    
    private readonly TypeConvert<TBase, T> _fromBase;
    private readonly TypeConvert<T, TBase> _toBase;
    
    public CollectionConvert(IList<TBase> collection, TypeConvert<TBase, T> fromBase, TypeConvert<T, TBase> toBase)
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

    private struct Enumerator(CollectionConvert<T, TBase> memberSet) : IEnumerator<T>
    {
        private CollectionConvert<T, TBase>? _memberSet = memberSet;
        
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