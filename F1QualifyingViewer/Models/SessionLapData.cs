using System.Text.Json.Serialization;
using QualifyingLapViewer.Converters;

namespace QualifyingLapViewer.Models;

public sealed class SessionLapData
{
    [JsonPropertyName("time")]
    [JsonConverter(typeof(FlexibleNullableDoubleListConverter))]
    public List<double?> Time { get; set; } = [];

    [JsonPropertyName("lap")]
    [JsonConverter(typeof(FlexibleNullableIntListConverter))]
    public List<int?> Lap { get; set; } = [];

    [JsonPropertyName("drv")]
    public List<string?> Driver { get; set; } = [];

    [JsonPropertyName("dNum")]
    public List<string?> DriverNumber { get; set; } = [];

    [JsonPropertyName("team")]
    public List<string?> Team { get; set; } = [];

    [JsonPropertyName("qs")]
    public List<string?> QualifyingSegment { get; set; } = [];

    [JsonPropertyName("del")]
    [JsonConverter(typeof(FlexibleNullableBoolListConverter))]
    public List<bool?> Deleted { get; set; } = [];

    [JsonPropertyName("pb")]
    [JsonConverter(typeof(FlexibleNullableBoolListConverter))]
    public List<bool?> PersonalBest { get; set; } = [];

    [JsonPropertyName("compound")]
    public List<string?> TireCompound { get; set; } = [];
}