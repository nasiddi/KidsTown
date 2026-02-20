using Newtonsoft.Json;

namespace PlanningCenterApiClient.Models.HouseholdResult;

//
public class Household : IPlanningCenterResponse
{
    // [JsonProperty(propertyName: "data")]
    // public Data? Data { get; set; }

    [JsonProperty("included")] public List<Included>? Included { get; set; }

//
//         [JsonProperty("meta")]
//         public Meta Meta { get; set; }
//         
    public Uri? NextLink => null;
//
}

//
// public class Data
// {
//         [JsonProperty("type")]
//         public string Type { get; set; }
//
//         [JsonProperty("id")]
//         [JsonConverter(typeof(ParseStringConverter))]
//         public long Id { get; set; }
//
// [JsonProperty(propertyName: "attributes")]
// public DataAttributes? Attributes { get; set; }
//
//         [JsonProperty("relationships")]
//         public DataRelationships Relationships { get; set; }
//
//         [JsonProperty("links")]
//         public DataLinks Links { get; set; }
// }
//
//         public class DataAttribute
//         {
//         [JsonProperty("avatar")]
//         public Uri Avatar { get; set; }
//
//         [JsonProperty("created_at")]
//         public DateTimeOffset CreatedAt { get; set; }
//
//         [JsonProperty("member_count")]
//         public long MemberCount { get; set; }
//
//         [JsonProperty("name")]
//         public string Name { get; set; }
//
//         [JsonProperty("primary_contact_id")]
//         [JsonConverter(typeof(ParseStringConverter))]
//         public long PrimaryContactId { get; set; }
//
//         [JsonProperty("primary_contact_name")]
//         public string PrimaryContactName { get; set; }
//
//         [JsonProperty("updated_at")]
//         public DateTimeOffset UpdatedAt { get; set; }
// }
//
//     public partial class DataLinks
//     {
//         [JsonProperty("household_memberships")]
//         public Uri HouseholdMemberships { get; set; }
//
//         [JsonProperty("people")]
//         public Uri People { get; set; }
//
//         [JsonProperty("self")]
//         public Uri Self { get; set; }
//     }
//
//     public partial class DataRelationships
//     {
//         [JsonProperty("primary_contact")]
//         public PrimaryC PrimaryContact { get; set; }
//
//         [JsonProperty("people")]
//         public People People { get; set; }
//     }
//
//     public partial class People
//     {
//         [JsonProperty("links")]
//         public PeopleLinks Links { get; set; }
//
//         [JsonProperty("data")]
//         public List<Parent> Data { get; set; }
//     }
//
//     public partial class Parent
//     {
//         [JsonProperty("type")]
//         public string Type { get; set; }
//
//         [JsonProperty("id")]
//         [JsonConverter(typeof(ParseStringConverter))]
//         public long Id { get; set; }
//     }
//
//     public partial class PeopleLinks
//     {
//         [JsonProperty("related")]
//         public Uri Related { get; set; }
//     }
//
//     public partial class PrimaryC
//     {
//         [JsonProperty("data")]
//         public Parent Data { get; set; }
//     }
//
public class Included
{
    // [JsonProperty(propertyName: "type")]
    // public string? Type { get; set; }
//
    [JsonProperty("id")]
    [JsonConverter(typeof(ParseStringConverter))]
    public long Id { get; set; }

//
    [JsonProperty("attributes")] public IncludedAttributes? Attributes { get; set; }
//
//         [JsonProperty("relationships")]
//         public IncludedRelationships Relationships { get; set; }
//
//         [JsonProperty("links")]
//         public IncludedLinks Links { get; set; }
}

//
public class IncludedAttributes
{
//         [JsonProperty("accounting_administrator")]
//         public bool AccountingAdministrator { get; set; }
//
//         [JsonProperty("anniversary")]
//         public DateTimeOffset? Anniversary { get; set; }
//
//         [JsonProperty("avatar")]
//         public Uri Avatar { get; set; }
//
//         [JsonProperty("birthdate")]
//         public DateTimeOffset?a Birthdate { get; set; }
//
//         [JsonProperty("can_create_forms")]
//         public bool CanCreateForms { get; set; }
//
    [JsonProperty("child")] public bool Child { get; set; }
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
//         [JsonProperty("first_name")]
//         public string FirstName { get; set; }
//
//         [JsonProperty("gender")]
//         public string Gender { get; set; }
//
//         [JsonProperty("given_name")]
//         public object GivenName { get; set; }
//
//         [JsonProperty("grade")]
//         public object Grade { get; set; }
//
//         [JsonProperty("graduation_year")]
//         public object GraduationYear { get; set; }
//
//         [JsonProperty("inactivated_at")]
//         public object InactivatedAt { get; set; }
//
//         [JsonProperty("last_name")]
//         public string LastName { get; set; }
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
//     public partial class IncludedLinks
//     {
//         [JsonProperty("self")]
//         public Uri Self { get; set; }
//     }
//
//     public partial class IncludedRelationships
//     {
//         [JsonProperty("primary_campus")]
//         public PrimaryC PrimaryCampus { get; set; }
//     }
//
//     public partial class Meta
//     {
//         [JsonProperty("can_include")]
//         public List<string> CanInclude { get; set; }
//
//         [JsonProperty("parent")]
//         public Parent Parent { get; set; }
//     }
//
//     public partial class Household
//     {
//         public static Household FromJson(string json) => JsonConvert.DeserializeObject<Household>(json, Converter.Settings);
//     }
//
//     public static class Serialize
//     {
//         public static string ToJson(this Household self) => JsonConvert.SerializeObject(self, Converter.Settings);
//     }
//
//     internal static class Converter
//     {
//         public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
//         {
//             MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
//             DateParseHandling = DateParseHandling.None,
//             Converters =
//             {
//                 new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
//             },
//         };
//     }
//
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