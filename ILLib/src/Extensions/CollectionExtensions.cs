using System.Text;
using Mono.Collections.Generic;

namespace ILLib.Extensions;

public static class CollectionExtensions
{
    public static void RemoveAll<T>(this Collection<T> self, Func<T, bool> predicate)
    {
        for (var i = 0; i < self.Count; i++)
        {
            if (predicate(self[i]))
            {
                self.RemoveAt(i);
                i--;
            }
        }

    }
    
    public static void CloneTo<T>(this Collection<T> self, IList<T> target, ParentInfo info)
    {
        foreach (var item in self)
        {
            if (item is Type { FullName: "<Module>" })
                continue;
            
            var clone = item.CloneMember(info);
            if (clone != null)
                target.Add(clone);
        }
    }
    
    public static void ReplaceContents<T>(this Collection<T> self, Collection<T> replacement, ParentInfo info)
    {
        self.Clear();
        replacement.CloneTo(self, info);
    }
    
    public static void ReplaceContents<T>(this Collection<T> self, IList<T> replacement)
    {
        self.Clear();
        foreach (var element in replacement)
        {
            self.Add(element);
        }
    }
    
    public static bool Matches<T>(this Collection<T> self, Collection<T> other)
    {
        if (self.Count != other.Count)
            return false;

        for (var i = 0; i < self.Count; i++)
        {
            if (self[i]?.ToString() != other[i]?.ToString())
                return false;
        }
        
        return true;
    }
    
    public static string ContentsToString<T>(this Collection<T> self)
    {
        var s = new StringBuilder();
        foreach (var element in self)
        {
            s.Append(element);
            s.Append(", ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
    
    public static string ContentsToString<T>(this Collection<T> self, Func<T, string> itemToString)
    {
        var s = new StringBuilder();
        foreach (var baseElement in self)
        {
            s.Append(itemToString.Invoke(baseElement));
            s.Append(", ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        return s.ToString();
    }
}
