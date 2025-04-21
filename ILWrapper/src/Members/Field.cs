
using ILWrapper.Containers;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.Members;

public class Field : IMember<Field, FieldReference>, IMember
{
    public FieldReference Base { get; }
    public MemberReference MemberBase => Base;
    static TypeConvert<FieldReference, Field> IMember<Field, FieldReference>.FromBase => instance => new Field(instance);
    
    public ParentInfo Info { get; }
    
    public Field(FieldReference @base)
    {
        Base = @base;
        CustomAttributes = new MemberSet<SubMembers.CustomAttribute, CustomAttribute>(Base.Resolve().CustomAttributes);
        
        Info = new ParentInfo(this);
    }
    
    public Field(string name, FieldAttributes attributes, Type fieldType) : this(new FieldDefinition(name, attributes, fieldType.Base)) {}

    /// <summary>
    /// Constructs a reference to the targeted field.
    /// </summary>
    /// <param name="name">the name of the targeted field</param>
    /// <param name="fieldType">the declaring type of the targeted field</param>
    /// <param name="declaringType"></param>
    public Field(string name, Type fieldType, Type declaringType) : this(new FieldReference(name, fieldType.Base, declaringType.Base)) {}
    /// <summary>
    /// Constructs a reference to the targeted field.
    /// </summary>
    /// <param name="name">the name of the targeted field</param>
    /// <param name="fieldType">the type of the targeted field</param>
    public Field(string name, Type fieldType) : this(new FieldReference(name, fieldType.Base)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public int Rva => Base.Resolve().RVA;
    public Module? Module => IMember<Module, ModuleDefinition>.Create(Base.Module);

    public Type FieldType { get => new(Base.FieldType); set => Base.FieldType = value.Base; }
    public object? Constant { get => Base.Resolve().Constant; set => Base.Resolve().Constant = value; }
    public byte[]? InitialValue { get => Base.Resolve().InitialValue; set => Base.Resolve().InitialValue = value; }
    public int Offset { get => Base.Resolve().Offset; set => Base.Resolve().Offset = value; }
    public Type? DeclaringType { get => IMember<Type, TypeReference>.Create(Base.DeclaringType); set => Base.DeclaringType = value?.Base; }
    public FieldAttributes Attributes { get => Base.Resolve().Attributes; set => Base.Resolve().Attributes = value; }
    public MetadataToken MetadataToken { get => Base.MetadataToken; set => Base.MetadataToken = value; }
    public MarshalInfo MarshalInfo { get => Base.Resolve().MarshalInfo; set => Base.Resolve().MarshalInfo = value; }
    
    public IMemberSet<SubMembers.CustomAttribute> CustomAttributes { get; }
    
    public Field Clone(ParentInfo info)
    {
        var clone = new Field(Name, Attributes, info.Remap(FieldType))
        {
            Constant = Constant,
            InitialValue = InitialValue,
            Offset = Offset,
            DeclaringType = info.Type,
            MetadataToken = MetadataToken,
            MarshalInfo = MarshalInfo
        };
        info.Field = clone;
        clone.CustomAttributes.ReplaceContents(CustomAttributes, info);
        
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
