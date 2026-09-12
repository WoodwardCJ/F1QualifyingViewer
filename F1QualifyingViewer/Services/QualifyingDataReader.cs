using System.Text.Json;
using QualifyingLapViewer.Models;

namespace QualifyingLapViewer.Services;

public sealed class QualifyingDataReader : IQualifyingDataReader
{
    private readonly JsonSerializerOptions _jsonOptions;

    public QualifyingDataReader()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<LapRecord>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(
                "A JSON file path is required.",
                nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The specified JSON file could not be found.",
                filePath);
        }

        await using var stream = File.OpenRead(filePath);

        var data = await JsonSerializer.DeserializeAsync<SessionLapData>(
            stream,
            _jsonOptions,
            cancellationToken);

        if (data is null)
            throw new InvalidDataException(
                "The JSON file could not be deserialized.");

        return ConvertToLapRecords(data);
    }

    private static IReadOnlyList<LapRecord> ConvertToLapRecords(
        SessionLapData data)
    {
        var count = data.Time.Count;

        ValidateColumnLength(data.Lap, count, nameof(data.Lap));
        ValidateColumnLength(data.Driver, count, nameof(data.Driver));
        ValidateColumnLength(
            data.DriverNumber,
            count,
            nameof(data.DriverNumber));
        ValidateColumnLength(
            data.Team,
            count,
            nameof(data.Team));
        ValidateColumnLength(
            data.QualifyingSegment,
            count,
            nameof(data.QualifyingSegment));
        ValidateColumnLength(
            data.Deleted,
            count,
            nameof(data.Deleted));
        ValidateColumnLength(
            data.PersonalBest,
            count,
            nameof(data.PersonalBest));
        ValidateColumnLength(
            data.TireCompound,
            count,
            nameof(data.TireCompound));

        var result = new List<LapRecord>(count);

        for (var i = 0; i < count; i++)
        {
            var driver = data.Driver[i];

            if (string.IsNullOrWhiteSpace(driver) ||
                driver.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(new LapRecord
            {
                Driver = driver,
                DriverNumber = data.DriverNumber[i],
                Team = data.Team[i],
                LapNumber = data.Lap[i],
                LapTimeSeconds = data.Time[i],
                Session = ParseSession(data.QualifyingSegment[i]),
                IsDeleted = data.Deleted[i] ?? false,
                IsPersonalBest = data.PersonalBest[i] ?? false,
                TireCompound = data.TireCompound[i],
            });
        }

        return result;
    }

    private static QualifyingSession? ParseSession(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "Q1" => QualifyingSession.Q1,
            "Q2" => QualifyingSession.Q2,
            "Q3" => QualifyingSession.Q3,
            _ => null
        };
    }

    private static void ValidateColumnLength<T>(
        IReadOnlyCollection<T> column,
        int expectedLength,
        string columnName)
    {
        if (column.Count != expectedLength)
        {
            throw new InvalidDataException(
                $"JSON column '{columnName}' contains {column.Count} " +
                $"values but the time column contains {expectedLength}.");
        }
    }
}
