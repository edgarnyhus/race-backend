using System;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class IdentityDto
    {
        public string? Connection { get; set; }
        [JsonProperty("user_id")]
        public string? UserId { get; set; }
        public string? Provider { get; set; }
        public bool? IsSocial { get; set; }
    }

    public class AppMetadataDto
    {
        [JsonProperty("tenant_id")]

        public string? TenantId { get; set; }
        [JsonProperty("organization_id")]
        public Guid? OrganizationId { get; set; }
        public string[]? Roles { get; set; }
    }

    public class UserMetadataDto
    {

    }

    public class MultifactorDto
    {

    }

    public class UserDto : EntityBaseDto
    {
        [JsonProperty("user_id")]
        public string? UserId { get; set; } = null!;
        public string? Name { get; set; } = null!;
        public string? Nickname { get; set; } = null!;
        public string? Email { get; set; }
        [JsonProperty("email_verified")]
        public bool? EmailVerified { get; set; }
        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        //public IdentityDto[]? Identities { get; set; }
        //[JsonProperty("app_metadata")]
        //public AppMetadataDto? AppMetadata { get; set; }
        //[JsonProperty("user_metadata")]
        //public UserMetadataDto? UserMetadata { get; set; }
        //public string? Picture { get; set; }
        [JsonProperty("last_login")]
        public DateTime? LastLogin { get; set; }
        [JsonProperty("logins_count")]
        public int? LoginsCount { get; set; }
        public bool? Blocked { get; set; }
        // [JsonProperty("user_settings")]
        // public UserSettingsDto? UserSettings { get; set; }
        [JsonProperty("tenannt_id")]
        public string? TenantId { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
    }
}