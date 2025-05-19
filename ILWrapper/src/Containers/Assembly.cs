using ILWrapper.Members;
using ILWrapper.MemberSet;
using ILWrapper.Other;
using Mono.Cecil;

namespace ILWrapper.Containers;

public class Assembly : IMember<Assembly, AssemblyDefinition>, IDisposable, IMemberContainer
{
    public AssemblyDefinition Base { get; }
    static TypeConvert<AssemblyDefinition, Assembly> IMember<Assembly, AssemblyDefinition>.FromBase => instance => new Assembly(instance);
    
    public ParentInfo Info { get; }
    
    public Assembly(AssemblyDefinition @base)
    {
        Base = @base;
        
        Modules = new MemberSet<Module, ModuleDefinition>(Base.Modules);
        
        Info = new ParentInfo(this);
    }

    public Assembly(AssemblyName name, string moduleName, ModuleKind kind) : this(AssemblyDefinition.CreateAssembly(name.Base, moduleName, kind)) {}
    
    public static Assembly ReadAssembly(AssemblyName name, string moduleName, ModuleKind kind)
        => new Assembly(AssemblyDefinition.CreateAssembly(name.Base, moduleName, kind));
    public static Assembly ReadAssembly(AssemblyName name, string moduleName, ModuleParameters parameters)
        => new Assembly(AssemblyDefinition.CreateAssembly(name.Base, moduleName, parameters));
    
    public static Assembly ReadAssembly(string fileName) => new Assembly(AssemblyDefinition.ReadAssembly(fileName));
    public static Assembly ReadAssembly(string fileName, ReaderParameters parameters)
        => new Assembly(AssemblyDefinition.ReadAssembly(fileName, parameters));
    
    public AssemblyName Name { get => new(Base.Name); set => Base.Name = value.Base; }
    public string FullName => Base.FullName;
    
    public Method? Entrypoint => IMember<Method, MethodReference>.Create(Base.EntryPoint);
    public Module MainModule => new(Base.MainModule);
    
    public readonly IMemberSet<Module> Modules;
    
    public Assembly Clone(ParentInfo info)
    {
        throw new NotImplementedException();
    }

    public void Write(Stream stream, WriterParameters? writerParameters = null)
    {
        if (writerParameters == null)
            Base.Write(stream);
        else
            Base.Write(stream, writerParameters);
    }
    public void Write(string fileName, WriterParameters? writerParameters = null)
    {
        if (writerParameters == null)
            Base.Write(fileName);
        else
            Base.Write(fileName, writerParameters);
    }
    public void Write(WriterParameters? writerParameters = null)
    {
        if (writerParameters == null)
            Base.Write();
        else
            Base.Write(writerParameters);
    }
    
    public override string ToString()
    {
        return FullName;
    }
    
    public void Dispose()
    {
        Base.Dispose();
    }
}
