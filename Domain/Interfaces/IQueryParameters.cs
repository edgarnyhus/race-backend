using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IQueryParameters
    {
        bool multitenancy { get; set; }
        string? state { get; set; }
        string? within_radius { get; set; }
        string? within_square { get; set; }
        string? within_area { get; set; }
        string? sign_type { get; set; }
        string? user_id { get; set; }
        string? category { get; set; }
        string? qr_code { get; set; }
        string? tenant_id { get; set; }
        string? organization_id { get; set; }
        string? organization_number { get; set; }
        string? customer_number { get; set; }
        string? phone_number { get; set; }
        string? email { get; set; }
        string? race_id { get; set; }
        string? sign_group_id { get; set; }

        // Paging      
        int page { get; set; }
        int page_size { get; set; }
    }
}
