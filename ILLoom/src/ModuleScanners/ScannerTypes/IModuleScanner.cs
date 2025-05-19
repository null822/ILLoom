using ILWrapper.Containers;

namespace ILLoom.ModuleScanners.ScannerTypes;

public interface IModuleScanner<T>
{
    public List<T> Scan(Module module);
}