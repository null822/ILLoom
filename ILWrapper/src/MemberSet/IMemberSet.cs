namespace ILWrapper.MemberSet;

public interface IMemberSet<T> : IList<T>
{
    public void CloneTo(IMemberSet<T> other, ParentInfo info);
    public void ReplaceContents(IMemberSet<T> replacement, ParentInfo info);

    public bool Matches(IMemberSet<T> other);
    
    public string ToString(Func<T, string> itemToString);
}