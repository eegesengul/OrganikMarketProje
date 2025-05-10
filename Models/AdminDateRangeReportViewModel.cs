namespace OrganikMarketProje.Models.Admin
{
    public class AdminDateRangeReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<TopProductItem> TopProducts { get; set; } = new();
        public List<OrderSummaryItem> Orders { get; set; } = new();
    }

    public class OrderSummaryItem
    {
        public int OrderId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
