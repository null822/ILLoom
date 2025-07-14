using ILLib.Extensions.Containers;
using ILLib.Extensions.Members;
using ILLib.Extensions.SubMembers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILLib.Extensions;

public static class MemberReferenceExtensions
{
    public static T CloneMember<T>(this T self, ParentInfo info)
    {
        return self switch
        {
            ModuleDefinition s => (T)(object)s.Clone(info),
            TypeDefinition s => (T)(object)s.Clone(info),
            
            EventDefinition s => (T)(object)s.Clone(info),
            FieldDefinition s => (T)(object)s.Clone(info),
            MethodDefinition s => (T)(object)s.Clone(info),
            PropertyDefinition s => (T)(object)s.Clone(info),
            
            CustomAttribute s => (T)(object)s.Clone(info),
            CustomAttributeArgument s => (T)(object)s.Clone(info),
            CustomAttributeNamedArgument s => (T)(object)s.Clone(info),
            Instruction s => (T)(object)s.Clone(info), 
            InterfaceImplementation s => (T)(object)s.Clone(info),
            MethodBody s => (T)(object)s.Clone(info),
            ParameterDefinition s => (T)(object)s.Clone(info),
            VariableDefinition s => (T)(object)s.Clone(info),
            
            _ => self
        };
    }

    public static ParentInfo get_Info<T>(this T self)
    {
        return self switch
        {
            ModuleDefinition s => new ParentInfo().With(s),
            TypeDefinition s => new ParentInfo().With(s),
            
            FieldDefinition s => new ParentInfo().With(s),
            MethodDefinition s => new ParentInfo().With(s),
            PropertyDefinition s => new ParentInfo().With(s),
            
            MethodBody s => new ParentInfo().With(s),
            
            _ => new ParentInfo()
        };
    }
}