using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Application.Models
{
    [JsonConverter(converterType: typeof(StringEnumConverter))]
    public enum CheckType
    {
        [EnumMember(Value = "CheckIn")]
        CheckIn,
        [EnumMember(Value = "CheckOut")]
        CheckOut,
        [EnumMember(Value = "GuestCheckIn")]
        GuestCheckIn

    }
}