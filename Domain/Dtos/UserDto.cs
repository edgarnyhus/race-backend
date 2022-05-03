using System;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class IdentityDto
    {
        public string? connection { get; set; }
        [JsonPropertyName("user_id")]
        public string? userId { get; set; }
        public string? provider { get; set; }
        public bool? isSocial { get; set; }
    }

    public class AppMetadataDto
    {
        [JsonPropertyName("tenant_id")]

        public string? tenantId { get; set; }
        [JsonPropertyName("organization_id")]
        public Guid? organizationId { get; set; }
        public string[]? roles { get; set; }
    }

    public class UserMetadataDto
    {

    }

    public class MultifactorDto
    {

    }

    public class UserDto : EntityBaseDto
    {
        [JsonPropertyName("user_id")]
        public string? userId { get; set; } = null!;
        public string? name { get; set; } = null!;
        public string? nickname { get; set; } = null!;
        public string? email { get; set; }
        [JsonPropertyName("email_verified")]
        public bool? emailVerified { get; set; }
        [JsonPropertyName("phone_number")]
        public string? phoneNumber { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime? createdAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime? updatedAt { get; set; }
        //public IdentityDto[]? identities { get; set; }
        //[JsonPropertyName("app_metadata")]
        //public AppMetadataDto? appMetadata { get; set; }
        //[JsonPropertyName("user_metadata")]
        //public UserMetadataDto? userMetadata { get; set; }
        //public string? picture { get; set; }
        [JsonPropertyName("last_login")]
        public DateTime? lastLogin { get; set; }
        [JsonPropertyName("logins_count")]
        public int? loginsCount { get; set; }
        public bool? blocked { get; set; }
        // [JsonPropertyName("user_settings")]
        // public UserSettingsDto? userSettings { get; set; }
        [JsonPropertyName("tenannt_id")]
        public string? tenantId { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
    }
}