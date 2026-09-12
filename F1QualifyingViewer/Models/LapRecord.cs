namespace QualifyingLapViewer.Models;

public sealed class LapRecord
{
    public string Driver { get; init; } = string.Empty;

    public string? DriverNumber { get; init; }

    public string? Team { get; init; }

    public int? LapNumber { get; init; }

    public double? LapTimeSeconds { get; init; }

    public QualifyingSession? Session { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsPersonalBest { get; init; }

    public string? TireCompound { get; init; }

    public bool IsValid =>
        LapTimeSeconds.HasValue &&
        LapTimeSeconds.Value > 0 &&
        !IsDeleted;
}