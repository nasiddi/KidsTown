using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#pragma warning disable 8618

namespace CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult
{
     public class People
     {
//         [JsonProperty("links")]
//         public DatumLinks Links { get; set; }
//
         [JsonProperty("data")]
         public List<Datum> Data { get; set; }

         [JsonProperty("included")]
         public List<Included> Included { get; set; }
//
//         //[JsonProperty("meta")]
//         //public Meta Meta { get; set; }
     }
//
     public class Datum
     {
//         [JsonProperty("type")]
//         public PeopleIncludedType PeopleIncludedType { get; set; }
//
         [JsonProperty("id")]
         [JsonConverter(typeof(ParseStringConverter))]
         public long Id { get; set; }
//
         [JsonProperty("attributes")]
         public DatumAttributes Attributes { get; set; }
//
         [JsonProperty("relationships")]
         public DatumRelationships Relationships { get; set; }
//
//         [JsonProperty("links")]
//         public DatumLinks Links { get; set; }
     }
//
     public class DatumAttributes
     {
//         [JsonProperty("accounting_administrator")]
//         public bool AccountingAdministrator { get; set; }
//
//         [JsonProperty("anniversary")]
//         public object Anniversary { get; set; }
//
//         [JsonProperty("avatar")]
//         public Uri Avatar { get; set; }
//
//         [JsonProperty("birthdate")]
//         public DateTimeOffset Birthdate { get; set; }
//
//         [JsonProperty("child")]
//         public bool Child { get; set; }
//
//         [JsonProperty("created_at")]
//         public DateTimeOffset CreatedAt { get; set; }
//
//         [JsonProperty("demographic_avatar_url")]
//         public Uri DemographicAvatarUrl { get; set; }
//
//         [JsonProperty("directory_status")]
//         public string DirectoryStatus { get; set; }
//
         [JsonProperty("first_name")]
         public string FirstName { get; set; }
//
//         [JsonProperty("gender")]
//         public string Gender { get; set; }
//
//         [JsonProperty("given_name")]
//         public object GivenName { get; set; }
//
//         [JsonProperty("grade")]
//         public long? Grade { get; set; }
//
//         [JsonProperty("graduation_year")]
//         public object GraduationYear { get; set; }
//
//         [JsonProperty("inactivated_at")]
//         public object InactivatedAt { get; set; }
//
         [JsonProperty("last_name")]
         public string LastName { get; set; }
//
//         [JsonProperty("medical_notes")]
//         public object MedicalNotes { get; set; }
//
//         [JsonProperty("membership")]
//         public string Membership { get; set; }
//
//         [JsonProperty("middle_name")]
//         public string MiddleName { get; set; }
//
//         [JsonProperty("name")]
//         public string Name { get; set; }
//
//         [JsonProperty("nickname")]
//         public object Nickname { get; set; }
//
//         [JsonProperty("passed_background_check")]
//         public bool PassedBackgroundCheck { get; set; }
//
//         [JsonProperty("people_permissions")]
//         public object PeoplePermissions { get; set; }
//
//         [JsonProperty("remote_id")]
//         public long RemoteId { get; set; }
//
//         [JsonProperty("school_type")]
//         public object SchoolType { get; set; }
//
//         [JsonProperty("site_administrator")]
//         public bool SiteAdministrator { get; set; }
//
//         [JsonProperty("status")]
//         public string Status { get; set; }
//
//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }
     }
//
//     public class DatumLinks
//     {
//         [JsonProperty("self")]
//         public Uri Self { get; set; }
//     }

     public class DatumRelationships
     {
//         [JsonProperty("primary_campus")]
//         public PrimaryCampus PrimaryCampus { get; set; }
//
         [JsonProperty("field_data")]
         public FieldData FieldData { get; set; }
     }
//
     public class FieldData
     {
//         [JsonProperty("links")]
//         public FieldDataLinks Links { get; set; }
//
         [JsonProperty("data")]
         public List<Parent> Data { get; set; }
     }
//
     public class Parent
     {
//         [JsonProperty("type")]
//         public PeopleIncludedType PeopleIncludedType { get; set; }
//
         [JsonProperty("id")]
         [JsonConverter(typeof(ParseStringConverter))]
         public long Id { get; set; }
     }
//
//     public class FieldDataLinks
//     {
//         [JsonProperty("related")]
//         public Uri Related { get; set; }
//     }
//
     public class PrimaryCampus
     {
         [JsonProperty("data")]
         public Parent Data { get; set; }
     }
//
     public class Included
     {
         [JsonProperty("type")]
         public PeopleIncludedType PeopleIncludedType { get; set; }
//
         [JsonProperty("id")]
         [JsonConverter(typeof(ParseStringConverter))]
         public long Id { get; set; }
//
         [JsonProperty("attributes")]
         public IncludedAttributes Attributes { get; set; }
//
         [JsonProperty("relationships")]
         public IncludedRelationships Relationships { get; set; }
//
//         [JsonProperty("links")]
//         public DatumLinks Links { get; set; }
     }
//
     public class IncludedAttributes
     {
//         [JsonProperty("file")]
//         public File File { get; set; }
//
//         [JsonProperty("file_content_type")]
//         public string FileContentType { get; set; }
//
//         [JsonProperty("file_name")]
//         public string FileName { get; set; }
//
//         [JsonProperty("file_size")]
//         public long? FileSize { get; set; }
//
         [JsonProperty("value")]
         public string Value { get; set; }
     }
//
//     public class File
//     {
//         [JsonProperty("url")]
//         public Uri Url { get; set; }
//     }
//
     public class IncludedRelationships
     {
         [JsonProperty("field_definition")]
         public PrimaryCampus FieldDefinition { get; set; }
//
//         [JsonProperty("customizable")]
//         public PrimaryCampus Customizable { get; set; }
     }
//
//     public class Meta
//     {
//         [JsonProperty("total_count")]
//         public long TotalCount { get; set; }
//
//         [JsonProperty("count")]
//         public long Count { get; set; }
//
//         [JsonProperty("can_order_by")]
//         public List<string> CanOrderBy { get; set; }
//
//         [JsonProperty("can_query_by")]
//         public List<string> CanQueryBy { get; set; }
//
//         [JsonProperty("can_include")]
//         public List<string> CanInclude { get; set; }
//
//         [JsonProperty("can_filter")]
//         public List<string> CanFilter { get; set; }
//
//         [JsonProperty("parent")]
//         public Parent Parent { get; set; }
//     }
//
     public enum PeopleIncludedType { FieldDatum, FieldDefinition, Organization, Person, PrimaryCampus };

     internal static class Converter
     {
         public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
         {
             MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
             DateParseHandling = DateParseHandling.None,
             Converters =
             {
                 TypeEnumConverter.Singleton,
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

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(PeopleIncludedType) || t == typeof(PeopleIncludedType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "FieldDatum":
                    return PeopleIncludedType.FieldDatum;
                case "FieldDefinition":
                    return PeopleIncludedType.FieldDefinition;
                case "Organization":
                    return PeopleIncludedType.Organization;
                case "Person":
                    return PeopleIncludedType.Person;
                case "PrimaryCampus":
                    return PeopleIncludedType.PrimaryCampus;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (PeopleIncludedType)untypedValue;
            switch (value)
            {
                case PeopleIncludedType.FieldDatum:
                    serializer.Serialize(writer, "FieldDatum");
                    return;
                case PeopleIncludedType.FieldDefinition:
                    serializer.Serialize(writer, "FieldDefinition");
                    return;
                case PeopleIncludedType.Organization:
                    serializer.Serialize(writer, "Organization");
                    return;
                case PeopleIncludedType.Person:
                    serializer.Serialize(writer, "Person");
                    return;
                case PeopleIncludedType.PrimaryCampus:
                    serializer.Serialize(writer, "PrimaryCampus");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
     }
 }
