using System;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Contracts
{
    public class UserSettingsContract
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        public string[]? Widgets { get; set; } = new string[]
            {""};

        [JsonPropertyName("certification_warning")]
        public int CertificationWarning { get; set; } = 30;

        public string? Language { get; set; } = "en";
    }
}