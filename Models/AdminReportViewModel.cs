namespace OrganikMarketProje.Models.Admin
{
    public class AdminReportViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public List<TopProductItem> TopProducts { get; set; } = new();
    }

    public class TopProductItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }
}
