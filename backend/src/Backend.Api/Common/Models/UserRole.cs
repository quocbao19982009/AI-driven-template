using System.Text.Json.Serialization;

namespace Backend.Common.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin
}
