using System.Diagnostics;

namespace GZipTest.Logger;
public class Logger : ILogger
{
    public void WriteMessage(string message)
    {
        Debug.WriteLine($"{Environment.CurrentManagedThreadId}: {message}");
    }

    public void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{Constants.Error}: {message}");
        Console.ResetColor();
    }
}