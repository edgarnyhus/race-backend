using Domain.Interfaces;

namespace Domain.Queries.Helpers
{
    public class QueryParameters : IQueryParameters
    {
        //public EquipmentType equipment_type { get; set; }
        public QueryParameters()
        {
            max_page_size = 1000;
            page_size = 500;
            page = 1;
        }

        public bool multitenancy { get; set; }
        public string? state { get; set; }
        public string? within_radius { get; set; }
        public string? within_square { get; set; }
        public string? within_area { get; set; }
        public string? sign_type { get; set; }
        public string? user_id { get; set; }
        public string? category { get; set; }
        public string? qr_code { get; set; }
        public string? tenant_id { get; set; }
        public string? organization_id { get; set; }
        public string? organization_number { get; set; }
        public string? customer_number { get; set; }
        public string? phone_number { get; set; }
        public string? email { get; set; }
        public string? race_id { get; set; }

        // Paging      
        public int max_page_size { get; set; }
        public int page { get; set; }
        private int _page_size { get; set; }
        public int page_size {
            get { return _page_size; }
            set {
                _page_size = (value > max_page_size) ? max_page_size : value;
            }
        }
    }
}