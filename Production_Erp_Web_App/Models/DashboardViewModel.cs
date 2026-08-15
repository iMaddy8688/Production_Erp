namespace Production_Erp_Web_App.Models;

public class DashboardViewModel
{
    public int TotalItems { get; set; }
    public int LowStockItems { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalSalesInvoices { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int TotalPurchaseInvoices { get; set; }
    public decimal TotalPurchaseAmount { get; set; }

    public List<RecentInvoiceRow> RecentSales { get; set; } = new();
    public List<RecentInvoiceRow> RecentPurchases { get; set; } = new();
}

public class RecentInvoiceRow
{
    public string InvoiceNumber { get; set; } = default!;
    public string PartyName { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
}
