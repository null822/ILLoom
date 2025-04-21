namespace ILLoom;

public static class Util
{
    public static void Heading(string s, char c = '=', ConsoleColor color = ConsoleColor.Magenta)
    {
        var dashes = Math.Max(0, 13 - s.Length + 15) / 2f + 1;
        var dashesL = (int)Math.Floor(dashes);
        var dashesR = (int)Math.Ceiling(dashes);
        Console.ForegroundColor = color;
        Console.WriteLine($"{"".PadRight(dashesL, c)}[ {s} ]{"".PadRight(dashesR, c)}");
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}