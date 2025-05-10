using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models.Admin;

namespace OrganikMarketProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportController : Controller
    {
        private readonly AppDbContext _context;

        public AdminReportController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUsers = _context.Users.Count();
            var totalOrders = _context.Orders.Count();

            var topProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            var viewModel = new AdminReportViewModel
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TopProducts = topProducts
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult DateRangeReport()
        {
            var viewModel = new AdminDateRangeReportViewModel
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DateRangeReport(DateTime startDate, DateTime endDate)
        {
            var ordersInRange = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            var totalRevenue = ordersInRange.Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price));
            var totalOrders = ordersInRange.Count;

            var topProducts = ordersInRange
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            var orderSummaries = ordersInRange.Select(o => new OrderSummaryItem
            {
                OrderId = o.Id,
                UserName = o.UserName ?? "",
                OrderDate = o.OrderDate,
                TotalAmount = o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)
            }).ToList();

            var viewModel = new AdminDateRangeReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TopProducts = topProducts,
                Orders = orderSummaries
            };

            return View("DateRangeReport", viewModel);
        }
    }
}
