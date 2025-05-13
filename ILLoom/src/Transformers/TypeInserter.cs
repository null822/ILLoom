using ILWrapper;
using ILWrapper.Containers;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class TypeInserter : ITransformer
{
    private readonly Type _type;
    private readonly Assembly _assembly;
    private readonly string _namespace;
    private readonly string[] _path;
    
    public TypeInserter(Type type, Assembly assembly, string signature)
    {
        _type = type;
        _assembly = assembly;
        
        var separatorIndex = signature.LastIndexOf('.');
        _namespace = signature[..separatorIndex];
        _path = signature[(separatorIndex + 1)..].Split('/');
    }

    public void Apply()
    {
        var targetReference = new TypeReference(
            _namespace,
            _path[0],
            _assembly.MainModule.Base,
            _assembly.MainModule.Base);
        
        var rootTypeDef = Program.MetadataResolver.Resolve(targetReference);
        
        // create any nested types
        var nestedTypes = new Type[_path.Length];
        
        // create or retrieve the nested types
        for (var i = 0; i < _path.Length - 1; i++)
        {
            // get the parent
            Type parent;
            if (i == 0) // special case for root type
            {
                if (rootTypeDef == null)
                {
                    // create a new root type, and add it to the module
                    parent = new Type(_namespace, _path[0], TypeAttributes.Class ^ TypeAttributes.Public);
                    _assembly.MainModule.Types.Add(parent);
                }
                else // otherwise, use the existing root type in the module
                    parent = new Type(rootTypeDef);
            }
            else
            {
                parent = nestedTypes[i - 1];
            }
            
            // get from the parent, or create if it doesn't exist, the next type, and add it to the nested types
            var type = parent.NestedTypes.FirstOrDefault(t => t != null && t.Name == _path[i], null);
            if (type == null)
            {
                type = new Type(_namespace, _path[i], TypeAttributes.Class | TypeAttributes.Public);
                if (i != 0)
                {
                    type.Attributes |= TypeAttributes.NestedPublic | TypeAttributes.NestedPrivate;
                }
            }

            nestedTypes[i] = type;
        }
        
        // remap the name of the type to insert and add it to the nested types
        var endType = _type.Clone(new ParentInfo(_assembly.MainModule));
        endType.Name = _path[^1];
        nestedTypes[^1] = endType;
        
        // link up the nested types
        for (var i = 1; i < _path.Length; i++)
        {
            nestedTypes[i].DeclaringType = nestedTypes[i - 1];
        }
        
        // the root type is already inserted into the module, so nothing more needs to be done
    }
}
