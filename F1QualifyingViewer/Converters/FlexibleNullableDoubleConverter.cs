using System.Text.Json;
using System.Text.Json.Serialization;

namespace QualifyingLapViewer.Converters;

public sealed class FlexibleNullableDoubleConverter : JsonConverter<double?>
{
    public override double? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetDouble(out double value))
                return value;

            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("None", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (double.TryParse(
                    value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        double? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

public sealed class FlexibleNullableDoubleListConverter
    : JsonConverter<List<double?>>
{
    private readonly FlexibleNullableDoubleConverter _itemConverter = new();

    public override List<double?> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected an array.");

        var result = new List<double?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            result.Add(
                _itemConverter.Read(
                    ref reader,
                    typeof(double?),
                    options));
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<double?> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            _itemConverter.Write(writer, item, options);
        }

        writer.WriteEndArray();
    }
}
