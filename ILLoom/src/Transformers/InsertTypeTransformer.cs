using ILLoom.Transformers.TransformerTypes;
using ILLib.Extensions;
using ILLib.Extensions.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.Transformers;

public class InsertTypeTransformer : ITransformer
{
    private readonly TypeDefinition _type;
    private readonly AssemblyDefinition _assembly;
    private readonly string _namespace;
    private readonly string[] _path;
    private readonly string _signature;

    private TypeDefinition? _endType;

    public string Name => $"{_type.FullName} as {_signature}";
    
    public InsertTypeTransformer(TypeDefinition type, AssemblyDefinition assembly, string name, string ns)
    {
        _type = type;
        _assembly = assembly;

        _signature = $"{ns}.{name}";
        _namespace = ns;
        _path = name.Split('/');
    }
    
    public void Apply()
    {
        // get the root type
        var targetReference = new TypeReference(
            _namespace,
            _path[0],
            _assembly.MainModule,
            _assembly.MainModule);
        var rootTypeDef = Program.MetadataResolver.Resolve(targetReference);
        TypeDefinition root;
        if (rootTypeDef != null) // if the root type exists, use it
        {
            root = rootTypeDef;
        }
        else // otherwise, create a new root type and add it to the module
        {
            root = new TypeDefinition(_namespace, _path[0], TypeAttributes.Class ^ TypeAttributes.Public);
            _assembly.MainModule.Types.Add(root);
        }
        
        // create an array for the nested types
        var nestedTypes = new TypeDefinition[_path.Length];
        
        // add the root type to the nested types
        nestedTypes[0] = root;
        
        // create or retrieve the nested types
        for (var i = 1; i < _path.Length - 1; i++)
        {
            // get the parent
            var parent = nestedTypes[i - 1];
            
            // get from the parent, or create if it doesn't exist, the next type, and add it to the nested types
            var type = parent.NestedTypes.FirstOrDefault(t => t != null && t.Name == _path[i], null);
            if (type == null)
            {
                // note that the namespace must be an empty string for the nested types to be added to the assembly
                type = new TypeDefinition("", _path[i], TypeAttributes.Class | TypeAttributes.Public);
                if (i != 0)
                {
                    type.Attributes |= TypeAttributes.NestedPublic | TypeAttributes.NestedPrivate;
                }
            }
            nestedTypes[i] = type;
        }
        
        // remap the name of the type to insert and add it to the nested types
        _endType = _type.Clone(Program.TargetInfo.With(_assembly.MainModule));
        _endType.CustomAttributes.RemoveAll(a => TypeExtensions.Is<DontCopyAttribute>(a.AttributeType));
        _endType.Name = _path[^1];
        _endType.DeclaringType = null;
        nestedTypes[^1] = _endType;
        
        // link up the nested types
        for (var i = 0; i < _path.Length - 1; i++)
        {
            nestedTypes[i].NestedTypes.Add(nestedTypes[i + 1]);
        }
        
        // the root type is already inserted into the module, so nothing more needs to be done
    }
}
