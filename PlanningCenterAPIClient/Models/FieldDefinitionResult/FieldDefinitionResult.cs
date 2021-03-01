using System;
using System.Collections.Generic;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using Newtonsoft.Json;
#pragma warning disable 8618

namespace CheckInsExtension.PlanningCenterAPIClient.Models.FieldDefinitionResult
{
    public class FieldDefinitions
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

        [JsonProperty("relationships")]
        public Relationships Relationships { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("config")]
        public object Config { get; set; }

        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("deleted_at")]
        public object DeletedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("tab_id")]
        public long TabId { get; set; }
    }

    public class Links
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }
    }

    public class Relationships
    {
        [JsonProperty("tab")]
        public Tab Tab { get; set; }
    }

    public class Tab
    {
        [JsonProperty("data")]
        public Parent Data { get; set; }
    }

    public class Parent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
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
}
