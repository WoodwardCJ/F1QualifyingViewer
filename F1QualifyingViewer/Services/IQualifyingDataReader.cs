using QualifyingLapViewer.Models;

namespace QualifyingLapViewer.Services;

public interface IQualifyingDataReader
{
    Task<IReadOnlyList<LapRecord>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
