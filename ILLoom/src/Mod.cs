using System.Reflection;
using ILLoom.ModuleScanners;
using ILLoom.Transformers;
using ILLoom.Transformers.TransformerTypes;
using ILLib;
using ILLib.Extensions;
using ILLib.Extensions.Containers;
using LoomModLib;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom;

public class Mod
{
    /// <summary>
    /// A cloned version of the main <see cref="Module"/> of the mod
    /// </summary>
    private readonly ModuleDefinition _module;
    
    /// <summary>
    /// The <see cref="IModConfig"/> of the mod
    /// </summary>
    public readonly IModConfig Config;
    
    public readonly List<InsertTypeTransformer> TypeInserters = [];
    public readonly List<InsertTransformer> Inserters = [];
    public readonly List<InjectEnumTransformer> EnumInjectors = [];
    public readonly List<IInjector> Injectors = [];
    
    
    /// <summary>
    /// An array of all <see cref="Type"/>s to be copied into the target assembly
    /// </summary>
    public readonly List<TypeDefinition> CopyTypes = [];
    
    public Mod(ModuleDefinition module, System.Reflection.Assembly assembly)
    {
        // clone the module without remapping
        _module = module.Clone(new ParentInfo());
        
        // add the module to the assembly resolver
        Program.AssemblyResolver.RegisterAssembly(_module.Assembly);

        // scan the module for the mod config, taking the first result
        Config = new ConfigScanner(assembly).Scan(_module)[0];
        
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
        EnumInjectors.AddRange(new InjectEnumScanner().Scan(_module));
    }
    
    public void ScanInjectors()
    {
        Injectors.AddRange(new InjectScanner().Scan(_module));
    }

    private static readonly Func<CustomAttribute, bool> IsDontCopy = a => a.AttributeType.Is<DontCopyAttribute>();
    private static readonly Func<IMemberDefinition, bool> HasDontCopy = m => m.CustomAttributes.Any(IsDontCopy);
    
    public void LoadCopyTypes()
    {
        // all remaining types will be copied into the target application
        foreach (var type in _module.Types)
        {
            if (type.CustomAttributes.Any(IsDontCopy))
                continue;

            var clone = type.Clone(Program.TargetInfo);
            RemoveDontCopyMembers(clone);
            
            if (clone.IsEmpty(true)) 
                continue;
            
            CopyTypes.Add(clone);
        }
    }
    
    private static void RemoveDontCopyMembers(TypeDefinition type)
    {
        var nestedTypes = type.NestedTypes;
        
        // remove all members with the [DontCopy] attribute
        type.Fields.RemoveAll(HasDontCopy);
        type.Methods.RemoveAll(HasDontCopy);
        type.Properties.RemoveAll(HasDontCopy);
        type.Events.RemoveAll(HasDontCopy);
        type.NestedTypes.RemoveAll(HasDontCopy);
        
        // recursively scan the (remaining) nested types
        foreach (var subType in nestedTypes)
        {
            RemoveDontCopyMembers(subType);
        }
    }
}
