using ILLoom.ModuleScanners.ScannerTypes;
using ILLib.Extensions.Containers;
using LoomModLib;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class ConfigScanner(System.Reflection.Assembly assembly) : IModuleScanner<IModConfig>
{
    public List<IModConfig> Scan(ModuleDefinition module)
    {
        var configs = new List<IModConfig>();
        
        for (var i = 0; i < module.Types.Count; i++)
        {
            var type = module.Types[i];
            if (type.FullName == "<Module>") continue;

            if (!type.Implements<IModConfig>())
                continue;
            
            var t = assembly.GetType(type.FullName);
            var config = t == null ? null : Activator.CreateInstance(t) as IModConfig;
            if (config != null)
            {
                module.Types.RemoveAt(i);
                configs.Add(config);
            }
        }
        
        if (configs.Count == 0)
            throw new MissingConfigException(module.Name);
        
        return configs;
    }
    
    public class MissingConfigException(string modName) : Exception($"Mod {modName} is Missing a Config");
}
