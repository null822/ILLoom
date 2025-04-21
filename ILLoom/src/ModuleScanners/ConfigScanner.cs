using ILWrapper.Containers;
using LoomModLib;

namespace ILLoom.ModuleScanners;

public class ConfigScanner(System.Reflection.Assembly assembly) : IModuleScanner<IModConfig>
{
    public IModConfig Scan(Module module)
    {
        for (var i = 0; i < module.Types.Count; i++)
        {
            var type = module.Types[i];
            if (type.FullName == "<Module>") continue;
            
            if (type.Interfaces.Any(interf => interf.Type.Is<IModConfig>()))
            {
                var t = assembly.GetType(type.FullName);
                var config = t == null ? null : Activator.CreateInstance(t) as IModConfig;
                if (config != null)
                {
                    module.Types.RemoveAt(i);
                    return config;
                }
            }
        }

        throw new MissingConfigException(module.Name);
    }
    
    public class MissingConfigException(string modName) : Exception($"Mod {modName} is Missing a Config");
}
