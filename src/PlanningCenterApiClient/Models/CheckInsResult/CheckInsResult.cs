﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

public class CheckIns : IPlanningCenterResponse
{
    [JsonProperty("links")] public AttendeeLinks? Links { get; set; }

    [JsonProperty("data")] public List<Attendee>? Attendees { get; set; }

    [JsonProperty("included")] public List<Included>? Included { get; set; }

//         [JsonProperty("meta")]
//         public Meta Meta { get; set; }

    public Uri? NextLink => Links?.Next;
}

public class Attendee
{
//         [JsonProperty("type")]
//         public string Type { get; set; }

    [JsonProperty("id")]
    [JsonConverter(typeof(ParseStringConverter))]
    public long Id { get; set; }

    [JsonProperty("attributes")] public AttendeeAttributes? Attributes { get; set; }

//         [JsonProperty("links")]
//         public AttendeeLinks Links { get; set; }

    [JsonProperty("relationships")] public AttendeeRelationships? Relationships { get; set; }
}

public class AttendeeAttributes
{
//         [JsonProperty("checked_out_at")]
//         public DateTimeOffset? CheckedOutAt { get; set; }

    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }

    [JsonProperty("emergency_contact_name")]
    public string? EmergencyContactName { get; set; }

    [JsonProperty("emergency_contact_phone_number")]
    public string? EmergencyContactPhoneNumber { get; set; }

    [JsonProperty("first_name")] public string? FirstName { get; set; }

    [JsonProperty("kind")] public AttendeeType Kind { get; set; }

    [JsonProperty("last_name")] public string? LastName { get; set; }

//         [JsonProperty("medical_notes")]
//         public object MedicalNotes { get; set; }

//         [JsonProperty("number")]
//         public long Number { get; set; }

    [JsonProperty("security_code")] public string? SecurityCode { get; set; }

//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }
}

public class AttendeeLinks
{
    // [JsonProperty("self")]
    // public Uri? Self { get; set; }

    [JsonProperty("next")] public Uri? Next { get; set; }

    // [JsonProperty("prev")]
    // public Uri? Prev { get; set; }
}

public class AttendeeRelationships
{
    [JsonProperty("locations")] public CheckInsLocations? Locations { get; set; }

    [JsonProperty("person")] public Relationship? Person { get; set; }

    [JsonProperty("event")] public Relationship? Event { get; set; }
}

public class Relationship
{
//         [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
//         public EventLinks Links { get; set; }

    [JsonProperty("data")] public ParentElement? Data { get; set; }
}

public class ParentElement
{
//         [JsonProperty("type")]
//         public IncludeType Type { get; set; }

    [JsonProperty("id")]
    [JsonConverter(typeof(ParseStringConverter))]
    public long Id { get; set; }
}

//     public class EventLinks
//     {
//         [JsonProperty("related")]
//         public Uri Related { get; set; }
//     }

public class CheckInsLocations
{
//         [JsonProperty("links")]
//         public EventLinks Links { get; set; }

    [JsonProperty("data")] public List<ParentElement>? Data { get; set; }
}

public class Included
{
    [JsonProperty("type")] public IncludeType Type { get; set; }

    [JsonProperty("id")]
    [JsonConverter(typeof(ParseStringConverter))]
    public long Id { get; set; }

    [JsonProperty("attributes")] public IncludedAttributes? Attributes { get; set; }

//         [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
//         public IncludedRelationships Relationships { get; set; }

//         [JsonProperty("links")]
//         public AttendeeLinks Links { get; set; }
}

public class IncludedAttributes
{
//         [JsonProperty("age_max_in_months")]
//         public long? AgeMaxInMonths { get; set; }

//         [JsonProperty("age_min_in_months")]
//         public long? AgeMinInMonths { get; set; }

//         [JsonProperty("age_on")]
//         public DateTimeOffset? AgeOn { get; set; }

//         [JsonProperty("age_range_by", NullValueHandling = NullValueHandling.Ignore)]
//         public string AgeRangeBy { get; set; }

//         [JsonProperty("attendees_per_volunteer")]
//         public object AttendeesPerVolunteer { get; set; }

//         [JsonProperty("child_or_adult")]
//         public string ChildOrAdult { get; set; }

//         [JsonProperty("created_at")]
//         public DateTimeOffset CreatedAt { get; set; }

//         [JsonProperty("effective_date")]
//         public DateTimeOffset? EffectiveDate { get; set; }

//         [JsonProperty("gender")]
//         public Gender? Gender { get; set; }

//         [JsonProperty("grade_max")]
//         public long? GradeMax { get; set; }

//         [JsonProperty("grade_min")]
//         public long? GradeMin { get; set; }

//         [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
//         public IncludeType? Kind { get; set; }

//         [JsonProperty("max_occupancy")]
//         public object MaxOccupancy { get; set; }

//         [JsonProperty("min_volunteers")]
//         public object MinVolunteers { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

//         [JsonProperty("opened", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? Opened { get; set; }

//         [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
//         public long? Position { get; set; }

//         [JsonProperty("questions", NullValueHandling = NullValueHandling.Ignore)]
//         public List<object> Questions { get; set; }

//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }

//         [JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
//         public List<Address> Addresses { get; set; }

//         [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
//         public Uri AvatarUrl { get; set; }

//         [JsonProperty("birthdate", NullValueHandling = NullValueHandling.Ignore)]
//         public DateTimeOffset? Birthdate { get; set; }

//         [JsonProperty("check_in_count", NullValueHandling = NullValueHandling.Ignore)]
//         public long? CheckInCount { get; set; }

//         [JsonProperty("child", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? Child { get; set; }

//         [JsonProperty("email_addresses", NullValueHandling = NullValueHandling.Ignore)]
//         public List<EmailAddress> EmailAddresses { get; set; }

//         [JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
//         public string FirstName { get; set; }

//         [JsonProperty("grade")]
//         public long? Grade { get; set; }

//         [JsonProperty("headcounter", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? Headcounter { get; set; }

//         [JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
//         public string LastName { get; set; }

//         [JsonProperty("medical_notes")]
//         public object MedicalNotes { get; set; }

//         [JsonProperty("middle_name")]
//         public string MiddleName { get; set; }

//         [JsonProperty("name_prefix")]
//         public object NamePrefix { get; set; }

//         [JsonProperty("name_suffix")]
//         public object NameSuffix { get; set; }

//         [JsonProperty("permission")]
//         public string Permission { get; set; }

//         [JsonProperty("phone_numbers", NullValueHandling = NullValueHandling.Ignore)]
//         public List<PhoneNumber> PhoneNumbers { get; set; }

//         [JsonProperty("archived_at")]
//         public object ArchivedAt { get; set; }

//         [JsonProperty("enable_services_integration", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? EnableServicesIntegration { get; set; }

//         [JsonProperty("frequency", NullValueHandling = NullValueHandling.Ignore)]
//         public string Frequency { get; set; }

//         [JsonProperty("integration_key")]
//         public object IntegrationKey { get; set; }

//         [JsonProperty("location_times_enabled", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? LocationTimesEnabled { get; set; }

//         [JsonProperty("pre_select_enabled", NullValueHandling = NullValueHandling.Ignore)]
//         public bool? PreSelectEnabled { get; set; }
}

//     public class Address
//     {
//         [JsonProperty("street")]
//         public string Street { get; set; }

//         [JsonProperty("street_line_1")]
//         public string StreetLine1 { get; set; }

//         [JsonProperty("street_line_2")]
//         public string StreetLine2 { get; set; }

//         [JsonProperty("city")]
//         public string City { get; set; }

//         [JsonProperty("state")]
//         public string State { get; set; }

//         [JsonProperty("zip")]
//         [JsonConverter(typeof(ParseStringConverter))]
//         public long Zip { get; set; }

//         [JsonProperty("location")]
//         public Location Location { get; set; }

//         [JsonProperty("primary")]
//         public bool Primary { get; set; }
//     }

//     public class EmailAddress
//     {
//         [JsonProperty("address")]
//         public string Address { get; set; }

//         [JsonProperty("location")]
//         public Location Location { get; set; }

//         [JsonProperty("primary")]
//         public bool Primary { get; set; }

//         [JsonProperty("blocked")]
//         public bool Blocked { get; set; }
//     }

//     public class PhoneNumber
//     {
//         [JsonProperty("number")]
//         public string Number { get; set; }

//         [JsonProperty("carrier")]
//         public object Carrier { get; set; }

//         [JsonProperty("location")]
//         public string Location { get; set; }

//         [JsonProperty("primary")]
//         public bool Primary { get; set; }
//     }

//     public class IncludedRelationships
//     {
//         [JsonProperty("parent")]
//         public RelationshipsParent Parent { get; set; }
//     }

//     public class RelationshipsParent
//     {
//         [JsonProperty("data")]
//         public ParentElement Data { get; set; }
//     }

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
//         public ParentElement Parent { get; set; }
//     }

public enum AttendeeType
{
    Guest,
    Regular,
    Volunteer
}

public enum IncludeType
{
    Event,
    Location,
    Organization,
    Parent,
    Person
}

public enum PurpleType
{
    CheckIn
}

public enum Location
{
    Home
}

public enum Gender
{
    F,
    M
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
            KindConverter.Singleton,
            KindEnumConverter.Singleton,
            PurpleTypeConverter.Singleton,
            LocationConverter.Singleton,
            GenderConverter.Singleton,
            new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
        }
    };
}

internal class KindConverter : JsonConverter
{
    public static readonly KindConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(AttendeeType) || t == typeof(AttendeeType?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "Guest" => AttendeeType.Guest,
            "Regular" => AttendeeType.Regular,
            "Volunteer" => AttendeeType.Volunteer,
            _ => throw new Exception("Cannot unmarshal type Kind")
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (AttendeeType) untypedValue;
        switch (value)
        {
            case AttendeeType.Guest:
                serializer.Serialize(writer, "Guest");
                return;
            case AttendeeType.Regular:
                serializer.Serialize(writer, "Regular");
                return;
            case AttendeeType.Volunteer:
                serializer.Serialize(writer, "Volunteer");
                return;
            default:
                throw new Exception("Cannot marshal type Kind");
        }
    }
}

internal class ParseStringConverter : JsonConverter
{
    // ReSharper disable once UnusedMember.Global
    public static readonly ParseStringConverter Singleton = new();

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

internal class KindEnumConverter : JsonConverter
{
    public static readonly KindEnumConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(IncludeType) || t == typeof(IncludeType?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "Event" => IncludeType.Event,
            "Location" => IncludeType.Location,
            "Organization" => IncludeType.Organization,
            "Parent" => IncludeType.Parent,
            "Person" => IncludeType.Person,
            _ => throw new Exception("Cannot unmarshal type KindEnum")
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (IncludeType) untypedValue;
        switch (value)
        {
            case IncludeType.Event:
                serializer.Serialize(writer, "Event");
                return;
            case IncludeType.Location:
                serializer.Serialize(writer, "Location");
                return;
            case IncludeType.Organization:
                serializer.Serialize(writer, "Organization");
                return;
            case IncludeType.Parent:
                serializer.Serialize(writer, "Parent");
                return;
            case IncludeType.Person:
                serializer.Serialize(writer, "Person");
                return;
            default:
                throw new Exception("Cannot marshal type KindEnum");
        }
    }
}

internal class PurpleTypeConverter : JsonConverter
{
    public static readonly PurpleTypeConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(PurpleType) || t == typeof(PurpleType?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        if (value == "CheckIn")
        {
            return PurpleType.CheckIn;
        }

        throw new Exception("Cannot unmarshal type PurpleType");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (PurpleType) untypedValue;
        if (value != PurpleType.CheckIn)
        {
            throw new Exception("Cannot marshal type PurpleType");
        }

        serializer.Serialize(writer, "CheckIn");
    }
}

internal class LocationConverter : JsonConverter
{
    public static readonly LocationConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(Location) || t == typeof(Location?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        if (value == "Home")
        {
            return Location.Home;
        }

        throw new Exception("Cannot unmarshal type Location");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (Location) untypedValue;
        if (value != Location.Home)
        {
            throw new Exception("Cannot marshal type Location");
        }

        serializer.Serialize(writer, "Home");
    }
}

internal class GenderConverter : JsonConverter
{
    public static readonly GenderConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(Gender) || t == typeof(Gender?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "F" => Gender.F,
            "M" => Gender.M,
            _ => throw new Exception("Cannot unmarshal type Gender")
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (Gender) untypedValue;
        switch (value)
        {
            case Gender.F:
                serializer.Serialize(writer, "F");
                return;
            case Gender.M:
                serializer.Serialize(writer, "M");
                return;
            default:
                throw new Exception("Cannot marshal type Gender");
        }
    }
}