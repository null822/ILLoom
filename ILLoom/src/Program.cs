using System.Diagnostics;
using System.Reflection;
using System.Text;
using ILWrapper;
using Mono.Cecil;
using Assembly = ILWrapper.Containers.Assembly;
using Module = ILWrapper.Containers.Module;

namespace ILLoom;

public static class Program
{
    private static readonly string RootDirectory = Directory.GetCurrentDirectory();
    private static readonly string ModDirectory = $"{RootDirectory}/mods";
    private static string _targetPath = "";
    
    private static Mod[] _mods = [];
    private static string[] _modIds = [];
    
    /// <summary>
    /// A <see cref="Dictionary{TKey,TValue}"/> remapping <see cref="MemberReference"/>s of members with the
    /// <see cref="LoomModLib.Attributes.HoistAttribute"/> or <see cref="LoomModLib.Attributes.HoistTypeAttribute"/>
    /// </summary>
    private static readonly Dictionary<string, MemberReference> HoistRemappings = [];
    
    private static Assembly _targetAssembly = null!;
    public static Module TargetModule => _targetAssembly.MainModule;
    
    public static readonly DefaultAssemblyResolver AssemblyResolver = new();
    public static readonly MetadataResolver MetadataResolver = new(AssemblyResolver);
    
    private static ParentInfo _targetInfo;
    
    private static System.Reflection.Assembly _patchedAssembly = null!;
    
    private static int Main(string[] args)
    {
        if (args.Length < 1)
            throw new Exception("No target application path supplied");
        
        Directory.CreateDirectory(ModDirectory);
        _targetPath = args[0];
        
        _targetAssembly = Assembly.ReadAssembly(_targetPath, new ReaderParameters
        {
            AssemblyResolver = AssemblyResolver,
            MetadataResolver = MetadataResolver
        });
        AssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(_targetPath));
        
        // create the remapper
        _targetInfo = TargetModule.Info;
        _targetInfo.Remapper = original => IMember.FromBaseRef(Remap(original.MemberBase));
        
        Util.Heading("Loading Mods");
        LoadMods();
        Util.Heading("Applying Mods");
        ApplyMods();
        Util.Heading("Build Assembly");
        BuildAssembly();
        Util.Heading("Launch Assembly");
        LaunchAssembly();
        Util.Heading("Modloading Complete", '#');
        
        return 0;
    }

    private static void LoadMods()
    {
        var modFiles = Directory.GetFiles(ModDirectory);
        var mods = new List<Mod>(modFiles.Length);
        var modIds = new List<string>(modFiles.Length);
        foreach (var modFile in modFiles)
        {
            var asmDef = Assembly.ReadAssembly(modFile, new ReaderParameters
            {
                AssemblyResolver = AssemblyResolver,
                MetadataResolver = MetadataResolver
            });
            var asm = System.Reflection.Assembly.LoadFile(modFile);
            var mod = new Mod(asmDef.MainModule, asm);
            modIds.Add(mod.Config.Id);
            mods.Add(mod);
        }
        _mods = mods.ToArray();
        _modIds = modIds.ToArray();
        mods.Clear();
        modIds.Clear();
    }

    private static void ApplyMods()
    {
        foreach (var mod in _mods)
        {
            Util.Heading(mod.Config.Id, '-');
            
            Console.WriteLine("  - Add Types");

            foreach (var type in mod.CopyTypes)
            {
                Console.WriteLine($"    - {type.FullName}");
                var clone = type.Clone(TargetModule.Info);
                TargetModule.Types.Add(clone);
            }
            
            Console.WriteLine("  - Apply Injectors");

            var injectors = mod.Injectors;
            
            var prevInjectorApplyStates = new int[injectors.Count];
            var injectorApplyStates = new int[injectors.Count];
            
            var completeCount = 0;
            var injectorIndex = 0;
            while (injectors.Count != 0)
            {
                if (injectorIndex >= injectors.Count)
                {
                    // exit if we applied all the injector
                    if (completeCount == injectors.Count) break;
                    
                    // if nothing changed in the previous iteration through the injectors, exit
                    if (prevInjectorApplyStates == injectorApplyStates)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("      - Some Injectors Failed to Apply:");
                        
                        for (var i = 0; i < injectorApplyStates.Length; i++)
                        {
                            if (injectorApplyStates[i] == 1) continue;
                            
                            Console.WriteLine($"          - {injectors[i].Signature}");
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Gray;
                        
                        break;
                    }
                    
                    // otherwise, go through the injectors again
                    prevInjectorApplyStates = injectorApplyStates;
                    completeCount = 0;
                    injectorIndex = 0;
                }
                
                // if the next injector was already successfully applied, skip it 
                if (injectorApplyStates[injectorIndex] == 1)
                {
                    injectorIndex++;
                    completeCount++;
                    continue;
                }
                
                // get the next injector
                var injector = injectors[injectorIndex];
                
                // TODO: apply the injectors
                
                // var success = ApplyInjector(injector);
                var success = 0;
                
                // store the result
                injectorApplyStates[injectorIndex] = success;
                
                // go to the next injector
                injectorIndex++;
            }
        }
    }

    private static void BuildAssembly()
    {
        foreach (var remapping in HoistRemappings)
        {
            Console.WriteLine($"{remapping.Key} => {remapping.Value.FullName}");
        }

        Directory.CreateDirectory("out");
        _targetAssembly.Write("out/patched.dll");
        
        var targetConfig = _targetPath.Remove(_targetPath.LastIndexOf('.')) + ".runtimeconfig.json";
        File.Copy(targetConfig, "out/patched.runtimeconfig.json", true);
        
        // convert ILWrapper.Member.Assembly into System.Reflection.Assembly
        var assemblyStream = new MemoryStream();
        _targetAssembly.Write(assemblyStream);
        _patchedAssembly = System.Reflection.Assembly.Load(assemblyStream.ToArray());
        assemblyStream.Dispose();
        _targetAssembly.Dispose();
    }

    private static void LaunchAssembly()
    {
        // invoke the entrypoint
        
        var action = ApplicationAction.Constructing;
        try
        {
            var entrypoint = _patchedAssembly.EntryPoint ?? throw new Exception("Game assembly does not contain an entrypoint");
            var o = Activator.CreateInstance(entrypoint.ReflectedType ??
                                             throw new Exception("Game assembly does not contain a valid entrypoint"));
            
            string[] applicationArgs = ["hello"];
            action = ApplicationAction.Running;
            
            Console.ForegroundColor = ConsoleColor.White;
            var exitCode = (int)(entrypoint.Invoke(o, [applicationArgs]) ?? 0);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Application Exited with Code {exitCode}");
        }
        catch (Exception e)
        {
            if (e is not TargetInvocationException { InnerException: not null } invocationException)
                throw;
            
            var innerException = invocationException.InnerException;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception thrown when {action.ToString().ToLower()} application: {innerException.GetType()}: {innerException.Message}");
            
            var trace = new StackTrace(innerException, true).GetFrames();
            for (var i = 0; i < trace.Length - 2; i++)
            {
                var frame = trace[i];
                Console.WriteLine($"   at {GetMethodSig(frame.GetMethod())} IL_{frame.GetILOffset():X4}");
            }
            
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    
    /// <summary>
    /// Register a new hoist remap to <see cref="HoistRemappings"/>
    /// </summary>
    /// <param name="from">the <see cref="MemberReference"/> to map from</param>
    /// <param name="to">the <see cref="MemberReference"/> to map to</param>
    public static void AddHoistRemap(string from, MemberReference to)
    {
        HoistRemappings.Add(from, to);
    }
    
    /// <summary>
    /// List version of <see cref="AddHoistRemap(Mono.Cecil.MemberReference,Mono.Cecil.MemberReference)"/>
    /// </summary>
    /// <param name="mappings">a list of mappings to add</param>
    public static void AddHoistRemappings(List<(string, MemberReference)> mappings)
    {
        foreach (var mapping in mappings)
        {
            if (!HoistRemappings.TryAdd(mapping.Item1, mapping.Item2))
            {
                Console.WriteLine($"Failed to add Hoist remapping: {mapping.Item1} => {mapping.Item2.FullName}");
            }
        }
    }
    
    /// <summary>
    /// Remap a <see cref="MemberReference"/> using the <see cref="HoistRemappings"/>
    /// </summary>
    public static MemberReference Remap(MemberReference reference)
    {
        return HoistRemappings.GetValueOrDefault(reference.FullName, reference);
    }
    
    private static string GetMethodSig(MethodBase? method)
    {
        if (method == null) return "";

        var s = new StringBuilder();

        s.Append(method.DeclaringType?.FullName);
        s.Append('.');
        s.Append(method.Name);
        s.Append('(');
        var parameters = method.GetParameters();
        if (parameters.Length != 0)
        {
            foreach (var parameter in parameters)
            {
                s.Append(parameter.ParameterType.Name);
                s.Append(' ');
            }

            s.Remove(s.Length - 1, 1);
        }
        s.Append(')');

        return s.ToString();
    }

    private enum ApplicationAction
    {
        Constructing,
        Running,
        Disposing
    }

}