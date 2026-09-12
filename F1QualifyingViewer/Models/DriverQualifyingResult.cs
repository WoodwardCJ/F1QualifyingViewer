namespace QualifyingLapViewer.Models;

public sealed class DriverQualifyingResult
{
    public string Driver { get; init; } = string.Empty;

    public string? DriverNumber { get; init; }

    public string? Team { get; init; }

    public TimeSpan? Q1Best { get; init; }

    public TimeSpan? Q2Best { get; init; }

    public TimeSpan? Q3Best { get; init; }

    public QualifyingSession? FinalSession { get; init; }

    public int FinalPosition { get; init; }

    public string? FinalTireCompound { get; init; }
}