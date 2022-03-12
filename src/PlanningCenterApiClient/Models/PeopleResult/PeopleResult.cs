using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.PeopleResult;

public class People : IPlanningCenterResponse
{
    [JsonProperty(propertyName: "links")]
    public DatumLinks? Links { get; set; }

    [JsonProperty(propertyName: "data")]
    public List<Datum>? Data { get; set; }

    [JsonProperty(propertyName: "included")]
    public List<Included>? Included { get; set; }

//         //[JsonProperty("meta")]
//         //public Meta Meta { get; set; }
    public Uri? NextLink => Links?.Next;
}

public class Datum
{
//         [JsonProperty("type")]
//         public PeopleIncludedType PeopleIncludedType { get; set; }

    [JsonProperty(propertyName: "id")]
    [JsonConverter(converterType: typeof(ParseStringConverter))]
    public long Id { get; set; }

    [JsonProperty(propertyName: "attributes")]
    public DatumAttributes? Attributes { get; set; }

    [JsonProperty(propertyName: "relationships")]
    public DatumRelationships? Relationships { get; set; }

//         [JsonProperty("links")]
//         public DatumLinks Links { get; set; }
}

public class DatumAttributes
{
//         [JsonProperty("accounting_administrator")]
//         public bool AccountingAdministrator { get; set; }

//         [JsonProperty("anniversary")]
//         public object Anniversary { get; set; }

//         [JsonProperty("avatar")]
//         public Uri Avatar { get; set; }

//         [JsonProperty("birthdate")]
//         public DateTimeOffset Birthdate { get; set; }

//         [JsonProperty("child")]
//         public bool Child { get; set; }

//         [JsonProperty("created_at")]
//         public DateTimeOffset CreatedAt { get; set; }

//         [JsonProperty("demographic_avatar_url")]
//         public Uri DemographicAvatarUrl { get; set; }

//         [JsonProperty("directory_status")]
//         public string DirectoryStatus { get; set; }

    [JsonProperty(propertyName: "first_name")]
    public string? FirstName { get; set; }

//         [JsonProperty("gender")]
//         public string Gender { get; set; }

//         [JsonProperty("given_name")]
//         public object GivenName { get; set; }

//         [JsonProperty("grade")]
//         public long? Grade { get; set; }

//         [JsonProperty("graduation_year")]
//         public object GraduationYear { get; set; }

//         [JsonProperty("inactivated_at")]
//         public object InactivatedAt { get; set; }

    [JsonProperty(propertyName: "last_name")]
    public string? LastName { get; set; }

//         [JsonProperty("medical_notes")]
//         public object MedicalNotes { get; set; }

//         [JsonProperty("membership")]
//         public string Membership { get; set; }

//         [JsonProperty("middle_name")]
//         public string MiddleName { get; set; }

    // [JsonProperty("name")]
    // public string? Name { get; set; }

//         [JsonProperty("nickname")]
//         public object Nickname { get; set; }

//         [JsonProperty("passed_background_check")]
//         public bool PassedBackgroundCheck { get; set; }

//         [JsonProperty("people_permissions")]
//         public object PeoplePermissions { get; set; }

//         [JsonProperty("remote_id")]
//         public long RemoteId { get; set; }

//         [JsonProperty("school_type")]
//         public object SchoolType { get; set; }

//         [JsonProperty("site_administrator")]
//         public bool SiteAdministrator { get; set; }

//         [JsonProperty("status")]
//         public string Status { get; set; }

//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }
}

public class DatumLinks
{
    // [JsonProperty("self")]
    // public Uri? Self { get; set; }
         
    [JsonProperty(propertyName: "next")]
    public Uri? Next { get; set; }
         
    // [JsonProperty("prev")]
    // public Uri? Prev { get; set; }
}

public class DatumRelationships
{
//         [JsonProperty("primary_campus")]
//         public PrimaryCampus PrimaryCampus { get; set; }

    [JsonProperty(propertyName: "field_data")]
    public DatumRelationship? FieldData { get; set; }
         
    [JsonProperty(propertyName: "households")]
    public DatumRelationship? Households { get; set; }
         
    [JsonProperty(propertyName: "phone_numbers")]
    public DatumRelationship? PhoneNumbers { get; set; }
}

public class DatumRelationship
{
//         [JsonProperty("links")]
//         public FieldDataLinks Links { get; set; }

    [JsonProperty(propertyName: "data")]
    public List<Parent>? Data { get; set; }
}

public class Parent
{
//         [JsonProperty("type")]
//         public PeopleIncludedType PeopleIncludedType { get; set; }

    [JsonProperty(propertyName: "id")]
    [JsonConverter(converterType: typeof(ParseStringConverter))]
    public long Id { get; set; }
}

//     public class FieldDataLinks
//     {
//         [JsonProperty("related")]
//         public Uri Related { get; set; }
//     }

public class Relationship
{
    [JsonProperty(propertyName: "data")]
    public Parent? Data { get; set; }
}

public class Included
{
    [JsonProperty(propertyName: "type")]
    public PeopleIncludedType PeopleIncludedType { get; set; }

    [JsonProperty(propertyName: "id")]
    [JsonConverter(converterType: typeof(ParseStringConverter))]
    public long Id { get; set; }

    [JsonProperty(propertyName: "attributes")]
    public IncludedAttributes? Attributes { get; set; }

    [JsonProperty(propertyName: "relationships")]
    public IncludedRelationships? Relationships { get; set; }

//         [JsonProperty("links")]
//         public DatumLinks Links { get; set; }
}

public class IncludedAttributes
{
//         [JsonProperty("file")]
//         public File File { get; set; }

//         [JsonProperty("file_content_type")]
//         public string FileContentType { get; set; }

//         [JsonProperty("file_name")]
//         public string FileName { get; set; }

//         [JsonProperty("file_size")]
//         public long? FileSize { get; set; }

    [JsonProperty(propertyName: "value")]
    public string? Value { get; set; }
         
    [JsonProperty(propertyName: "name")]
    public string? Name { get; set; }
         
    [JsonProperty(propertyName: "number")]
    public string? Number { get; set; }
         
    [JsonProperty(propertyName: "location")]
    public string? NumberType { get; set; }
         
    [JsonProperty(propertyName: "primary")]
    public bool? Primary { get; set; }
}

//     public class File
//     {
//         [JsonProperty("url")]
//         public Uri Url { get; set; }
//     }

public class IncludedRelationships
{
    [JsonProperty(propertyName: "field_definition")]
    public Relationship? FieldDefinition { get; set; }

//         [JsonProperty("customizable")]
//         public PrimaryCampus Customizable { get; set; }
}

//     public class Meta
//     {
//         [JsonProperty("total_count")]
//         public long TotalCount { get; set; }

//         [JsonProperty("count")]
//         public long Count { get; set; }

//         [JsonProperty("can_order_by")]
//         public List<string> CanOrderBy { get; set; }

//         [JsonProperty("can_query_by")]
//         public List<string> CanQueryBy { get; set; }

//         [JsonProperty("can_include")]
//         public List<string> CanInclude { get; set; }

//         [JsonProperty("can_filter")]
//         public List<string> CanFilter { get; set; }

//         [JsonProperty("parent")]
//         public Parent Parent { get; set; }
//     }

public enum PeopleIncludedType { FieldDatum, FieldDefinition, Organization, Person, PrimaryCampus, Household, PhoneNumber }

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
            TypeEnumConverter.Singleton,
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

    // ReSharper disable once UnusedMember.Global
    public static readonly ParseStringConverter Singleton = new();
}

internal class TypeEnumConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(PeopleIncludedType) || t == typeof(PeopleIncludedType?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader: reader);
        return value switch
        {
            "FieldDatum" => PeopleIncludedType.FieldDatum,
            "FieldDefinition" => PeopleIncludedType.FieldDefinition,
            "Organization" => PeopleIncludedType.Organization,
            "Person" => PeopleIncludedType.Person,
            "PrimaryCampus" => PeopleIncludedType.PrimaryCampus,
            "Household" => PeopleIncludedType.Household,
            "PhoneNumber" => PeopleIncludedType.PhoneNumber,
            _ => throw new(message: "Cannot unmarshal type TypeEnum")
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(jsonWriter: writer, value: null);
            return;
        }
        var value = (PeopleIncludedType)untypedValue;
        switch (value)
        {
            case PeopleIncludedType.FieldDatum:
                serializer.Serialize(jsonWriter: writer, value: "FieldDatum");
                return;
            case PeopleIncludedType.FieldDefinition:
                serializer.Serialize(jsonWriter: writer, value: "FieldDefinition");
                return;
            case PeopleIncludedType.Organization:
                serializer.Serialize(jsonWriter: writer, value: "Organization");
                return;
            case PeopleIncludedType.Person:
                serializer.Serialize(jsonWriter: writer, value: "Person");
                return;
            case PeopleIncludedType.PrimaryCampus:
                serializer.Serialize(jsonWriter: writer, value: "PrimaryCampus");
                return;
            case PeopleIncludedType.Household:
                serializer.Serialize(jsonWriter: writer, value: "Household");
                return;
            case PeopleIncludedType.PhoneNumber:
                serializer.Serialize(jsonWriter: writer, value: "PhoneNumber");
                return;
            default:
                throw new(message: "Cannot marshal type TypeEnum");
        }
    }

    public static readonly TypeEnumConverter Singleton = new();
}