namespace ILWrapper.MemberSet;

public interface IMemberSet<T> : IList<T>
{
    public void CloneTo(IMemberSet<T> other, ParentInfo info);
    public void ReplaceContents(IMemberSet<T> replacement, ParentInfo info);

    public bool Matches(IMemberSet<T> other);
    
    public string ToString(Func<T, string> itemToString);

    public void RemoveAll(Func<T, bool> predicate)
    {
        for (var i = 0; i < Count; i++)
        {
            if (predicate(this[i]))
            {
                RemoveAt(i);
                i--;
            }
        }
    }
}