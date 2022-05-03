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
        public string? tenantId { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        public string[]? roles { get; set; }
    }

    public class UserMetadataContract
    {

    }

    public class UserContract
    {
        public string? id { get; set; }
        public string? name { get; set; } = null!;
        public string? nickname { get; set; } = null!;
        public string? email { get; set; }
        [JsonPropertyName("phone_number")]
        public string? phoneNumber { get; set; }
        //[JsonPropertyName("user_metadata")]
        //public UserMetadataContract? userMetadata { get; set; }
        //[JsonPropertyName("app_metadata")]
        //public AppMetadataContract? appMetadata { get; set; }
        //public string? picture { get; set; }
        //[JsonPropertyName("user_id")]
        //public string? userId { get; set; } = null!;
        //public string? password { get; set; }
        //[JsonProperty("email_verified")]
        //public bool? emailVerified { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
    }

}