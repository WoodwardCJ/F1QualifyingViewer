using System.Text.Json;
using System.Text.Json.Serialization;

namespace QualifyingLapViewer.Converters;

public sealed class FlexibleNullableIntConverter : JsonConverter<int?>
{
    public override int? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int value))
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

            if (int.TryParse(
                    value,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out int parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        int? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

public sealed class FlexibleNullableIntListConverter
    : JsonConverter<List<int?>>
{
    private readonly FlexibleNullableIntConverter _itemConverter = new();

    public override List<int?> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected an array.");

        var result = new List<int?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            result.Add(
                _itemConverter.Read(
                    ref reader,
                    typeof(int?),
                    options));
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<int?> value,
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
