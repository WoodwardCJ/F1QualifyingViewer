using QualifyingLapViewer.Models;

namespace QualifyingLapViewer.Services;

public interface IQualifyingAnalysisService
{
    IReadOnlyList<string> GetDrivers();

    DriverQualifyingResult AnalyseDriver(string driver);

    IReadOnlyList<DriverQualifyingResult> GetClassification();
}
