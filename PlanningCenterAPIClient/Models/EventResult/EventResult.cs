#pragma warning disable 8618
namespace CheckInsExtension.PlanningCenterAPIClient.Models.EventResult
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class EventResult
    {
        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("data")]
        public List<Datum> Data { get; set; }

        [JsonProperty("included")]
        public List<object> Included { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class Datum
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("archived_at")]
        public object ArchivedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("enable_services_integration")]
        public bool EnableServicesIntegration { get; set; }

        [JsonProperty("frequency")]
        public string Frequency { get; set; }

        [JsonProperty("integration_key")]
        public object IntegrationKey { get; set; }

        [JsonProperty("location_times_enabled")]
        public bool LocationTimesEnabled { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pre_select_enabled")]
        public bool PreSelectEnabled { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class Links
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }
    }

    public class Meta
    {
        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("can_order_by")]
        public List<string> CanOrderBy { get; set; }

        [JsonProperty("can_query_by")]
        public List<string> CanQueryBy { get; set; }

        [JsonProperty("can_include")]
        public List<string> CanInclude { get; set; }

        [JsonProperty("can_filter")]
        public List<string> CanFilter { get; set; }

        [JsonProperty("parent")]
        public Parent Parent { get; set; }
    }

    public class Parent
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
