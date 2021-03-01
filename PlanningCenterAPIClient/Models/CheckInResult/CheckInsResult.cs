using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable 8618

namespace CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult
{
    public class CheckIns
    {
        [JsonProperty("links")]
        public AttendeeLinks Links { get; set; }

        [JsonProperty("data")]
        public List<Attendee> Attendees { get; set; }

        [JsonProperty("included")]
        public List<Included> Included { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class Attendee
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("attributes")]
        public AttendeeAttributes Attributes { get; set; }

        [JsonProperty("links")]
        public AttendeeLinks Links { get; set; }

        [JsonProperty("relationships")]
        public AttendeeRelationships Relationships { get; set; }
    }

    public class AttendeeAttributes
    {
        [JsonProperty("checked_out_at")]
        public DateTimeOffset? CheckedOutAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("emergency_contact_name")]
        public string EmergencyContactName { get; set; }

        [JsonProperty("emergency_contact_phone_number")]
        public string EmergencyContactPhoneNumber { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("kind")]
        public AttendeeTypeEnum Kind { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("medical_notes")]
        public object MedicalNotes { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("security_code")]
        public string SecurityCode { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class AttendeeLinks
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }
    }

    public class AttendeeRelationships
    {
        [JsonProperty("locations")]
        public Locations Locations { get; set; }

        [JsonProperty("person")]
        public Person Person { get; set; }
    }

    public class Locations
    {
        [JsonProperty("links")]
        public LocationsLinks Links { get; set; }

        [JsonProperty("data")]
        public List<ParentElement> Data { get; set; }
    }

    public class ParentElement
    {
        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }

    public class LocationsLinks
    {
        [JsonProperty("related")]
        public Uri Related { get; set; }
    }

    public class Person
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public LocationsLinks Links { get; set; }

        [JsonProperty("data")]
        public ParentElement? Data { get; set; }
    }

    public class Included
    {
        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("attributes")]
        public IncludedAttributes Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IncludedRelationships Relationships { get; set; }

        [JsonProperty("links")]
        public AttendeeLinks Links { get; set; }
    }

    public class IncludedAttributes
    {
        [JsonProperty("age_max_in_months")]
        public long? AgeMaxInMonths { get; set; }

        [JsonProperty("age_min_in_months")]
        public long? AgeMinInMonths { get; set; }

        [JsonProperty("age_on")]
        public DateTimeOffset? AgeOn { get; set; }

        [JsonProperty("age_range_by", NullValueHandling = NullValueHandling.Ignore)]
        public string AgeRangeBy { get; set; }

        [JsonProperty("attendees_per_volunteer")]
        public object AttendeesPerVolunteer { get; set; }

        [JsonProperty("child_or_adult")]
        public string ChildOrAdult { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("effective_date")]
        public DateTimeOffset? EffectiveDate { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("grade_max")]
        public long? GradeMax { get; set; }

        [JsonProperty("grade_min")]
        public long? GradeMin { get; set; }

        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        public TypeEnum? Kind { get; set; }

        [JsonProperty("max_occupancy")]
        public object MaxOccupancy { get; set; }

        [JsonProperty("min_volunteers")]
        public object MinVolunteers { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("opened", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Opened { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("questions", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Questions { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
        public List<Address> Addresses { get; set; }

        [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("birthdate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Birthdate { get; set; }

        [JsonProperty("check_in_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? CheckInCount { get; set; }

        [JsonProperty("child", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Child { get; set; }

        [JsonProperty("email_addresses", NullValueHandling = NullValueHandling.Ignore)]
        public List<EmailAddress> EmailAddresses { get; set; }

        [JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty("grade")]
        public long? Grade { get; set; }

        [JsonProperty("headcounter", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Headcounter { get; set; }

        [JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        [JsonProperty("medical_notes")]
        public object MedicalNotes { get; set; }

        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }

        [JsonProperty("name_prefix")]
        public object NamePrefix { get; set; }

        [JsonProperty("name_suffix")]
        public object NameSuffix { get; set; }

        [JsonProperty("permission")]
        public string Permission { get; set; }

        [JsonProperty("phone_numbers", NullValueHandling = NullValueHandling.Ignore)]
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }

    public class Address
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("street_line_1")]
        public string StreetLine1 { get; set; }

        [JsonProperty("street_line_2")]
        public string StreetLine2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Zip { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }

    public class EmailAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }

        [JsonProperty("blocked")]
        public bool Blocked { get; set; }
    }

    public class PhoneNumber
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("carrier")]
        public object Carrier { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }

    public class IncludedRelationships
    {
        [JsonProperty("parent")]
        public RelationshipsParent Parent { get; set; }
    }

    public class RelationshipsParent
    {
        [JsonProperty("data")]
        public ParentElement Data { get; set; }
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
        public ParentElement Parent { get; set; }
    }

    public enum TypeEnum { Location, Organization, Parent, Person };
    public enum AttendeeTypeEnum { Regular, Guest, Volunteer };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TypeEnumConverter.Singleton,
                AttendeeTypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
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

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
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

        public static readonly ParseStringConverter Singleton = new();
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Location":
                    return TypeEnum.Location;
                case "Organization":
                    return TypeEnum.Organization;
                case "Parent":
                    return TypeEnum.Parent;
                case "Person":
                    return TypeEnum.Person;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Location:
                    serializer.Serialize(writer, "Location");
                    return;
                case TypeEnum.Organization:
                    serializer.Serialize(writer, "Organization");
                    return;
                case TypeEnum.Parent:
                    serializer.Serialize(writer, "Parent");
                    return;
                case TypeEnum.Person:
                    serializer.Serialize(writer, "Person");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new();
    }
    
    internal class AttendeeTypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AttendeeTypeEnum) || t == typeof(AttendeeTypeEnum?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Regular":
                    return AttendeeTypeEnum.Regular;
                case "Guest":
                    return AttendeeTypeEnum.Guest;
                case "Volunteer":
                    return AttendeeTypeEnum.Volunteer;
            }
            throw new Exception("Cannot unmarshal type AttendeeTypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (AttendeeTypeEnum)untypedValue;
            switch (value)
            {
                case AttendeeTypeEnum.Regular:
                    serializer.Serialize(writer, "Regular");
                    return;
                case AttendeeTypeEnum.Guest:
                    serializer.Serialize(writer, "Guest");
                    return;
                case AttendeeTypeEnum.Volunteer:
                    serializer.Serialize(writer, "Volunteer");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly AttendeeTypeEnumConverter Singleton = new();
    }
}
