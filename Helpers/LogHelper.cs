using System;

namespace SeoAnalyzer.Helpers;

/// <summary>
/// Provides logging utility methods for tracking execution progress and status messages.
/// </summary>
public static class LogHelper
{
    public static bool IsEnabled { get; set; } = true;

    public static Action<string>? CustomLogger { get; set; }

    public static void Info(string message, bool enableLog = true)
    {
        Log("INFO", message, ConsoleColor.Cyan, enableLog);
    }

    public static void Success(string message, bool enableLog = true)
    {
        Log("SUCCESS", message, ConsoleColor.Green, enableLog);
    }

    public static void Warning(string message, bool enableLog = true)
    {
        Log("WARN", message, ConsoleColor.Yellow, enableLog);
    }

    public static void Error(string message, bool enableLog = true)
    {
        Log("ERROR", message, ConsoleColor.Red, enableLog);
    }

    public static void Log(string level, string message, ConsoleColor color = ConsoleColor.Gray, bool enableLog = true)
    {
        if (!IsEnabled || !enableLog)
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var formattedMessage = $"[{timestamp}] [SeoAnalyzer] [{level}] {message}";

        if (CustomLogger != null)
        {
            CustomLogger(formattedMessage);
            return;
        }

        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp}] ");

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[SeoAnalyzer] ");

        Console.ForegroundColor = color;
        Console.Write($"[{level}] ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);

        Console.ForegroundColor = originalColor;
    }
}
