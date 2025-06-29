using Game;
using LoomModLib;
using LoomModLib.Attributes;

namespace TestMod;

public class ExampleInjectors
{
    // apply after FortInject has been applied
    [InjectorDependency(nameof(_fortInject))]
    // apply after the injector MainInject from the mod "cool-other-mod" has been applied
    // this dependency will be ignored if the injector or mod was not found (it is an optional dependency)
    [InjectorDependency("MainInject", "Injectors", "cool-other-mod", optional: true)]
    // do not apply if the injector NukeMain from the mod "other-mod" is present
    [InjectorDependency("NukeMain", "ProgramInjectors", "other-mod", invert: true)]
    // only apply after the injector FixMain from mod "bugfix-mod" has been applied first
    // will not apply if that mod or injector was not found
    [InjectorDependency("FixMain", "Injectors", "bugfix-mod")]
    // inject into the method group Program.Main (in class Main)
    [Inject(nameof(Program.Main), typeof(Program))]
    // the accessibility and attributes (abstract, virtual, static, etc.) do not have to match, but the parameters must
    [InjectHead]
    public void TestInjector /* injector name */ (string[] args /* inject specifically into Program.Main(string[]) */)
    {
        Console.WriteLine("Injected Hello from test mod :D");
        Console.WriteLine($"Intercepted args[0] = {args[0]}");
        Console.WriteLine(Test2(69));
        var fort = new FortressHoist();
        Console.WriteLine("Fortress Value = " + fort.GetValue());
        
        Injector.Return();
    }
    
    // inserts a new method (CheckValue(Int32)) into Program that returns a string, is private, and is static
    // the method is also automatically hoisted (see ExampleInjectors.ProgramFortress) into this current class
    [Insert("CheckValue", typeof(Program))]
    public static string Test2(int value)
    {
        return value == 69 ? "Injected Call" : "Injected Call (parameter issue?)";
    }
    
    // almost anything can be inserted. again, they are automatically hoisted
    [Insert("Message", typeof(Program))]
    public static readonly Message2 Name = new Message2("Inserted Field");
    
    // types (classes, interfaces, structs, etc.) are inserted automatically since they must be in the target application to be used
    // the name (and location) of the inserted type can be changed, however, like so:
    [InsertType("Game", "1.0.0.0", "Game.NewGame/A/B/C/AnotherMessage")]
    // this is, however, optional
    // note that they are automatically hoisted too
    public class Message2
    {
        private string _value;
        
        public Message2(string m)
        {
            _value = m;
        }
        
        public override string ToString()
        {
            return _value;
        }
    }
    
    // fields can have their default value overridden
    // in this case, a constant field is being overridden
    // [Inject("StartValue", typeof(FortressHoist))]
    // note that, just like Insertions, Injections are also automatically hoisted
    private int _fortInject = 32;
    
    // sometimes, members that need to be accessed by the mod are not accessible. for that, they can be "hoisted",
    // giving access to them from any code that can access the hoisting class or member
    // all references, calls, and injections to hoisting classes (in this case, ExampleInjectors.ProgramFortress) or any of
    // its members will be redirected to Program.Fortress and its members instead
    // nested types can be hoisted too
    [HoistType("Game", "1.0.0.0", "Game.Game/Fortress")]
    private class FortressHoist
    {
        // hoist the GetValue method in Program.Fortress
        [Hoist("GetValue")]
        // the methods accessibility, attributes, body, and name do not need to match the actual method
        // hence, the method can be marked as extern to avoid having to declare a body
        public extern int GetValue();

        [Hoist("IncreaseBy")]
        // however, the parameters do need to match the target's parameters
        public extern void IncreaseBy(int v);
        
        // fields can also be hoisted
        [Hoist("_offset")]
        public int Offset;
        
        // hoisting classes can contain more hoisted types
        [HoistType("Game", "1.0.0.0", "Game.Game/Fortress/Basement")]
        private class BasementHoist
        {
            // enums, structs, and interfaces can also be hoisted
            [HoistType("Game", "1.0.0.0", "Game.Game/Fortress/Basement/BasementType")]
            public enum BasementType;
            
            // Hoisting classes can also contain injectors
            // enums can also have members injected into them
            [InjectEnum(typeof(BasementType))]
            // doing this does NOT hoist the enum automatically
            public enum BasementTypeInjector
            {
                Cellar, // adds a new definition called `Cellar`
                Storage = 12, // overrides the value of BasementType.Storage to be 12 instead of 1
                ServerRoom = 1, // adds a new definition called `ServerRoom`
                // Note that `ServerRoom` it will not be equal to 1 since `(BasementType)1` is already defined as `Storage`
                [ForceEnumValue]
                MethLab = 2 // adds a new definition called `MethLab`, forcing the value to be `2` just like the exiting `BlueCrystalLab`
            }
        }
    }
}
