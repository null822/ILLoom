namespace TestMod;

public class SecondaryClass
{
    public static readonly string Id = "test-mod";
    
    public void Print()
    {
        Console.WriteLine($"Greetings from {nameof(SecondaryClass)}");
        Console.WriteLine($"Your ID is: {Id}");
    }
}
