using Mono.Cecil;

namespace ILLib;

public class AssemblyResolver : IAssemblyResolver
{
    private readonly Dictionary<string, Dictionary<Version, AssemblyDefinition>> _assemblies = [];

    public AssemblyDefinition Resolve(AssemblyNameReference assemblyName)
        => Resolve(assemblyName, new ReaderParameters());
    
    public AssemblyDefinition Resolve(AssemblyNameReference assemblyName, ReaderParameters parameters)
    {
        if (_assemblies.TryGetValue(assemblyName.Name, out var assemblies))
        {
            if (assemblies.Count == 0)
                throw new AssemblyNotFoundException(assemblyName);
            
            if (assemblyName.Version == Util.AllVersions)
            {
                return assemblies.Values.First();
            }
            if (assemblies.TryGetValue(assemblyName.Version, out var assembly))
            {
                return assembly;
            }
        }
            

        throw new AssemblyNotFoundException(assemblyName);
    }
    
    public void RegisterAssembly(AssemblyDefinition assembly)
    {
        var name = assembly.Name;
        _assemblies.TryAdd(name.Name, 
            new Dictionary<Version, AssemblyDefinition>
            {
                { name.Version, assembly }
            });
    }
    
    public void RegisterAssembly(string assemblyPath, ReaderParameters? readerParameters = null)
    {
        var asm = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters ?? new ReaderParameters());
        RegisterAssembly(asm);
    }
    
    public void RegisterAssemblies(string path, ReaderParameters? readerParameters = null)
    {
        foreach (var assemblyPath in Directory.GetFiles(path, "*.dll"))
        {
            try
            {
                RegisterAssembly(assemblyPath, readerParameters);
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }
    }
    
    public void Dispose()
    {
        _assemblies.Clear();
    }
}

public class AssemblyNotFoundException(AssemblyNameReference name)
    : Exception($"Assembly \"{name.Name}\" v{name.Version} was not found in {nameof(AssemblyResolver)}");