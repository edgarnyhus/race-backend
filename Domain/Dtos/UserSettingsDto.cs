using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Dtos
{
    public class UserSettingsDto : EntityBaseDto
    {
        public string? UserId { get; set; }
        public string[]? Widgets { get; set; }
        [JsonPropertyName("certification_warning")]
        public int CertificationWarning { get; set; }
        public string? Language { get; set; }
    }
}
