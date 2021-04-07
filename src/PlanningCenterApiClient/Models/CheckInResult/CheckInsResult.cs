using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KidsTown.PlanningCenterApiClient.Models.CheckInResult
{
    public class CheckIns : IPlanningCenterResponse
    {
         [JsonProperty(propertyName: "links")]
         public AttendeeLinks? Links { get; set; }

         [JsonProperty(propertyName: "data")]
         public List<Attendee>? Attendees { get; set; }

         [JsonProperty(propertyName: "included")]
         public List<Included>? Included { get; set; }

//         [JsonProperty("meta")]
//         public Meta Meta { get; set; }

        public Uri? NextLink => Links?.Next;
    }

     public class Attendee
     {
//         [JsonProperty("type")]
//         public string Type { get; set; }

         [JsonProperty(propertyName: "id")]
         [JsonConverter(converterType: typeof(ParseStringConverter))]
         public long Id { get; set; }

         [JsonProperty(propertyName: "attributes")]
         public AttendeeAttributes? Attributes { get; set; }

//         [JsonProperty("links")]
//         public AttendeeLinks Links { get; set; }

         [JsonProperty(propertyName: "relationships")]
         public AttendeeRelationships? Relationships { get; set; }
     }

     public class AttendeeAttributes
     {
//         [JsonProperty("checked_out_at")]
//         public DateTimeOffset? CheckedOutAt { get; set; }

         [JsonProperty(propertyName: "created_at")]
         public DateTime CreatedAt { get; set; }

//         [JsonProperty("emergency_contact_name")]
//         public string EmergencyContactName { get; set; }

//         [JsonProperty("emergency_contact_phone_number")]
//         public string EmergencyContactPhoneNumber { get; set; }

         [JsonProperty(propertyName: "first_name")]
         public string? FirstName { get; set; }

         [JsonProperty(propertyName: "kind")]
         public AttendeeType Kind { get; set; }

         [JsonProperty(propertyName: "last_name")]
         public string? LastName { get; set; }

//         [JsonProperty("medical_notes")]
//         public object MedicalNotes { get; set; }

//         [JsonProperty("number")]
//         public long Number { get; set; }

         [JsonProperty(propertyName: "security_code")]
         public string? SecurityCode { get; set; }

//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }
     }

     public class AttendeeLinks
     {
         // [JsonProperty("self")]
         // public Uri? Self { get; set; }
         
         [JsonProperty(propertyName: "next")]
         public Uri? Next { get; set; }
         
         // [JsonProperty("prev")]
         // public Uri? Prev { get; set; }
     }

     public class AttendeeRelationships
     {
         [JsonProperty(propertyName: "locations")]
         public Locations? Locations { get; set; }

         [JsonProperty(propertyName: "person")]
         public Relationship? Person { get; set; }

         [JsonProperty(propertyName: "event")]
         public Relationship? Event { get; set; }
     }

     public class Relationship
     {
//         [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
//         public EventLinks Links { get; set; }

         [JsonProperty(propertyName: "data")]
         public ParentElement? Data { get; set; }
     }

     public class ParentElement
     {
//         [JsonProperty("type")]
//         public IncludeType Type { get; set; }

         [JsonProperty(propertyName: "id")]
         [JsonConverter(converterType: typeof(ParseStringConverter))]
         public long Id { get; set; }
     }

//     public class EventLinks
//     {
//         [JsonProperty("related")]
//         public Uri Related { get; set; }
//     }

     public class Locations
     {
//         [JsonProperty("links")]
//         public EventLinks Links { get; set; }

         [JsonProperty(propertyName: "data")]
         public List<ParentElement>? Data { get; set; }
     }

     public class Included
     {
         [JsonProperty(propertyName: "type")]
         public IncludeType Type { get; set; }

         [JsonProperty(propertyName: "id")]
         [JsonConverter(converterType: typeof(ParseStringConverter))]
         public long Id { get; set; }

         [JsonProperty(propertyName: "attributes")]
         public IncludedAttributes? Attributes { get; set; }

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

         [JsonProperty(propertyName: "name", NullValueHandling = NullValueHandling.Ignore)]
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

     public enum AttendeeType { Guest, Regular, Volunteer }

     public enum IncludeType { Event, Location, Organization, Parent, Person }

     public enum PurpleType { CheckIn }

     public enum Location { Home }

     public enum Gender { F, M }

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
                 new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
             }
         };
     }

     internal class KindConverter : JsonConverter
     {
         public override bool CanConvert(Type t) => t == typeof(AttendeeType) || t == typeof(AttendeeType?);

         public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
         {
             if (reader.TokenType == JsonToken.Null) return null;
             var value = serializer.Deserialize<string>(reader: reader);
             return value switch
             {
                 "Guest" => AttendeeType.Guest,
                 "Regular" => AttendeeType.Regular,
                 "Volunteer" => AttendeeType.Volunteer,
                 _ => throw new Exception(message: "Cannot unmarshal type Kind")
             };
         }

         public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
         {
             if (untypedValue == null)
             {
                 serializer.Serialize(jsonWriter: writer, value: null);
                 return;
             }
             var value = (AttendeeType)untypedValue;
             switch (value)
             {
                 case AttendeeType.Guest:
                     serializer.Serialize(jsonWriter: writer, value: "Guest");
                     return;
                 case AttendeeType.Regular:
                     serializer.Serialize(jsonWriter: writer, value: "Regular");
                     return;
                 case AttendeeType.Volunteer:
                     serializer.Serialize(jsonWriter: writer, value: "Volunteer");
                     return;
                 default:
                     throw new Exception(message: "Cannot marshal type Kind");
             }
         }

         public static readonly KindConverter Singleton = new();
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

     internal class KindEnumConverter : JsonConverter
     {
         public override bool CanConvert(Type t) => t == typeof(IncludeType) || t == typeof(IncludeType?);

         public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
         {
             if (reader.TokenType == JsonToken.Null) return null;
             var value = serializer.Deserialize<string>(reader: reader);
             return value switch
             {
                 "Event" => IncludeType.Event,
                 "Location" => IncludeType.Location,
                 "Organization" => IncludeType.Organization,
                 "Parent" => IncludeType.Parent,
                 "Person" => IncludeType.Person,
                 _ => throw new Exception(message: "Cannot unmarshal type KindEnum")
             };
         }

         public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
         {
             if (untypedValue == null)
             {
                 serializer.Serialize(jsonWriter: writer, value: null);
                 return;
             }
             var value = (IncludeType)untypedValue;
             switch (value)
             {
                 case IncludeType.Event:
                     serializer.Serialize(jsonWriter: writer, value: "Event");
                     return;
                 case IncludeType.Location:
                     serializer.Serialize(jsonWriter: writer, value: "Location");
                     return;
                 case IncludeType.Organization:
                     serializer.Serialize(jsonWriter: writer, value: "Organization");
                     return;
                 case IncludeType.Parent:
                     serializer.Serialize(jsonWriter: writer, value: "Parent");
                     return;
                 case IncludeType.Person:
                     serializer.Serialize(jsonWriter: writer, value: "Person");
                     return;
                 default:
                     throw new Exception(message: "Cannot marshal type KindEnum");
             }
         }

         public static readonly KindEnumConverter Singleton = new();
     }

     internal class PurpleTypeConverter : JsonConverter
     {
         public override bool CanConvert(Type t) => t == typeof(PurpleType) || t == typeof(PurpleType?);

         public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
         {
             if (reader.TokenType == JsonToken.Null) return null;
             var value = serializer.Deserialize<string>(reader: reader);
             if (value == "CheckIn")
             {
                 return PurpleType.CheckIn;
             }
             throw new Exception(message: "Cannot unmarshal type PurpleType");
         }

         public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
         {
             if (untypedValue == null)
             {
                 serializer.Serialize(jsonWriter: writer, value: null);
                 return;
             }
             var value = (PurpleType)untypedValue;
             if (value != PurpleType.CheckIn) throw new Exception(message: "Cannot marshal type PurpleType");
             serializer.Serialize(jsonWriter: writer, value: "CheckIn");
         }

         public static readonly PurpleTypeConverter Singleton = new();
     }

     internal class LocationConverter : JsonConverter
     {
         public override bool CanConvert(Type t) => t == typeof(Location) || t == typeof(Location?);

         public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
         {
             if (reader.TokenType == JsonToken.Null) return null;
             var value = serializer.Deserialize<string>(reader: reader);
             if (value == "Home")
             {
                 return Location.Home;
             }
             throw new Exception(message: "Cannot unmarshal type Location");
         }

         public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
         {
             if (untypedValue == null)
             {
                 serializer.Serialize(jsonWriter: writer, value: null);
                 return;
             }
             var value = (Location)untypedValue;
             if (value != Location.Home) throw new Exception(message: "Cannot marshal type Location");
             serializer.Serialize(jsonWriter: writer, value: "Home");
         }

         public static readonly LocationConverter Singleton = new();
     }

     internal class GenderConverter : JsonConverter
     {
         public override bool CanConvert(Type t) => t == typeof(Gender) || t == typeof(Gender?);

         public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
         {
             if (reader.TokenType == JsonToken.Null) return null;
             var value = serializer.Deserialize<string>(reader: reader);
             return value switch
             {
                 "F" => Gender.F,
                 "M" => Gender.M,
                 _ => throw new Exception(message: "Cannot unmarshal type Gender")
             };
         }

         public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
         {
             if (untypedValue == null)
             {
                 serializer.Serialize(jsonWriter: writer, value: null);
                 return;
             }
             var value = (Gender)untypedValue;
             switch (value)
             {
                 case Gender.F:
                     serializer.Serialize(jsonWriter: writer, value: "F");
                     return;
                 case Gender.M:
                     serializer.Serialize(jsonWriter: writer, value: "M");
                     return;
                 default:
                     throw new Exception(message: "Cannot marshal type Gender");
             }
         }

         public static readonly GenderConverter Singleton = new();
     }
}
