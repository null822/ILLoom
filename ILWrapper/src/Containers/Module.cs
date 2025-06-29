using ILWrapper.Members;
using ILWrapper.MemberSet;
using Mono.Cecil;
using ISymbolReader = Mono.Cecil.Cil.ISymbolReader;

namespace ILWrapper.Containers;

public class Module : IMember<Module, ModuleDefinition>, IMemberContainer
{
    public ModuleDefinition Base { get; }
    static TypeConvert<ModuleDefinition, Module> IMember<Module, ModuleDefinition>.FromBase => instance => new Module(instance);
    
    public ParentInfo Info { get; set; }
    
    public Module(ModuleDefinition @base)
    {
        Base = @base;
        
        Types = MemberSet<Type, TypeReference>.From(Base.Types);
        CustomAttributes = new MemberSet<SubMembers.CustomAttribute, CustomAttribute>(Base.CustomAttributes);
        
        Info = new ParentInfo().With(this);
    }

    public Module(string name, ModuleKind kind) : this(ModuleDefinition.CreateModule(name, kind)) {}
    public Module(string name, ModuleParameters parameters) : this(ModuleDefinition.CreateModule(name, parameters)) {}
    
    public Module(string fileName) : this(ModuleDefinition.ReadModule(fileName)) { }
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FileName => Base.FileName;
    public string FullName => FileName;
    
    public Method? Entrypoint { get => IMember<Method, MethodReference>.Create(Base.EntryPoint); set => Base.EntryPoint = value?.Base.Resolve(); }
    public ModuleAttributes Attributes { get => Base.Attributes; set => Base.Attributes = value; }
    public TargetArchitecture TargetArchitecture { get => Base.Architecture; set => Base.Architecture = value; }
    public TargetRuntime TargetRuntime { get => Base.Runtime; set => Base.Runtime = value; }
    public string RuntimeVersion { get => Base.RuntimeVersion; set => Base.RuntimeVersion = value; }
    public ModuleCharacteristics Characteristics { get => Base.Characteristics; set => Base.Characteristics = value; }
    public ModuleKind Kind { get => Base.Kind; set => Base.Kind = value; }
    public Guid Mvid { get => Base.Mvid; set => Base.Mvid = value; }
    public MetadataKind MetadataKind { get => Base.MetadataKind; set => Base.MetadataKind = value; }
    public MetadataToken MetadataToken { get => Base.MetadataToken; set => Base.MetadataToken = value; }
    
    
    public Assembly Assembly => IMember<Assembly, AssemblyDefinition>.Create(Base.Assembly);
    public IAssemblyResolver? AssemblyResolver => Base.AssemblyResolver;
    public IMetadataResolver? MetadataResolver => Base.MetadataResolver;
    public MetadataScopeType? MetadataScopeType => Base.MetadataScopeType;
    public ISymbolReader? SymbolReader => Base.SymbolReader;
    public TypeSystem? TypeSystem => Base.TypeSystem;
    public bool IsMain => Base.IsMain;
    public bool HasResources => Base.HasResources;
    public bool HasSymbols => Base.HasSymbols;
    public bool HasTypes => Base.HasTypes;
    public bool HasAssemblyReferences => Base.HasAssemblyReferences;
    public bool HasCustomAttributes => Base.HasCustomAttributes;
    public bool HasExportedTypes => Base.HasExportedTypes;
    public bool HasModuleReferences => Base.HasModuleReferences;
    public bool HasCustomDebugInformations => Base.HasCustomDebugInformations;
    public bool HasDebugHeader => Base.HasDebugHeader;
    
    public readonly IMemberSet<Type> Types;
    public readonly IMemberSet<SubMembers.CustomAttribute> CustomAttributes;
    //TODO: public readonly IMemberSet<SubMembers.ExportedType> ExportedTypes;
    //TODO: public readonly IMemberSet<SubMembers.AssemblyNameReference> AssemblyReferences;
    //TODO: public readonly IMemberSet<SubMembers.ModuleReference> ModuleReferences;
    //TODO: public readonly IMemberSet<SubMembers.Resource> Resources;
    
    
    public Module Clone(ParentInfo info)
    {
        var clone = new Module(Name, new ModuleParameters
        {
            Kind = Kind,
            AssemblyResolver = AssemblyResolver,
            Architecture = TargetArchitecture,
            MetadataResolver = MetadataResolver,
            Runtime = TargetRuntime
        })
        {
            Entrypoint = Entrypoint,
            Attributes = Attributes,
            RuntimeVersion = RuntimeVersion,
            Characteristics = Characteristics,
            Kind = Kind,
            Mvid = Mvid,
            MetadataKind = MetadataKind,
            MetadataToken = MetadataToken
        };
        info.Module = clone;
        clone.Types.ReplaceContents(Types, info);
        clone.CustomAttributes.ReplaceContents(CustomAttributes, info);

        return clone;
        
    }

    public Field ImportReference(Field f) => new(Base.ImportReference(f.Base));
    public Method ImportReference(Method m) => new(Base.ImportReference(m.Base));
    public Type ImportReference(Type t) => new(Base.ImportReference(t.Base));
    
    public Field TryImportReference(Field f, out bool success)
    {
        try
        {
            success = true;
            return ImportReference(f);
        }
        catch
        {
            success = false;
            return f;
        }
    }
    public Method TryImportReference(Method m, out bool success)
    {
        try
        {
            success = true;
            return ImportReference(m);
        }
        catch
        {
            success = false;
            return m;
        }
    }
    public Type TryImportReference(Type t, out bool success)
    {
        try
        {
            success = true;
            return ImportReference(t);
        }
        catch
        {
            success = false;
            return t;
        }
    }
    
    
    public override string ToString()
    {
        return FullName;
    }
}
