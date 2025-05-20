namespace Game;

public class Game
{
    private readonly Fortress _fort = new();
    
    public int Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"args[0] = \"{args[0]}\"");
        Test();
        
        Console.WriteLine(_fort.GetValue());
        
        return 0;
    }
    
    private static void Test()
    {
        Console.WriteLine("Hello from Test() method");
        throw new Exception("test exception 123");
    }
    
    private sealed class Fortress
    {
        private int _offset;
        private const int StartValue = 42;
        private readonly Basement _basement = new(Basement.BasementType.Storage);
        
        internal int GetValue()
        {
            return StartValue + _offset;
        }
        
        internal void IncreaseBy(int v)
        {
            _offset += v;
        }

        internal void SetType(Basement.BasementType type)
        {
            _basement.SetType(type);
        }
        
        internal sealed class Basement
        {
            private BasementType _type;
            
            public Basement(BasementType type = BasementType.Empty)
            {
                _type = type;
            }
            
            public void SetType(BasementType type)
            {
                if (type == BasementType.BlueCrystalLab)
                {
                    Console.WriteLine("Blue Crystals are not allowed.");
                    return;
                }
                _type = type;
            }
            
            public enum BasementType
            {
                Empty = 0,
                Storage = 1,
                BlueCrystalLab = 2
            }
        }
    }
}