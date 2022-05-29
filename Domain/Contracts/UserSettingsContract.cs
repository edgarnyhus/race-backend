using Domain.Models;
using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class UserSettingsContract
    {
        [JsonProperty("user_id")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        public string[]? Widgets { get; set; } = new string[]
            {""};

        [JsonProperty("certification_warning")]
        public int CertificationWarning { get; set; } = 30;

        public string? Language { get; set; } = "en";
    }
}