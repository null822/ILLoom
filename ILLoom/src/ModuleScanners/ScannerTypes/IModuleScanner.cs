using Mono.Cecil;

namespace ILLoom.ModuleScanners.ScannerTypes;

public interface IModuleScanner<T>
{
    public List<T> Scan(ModuleDefinition module);
}