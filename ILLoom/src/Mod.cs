using ILLoom.ModuleScanners;
using ILLoom.Transformers;
using ILWrapper;
using ILWrapper.Containers;
using LoomModLib;
using Type = ILWrapper.Containers.Type;

namespace ILLoom;

public class Mod
{
    /// <summary>
    /// A cloned version of the main <see cref="Module"/> of the mod
    /// </summary>
    private readonly Module _module;
    
    /// <summary>
    /// The <see cref="IModConfig"/> of the mod
    /// </summary>
    public readonly IModConfig Config;
    
    public readonly List<TypeInserter> TypeInserters = [];
    public readonly List<Inserter> Inserters = [];
    public readonly List<IInjector> Injectors = [];
    
    
    /// <summary>
    /// An array of all <see cref="Type"/>s to be copied into the target assembly
    /// </summary>
    public readonly List<Type> CopyTypes = [];
    
    public Mod(Module module, System.Reflection.Assembly assembly)
    {
        // clone the module
        _module = module.Clone(new ParentInfo());
        
        // add the module to the assembly resolver
        Program.AssemblyResolver.RegisterAssembly(_module.Assembly);

        // scan the module for the mod config
        Config = new ConfigScanner(assembly).Scan(_module);
        
        Console.WriteLine($" -> Loaded Mod {Config.Id}");
    }

    public void ScanTypeInserters()
    {
        TypeInserters.AddRange(new InsertTypeScanner().Scan(_module));
    }
    
    public void RegisterTypeHoisters()
    {
        Program.AddHoistRemappings(new HoistTypeScanner().Scan(_module));
    }
    
    public void RegisterHoisters()
    {
        Program.AddHoistRemappings(new HoistScanner().Scan(_module));
    }

    public void ScanInserters()
    {
        Inserters.AddRange(new InsertScanner().Scan(_module));
    }
    
    public void ScanEnumInjectors()
    {
        //TODO: implement injectors
        new InjectEnumScanner().Scan(_module);
    }
    
    public void ScanInjectors()
    {
        //TODO: implement injectors
        new InjectScanner().Scan(_module);
    }
    
    public void LoadCopyTypes()
    {
        // all remaining types will be copied into the target application
        foreach (var type in _module.Types)
        {
            if (type.IsEmpty) continue;
            CopyTypes.Add(type);
        }
    }
    
}
