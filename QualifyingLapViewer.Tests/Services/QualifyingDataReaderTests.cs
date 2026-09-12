using QualifyingLapViewer.Services;
using System.Text.Json;
using Xunit;
using Assert = Xunit.Assert;

namespace QualifyingLapViewer.Tests.Services;

public sealed class QualifyingDataReaderTests : IDisposable
{
    private readonly string _temporaryDirectory;

    public QualifyingDataReaderTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "QualifyingLapViewerTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task ReadAsync_ReadsColumnOrientedJson()
    {
        var json = """
        {
          "time": [81.5, 80.5, 79.5],
          "lap": [1, 2, 3],
          "drv": ["HUL", "HUL", "HUL"],
          "dNum": ["27", "27", "27"],
          "team": ["Audi", "Audi", "Audi"],
          "qs": ["Q1", "Q2", "Q3"],
          "del": [false, false, false],
          "pb": [true, true, true],
          "compound": ["SOFT", "SOFT",  "SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "test.json",
            json);

        var reader = new QualifyingDataReader();

        var result = await reader.ReadAsync(file);

        Assert.Equal(3, result.Count);

        Assert.Equal("HUL", result[0].Driver);
        Assert.Equal("27", result[0].DriverNumber);
        Assert.Equal("Audi", result[0].Team);
        Assert.Equal("SOFT", result[0].TireCompound);

        Assert.Equal(81.5, result[0].LapTimeSeconds);
        Assert.Equal(80.5, result[1].LapTimeSeconds);
        Assert.Equal(79.5, result[2].LapTimeSeconds);

        Assert.Equal(
            Models.QualifyingSession.Q1,
            result[0].Session);

        Assert.Equal(
            Models.QualifyingSession.Q2,
            result[1].Session);

        Assert.Equal(
            Models.QualifyingSession.Q3,
            result[2].Session);
    }

    [Fact]
    public async Task ReadAsync_HandlesNoneValues()
    {
        var json = """
        {
          "time": [81.5, "None", null],
          "lap": [1, "None", null],
          "drv": ["HUL", "HUL", "HUL"],
          "dNum": ["27", "27", "27"],
          "team": ["Audi", "Audi", "Audi"],
          "qs": ["Q1", "Q2", "Q3"],
          "del": [false, "None", null],
          "pb": [true, "None", null],
          "compound": ["SOFT", "SOFT", "SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "none-values.json",
            json);

        var reader = new QualifyingDataReader();

        var result = await reader.ReadAsync(file);

        Assert.Equal(3, result.Count);

        Assert.Equal(
            81.5,
            result[0].LapTimeSeconds);

        Assert.Null(
            result[1].LapTimeSeconds);

        Assert.Null(
            result[2].LapTimeSeconds);

        Assert.Null(
            result[1].LapNumber);

        Assert.Null(
            result[2].LapNumber);

        Assert.False(result[1].IsDeleted);
        Assert.False(result[2].IsDeleted);
    }

    [Fact]
    public async Task ReadAsync_HandlesDeletedLaps()
    {
        var json = """
        {
          "time": [80.0, 81.0],
          "lap": [1, 2],
          "drv": ["HUL", "HUL"],
          "dNum": ["27", "27"],
          "team": ["Audi", "Audi"],
          "qs": ["Q1", "Q1"],
          "del": [true, false],
          "pb": [false, true],
          "compound": ["SOFT", "SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "deleted.json",
            json);

        var reader = new QualifyingDataReader();

        var result = await reader.ReadAsync(file);

        Assert.True(result[0].IsDeleted);
        Assert.False(result[1].IsDeleted);
    }

    [Fact]
    public async Task ReadAsync_ThrowsWhenFileDoesNotExist()
    {
        var reader = new QualifyingDataReader();

        var file = Path.Combine(
            _temporaryDirectory,
            "does-not-exist.json");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => reader.ReadAsync(file));
    }

    [Fact]
    public async Task ReadAsync_ThrowsForMismatchedColumnLengths()
    {
        var json = """
        {
          "time": [81.5, 80.5],
          "lap": [1],
          "drv": ["HUL", "HUL"],
          "dNum": ["27", "27"],
          "team": ["Audi", "Audi"],
          "qs": ["Q1", "Q2"],
          "del": [false, false],
          "pb": [true, true],
          "compound": ["SOFT", "SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "invalid-columns.json",
            json);

        var reader = new QualifyingDataReader();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => reader.ReadAsync(file));
    }

    [Fact]
    public async Task ReadAsync_IgnoresDriversWithMissingDriverCode()
    {
        var json = """
        {
          "time": [81.5, 80.5],
          "lap": [1, 2],
          "drv": ["HUL", "None"],
          "dNum": ["27", "27"],
          "team": ["Audi", "Audi"],
          "qs": ["Q1", "Q1"],
          "del": [false, false],
          "pb": [true, true],
          "compound": ["SOFT", "SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "missing-driver.json",
            json);

        var reader = new QualifyingDataReader();

        var result = await reader.ReadAsync(file);

        Assert.Single(result);
        Assert.Equal("HUL", result[0].Driver);
    }

    [Fact]
    public async Task ReadAsync_IgnoresUnknownJsonProperties()
    {
        var json = """
        {
          "time": [81.5],
          "lap": [1],
          "drv": ["HUL"],
          "dNum": ["27"],
          "team": ["Audi"],
          "qs": ["Q1"],
          "del": [false],
          "pb": [true],
          "someFutureField": ["something"],
          "anotherField": 123,
          "compound": ["SOFT"]
        }
        """;

        var file = await WriteFileAsync(
            "additional-properties.json",
            json);

        var reader = new QualifyingDataReader();

        var result = await reader.ReadAsync(file);

        Assert.Single(result);
        Assert.Equal(
            81.5,
            result[0].LapTimeSeconds);
    }

    private async Task<string> WriteFileAsync(
        string fileName,
        string contents)
    {
        var file = Path.Combine(
            _temporaryDirectory,
            fileName);

        await File.WriteAllTextAsync(
            file,
            contents);

        return file;
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(
                _temporaryDirectory,
                recursive: true);
        }
    }
}
