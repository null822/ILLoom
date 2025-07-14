namespace ILLib;

public static class Util
{
    public static readonly Version AllVersions = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    
    private const ConsoleColor DefaultColor = ConsoleColor.Gray;
    
    private const bool EnableLog = true;
    private const bool EnableWarn = true;
    private const bool EnableError = true;
    
    private const ConsoleColor LogColor = ConsoleColor.DarkBlue;
    private const ConsoleColor WarnColor = ConsoleColor.Cyan;
    private const ConsoleColor ErrorColor = ConsoleColor.Red;
    
    public static void Log(string message)
    {
        if (!EnableLog) return;

        Console.ForegroundColor = LogColor;
        Console.WriteLine($"[ LOG ]: {message}");
        Console.ForegroundColor = DefaultColor;
    }
    
    public static void Warn(string message)
    {
        if (!EnableWarn) return;

        Console.ForegroundColor = WarnColor;
        Console.WriteLine($"[WARN ]: {message}");
        Console.ForegroundColor = DefaultColor;
    }
    
    public static void Error(string message)
    {
        if (!EnableError) return;

        Console.ForegroundColor = ErrorColor;
        Console.WriteLine($"[ERROR]: {message}");
        Console.ForegroundColor = DefaultColor;
    }
}