using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Models
{
    public class UserSettings : EntityBase
    {
        public User? User { get; set; }
        public Guid UserId { get; set; }

        public string[] Widgets { get; set; } =
            new string[] {"ContainerTotal", "ContainerLost", "Vehicles", "ContainerState", "AreaOverview"};
    public int? CertificationWarning { get; set; } = 30;
        public string? Language { get; set; } = "en";
    }
}
