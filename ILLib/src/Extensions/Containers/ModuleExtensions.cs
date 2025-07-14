using Mono.Cecil;

namespace ILLib.Extensions.Containers;

public static class ModuleExtensions
{
    public static ModuleDefinition Clone(this ModuleDefinition self, ParentInfo info)
    {
        var clone = ModuleDefinition.CreateModule(self.Name, new ModuleParameters
        {
            Kind = self.Kind,
            AssemblyResolver = self.AssemblyResolver,
            Architecture = self.Architecture,
            MetadataResolver = self.MetadataResolver,
            Runtime = self.Runtime
        });
        
        clone.EntryPoint = self.EntryPoint;
        clone.Attributes = self.Attributes;
        clone.RuntimeVersion = self.RuntimeVersion;
        clone.Characteristics = self.Characteristics;
        clone.Kind = self.Kind;
        clone.Mvid = self.Mvid;
        clone.MetadataKind = self.MetadataKind;
        clone.MetadataToken = self.MetadataToken;
        
        info.Module = clone;
        clone.Types.ReplaceContents(self.Types, info);
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        
        return clone;
    }
    
    public static FieldReference TryImportReference(this ModuleDefinition self, FieldReference f, out bool success)
    {
        try
        {
            success = true;
            return self.ImportReference(f);
        }
        catch
        {
            success = false;
            return f;
        }
    }
    public static MethodReference TryImportReference(this ModuleDefinition self, MethodReference m, out bool success)
    {
        try
        {
            success = true;
            return self.ImportReference(m);
        }
        catch
        {
            success = false;
            return m;
        }
    }
    public static TypeReference TryImportReference(this ModuleDefinition self, TypeReference t, out bool success)
    {
        try
        {
            success = true;
            return self.ImportReference(t);
        }
        catch
        {
            success = false;
            return t;
        }
    }
    
    public static FieldReference TryImportReference(this ModuleDefinition self, FieldReference f)
    {
        return self.TryImportReference(f, out _);
    }
    public static MethodReference TryImportReference(this ModuleDefinition self, MethodReference m)
    {
        return self.TryImportReference(m, out _);
    }
    public static TypeReference TryImportReference(this ModuleDefinition self, TypeReference t)
    {
        return self.TryImportReference(t, out _);
    }
}
