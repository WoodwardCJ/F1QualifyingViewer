using System.Text.Json;
using System.Text.Json.Serialization;

namespace QualifyingLapViewer.Converters;

public sealed class FlexibleNullableBoolConverter : JsonConverter<bool?>
{
    public override bool? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("None", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (bool.TryParse(value, out bool parsed))
                return parsed;

            if (value == "1")
                return true;

            if (value == "0")
                return false;
        }

        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        bool? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteBooleanValue(value.Value);
        else
            writer.WriteNullValue();
    }
}

public sealed class FlexibleNullableBoolListConverter
    : JsonConverter<List<bool?>>
{
    private readonly FlexibleNullableBoolConverter _itemConverter = new();

    public override List<bool?> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected an array.");

        var result = new List<bool?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            result.Add(
                _itemConverter.Read(
                    ref reader,
                    typeof(bool?),
                    options));
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<bool?> value,
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
