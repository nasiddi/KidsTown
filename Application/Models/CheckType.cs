using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Application.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CheckType
    {
        [EnumMember(Value = "CheckInUpdateJobs")]
        CheckIn,
        [EnumMember(Value = "CheckOut")]
        CheckOut,

    }
}