using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.EventResult
{
    public class Event
    {
    //     [JsonProperty(propertyName: "links")]
    //     public Links? Links { get; set; }

        [JsonProperty(propertyName: "data")]
        public List<Datum>? Data { get; set; }

    //     [JsonProperty(propertyName: "included")]
    //     public List<object>? Included { get; set; }

    //     [JsonProperty(propertyName: "meta")]
    //     public Meta? Meta { get; set; }
    }

    public class Datum
    {
    //     [JsonProperty(propertyName: "type")]
    //     public string? Type { get; set; }

        [JsonProperty(propertyName: "id")]
        [JsonConverter(converterType: typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty(propertyName: "attributes")]
        public Attributes? Attributes { get; set; }

    //     [JsonProperty(propertyName: "links")]
    //     public Links? Links { get; set; }
    }

    public class Attributes
    {
    //     [JsonProperty(propertyName: "archived_at")]
    //     public object? ArchivedAt { get; set; }

    //     [JsonProperty(propertyName: "created_at")]
    //     public DateTimeOffset CreatedAt { get; set; }

    //     [JsonProperty(propertyName: "enable_services_integration")]
    //     public bool EnableServicesIntegration { get; set; }

    //     [JsonProperty(propertyName: "frequency")]
    //     public string? Frequency { get; set; }

    //     [JsonProperty(propertyName: "integration_key")]
    //     public object? IntegrationKey { get; set; }

    //     [JsonProperty(propertyName: "location_times_enabled")]
    //     public bool LocationTimesEnabled { get; set; }

        [JsonProperty(propertyName: "name")]
        public string? Name { get; set; }

    //     [JsonProperty(propertyName: "pre_select_enabled")]
    //     public bool PreSelectEnabled { get; set; }

    //     [JsonProperty(propertyName: "updated_at")]
    //     public DateTimeOffset UpdatedAt { get; set; }
    }

    // public class Links
    // {
    //     [JsonProperty(propertyName: "self")]
    //     public Uri? Self { get; set; }
    // }

    // public class Meta
    // {
    //     [JsonProperty(propertyName: "total_count")]
    //     public long TotalCount { get; set; }

    //     [JsonProperty(propertyName: "count")]
    //     public long Count { get; set; }

    //     [JsonProperty(propertyName: "can_order_by")]
    //     public List<string>? CanOrderBy { get; set; }

    //     [JsonProperty(propertyName: "can_query_by")]
    //     public List<string>? CanQueryBy { get; set; }

    //     [JsonProperty(propertyName: "can_include")]
    //     public List<string>? CanInclude { get; set; }

    //     [JsonProperty(propertyName: "can_filter")]
    //     public List<string>? CanFilter { get; set; }

    //     [JsonProperty(propertyName: "parent")]
    //     public Parent? Parent { get; set; }
    // }

    // public class Parent
    // {
    //     [JsonProperty(propertyName: "id")]
    //     [JsonConverter(converterType: typeof(ParseStringConverter))]
    //     public long Id { get; set; }

    //     [JsonProperty(propertyName: "type")]
    //     public string? Type { get; set; }
    // }

    // public class Welcome
    // {
    //     // ReSharper disable once UnusedMember.Global
    //     public static Welcome? FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(value: json, settings: Converter.Settings);
    // }

    // // ReSharper disable once UnusedType.Global
    // ReSharper disable once UnusedType.Global
    public static class Serialize
    {
        // ReSharper disable once UnusedMember.Global
        // public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(value: self, settings: Converter.Settings);
    }
    
    // ReSharper disable once UnusedType.Global
    internal static class Converter
    {
        // ReSharper disable once UnusedMember.Global
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
            throw new Exception(message: "Cannot unmarshal type long");
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
    
        // ReSharper disable once UnusedMember.Global
        public static readonly ParseStringConverter Singleton = new();
    }
}
