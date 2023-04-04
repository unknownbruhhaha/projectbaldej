namespace BaldejFramework;

class LuaFuncs
{
    public void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(message, ConsoleColor.Gray);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void SharpGCCollect()
    {
        GC.Collect();
    }
}
