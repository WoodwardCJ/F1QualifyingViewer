using QualifyingLapViewer.Models;

namespace QualifyingLapViewer.Presentation;

public sealed class ConsoleRenderer
{
    public void PrintHeader(string filePath)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(
            "============================================================");
        Console.WriteLine(
            "                 F1 QUALIFYING LAP VIEWER                  ");
        Console.WriteLine(
            "============================================================");
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine($"Dataset: {filePath}");
        Console.WriteLine();
    }

    public void PrintDriverResult(
        DriverQualifyingResult result)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"Driver: {result.Driver}");

        Console.ResetColor();

        if (!string.IsNullOrWhiteSpace(result.Team))
            Console.WriteLine($"Team:   {result.Team}");

        if (!string.IsNullOrWhiteSpace(result.DriverNumber))
            Console.WriteLine($"Number: {result.DriverNumber}");

        Console.WriteLine();

        Console.WriteLine(
            "------------------------------------------------------------");

        Console.WriteLine(
            $"{"Session",-12}{"Best Valid Lap",-20}");

        Console.WriteLine(
            "------------------------------------------------------------");

        Console.WriteLine(
            $"{"Q1",-12}{FormatTime(result.Q1Best),-20}");

        Console.WriteLine(
            $"{"Q2",-12}{FormatTime(result.Q2Best),-20}");

        Console.WriteLine(
            $"{"Q3",-12}{FormatTime(result.Q3Best),-20}");

        Console.WriteLine(
            "------------------------------------------------------------");

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine(
            $"Final qualifying position: P{result.FinalPosition}");

        Console.WriteLine(
            $"Final qualifying session:  {result.FinalSession?.ToString() ?? "N/A"}");

        Console.WriteLine(
            $"Final tire compound:  {result.FinalTireCompound?.ToString() ?? "N/A"}");

        Console.ResetColor();

        Console.WriteLine();
    }

    public void PrintClassification(
        IReadOnlyList<DriverQualifyingResult> results)
    {
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(
            "============================================================");
        Console.WriteLine(
            "                 QUALIFYING CLASSIFICATION                  ");
        Console.WriteLine(
            "============================================================");
        Console.ResetColor();

        Console.WriteLine();

        Console.WriteLine(
            $"{"Pos",-6}{"Driver",-10}{"Q1",-15}{"Q2",-15}{"Q3",-15}");

        Console.WriteLine(
            new string('-', 61));

        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.FinalPosition,-6}" +
                $"{result.Driver,-10}" +
                $"{FormatTime(result.Q1Best),-15}" +
                $"{FormatTime(result.Q2Best),-15}" +
                $"{FormatTime(result.Q3Best),-15}");
        }

        Console.WriteLine();
    }

    public void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {message}");
        Console.ResetColor();
        Console.WriteLine();
    }

    public void PrintDrivers(
        IReadOnlyList<string> drivers)
    {
        Console.WriteLine("Available drivers:");

        foreach (var driver in drivers)
        {
            Console.Write($"  {driver}  ");
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    private static string FormatTime(TimeSpan? time)
    {
        if (!time.HasValue)
            return "-";

        return $"{(int)time.Value.TotalMinutes}:{time.Value.Seconds:00}.{time.Value.Milliseconds:000}";
    }
}
