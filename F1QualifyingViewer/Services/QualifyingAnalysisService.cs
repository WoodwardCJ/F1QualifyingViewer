using QualifyingLapViewer.Models;

namespace QualifyingLapViewer.Services;

public sealed class QualifyingAnalysisService : IQualifyingAnalysisService
{
    private readonly IReadOnlyList<LapRecord> _laps;

    public QualifyingAnalysisService(
        IReadOnlyList<LapRecord> laps)
    {
        _laps = laps ??
            throw new ArgumentNullException(nameof(laps));
    }

    public IReadOnlyList<string> GetDrivers()
    {
        return _laps
            .Select(x => x.Driver)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    public DriverQualifyingResult AnalyseDriver(string driver)
    {
        if (string.IsNullOrWhiteSpace(driver))
        {
            throw new ArgumentException(
                "Driver code is required.",
                nameof(driver));
        }

        var matchingDriver = GetDrivers()
            .FirstOrDefault(x =>
                x.Equals(
                    driver.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (matchingDriver is null)
        {
            throw new KeyNotFoundException(
                $"Driver '{driver}' was not found in the dataset.");
        }

        var classification = GetClassification();

        var result = classification.FirstOrDefault(x =>
            x.Driver.Equals(
                matchingDriver,
                StringComparison.OrdinalIgnoreCase));

        if (result is null)
        {
            throw new KeyNotFoundException(
                $"Driver '{driver}' was not found in the classification.");
        }

        return result;
    }

    public IReadOnlyList<DriverQualifyingResult> GetClassification()
    {
        var drivers = _laps
            .GroupBy(
                x => x.Driver,
                StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                BuildDriverResult(
                    group.Key,
                    group.ToList(),
                    0))
            .ToList();

        var ordered = drivers
            .OrderByDescending(x => SegmentRank(x.FinalSession))
            .ThenBy(x => BestTimeForFinalSession(x))
            .ThenBy(x => x.Driver, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<DriverQualifyingResult>(
            ordered.Count);

        for (var i = 0; i < ordered.Count; i++)
        {
            var item = ordered[i];

            results.Add(new DriverQualifyingResult
            {
                Driver = item.Driver,
                DriverNumber = item.DriverNumber,
                Team = item.Team,
                Q1Best = item.Q1Best,
                Q2Best = item.Q2Best,
                Q3Best = item.Q3Best,
                FinalSession = item.FinalSession,
                FinalPosition = i + 1,
                FinalTireCompound = item.FinalTireCompound
            });
        }

        return results;
    }

    private DriverQualifyingResult BuildDriverResult(
        string driver,
        IReadOnlyList<LapRecord> driverLaps,
        int position)
    {
        var q1 = GetBestValidLap(
            driverLaps,
            QualifyingSession.Q1);

        var q2 = GetBestValidLap(
            driverLaps,
            QualifyingSession.Q2);

        var q3 = GetBestValidLap(
            driverLaps,
            QualifyingSession.Q3);

        var (finalSession, tireCompond) = DetermineFinalSession(
            driverLaps,
            q1,
            q2,
            q3);

        var identity = driverLaps.FirstOrDefault();

        return new DriverQualifyingResult
        {
            Driver = driver,
            DriverNumber = identity?.DriverNumber,
            Team = identity?.Team,
            Q1Best = ToTimeSpan(q1?.LapTimeSeconds),
            Q2Best = ToTimeSpan(q2?.LapTimeSeconds),
            Q3Best = ToTimeSpan(q3?.LapTimeSeconds),
            FinalSession = finalSession,
            FinalPosition = position,
            FinalTireCompound = tireCompond
        };
    }

    private static LapRecord? GetBestValidLap(
        IEnumerable<LapRecord> laps,
        QualifyingSession session)
    {
        return laps
            .Where(x =>
                x.Session == session &&
                x.IsValid)
            .OrderBy(x => x.LapTimeSeconds)
            .FirstOrDefault();
    }

    private static (QualifyingSession?, string?) DetermineFinalSession(
        IEnumerable<LapRecord> driverLaps,
        LapRecord? q1,
        LapRecord? q2,
        LapRecord? q3)
    {
        // A driver belongs to the highest qualifying segment
        // represented in the data. We use both valid and invalid
        // laps here so a driver who entered a segment but failed
        // to record a valid lap is still classified in that segment.

        var sessions = driverLaps
            .Where(x => x.Session.HasValue)
            .Select(x => x.Session!.Value)
            .Distinct()
            .ToList();

        if (sessions.Contains(QualifyingSession.Q3))
            return (QualifyingSession.Q3, q3?.TireCompound);

        if (sessions.Contains(QualifyingSession.Q2))
            return (QualifyingSession.Q2, q2?.TireCompound);

        if (sessions.Contains(QualifyingSession.Q1))
            return (QualifyingSession.Q1, q1?.TireCompound);

        // Defensive fallback for unusual data.
        if (q3 is not null)
            return (QualifyingSession.Q3, q3?.TireCompound);

        if (q2 is not null)
            return (QualifyingSession.Q2, q2?.TireCompound);

        if (q1 is not null)
            return (QualifyingSession.Q1, q1?.TireCompound);

        return (null,null);
    }

    private static int SegmentRank(
        QualifyingSession? session)
    {
        return session switch
        {
            QualifyingSession.Q3 => 3,
            QualifyingSession.Q2 => 2,
            QualifyingSession.Q1 => 1,
            _ => 0
        };
    }

    private static TimeSpan BestTimeForFinalSession(
        DriverQualifyingResult result)
    {
        return result.FinalSession switch
        {
            QualifyingSession.Q3 =>
                result.Q3Best ?? TimeSpan.MaxValue,

            QualifyingSession.Q2 =>
                result.Q2Best ?? TimeSpan.MaxValue,

            QualifyingSession.Q1 =>
                result.Q1Best ?? TimeSpan.MaxValue,

            _ => TimeSpan.MaxValue
        };
    }

    private static TimeSpan? ToTimeSpan(double? seconds)
    {
        if (!seconds.HasValue || seconds.Value <= 0)
            return null;

        return TimeSpan.FromSeconds(seconds.Value);
    }
}
