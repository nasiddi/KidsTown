using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.PhoneNumberPatch;

public class PhoneNumber
{
    [JsonProperty(propertyName: "data")]
    public Data? Data { get; set; }
}

public class Data
{
    [JsonProperty(propertyName: "type")]
    public string? Type { get; set; }

    [JsonProperty(propertyName: "id")]
    [JsonConverter(converterType: typeof(ParseStringConverter))]
    public long? Id { get; set; }

    [JsonProperty(propertyName: "attributes")]
    public Attributes? Attributes { get; set; }
}

public class Attributes
{
    [JsonProperty(propertyName: "number")]
    public string? Number { get; set; }

    [JsonProperty(propertyName: "location")]
    public string Location = "Mobile";
}

public static class Serialize
{
    public static string ToJson(this PhoneNumber self) => JsonConvert.SerializeObject(value: self, settings: Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        }
    };
}

internal class ParseStringConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader: reader);
        if (long.TryParse(s: value, result: out var l))
        {
            return l;
        }
        throw new(message: "Cannot unmarshal type long");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(jsonWriter: writer, value: null);
            return;
        }
        var value = (long)untypedValue;
        serializer.Serialize(jsonWriter: writer, value: value.ToString());
    }
}