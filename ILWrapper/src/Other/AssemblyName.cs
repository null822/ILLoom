
using ILWrapper.MemberSet;
using Mono.Cecil;

namespace ILWrapper.Other;

public class AssemblyName : IMember<AssemblyName, AssemblyNameDefinition>
{
    public AssemblyNameDefinition Base { get; }
    static TypeConvert<AssemblyNameDefinition, AssemblyName> IMember<AssemblyName, AssemblyNameDefinition>.FromBase => instance => new AssemblyName(instance);
    
    public AssemblyName(AssemblyNameDefinition @base) => Base = @base;
    public AssemblyName(string name, Version version) : this(new AssemblyNameDefinition(name, version)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public MetadataScopeType MetadataScopeType => Base.MetadataScopeType;
    
    public AssemblyAttributes Attributes { get => Base.Attributes; set => Base.Attributes = value; }
    public MetadataToken MetadataToken { get => Base.MetadataToken; set => Base.MetadataToken = value; }
    public string Culture { get => Base.Culture; set => Base.Culture = value; }
    public Version Version { get => Base.Version; set => Base.Version = value; }
    public AssemblyHashAlgorithm HashAlgorithm { get => Base.HashAlgorithm; set => Base.HashAlgorithm = value; }
    public byte[] Hash { get => Base.Hash; set => Base.Hash = value; }
    public byte[] PublicKey { get => Base.PublicKey; set => Base.PublicKey = value; }
    public byte[] PublicKeyToken { get => Base.PublicKeyToken; set => Base.PublicKeyToken = value; }
    public bool IsRetargetable { get => Base.IsRetargetable; set => Base.IsRetargetable = value; }
    public bool IsWindowsRuntime { get => Base.IsWindowsRuntime; set => Base.IsWindowsRuntime = value; }
    public bool IsSideBySideCompatible { get => Base.IsSideBySideCompatible; set => Base.IsSideBySideCompatible = value; }
    public bool HasPublicKey { get => Base.HasPublicKey; set => Base.HasPublicKey = value; }
    
    public AssemblyName Clone(ParentInfo info)
    {
        return new AssemblyName(Name, Version)
        {
            Attributes = Attributes,
            MetadataToken = MetadataToken,
            Culture = Culture,
            HashAlgorithm = HashAlgorithm,
            Hash = Hash,
            PublicKey = PublicKey,
            PublicKeyToken = PublicKeyToken,
            IsRetargetable = IsRetargetable,
            IsWindowsRuntime = IsWindowsRuntime,
            IsSideBySideCompatible = IsSideBySideCompatible,
            HasPublicKey = HasPublicKey
        };
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
