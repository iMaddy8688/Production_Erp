using Production_Erp_Web_App.Domain.Common;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class PurchaseInvoice:BaseEntity
    {
        public string InvoiceNumber { get; set; } = default!;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = default!;
    }
}
