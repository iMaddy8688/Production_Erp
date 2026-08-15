using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Production_Erp_Web_App.DbApp;
using Production_Erp_Web_App.Models;

namespace Production_Erp_Web_App.Controllers;

public class DashboardController : Controller
{
    private const int RecentRowCount = 5;

    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalItems = await _context.Items.CountAsync(),
            LowStockItems = await _context.Items.CountAsync(i => i.IsActive && i.StockQuantity <= i.ReorderLevel),
            TotalSuppliers = await _context.Suppliers.CountAsync(s =>    s.IsActive),
            TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive),
            TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
            TotalSalesInvoices = await _context.SalesInvoices.CountAsync(),
            TotalSalesAmount = await _context.SalesInvoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m,
            TotalPurchaseInvoices = await _context.PurchaseInvoices.CountAsync(),
            TotalPurchaseAmount = await _context.PurchaseInvoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m,

            RecentSales = await _context.SalesInvoices
                .Include(s => s.Customer)
                .OrderByDescending(s => s.InvoiceDate)
                .Take(RecentRowCount)
                .Select(s => new RecentInvoiceRow
                {
                    InvoiceNumber = s.InvoiceNumber,
                    PartyName = s.Customer.Name,
                    InvoiceDate = s.InvoiceDate,
                    TotalAmount = s.TotalAmount,
                })
                .ToListAsync(),

            RecentPurchases = await _context.PurchaseInvoices
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.InvoiceDate)
                .Take(RecentRowCount)
                .Select(p => new RecentInvoiceRow
                {
                    InvoiceNumber = p.InvoiceNumber,
                    PartyName = p.Supplier.Name,
                    InvoiceDate = p.InvoiceDate,
                    TotalAmount = p.TotalAmount,
                })
                .ToListAsync(),
        };

        return View(model);
    }
}
