using Newtonsoft.Json;
using Domain.Dtos;
using Domain.Models;
using System;
using System.Text.Json.Serialization;

namespace Domain.Contracts
{
    public class AppMetadataContract
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        public string[]? Roles { get; set; }
    }

    public class UserMetadataContract
    {

    }

    public class UserContract
    {
        public string? Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? Nickname { get; set; } = null!;
        public string? Email { get; set; }
        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }
        //[JsonProperty("user_metadata")]
        //public UserMetadataContract? userMetadata { get; set; }
        [JsonProperty("app_metadata")]
        public AppMetadataContract? AppMetadata { get; set; }
        //public string? Picture { get; set; }
        //[JsonProperty("user_id")]
        //public string? UserId { get; set; } = null!;
        //public string? Password { get; set; }
        //[JsonProperty("email_verified")]
        //public bool? EmailVerified { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
    }

}