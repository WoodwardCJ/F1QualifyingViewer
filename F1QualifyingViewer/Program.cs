using QualifyingLapViewer.Presentation;
using QualifyingLapViewer.Services;
using System.Text.Json;

namespace QualifyingLapViewer;

internal static class Program
{
    private static async Task Main()
    {
        var renderer = new ConsoleRenderer();

        Console.Title = "F1 Qualifying Lap Viewer";

        renderer.PrintHeader(
            "TracingInsights session_laptimes.json");

        Console.Write("Enter the path to session_laptimes.json: ");

        var filePath = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            renderer.PrintError("No file path was supplied.");
            Pause();
            return;
        }

        try
        {
            IQualifyingDataReader reader =
                new QualifyingDataReader();

            var laps = await reader.ReadAsync(filePath);

            if (laps.Count == 0)
            {
                renderer.PrintError(
                    "The dataset did not contain any lap records.");

                Pause();
                return;
            }

            IQualifyingAnalysisService analysis =
                new QualifyingAnalysisService(laps);

            Console.WriteLine(
                $"Loaded {laps.Count:N0} lap records.");

            Console.WriteLine();

            renderer.PrintDrivers(
                analysis.GetDrivers());

            while (true)
            {
                Console.Write(
                    "Enter driver code (e.g. HUL, BOR), " +
                    "'classification' or 'exit': ");

                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine();
                    continue;
                }

                if (input.Equals(
                        "exit",
                        StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (input.Equals(
                        "classification",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var classification =
                        analysis.GetClassification();

                    renderer.PrintClassification(
                        classification);

                    continue;
                }

                try
                {
                    var result =
                        analysis.AnalyseDriver(input);

                    renderer.PrintDriverResult(result);
                }
                catch (KeyNotFoundException ex)
                {
                    renderer.PrintError(ex.Message);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            renderer.PrintError(
                $"File not found: {ex.FileName}");
        }
        catch (JsonException ex)
        {
            renderer.PrintError(
                $"The JSON file is invalid: {ex.Message}");
        }
        catch (InvalidDataException ex)
        {
            renderer.PrintError(ex.Message);
        }
        catch (Exception ex)
        {
            renderer.PrintError(
                $"Unexpected error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Application finished.");
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();
    }
}
