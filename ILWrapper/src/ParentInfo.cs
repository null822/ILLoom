using System.Diagnostics.CodeAnalysis;
using System.Text;
using ILWrapper.Containers;
using ILWrapper.Members;
using ILWrapper.SubMembers;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper;

public delegate T Remap<T>(T? original) where T : MemberReference;

public struct ParentInfo
{
    public Remap<MemberReference>? Remapper { get; set; }
    public T RemapRef<T>(T? original) where T : MemberReference
    {
        if (Remapper == null)
            return original!;
        
        return (T)Remapper(original);
    }
    public T Remap<T>(T? original) where T : IMember
    {
        if (Remapper == null)
            return original!;
        
        return (T)IMember.FromBaseRef(Remapper(original?.MemberBase));
    }

    public void RequireTypes(params ParentInfoType[] infoTypes)
    {
        MissingParentInfoException.ThrowIfMissing(this, infoTypes);
    }
    
    public Assembly? Assembly { get; set; }
    public Module? Module { get; set; }
    public Type? Type { get; set; }
    
    public Method? Method { get; set; }
    public Field? Field { get; set; }
    public Property? Property { get; set; }
    public Event? Event { get; set; }
    
    public MethodBody? MethodBody { get; set; }
    
    public ParentInfo() { }
    public ParentInfo(Assembly? assembly = null)
    {
        Assembly = assembly;
    }
    public ParentInfo(Module? module = null) : this(module?.Assembly)
    {
        Module = module;
    }
    public ParentInfo(Type? type = null) : this(type?.Module)
    {
        Type = type;
    }
    
    public ParentInfo(Method? method = null) : this(method?.DeclaringType)
    {
        Method = method;
    }
    public ParentInfo(MethodBody? methodBody = null) : this(methodBody?.Method)
    {
        MethodBody = methodBody;
    }
    
    public ParentInfo(Field? field = null) : this(field?.DeclaringType)
    {
        Field = field;
    }
    public ParentInfo(Property? property = null) : this(property?.DeclaringType)
    {
        Property = property;
    }
    

    public override string ToString()
    {
        return $"asm:{Assembly?.ToString() ?? "null"}" +
               $"/mod:{Module?.ToString() ?? "null"}" +
               $"/typ:{Type?.ToString() ?? "null"}" +
               $"/(" +
               $"met:{Method?.ToString() ?? "null"}/mbo:{MethodBody?.ToString() ?? "null"}" +
               $"|fie:{Field?.ToString() ?? "null"}" +
               $"|pro:{Property?.ToString() ?? "null"})";
    }
}

public enum ParentInfoType
{
    Assembly,
    Module,
    Type,
    
    Method,
    Field,
    
    MethodBody
}

public class MissingParentInfoException : Exception
{
    private MissingParentInfoException(ParentInfoType[] missingInfo) : base($"{nameof(ParentInfo)} is missing values for: {InfoToString(missingInfo)}") { }
    
    public static void ThrowIfMissing(ParentInfo info, params ParentInfoType[] infoTypes)
    {
        var missingTypes = new List<ParentInfoType>();

        foreach (var type in infoTypes)
        {
            var isMissing = type switch
            {
                ParentInfoType.Assembly => info.Assembly == null,
                ParentInfoType.Module => info.Module == null,
                ParentInfoType.Type => info.Type == null,
                
                ParentInfoType.Method => info.Method == null,
                ParentInfoType.Field => info.Field == null,
                
                ParentInfoType.MethodBody => info.MethodBody == null,
                _ => false
            };
            if (isMissing) missingTypes.Add(type);
        }
        
        if (missingTypes.Count != 0)
            throw new MissingParentInfoException(missingTypes.ToArray());
    }
    
    private static string InfoToString(ParentInfoType[] missingInfo)
    {
        var s = new StringBuilder();
        foreach (var info in missingInfo)
        {
            s.Append($"{info.ToString()}, ");
        }
        if (s.Length > 0) s.Remove(s.Length - 2, 2);
        
        return s.ToString();
    }
}
