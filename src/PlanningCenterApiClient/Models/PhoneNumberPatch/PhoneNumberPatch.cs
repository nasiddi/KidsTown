using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.PhoneNumberPatch;

public class PhoneNumber
{
    [JsonProperty("data")] public Data? Data { get; set; }
}

public class Data
{
    [JsonProperty("type")] public string? Type { get; set; }

    [JsonProperty("id")]
    [JsonConverter(typeof(ParseStringConverter))]
    public long? Id { get; set; }

    [JsonProperty("attributes")] public Attributes? Attributes { get; set; }
}

public class Attributes
{
    [JsonProperty("location")] public string Location = "Mobile";

    [JsonProperty("number")] public string? Number { get; set; }
}

public static class Serialize
{
    public static string ToJson(this PhoneNumber self)
    {
        return JsonConvert.SerializeObject(self, Converter.Settings);
    }
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
        }
    };
}

internal class ParseStringConverter : JsonConverter
{
    public override bool CanConvert(Type t)
    {
        return t == typeof(long) || t == typeof(long?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        if (long.TryParse(value, out var l))
        {
            return l;
        }

        throw new Exception("Cannot unmarshal type long");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (long) untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}