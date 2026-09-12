using QualifyingLapViewer.Models;
using QualifyingLapViewer.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace QualifyingLapViewer.Tests.Services;

public sealed class QualifyingAnalysisServiceTests
{
    [Fact]
    public void GetDrivers_ReturnsDistinctDrivers()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("HUL", QualifyingSession.Q1, 81.500),
            CreateLap("HUL", QualifyingSession.Q1, 81.200),
            CreateLap("BOR", QualifyingSession.Q1, 82.000),
            CreateLap("BOR", QualifyingSession.Q2, 81.000)
        };

        var service = new QualifyingAnalysisService(laps);

        var drivers = service.GetDrivers();

        Assert.Equal(
            ["BOR", "HUL"],
            drivers);
    }

    [Fact]
    public void AnalyseDriver_ReturnsFastestValidLapForEachSession()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("HUL", QualifyingSession.Q1, 82.100),
            CreateLap("HUL", QualifyingSession.Q1, 81.500),
            CreateLap("HUL", QualifyingSession.Q1, 81.800),

            CreateLap("HUL", QualifyingSession.Q2, 80.900),
            CreateLap("HUL", QualifyingSession.Q2, 80.500),
            CreateLap("HUL", QualifyingSession.Q2, 80.700),

            CreateLap("HUL", QualifyingSession.Q3, 79.900),
            CreateLap("HUL", QualifyingSession.Q3, 79.500),
            CreateLap("HUL", QualifyingSession.Q3, 79.700)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("HUL");

        Assert.Equal(
            TimeSpan.FromSeconds(81.5),
            result.Q1Best);

        Assert.Equal(
            TimeSpan.FromSeconds(80.5),
            result.Q2Best);

        Assert.Equal(
            TimeSpan.FromSeconds(79.5),
            result.Q3Best);

        Assert.Equal(
            QualifyingSession.Q3,
            result.FinalSession);
    }

    [Fact]
    public void AnalyseDriver_IgnoresDeletedLaps()
    {
        var laps = new List<LapRecord>
        {
            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                80.000,
                isDeleted: true),

            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                81.500),

            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                82.000)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("HUL");

        Assert.Equal(
            TimeSpan.FromSeconds(81.5),
            result.Q1Best);
    }

    [Fact]
    public void AnalyseDriver_ReturnsNullWhenNoValidLapExists()
    {
        var laps = new List<LapRecord>
        {
            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                80.000,
                isDeleted: true),

            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                null)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("HUL");

        Assert.Null(result.Q1Best);
    }

    [Fact]
    public void AnalyseDriver_IsCaseInsensitive()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("HUL", QualifyingSession.Q1, 81.500)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("hul");

        Assert.Equal("HUL", result.Driver);
        Assert.Equal(
            TimeSpan.FromSeconds(81.5),
            result.Q1Best);
    }

    [Fact]
    public void AnalyseDriver_ThrowsForUnknownDriver()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("HUL", QualifyingSession.Q1, 81.500)
        };

        var service = new QualifyingAnalysisService(laps);

        Assert.Throws<KeyNotFoundException>(
            () => service.AnalyseDriver("VER"));
    }

    [Fact]
    public void AnalyseDriver_DriverWhoOnlyReachesQ1_IsClassifiedInQ1()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("BOR", QualifyingSession.Q1, 82.100),
            CreateLap("BOR", QualifyingSession.Q1, 81.900)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("BOR");

        Assert.Equal(
            QualifyingSession.Q1,
            result.FinalSession);
    }

    [Fact]
    public void AnalyseDriver_DriverWhoReachesQ2_IsClassifiedInQ2()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("BOR", QualifyingSession.Q1, 82.100),
            CreateLap("BOR", QualifyingSession.Q2, 81.900)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("BOR");

        Assert.Equal(
            QualifyingSession.Q2,
            result.FinalSession);
    }

    [Fact]
    public void AnalyseDriver_DriverWhoReachesQ3_IsClassifiedInQ3()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("VER", QualifyingSession.Q1, 80.100),
            CreateLap("VER", QualifyingSession.Q2, 79.500),
            CreateLap("VER", QualifyingSession.Q3, 78.900)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("VER");

        Assert.Equal(
            QualifyingSession.Q3,
            result.FinalSession);
    }

    [Fact]
    public void GetClassification_PlacesQ3DriversAheadOfQ2Drivers()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("DRIVER1", QualifyingSession.Q1, 82.000),
            CreateLap("DRIVER1", QualifyingSession.Q2, 81.000),

            CreateLap("DRIVER2", QualifyingSession.Q1, 83.000),
            CreateLap("DRIVER2", QualifyingSession.Q2, 82.000),
            CreateLap("DRIVER2", QualifyingSession.Q3, 80.000)
        };

        var service = new QualifyingAnalysisService(laps);

        var classification = service.GetClassification();

        Assert.Equal(
            "DRIVER2",
            classification[0].Driver);

        Assert.Equal(
            "DRIVER1",
            classification[1].Driver);

        Assert.Equal(
            1,
            classification[0].FinalPosition);

        Assert.Equal(
            2,
            classification[1].FinalPosition);
    }

    [Fact]
    public void GetClassification_OrdersDriversByBestLapWithinSameSession()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("SLOW", QualifyingSession.Q3, 80.500),
            CreateLap("FAST", QualifyingSession.Q3, 79.500),
            CreateLap("MEDIUM", QualifyingSession.Q3, 80.000)
        };

        var service = new QualifyingAnalysisService(laps);

        var classification = service.GetClassification();

        Assert.Equal(
            "FAST",
            classification[0].Driver);

        Assert.Equal(
            "MEDIUM",
            classification[1].Driver);

        Assert.Equal(
            "SLOW",
            classification[2].Driver);
    }

    [Fact]
    public void GetClassification_AssignsSequentialPositions()
    {
        var laps = new List<LapRecord>
        {
            CreateLap("A", QualifyingSession.Q3, 78.0),
            CreateLap("B", QualifyingSession.Q3, 79.0),
            CreateLap("C", QualifyingSession.Q2, 80.0),
            CreateLap("D", QualifyingSession.Q1, 81.0)
        };

        var service = new QualifyingAnalysisService(laps);

        var classification = service.GetClassification();

        Assert.Equal(1, classification[0].FinalPosition);
        Assert.Equal(2, classification[1].FinalPosition);
        Assert.Equal(3, classification[2].FinalPosition);
        Assert.Equal(4, classification[3].FinalPosition);
    }

    [Fact]
    public void AnalyseDriver_PreservesDriverIdentity()
    {
        var laps = new List<LapRecord>
        {
            CreateLap(
                "HUL",
                QualifyingSession.Q1,
                81.500,
                driverNumber: "27",
                team: "Audi")
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("HUL");

        Assert.Equal("HUL", result.Driver);
        Assert.Equal("27", result.DriverNumber);
        Assert.Equal("Audi", result.Team);
    }

    [Fact]
    public void AnalyseDriver_AllowsDeletedQ3LapButUsesValidQ3Lap()
    {
        var laps = new List<LapRecord>
        {
            CreateLap(
                "HUL",
                QualifyingSession.Q3,
                78.000,
                isDeleted: true),

            CreateLap(
                "HUL",
                QualifyingSession.Q3,
                80.000)
        };

        var service = new QualifyingAnalysisService(laps);

        var result = service.AnalyseDriver("HUL");

        Assert.Equal(
            TimeSpan.FromSeconds(80),
            result.Q3Best);

        Assert.Equal(
            QualifyingSession.Q3,
            result.FinalSession);
    }

    private static LapRecord CreateLap(
        string driver,
        QualifyingSession session,
        double? lapTime,
        bool isDeleted = false,
        string? driverNumber = null,
        string? team = null)
    {
        return new LapRecord
        {
            Driver = driver,
            DriverNumber = driverNumber,
            Team = team,
            LapNumber = 1,
            LapTimeSeconds = lapTime,
            Session = session,
            IsDeleted = isDeleted,
            IsPersonalBest = false
        };
    }
}
