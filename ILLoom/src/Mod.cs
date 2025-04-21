using ILLoom.ModuleScanners;
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
    /// <summary>
    /// An array of all <see cref="IInjector"/>s declared in the mod
    /// </summary>
    public readonly List<IInjector> Injectors = [];
    /// <summary>
    /// An array of all <see cref="Type"/>s to be copied into the target assembly
    /// </summary>
    public readonly List<Type> CopyTypes = [];
    
    public Mod(Module module, System.Reflection.Assembly assembly)
    {
        Console.WriteLine($"Loading Mod: {module.Name}");
        _module = module.Clone(new ParentInfo());
        
        // scan the mod's module
        
        Config = new ConfigScanner(assembly).Scan(_module);
        
        Console.WriteLine($"Created Mod: {Config.Id}");
    }

    public void ScanTypeInserters()
    {
        new InsertTypeScanner().Scan(_module);
    }
    
    public void ScanTypeHoisters()
    {
        Program.AddHoistRemappings(new HoistTypeScanner().Scan(_module));
        Program.AddHoistRemappings(new HoistScanner().Scan(_module));
    }

    public void Scan()
    {
        //TODO: implement injectors and inserters
        
        new InsertScanner().Scan(_module);
        
        new InjectorEnumScanner().Scan(_module);
        new InjectorScanner().Scan(_module);
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
