using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using Newtonsoft.Json;

namespace Domain.Models
{
    /// 
    /// Auth0 User
    /// 

    
    public class UserList
    {
        public IList<string>? users { get; set; }
    }

    public class Identity
    {
        public string? Connection { get; set; }
        [JsonProperty("user_id")]
        public string? UserId { get; set; }
        public string? Provider { get; set; }
        public bool? IsSocial { get; set; }
    }

    public class AppMetadata
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        public string[]? Roles { get; set; }
    }

    public class UserMetadata
    {

    }

    public class Multifactor
    {

    }

    public class User : EntityBase
    {
        [ForeignKey("TenantId")]
        public Guid? TenantId { get; set; }
        [JsonProperty("user_id")]
        public string? UserId { get; set; }
        public string? Email { get; set; }
        [JsonProperty("email_verified")]
        public bool? EmailVerified { get; set; }
        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        public Identity[]? Identities { get; set; }
        [JsonProperty("app_metadata")]
        public AppMetadata? AppMetadata { get; set; }
        [JsonProperty("user_metadata")]
        public UserMetadata? UserMetadata { get; set; }
        public string? Picture { get; set; }
        public string? Name { get; set; } = null!;
        public string? Nickname { get; set; } = null!;
        [JsonProperty("last_login")]
        public DateTime? LastLogin { get; set; }
        [JsonProperty("logins_count")]
        public int? LoginsCount { get; set; }
        public bool? Blocked { get; set; }
        public string? Password { get; set; }
        public string? Connection { get; set; } = null!;
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public UserSettings? UserSettings { get; set; } 
    }
}