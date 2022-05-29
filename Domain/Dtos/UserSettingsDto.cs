using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class UserSettingsDto : EntityBaseDto
    {
        public string? UserId { get; set; }
        public string[]? Widgets { get; set; }
        [JsonProperty("certification_warning")]
        public int CertificationWarning { get; set; }
        public string? Language { get; set; }
    }
}
